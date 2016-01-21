using Chatty.Model;
using Chatty.Model.INotify;
using System.Collections.Generic;
using System.Linq;

namespace Chatty
{
    public class UserManager {
        private List<Group> _groupList;
        private List<Client> _clientList;
        private Dictionary<string, ChatHistory> _chatHistories;
        private Dictionary<string, List<Message>> _openMessages;

        public void AddClient(Client client) {
            if (_clientList == null)
                _clientList = new List<Client>();

            if(!_clientList.Any(clientObj => clientObj.PublicKeyHash == client.PublicKeyHash)) {
                _clientList.Add(client);
                AddChatHistory(client.PublicKeyHash, false);
            }
        }

        public Client GetClient(string identifier) {
            if (_clientList == null || !_clientList.Any(client => client.PublicKeyHash == identifier))
                return null;

            return _clientList.Single(client => client.PublicKeyHash == identifier);
        }

        public void AddGroup(Group group) {
            if (_groupList == null)
                _groupList = new List<Group>();

            if(!_groupList.Any(groupObj => groupObj.GroupHash.Equals(group.GroupHash))) {
                _groupList.Add(group);
                AddChatHistory(group.GroupHash, true);
            }
        }

        public Group GetGroup(string identifier) {
            if(_groupList != null) {
                if(_groupList.Any(group => group.GroupHash.Equals(identifier)))
                    return _groupList.Find(group => group.GroupHash.Equals(identifier));
            }
            return null;
        }

        public void AddChatHistory(string identifier, bool isGroup) {
            if(_chatHistories == null)
                _chatHistories = new Dictionary<string, ChatHistory>();

            if(!_chatHistories.ContainsKey(identifier))
                _chatHistories.Add(identifier, new ChatHistory() { IsGroup = isGroup, Identifier = identifier });
        }

        public ChatHistory GetChatHistory(string identifier) {
            if(_chatHistories != null && _chatHistories.ContainsKey(identifier))
                return _chatHistories[identifier];

            return null;
        }



        public void SaveMessage(string identifier, string message, long timeStamp) {
            if(_openMessages == null)
                _openMessages = new Dictionary<string, List<Message>>();

            if(_openMessages.ContainsKey(identifier)) {
                _openMessages[identifier].Add(new Message() { MessageString = message, TimeStamp = timeStamp });
            }
            else {
                _openMessages.Add(identifier, new List<Message>() { new Message() { MessageString = message, TimeStamp = timeStamp } });
            }
        }

        public List<Message> RetrieveMessages(string identifier) {
            if(_openMessages != null && _openMessages.ContainsKey(identifier))
                return _openMessages[identifier];

            return null;
        }

        public bool IsGroup(string identifier) => GetClient(identifier) == null;
    }
}