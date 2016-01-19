using Newtonsoft.Json;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

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

                    _publicKeyHash = SHA1Hash(PublicKey);
                }
                return _publicKeyHash;
            }
            set { _publicKeyHash = value; }
        }

        static string SHA1Hash(string input) {
            var hash = (new SHA1Managed()).ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Join("", hash.Select(b => b.ToString("x2")).ToArray());
        }
    }
}