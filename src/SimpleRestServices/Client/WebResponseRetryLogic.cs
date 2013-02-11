using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace JSIStudios.SimpleRESTServices.Client
{
    public class RequestRetryLogic : IRetryLogic<Response, int>
    {
        public Response Execute(Func<Response> callback, int retryCount = 1, int retryDelayInMs = 0)
        {
            return Execute(callback, new int[0], retryCount);
        }

        public Response Execute(Func<Response> callback, IEnumerable<int> non200SuccessCodes, int retryCount = 1, int retryDelayInMs = 0)
        {
            Response response;
            do
            {
                response = callback();
                if (IsRequestSucessful(response, non200SuccessCodes))
                    return response;

                retryCount = retryCount - 1;

                if (retryDelayInMs > 0 && retryCount > 0)
                    Thread.Sleep(retryDelayInMs);
            }
            while (retryCount >= 0);

            return response;
        }

        private static bool IsRequestSucessful(Response response, IEnumerable<int> non200SuccessCodes)
        {
            if (response != null && response.StatusCode < 300)
                return true;

            if (non200SuccessCodes == null || !non200SuccessCodes.Any())
                return false;

            return non200SuccessCodes.Contains(response.StatusCode);
        }
    }
}
