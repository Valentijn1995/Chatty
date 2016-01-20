using Chatty.Model;
using System.Collections.Generic;
using System.Linq;

namespace Chatty
{
    public class UserManager {
        private List<Group> _groupList;
        private List<Client> _clientList;
        private Dictionary<string, ChatHistory> _chatHistories;

        public void AddClient(Client client) {
            if (_clientList == null)
                _clientList = new List<Client>();

            _clientList.Add(client);
            AddChatHistory(client.PublicKeyHash, false);
        }

        public Client GetClient(string identifier) {
            if (_clientList == null || !_clientList.Any(client => client.PublicKey == identifier))
                return null;

            return _clientList.Single(client => client.PublicKey == identifier);
        }

        public void AddGroup(Group group) {
            if (_groupList == null)
                _groupList = new List<Group>();

            _groupList.Add(group);
            AddChatHistory(group.GroupHash, true);
        }

        public Group GetGroup(string clientIdentifier) {
            if(_groupList != null) {
                foreach(Group group in _groupList) {
                    if(group.ClientList.Any(client => client.PublicKeyHash.Equals(clientIdentifier)))
                        return group;
                }
            }
            return null;
        }

        public void AddChatHistory(string identifier, bool isGroup) {
            if(_chatHistories == null)
                _chatHistories = new Dictionary<string, ChatHistory>();

            _chatHistories.Add(identifier, new ChatHistory() { IsGroup = isGroup, Identifier = identifier });
        }

        public ChatHistory GetChatHistory(string identifier) {
            if(_chatHistories != null && _chatHistories.ContainsKey(identifier))
                return _chatHistories[identifier];

            return null;
        }
    }
}