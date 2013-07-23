namespace JSIStudios.SimpleRESTServices.Client.Json
{
    public class JsonRequestSettings : RequestSettings
    {
        public static readonly string JsonContentType = "application/json";

        public JsonRequestSettings()
        {
            ContentType = JsonContentType;
            Accept = JsonContentType;
        }
    }
}
