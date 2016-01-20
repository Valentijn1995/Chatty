using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Quobject.SocketIoClientDotNet.Client;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Chatty.Model
{
    public class SocketIOListener : IMessageListener
    {
        public event EventHandler<UserSearchEventArgs> OnUserSearch;
        public event EventHandler<UserComfirmEventArgs> OnUserConfirm;
        public event EventHandler<GroupJoinedEventArgs> OnGroupJoined;
        public event EventHandler<MessageReceivedEventArgs> OnMessageReceived;

        private Socket _socket;

        public SocketIOListener(string adress) {
            Initialize(adress);
        }

        public void Initialize(string adress) {
            _socket = IO.Socket("http://localhost:3000/");
            
            _socket.On("register-accepted", (data) => {
                Console.WriteLine("Succesfull connected with Server");
            });
            _socket.On("register-failed", (data) => {
                string errorMessage = data.ToString();
                Console.WriteLine("Failed to connected with Server: {0}", errorMessage);
            });
            _socket.On("message", (data) => {
                var message = JsonConvert.DeserializeObject<JsonReceivedMessage>(data.ToString());
                OnMessageReceived(null, new MessageReceivedEventArgs() { Identifier = message.Sender, Message = message.Message, TimeStamp = message.Timestamp });
            });
            _socket.On("user-search", (data) => {
                List<Client> clients = JsonConvert.DeserializeObject<List<Client>>(data.ToString());
                OnUserSearch(null, new UserSearchEventArgs() { FoundMembers = clients });
            });
            _socket.On("user-confirm", (data) => {
                var client = JsonConvert.DeserializeObject<Client>(data.ToString());
                OnUserConfirm(null, new UserComfirmEventArgs() { PublicKey = client.PublicKey, UserName = client.UserName });
            });
            _socket.On("joined-group", (data) => {
                var message = JsonConvert.DeserializeObject<JsonJoinedGroup>(data.ToString());
                OnGroupJoined(null, new GroupJoinedEventArgs() {  GroupName = message.GroupName, Members = message.Members, GroupHash = message.GroupHash });
            });

            _socket.Connect();
        }

        public void Disconnect() {
            if(_socket != null)
                _socket.Disconnect(); 
        }

        public void Register(string username, string publicKey) {
            if (_socket != null)
                _socket.Emit("register", JObject.FromObject(new JsonConnectUser() { UserName = username, PublicKey = publicKey }));
        }
        
        public void SendMessage(string identifier, string message) {
            if(_socket != null)
                _socket.Emit("message", JObject.FromObject(new JsonSendMessage() { ReceiverIdentifier = identifier, Message = message }));
        }

        public void SendMessage(List<string> receivers, string message) {
            if (_socket != null)
                receivers.ForEach(receiver => { SendMessage(receiver, message); });
        }

        public void ConfirmUser(string identifier) {
            if(_socket != null)
                _socket.Emit("comfirm-user", identifier);
        }

        public void SearchUser(string userName) {
            if (_socket != null)
                _socket.Emit("user-search", userName);
        }

        public void CreateGroup(string groupName, List<Client> members) 
        {
            if (_socket != null)
                _socket.Emit("create-group", JObject.FromObject(new JsonCreateGroup() { GroupName = groupName, Members = members.Select(member => member.PublicKeyHash).ToList() }));
        }
    }

    class JsonSendMessage
    {
        [JsonProperty("receiver")]
        public string ReceiverIdentifier { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
    }

    class JsonReceivedMessage {
        [JsonProperty("sender")]
        public string Sender { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }
    }

    [Serializable]
    class JsonConnectUser {
        [JsonProperty("userName")]
        public string UserName { get; set; }
        [JsonProperty("publicKey")]
        public string PublicKey { get; set; }
    }

    class JsonCreateGroup {
        [JsonProperty("groupName")]
        public string GroupName { get; set; }
        [JsonProperty("members")]
        public List<string> Members { get; set; }
    }

    class JsonJoinedGroup {
        [JsonProperty("groupName")]
        public string GroupName { get; set; }
        [JsonProperty("groupHash")]
        public string GroupHash { get; set; }
        [JsonProperty("members")]
        public List<Model.Client> Members { get; set; }
    }
}