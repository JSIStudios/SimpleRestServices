using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using JSIStudios.SimpleRESTServices.Core;
using Newtonsoft.Json;

namespace JSIStudios.SimpleRESTServices.Client.Json
{
    public class JsonRestServices : IRestService
    {
        private readonly IRetryLogic<Response, int> _retryLogic;
        private readonly IRequestLogger _logger;
        private readonly IUrlBuilder _urlBuilder;

        public JsonRestServices():this(null, null, null){}
        public JsonRestServices(IRequestLogger requestLogger):this(null, requestLogger, null){}
        public JsonRestServices(IRetryLogic<Response, int> retryLogic, IRequestLogger logger, IUrlBuilder urlBuilder)
        {
            if(retryLogic == null)
                retryLogic = new RequestRetryLogic();
            if(urlBuilder == null)
                urlBuilder = new UrlBuilder();

            _retryLogic = retryLogic;
            _logger = logger;
            _urlBuilder = urlBuilder;
        }

        public Response<T> Execute<T, TBody>(string url, HttpMethod method, TBody body, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings)
        {
            return Execute<T, TBody>(new Uri(url), method, body, headers, queryStringParameters, settings);
        }

        public Response<T> Execute<T, TBody>(Uri url, HttpMethod method, TBody body, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters,  RequestSettings settings)
        {
            var rawBody = JsonConvert.SerializeObject(body, new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});
            return Execute<T>(url, method, rawBody, headers, queryStringParameters, settings);
        }

        public Response<T> Execute<T>(string url, HttpMethod method, string body, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings)
        {
            return Execute<T>(new Uri(url), method, body, headers, queryStringParameters, settings);
        }

        public Response<T> Execute<T>(Uri url, HttpMethod method, string body, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings)
        {
            return Execute(url, method, (resp, isError) => BuildWebResponse<T>(resp), body, headers, queryStringParameters, settings) as Response<T>;
        }

        public Response Execute<TBody>(string url, HttpMethod method, TBody body, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings)
        {
            return Execute(new Uri(url), method, body, headers, queryStringParameters, settings);
        }

        public Response Execute<TBody>(Uri url, HttpMethod method, TBody body, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings)
        {
            var rawBody = JsonConvert.SerializeObject(body, new JsonSerializerSettings(){NullValueHandling = NullValueHandling.Ignore});
            return Execute(url, method, rawBody, headers, queryStringParameters, settings);
        }

        public Response Execute(string url, HttpMethod method, string body, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings)
        {
            return Execute(new Uri(url), method, body, headers, queryStringParameters, settings);
        }

        public Response Execute(Uri url, HttpMethod method, string body, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings)
        {
            return Execute(url, method, null, body, headers, queryStringParameters, settings);
        }

        public Response Execute(Uri url, HttpMethod method, Func<HttpWebResponse, bool, Response> responseBuilderCallback, string body, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings)
        {
            return ExecuteRequest(url, method, responseBuilderCallback, headers, queryStringParameters, settings, (req) =>
            {
                // Encode the parameters as form data:
                if (!string.IsNullOrWhiteSpace(body))
                {
                    byte[] formData = UTF8Encoding.UTF8.GetBytes(body);
                    req.ContentLength = formData.Length;

                    // Send the request:
                    using (Stream post = req.GetRequestStream())
                    {
                        post.Write(formData, 0, formData.Length);
                    }
                }

                return body;
            });     
        }

        public Response<T> Stream<T>(string url, HttpMethod method, Stream content, int chunkSize, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings, Action<long> progressUpdated)
        {
            return Stream<T>(new Uri(url), method, content, chunkSize, headers, queryStringParameters, settings, progressUpdated)  as Response<T>;
        }

        public Response Stream(string url, HttpMethod method, Stream content, int chunkSize, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings, Action<long> progressUpdated)
        {
            return Stream(new Uri(url), method, content, chunkSize, headers, queryStringParameters, settings, progressUpdated);
        }

        public Response<T> Stream<T>(Uri url, HttpMethod method, Stream content, int chunkSize, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings, Action<long> progressUpdated)
        {
            return Stream(url, method, (resp, isError) => BuildWebResponse<T>(resp), content, chunkSize, headers, queryStringParameters, settings, progressUpdated) as Response<T>;
        }

        public Response Stream(Uri url, HttpMethod method, Stream content, int chunkSize, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings, Action<long> progressUpdated)
        {
            return Stream(url, method, null, content, chunkSize, headers, queryStringParameters, settings, progressUpdated);
        }

