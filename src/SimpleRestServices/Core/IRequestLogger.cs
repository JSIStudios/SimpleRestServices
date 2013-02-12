using System;
using System.Collections.Generic;
using JSIStudios.SimpleRESTServices.Client;

namespace JSIStudios.SimpleRESTServices.Core
{
    public interface IRequestLogger
    {
        void Log(HttpMethod httpMethod, string uri, Dictionary<string, string> requestHeaders, string requestBody, Response response, DateTime requestStartDateUtc, DateTime requestEndDateUtc);
    }
}