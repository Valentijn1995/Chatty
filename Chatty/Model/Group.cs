using System.Collections.Generic;

namespace Chatty.Model
{
    public class Group {
        public string GroupId { get; set; }
        public HashSet<Client> ClientList { get; set; }
    }
}
