using System;
using System.Collections.Generic;
using System.Net;

namespace JSIStudios.SimpleRESTServices.Client
{
    [Serializable]
    public class Response<T> : Response
    {
        public T Data { get; private set; }

        public Response(HttpStatusCode responseCode, string status, T data, IList<HttpHeader> headers, string rawBody)
            : base(responseCode, status, headers, rawBody)
        {
            Data = data;
        }

        public Response(HttpStatusCode statusCode, T data, IList<HttpHeader> headers, string rawBody)
            : this(statusCode, statusCode.ToString(), data, headers, rawBody)
        {
        }

        public Response(Response baseResponse, T data)
            : this((baseResponse == null) ? default(int) : baseResponse.StatusCode,
                (baseResponse == null) ? null : baseResponse.Status, data,
                (baseResponse == null) ? null : baseResponse.Headers,
                (baseResponse == null) ? null : baseResponse.RawBody) { }
    }
}
