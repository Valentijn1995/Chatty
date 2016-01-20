using System.Collections.Generic;

namespace Chatty.Model
{
    public class Group {
        public string GroupName { get; set; }
        public string GroupHash { get; set; }
        public List<Client> ClientList { get; set; }
    }
}
