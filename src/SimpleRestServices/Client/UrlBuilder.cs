using System;
using System.Collections.Generic;
using System.Linq;
using JSIStudios.SimpleRESTServices.Core;

namespace JSIStudios.SimpleRESTServices.Client
{
    public class UrlBuilder : IUrlBuilder
    {
        public Uri Build(Uri baseUrl, Dictionary<string, string> queryStringParameters)
        {
            return new Uri(Build(baseUrl.AbsoluteUri, queryStringParameters));
        }

        public string Build(string baseAbsoluteUrl, Dictionary<string, string> queryStringParameters)
        {
            if (queryStringParameters != null && queryStringParameters.Count > 0)
            {
                var paramsCombinedList =
                    queryStringParameters.Select(
                        param =>
                        string.Format("{0}={1}", System.Web.HttpUtility.HtmlEncode(param.Key),
                                      System.Web.HttpUtility.HtmlEncode(param.Value)));
                var paramsCombined = string.Join("&", paramsCombinedList);

                var separator = baseAbsoluteUrl.Contains("?") || baseAbsoluteUrl.Contains(System.Web.HttpUtility.HtmlEncode("?")) ? "&" : "?";
                return baseAbsoluteUrl + separator + paramsCombined;
            }

            return baseAbsoluteUrl;
        }
    }
}
