using System;
using System.Collections.Generic;

namespace JSIStudios.SimpleRESTServices.Core
{
    public interface IUrlBuilder
    {
        Uri Build(Uri baseUrl, Dictionary<string, string> queryStringParameters);
    }
}