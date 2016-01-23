using Newtonsoft.Json;

namespace Chatty.Model
{
    public class Client {
        private string _publicKeyHash;

        [JsonProperty("userName")]
        public string UserName { get; set; }
        [JsonProperty("publicKey")]
        public string PublicKey { get; set; }
        [JsonProperty("publicKeyHash")]
        public string PublicKeyHash {
            get{
                if(_publicKeyHash == null) {
                    if(PublicKey == null)
                        return null;

                    _publicKeyHash = SecurityManager.SHA1hash(PublicKey);
                }
                return _publicKeyHash;
            }
            set { _publicKeyHash = value; }
        }
    }
}