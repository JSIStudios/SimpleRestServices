using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using JSIStudios.SimpleRESTServices.Core;
//using Newtonsoft.Json;

namespace JSIStudios.SimpleRESTServices.Client.Json
{
    public class JsonRestServices : RestServiceBase, IRestService
    {
        public JsonRestServices():this(null) {}
        public JsonRestServices(IRequestLogger requestLogger) : this(requestLogger, new RequestRetryLogic(), new UrlBuilder(), new JsonStringSerializer()) {}
        public JsonRestServices(IRequestLogger logger, IRetryLogic<Response, int> retryLogic, IUrlBuilder urlBuilder, IStringSerializer stringSerializer) : base(stringSerializer, logger, retryLogic, urlBuilder) { }

        public override Response Execute(Uri url, HttpMethod method, Func<HttpWebResponse, bool, Response> responseBuilderCallback, string body, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings)
        {
            if (settings == null)
                settings = new JsonRequestSettings();

            return base.Execute(url, method, responseBuilderCallback, body, headers, queryStringParameters, settings);
        }

        public override Response Stream(Uri url, HttpMethod method, Func<HttpWebResponse, bool, Response> responseBuilderCallback, Stream contents, int bufferSize, long maxReadLength, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings, Action<long> progressUpdated)
        {
            if (settings == null)
                settings = new JsonRequestSettings();

            return base.Stream(url, method, responseBuilderCallback, contents, bufferSize, maxReadLength, headers, queryStringParameters, settings, progressUpdated);
        }
    }
}
