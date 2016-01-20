using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatty.Model {
    public class User {
        public string UserName { get; set; }
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
    }
}
