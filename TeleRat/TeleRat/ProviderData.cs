using Newtonsoft.Json;

namespace Botnet
{
    public class ProviderData
    {
        [JsonProperty("ip")]
        public string IP { get; set; }
        [JsonProperty("city")]
        public string City { get; set; }
        [JsonProperty("region")]
        public string Region { get; set; }
        [JsonProperty("country_name")]
        public string Country { get; set; }
        [JsonProperty("org")]
        public string Org { get; set; }
        [JsonProperty("country_calling_code")]
        public string CallingCode { get; set; }
        [JsonProperty("timezone")]
        public string Timezone { get; set; }
    }
}
