using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using JSIStudios.SimpleRESTServices.Core;

namespace JSIStudios.SimpleRESTServices.Client
{
    public class RequestRetryLogic : IRetryLogic<Response, HttpStatusCode>
    {
        public Response Execute(Func<Response> callback, int retryCount = 1, TimeSpan? retryDelay = null)
        {
            return Execute(callback, Enumerable.Empty<HttpStatusCode>(), retryCount);
        }

        public Response Execute(Func<Response> callback, IEnumerable<HttpStatusCode> non200SuccessCodes, int retryCount = 1, TimeSpan? retryDelay = null)
        {
            Response response;
            do
            {
                response = callback();
                if (IsRequestSucessful(response, non200SuccessCodes))
                    return response;

                retryCount = retryCount - 1;

                if (retryDelay != null && retryCount > 0)
                    Thread.Sleep(retryDelay ?? TimeSpan.Zero);
            }
            while (retryCount >= 0);

            return response;
        }

        private static bool IsRequestSucessful(Response response, IEnumerable<HttpStatusCode> non200SuccessCodes)
        {
            if (response != null && response.StatusCode < (HttpStatusCode)300)
                return true;

            if (non200SuccessCodes == null)
                return false;

            return non200SuccessCodes.Contains(response.StatusCode);
        }
    }
}