        private Response Stream(Uri url, HttpMethod method, Func<HttpWebResponse, bool, Response> responseBuilderCallback, Stream contents, int chunkSize, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings, Action<long> progressUpdated)
        {
            return ExecuteRequest(url, method, responseBuilderCallback, headers, queryStringParameters, settings, (req) =>
            {
                long bytesWritten = 0;

                req.AllowWriteStreamBuffering = false;
                if (req.ContentLength == -1L)
                    req.SendChunked = true;
                
                using (Stream stream = req.GetRequestStream())
                {
                    var buffer = new byte[chunkSize];
                    int count;
                    while ((count = contents.Read(buffer, 0, chunkSize)) > 0)
                    {
                        stream.Write(buffer, 0, count);
                        if (progressUpdated != null)
                        {
                            bytesWritten += count;
                            progressUpdated(bytesWritten);
                        }
                    }
                }

                return "[STREAM CONTENT]";
            });
        }

        private Response ExecuteRequest(Uri url, HttpMethod method, Func<HttpWebResponse, bool, Response> responseBuilderCallback, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings, Func<HttpWebRequest, string> executeCallback)
        {
            url = _urlBuilder.Build(url, queryStringParameters);

            if (settings == null)
                settings = new JsonRequestSettings();
            else
            {
                if (!(settings is JsonRequestSettings))
                    throw new Exception("Unsupported settings type: JsonRestService only support the JsonRequestSettings");
            }

            return _retryLogic.Execute(() =>
            {
                Response response;

                var startTime = DateTime.UtcNow;

                string requestBodyText = null;
                try
                {
                    var req = WebRequest.Create(url) as HttpWebRequest;
                    req.Method = method.ToString();
                    req.ContentType = settings.ContentType;
                    req.Accept = settings.Accept;

                    if (settings.Timeout > default(int))
                        req.Timeout = settings.Timeout;

                    if (!string.IsNullOrWhiteSpace(settings.UserAgent))
                        req.UserAgent = settings.UserAgent;

                    if (settings.Credecials != null)
                        req.Credentials = settings.Credecials;

                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            req.Headers.Add(header.Key, header.Value);
                        }
                    }

                    requestBodyText = executeCallback(req);

                    using (var resp = req.GetResponse() as HttpWebResponse)
                    {
                        if (responseBuilderCallback != null)
                            response = responseBuilderCallback(resp, false);
                        else
                            response = BuildWebResponse(resp);
                    }
                }
                catch (WebException ex)
                {
                    try
                    {
                        using (var resp = ex.Response as HttpWebResponse)
                        {
                            if (responseBuilderCallback != null)
                                response = responseBuilderCallback(resp, true);
                            else
                                response = BuildWebResponse(resp);
                        }
                    }
                    catch (Exception)
                    {
                        response = null;
                    }
                }
                var endTime = DateTime.UtcNow;

                // Log the request
                if (_logger != null)
                    _logger.Log(method, url.OriginalString, headers, requestBodyText, response, startTime, endTime);

                if (response != null && settings.ResponseActions != null && settings.ResponseActions.ContainsKey(response.StatusCode))
                {
                    var action = settings.ResponseActions[response.StatusCode];
                    if (action != null)
                        action(response);
                }

                return response;
            }, settings.Non200SuccessCodes, settings.RetryCount, settings.RetryDelayInMS);
        }

        private Response BuildWebResponse(HttpWebResponse resp)
        {
            if (resp == null)
                return new Response(0, null, null);

            try
            {
                string respBody;
                using (var reader = new StreamReader(resp.GetResponseStream()))
                {
                    respBody = reader.ReadToEnd();
                }

                var respHeaders =
                    resp.Headers.AllKeys.Select(key => new HttpHeader() { Key = key, Value = resp.GetResponseHeader(key) })
                        .ToList();
                return new Response(resp.StatusCode, respHeaders, respBody);
            }
            catch (Exception)
            {
                return new Response(0, null, null);
            }
        }

        private Response<T> BuildWebResponse<T>(HttpWebResponse resp)
        {
            var baseReponse = BuildWebResponse(resp);
            T data = default(T);
            try
            {
                if (baseReponse != null && !string.IsNullOrWhiteSpace(baseReponse.RawBody))
                    data = JsonConvert.DeserializeObject<T>(baseReponse.RawBody);
            }
            catch (JsonReaderException) { }
            catch (JsonSerializationException) { }
            return new Response<T>(baseReponse, data);
        }
    }
}
