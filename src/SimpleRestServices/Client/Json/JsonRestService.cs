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

        public JsonRestServices(IRetryLogic<Response, int> retryLogic = null, IRequestLogger logger = null, IUrlBuilder urlBuilder = null)
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

        private Response Execute(Uri url, HttpMethod method, Func<HttpWebResponse, bool, Response> callback, string body, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings)
        {
            url = _urlBuilder.Build(url, queryStringParameters);

            if (settings == null)
                settings = new JsonRequestSettings();
            else
            {
                if(!(settings is JsonRequestSettings))
                    throw new Exception("Unsupported settings type: JsonRestService only support the JsonRequestSettings");
            }

            return _retryLogic.Execute(() =>
            {
                var req = WebRequest.Create(url) as HttpWebRequest;
                req.Method = method.ToString();
                req.ContentType = settings.ContentType;
                req.Accept = settings.Accept;

                if (!string.IsNullOrWhiteSpace(settings.UserAgent))
                    req.UserAgent = settings.UserAgent;

                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        req.Headers.Add(header.Key, header.Value);
                    }
                }

                // Encode the parameters as form data:
                if ((method == HttpMethod.POST || method == HttpMethod.PUT) && !string.IsNullOrWhiteSpace(body))
                {
                    byte[] formData = UTF8Encoding.UTF8.GetBytes(body);
                    req.ContentLength = formData.Length;

                    // Send the request:
                    using (Stream post = req.GetRequestStream())
                    {
                        post.Write(formData, 0, formData.Length);
                    }
                }

                var startTime = DateTime.UtcNow;
                Response response;

                try
                {
                    using (var resp = req.GetResponse() as HttpWebResponse)
                    {
                        if (callback != null)
                            response = callback(resp, false);
                        else
                            response = BuildWebResponse(resp);
                    }
                }
                catch (WebException ex)
                {
                    using (var resp = ex.Response as HttpWebResponse)
                    {
                        if (callback != null)
                            response = callback(resp, true);
                        else
                            response = BuildWebResponse(resp);
                    }
                }

                var endTime = DateTime.UtcNow;

                // Log the request
                if (_logger != null)
                    _logger.Log(method, url.OriginalString, headers, body, response, startTime, endTime);

                if (settings.ResponseActions != null && settings.ResponseActions.ContainsKey(response.StatusCode))
                {
                    var action = settings.ResponseActions[response.StatusCode];
                    if(action != null)
                        action(response);
                }

                return response;
            }, settings.Non200SuccessCodes, settings.RetryCount, settings.RetryDelayInMS);
        }

        private Response BuildWebResponse(HttpWebResponse resp)
        {
            if (resp == null)
                return new Response(0, null, null);

            string respBody;
            using (var reader = new StreamReader(resp.GetResponseStream()))
            {
                respBody = reader.ReadToEnd();
            }

            var respHeaders = resp.Headers.AllKeys.Select(key => new HttpHeader() { Key = key, Value = resp.GetResponseHeader(key) }).ToList();
            return new Response(resp.StatusCode, respHeaders, respBody);
        }

        private Response<T> BuildWebResponse<T>(HttpWebResponse resp)
        {
            var baseReponse = BuildWebResponse(resp);
            T data = default(T);
            try
            {
                data = JsonConvert.DeserializeObject<T>(baseReponse.RawBody);
            }
            catch (JsonReaderException) { }
            return new Response<T>(baseReponse, data);
        }
    }
}
