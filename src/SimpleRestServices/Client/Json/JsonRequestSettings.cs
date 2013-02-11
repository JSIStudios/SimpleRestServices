namespace JSIStudios.SimpleRESTServices.Client.Json
{
    public class JsonRequestSettings : RequestSettings
    {
        public override string ContentType { get; set; }

        public override string Accept { get; set; }

        public JsonRequestSettings()
        {
            ContentType = "application/json";
            Accept = "application/json";
        }
    }
}
