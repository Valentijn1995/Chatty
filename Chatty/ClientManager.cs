using Chatty.Model;
using System.Collections.Generic;
using System.Linq;

namespace Chatty
{
    public class ClientManager {
        private HashSet<Group> GroupList { get; set; }
        private HashSet<Client> ClientList { get; set; }

        public void AddClient(Client client) {
            if (ClientList == null)
                ClientList = new HashSet<Client>();

            ClientList.Add(client);
        }

        public Client GetClient(string clientId) {
            if (ClientList == null || !ClientList.Any(client => client.PublicKey == clientId))
                return null;

            return ClientList.Single(client => client.PublicKey == clientId);
        }

        public void AddGroup(Group group) {
            if (GroupList == null)
                GroupList = new HashSet<Group>();

            GroupList.Add(group);
        }

        public Group GetGroup(string groupId) {
            if (GroupList == null || !GroupList.Any(group => group.GroupId == groupId))
                return null;

            return GroupList.Single(group => group.GroupId == groupId);
        }
    }
}