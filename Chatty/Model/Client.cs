using Newtonsoft.Json;

namespace Chatty.Model
{
    public class Client {
        [JsonProperty("userName")]
        public string UserName { get; set; }
        [JsonProperty("publicKey")]
        public string PublicKey { get; set; }
        public string PublicKeyHash { get; set; }
    }
}