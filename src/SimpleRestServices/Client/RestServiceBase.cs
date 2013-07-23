﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using JSIStudios.SimpleRESTServices.Client.Json;
using JSIStudios.SimpleRESTServices.Core;

namespace JSIStudios.SimpleRESTServices.Client
{
    public abstract class RestServiceBase
    {
        private readonly IRetryLogic<Response, int> _retryLogic;
        private readonly IRequestLogger _logger;
        private readonly IUrlBuilder _urlBuilder;
        private readonly IStringSerializer _stringSerializer;

        protected RestServiceBase(IStringSerializer stringSerializer) : this(stringSerializer, null) { }

        protected RestServiceBase(IStringSerializer stringSerializer, IRequestLogger requestLogger) : this(stringSerializer, requestLogger, new RequestRetryLogic(), new UrlBuilder()) { }

        protected RestServiceBase(IStringSerializer stringSerializer, IRequestLogger logger, IRetryLogic<Response, int> retryLogic, IUrlBuilder urlBuilder)
        {
            _retryLogic = retryLogic;
            _logger = logger;
            _urlBuilder = urlBuilder;
            _stringSerializer = stringSerializer;
        }

        public virtual Response<T> Execute<T, TBody>(string url, HttpMethod method, TBody body, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings)
        {
            return Execute<T, TBody>(new Uri(url), method, body, headers, queryStringParameters, settings);
        }

        public virtual Response<T> Execute<T, TBody>(Uri url, HttpMethod method, TBody body, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings)
        {
            var rawBody = _stringSerializer.Serialize(body);
            return Execute<T>(url, method, rawBody, headers, queryStringParameters, settings);
        }

        public virtual Response<T> Execute<T>(string url, HttpMethod method, string body, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings)
        {
            return Execute<T>(new Uri(url), method, body, headers, queryStringParameters, settings);
        }

        public virtual Response<T> Execute<T>(Uri url, HttpMethod method, string body, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings)
        {
            return Execute(url, method, (resp, isError) => BuildWebResponse<T>(resp), body, headers, queryStringParameters, settings) as Response<T>;
        }

        public virtual Response Execute<TBody>(string url, HttpMethod method, TBody body, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings)
        {
            return Execute(new Uri(url), method, body, headers, queryStringParameters, settings);
        }

        public virtual Response Execute<TBody>(Uri url, HttpMethod method, TBody body, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings)
        {
            var rawBody = _stringSerializer.Serialize(body);
            return Execute(url, method, rawBody, headers, queryStringParameters, settings);
        }

        public virtual Response Execute(string url, HttpMethod method, string body, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings)
        {
            return Execute(new Uri(url), method, body, headers, queryStringParameters, settings);
        }

        public virtual Response Execute(Uri url, HttpMethod method, string body, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings)
        {
            return Execute(url, method, null, body, headers, queryStringParameters, settings);
        }

        public virtual Response Execute(Uri url, HttpMethod method, Func<HttpWebResponse, bool, Response> responseBuilderCallback, string body, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings)
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

        public virtual Response<T> Stream<T>(string url, HttpMethod method, Stream content, int bufferSize, long maxReadLength, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings, Action<long> progressUpdated)
        {
            return Stream<T>(new Uri(url), method, content, bufferSize, maxReadLength, headers, queryStringParameters, settings, progressUpdated)  as Response<T>;
        }

        public virtual Response Stream(string url, HttpMethod method, Stream content, int bufferSize, long maxReadLength, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings, Action<long> progressUpdated)
        {
            return Stream(new Uri(url), method, content, bufferSize, maxReadLength, headers, queryStringParameters, settings, progressUpdated);
        }

        public virtual Response<T> Stream<T>(Uri url, HttpMethod method, Stream content, int bufferSize, long maxReadLength, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings, Action<long> progressUpdated)
        {
            return Stream(url, method, (resp, isError) => BuildWebResponse<T>(resp), content, bufferSize, maxReadLength, headers, queryStringParameters, settings, progressUpdated) as Response<T>;
        }

        public virtual Response Stream(Uri url, HttpMethod method, Stream content, int bufferSize, long maxReadLength, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings, Action<long> progressUpdated)
        {
            return Stream(url, method, null, content, bufferSize, maxReadLength, headers, queryStringParameters, settings, progressUpdated);
        }

        public virtual Response Stream(Uri url, HttpMethod method, Func<HttpWebResponse, bool, Response> responseBuilderCallback, Stream contents, int bufferSize, long maxReadLength, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings, Action<long> progressUpdated)
        {
            return ExecuteRequest(url, method, responseBuilderCallback, headers, queryStringParameters, settings, (req) =>
            {
                long bytesWritten = 0;

                if (settings.ChunkRequest || maxReadLength > 0 )
                {
                    req.SendChunked = settings.ChunkRequest;
                    req.AllowWriteStreamBuffering = false;

                    req.ContentLength = contents.Length > maxReadLength ? maxReadLength : contents.Length;
                }

                using (Stream stream = req.GetRequestStream())
                {
                    var buffer = new byte[bufferSize];
                    int count;
                    while ((count = contents.Read(buffer, 0, bufferSize)) > 0)
                    {
                        if (maxReadLength > 0 && bytesWritten + count > maxReadLength)
                            count = (int) maxReadLength - (int) bytesWritten;

                        bytesWritten += count;
                        stream.Write(buffer, 0, count);

                        if (progressUpdated != null)
                            progressUpdated(bytesWritten);

                        if (maxReadLength > 0 && bytesWritten >= maxReadLength)
                            break;
                    }
                }

                return "[STREAM CONTENT]";
            });
        }

        public virtual Response ExecuteRequest(Uri url, HttpMethod method, Func<HttpWebResponse, bool, Response> responseBuilderCallback, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings, Func<HttpWebRequest, string> executeCallback)
        {
            url = _urlBuilder.Build(url, queryStringParameters);

            if (settings == null)
                settings = new RequestSettings();

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
                    if(settings.ContentLength > 0 || settings.AllowZeroContentLength)
                        req.ContentLength = settings.ContentLength;

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
                    if (ex.Response == null)
                        throw;

                    using (var resp = ex.Response as HttpWebResponse)
                    {
                        if (responseBuilderCallback != null)
                            response = responseBuilderCallback(resp, true);
                        else
                            response = BuildWebResponse(resp);
                    }
                }
                var endTime = DateTime.UtcNow;

                // Log the request
                if (_logger != null)
                    _logger.Log(method, url.OriginalString, headers, requestBodyText, response, startTime, endTime, settings.ExtendedLogginData);

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
                throw new ArgumentNullException("resp");

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

        private Response<T> BuildWebResponse<T>(HttpWebResponse resp)
        {
            var baseReponse = BuildWebResponse(resp);
            T data = default(T);
            try
            {
                if (baseReponse != null && !string.IsNullOrWhiteSpace(baseReponse.RawBody))
                    data = _stringSerializer.Deserialize<T>(baseReponse.RawBody);
            }
            catch (StringSerializationException) { }
            return new Response<T>(baseReponse, data);
        }
    }
}
