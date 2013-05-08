namespace JSIStudios.SimpleRESTServices.Client.Json
{
    public class JsonRequestSettings : RequestSettings
    {
        public JsonRequestSettings()
        {
            ContentType = "application/json";
            Accept = "application/json";
        }
    }
}
