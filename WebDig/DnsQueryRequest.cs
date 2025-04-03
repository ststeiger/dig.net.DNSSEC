
namespace WebDig
{
    using System.Text.Json.Serialization;


    // Request model
    public class DnsQueryRequest
    {
        [JsonPropertyName("dnsServer")]
        public string DnsServer { get; set; } = "8.8.8.8";

        [JsonPropertyName("query")]
        public string Query { get; set; } = "";

        [JsonPropertyName("qtype")]
        public string QType { get; set; } = "A";

        [JsonPropertyName("qclass")]
        public string QClass { get; set; } = "IN";

        [JsonPropertyName("recursion")]
        public bool Recursion { get; set; } = true;

        [JsonPropertyName("useCache")]
        public bool UseCache { get; set; } = true;

        [JsonPropertyName("arpaConvert")]
        public bool ArpaConvert { get; set; } = true;

        [JsonPropertyName("transport")]
        public string Transport { get; set; } = "UDP";

        [JsonPropertyName("attempts")]
        public int Attempts { get; set; } = 3;

        [JsonPropertyName("timeout")]
        public int Timeout { get; set; } = 5;
    }


}
