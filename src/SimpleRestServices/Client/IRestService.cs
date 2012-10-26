using System;
using System.Collections.Generic;

namespace SimpleRestServices.Client
{
    public interface IRestService
    {
        Response<T> Execute<T, TBody>(String url, HttpMethod method, TBody body, Dictionary<string, string> headers = null, RequestSettings settings = null);
        Response<T> Execute<T, TBody>(Uri url, HttpMethod method, TBody body, Dictionary<string, string> headers = null, RequestSettings settings = null);
        Response<T> Execute<T>(String url, HttpMethod method, string body = null, Dictionary<string, string> headers = null, RequestSettings settings = null);
        Response<T> Execute<T>(Uri url, HttpMethod method, string body = null, Dictionary<string, string> headers = null, RequestSettings settings = null);
        Response Execute<TBody>(String url, HttpMethod method, TBody body, Dictionary<string, string> headers = null, RequestSettings settings = null);
        Response Execute<TBody>(Uri url, HttpMethod method, TBody body, Dictionary<string, string> headers = null, RequestSettings settings = null);
        Response Execute(String url, HttpMethod method, string body = null, Dictionary<string, string> headers = null, RequestSettings settings = null);
        Response Execute(Uri url, HttpMethod method, string body = null, Dictionary<string, string> headers = null, RequestSettings settings = null);
    }
}