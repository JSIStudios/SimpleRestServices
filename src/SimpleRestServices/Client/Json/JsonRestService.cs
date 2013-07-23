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
        public JsonRestServices(IRequestLogger logger, IRetryLogic<Response, HttpStatusCode> retryLogic, IUrlBuilder urlBuilder, IStringSerializer stringSerializer) : base(stringSerializer, logger, retryLogic, urlBuilder) { }

        /// <summary>
        /// Gets the default <see cref="RequestSettings"/> to use for requests sent from this service.
        /// </summary>
        /// <remarks>
        /// This implementation uses a new instance of <see cref="JsonRequestSettings"/> as the
        /// default request settings.
        /// </remarks>
        protected override RequestSettings DefaultRequestSettings
        {
            get
            {
                return new JsonRequestSettings();
            }
        }

        public override Response Execute(Uri url, HttpMethod method, Func<HttpWebResponse, bool, Response> responseBuilderCallback, string body, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings)
        {
            return base.Execute(url, method, responseBuilderCallback, body, headers, queryStringParameters, settings);
        }

        public override Response Stream(Uri url, HttpMethod method, Func<HttpWebResponse, bool, Response> responseBuilderCallback, Stream contents, int bufferSize, long maxReadLength, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings, Action<long> progressUpdated)
        {
            return base.Stream(url, method, responseBuilderCallback, contents, bufferSize, maxReadLength, headers, queryStringParameters, settings, progressUpdated);
        }
    }
}
