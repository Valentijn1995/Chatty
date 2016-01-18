using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChattyClient.Model {
    public class Group {
        public string GroupId { get; set; }
        public HashSet<Client> ClientList { get; set; }
    }
}
