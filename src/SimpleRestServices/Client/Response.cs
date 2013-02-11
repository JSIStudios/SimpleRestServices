using System;
using System.Collections.Generic;
using System.Net;

namespace SimpleRestServices.Client
{
    [Serializable]
    public class Response
    {
        public int StatusCode { get; private set; }

        public string Status { get; private set; }

        public IList<HttpHeader> Headers { get; private set; }

        public string RawBody { get; private set; }

        public Response(int responseCode, string status, IList<HttpHeader> headers, string rawBody)
        {
            StatusCode = responseCode;
            Status = status;
            Headers = headers;
            RawBody = rawBody;
        }

        public Response(HttpStatusCode statusCode, IList<HttpHeader> headers, string rawBody)
            : this((int)statusCode, statusCode.ToString(), headers, rawBody)
        {
        }
    }

    [Serializable]
    public class Response<T> : Response
    {
        public T Data { get; private set; }

        public Response(int responseCode, string status, T data, IList<HttpHeader> headers, string rawBody)
            : base(responseCode, status, headers, rawBody)
        {
            Data = data;
        }

        public Response(HttpStatusCode statusCode, T data, IList<HttpHeader> headers, string rawBody)
            : this((int)statusCode, statusCode.ToString(), data, headers, rawBody)
        {
        }

        public Response(Response baseResponse, T data)
            : this((baseResponse == null) ? default(int) : baseResponse.StatusCode,
                (baseResponse == null) ? null : baseResponse.Status, data,
                (baseResponse == null) ? null : baseResponse.Headers,
                (baseResponse == null) ? null : baseResponse.RawBody) { }
    }

    [Serializable]
    public class HttpHeader
    {
        public string Key { get; set; }

        public string Value { get; set; }
    }  
}
