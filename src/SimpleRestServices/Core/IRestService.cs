using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace JSIStudios.SimpleRESTServices.Client
{
    public interface IRestService
    {
        Response<T> Execute<T, TBody>(String url, HttpMethod method, TBody body, Dictionary<string, string> headers = null, Dictionary<string, string> queryStringParameters = null, RequestSettings settings = null);
        Response<T> Execute<T, TBody>(Uri url, HttpMethod method, TBody body, Dictionary<string, string> headers = null, Dictionary<string, string> queryStringParameters = null, RequestSettings settings = null);
        Response<T> Execute<T>(String url, HttpMethod method, string body = null, Dictionary<string, string> headers = null, Dictionary<string, string> queryStringParameters = null, RequestSettings settings = null);
        Response<T> Execute<T>(Uri url, HttpMethod method, string body = null, Dictionary<string, string> headers = null, Dictionary<string, string> queryStringParameters = null, RequestSettings settings = null);
        Response Execute<TBody>(String url, HttpMethod method, TBody body, Dictionary<string, string> headers = null, Dictionary<string, string> queryStringParameters = null, RequestSettings settings = null);
        Response Execute<TBody>(Uri url, HttpMethod method, TBody body, Dictionary<string, string> headers = null, Dictionary<string, string> queryStringParameters = null, RequestSettings settings = null);
        Response Execute(String url, HttpMethod method, string body = null, Dictionary<string, string> headers = null, Dictionary<string, string> queryStringParameters = null, RequestSettings settings = null);
        Response Execute(Uri url, HttpMethod method, string body = null, Dictionary<string, string> headers = null, Dictionary<string, string> queryStringParameters = null, RequestSettings settings = null);
        Response<T> Stream<T>(Uri url, HttpMethod method, Stream content, int bufferSize, long maxReadLength = 0, Dictionary<string, string> headers = null, Dictionary<string, string> queryStringParameters = null, RequestSettings settings = null, Action<long> progressUpdated = null);
        Response Stream(Uri url, HttpMethod method, Stream content, int bufferSize, long maxReadLength = 0, Dictionary<string, string> headers = null, Dictionary<string, string> queryStringParameters = null, RequestSettings settings = null, Action<long> progressUpdated = null);
        Response<T> Stream<T>(string url, HttpMethod method, Stream content, int bufferSize, long maxReadLength = 0, Dictionary<string, string> headers = null, Dictionary<string, string> queryStringParameters = null, RequestSettings settings = null, Action<long> progressUpdated = null);
        Response Stream(string url, HttpMethod method, Stream content, int bufferSize, long maxReadLength = 0, Dictionary<string, string> headers = null, Dictionary<string, string> queryStringParameters = null, RequestSettings settings = null, Action<long> progressUpdated = null);
        Response Execute(Uri url, HttpMethod method, Func<HttpWebResponse, bool, Response> responseBuilderCallback, string body, Dictionary<string, string> headers, Dictionary<string, string> queryStringParameters, RequestSettings settings);
    }
}