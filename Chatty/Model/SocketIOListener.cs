using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Chatty.Model
{
    public class SocketIOListener : IMessageListener
    {
        public event EventHandler<UserSearchEventArgs> OnUserSearch;
        public event EventHandler<UserComfirmEventArgs> OnUserConfirm;
        public event EventHandler<GroupJoinedEventArgs> OnGroupJoined;
        public event EventHandler<MessageReceivedEventArgs> OnMessageReceived;

        private SocketIOClient.Client _socket;

        public SocketIOListener(string adress) {
            //Initialize(adress);
        }

        public void Initialize(string adress) {
            _socket = new SocketIOClient.Client(adress);
            
            _socket.On("register-accepted", (data) => {
                Console.WriteLine("Succesfull connected with Server");
            });
            _socket.On("register-failed", (data) => {
                string errorMessage = data.MessageText;
                Console.WriteLine("Failed to connected with Server: {0}", errorMessage);
            });
            _socket.On("message", (data) => {
                var message = JsonConvert.DeserializeObject<JsonReceivedMessage>(data.Json.ToJsonString());
                OnMessageReceived(null, new MessageReceivedEventArgs() { Identifier = message.Sender, Message = message.Message, TimeStamp = message.Timestamp });
            });
            _socket.On("user-search", (data) => {
                List<Client> clients = JsonConvert.DeserializeObject<List<Client>>(data.Json.ToJsonString());
                OnUserSearch(null, new UserSearchEventArgs() { FoundMembers = clients });
            });
            _socket.On("user-confirm", (data) => {
                string publicKey = data.MessageText;
                OnUserConfirm(null, new UserComfirmEventArgs() { PublicKey = publicKey });
            });
            _socket.On("joined-group", (data) => {
                var message = JsonConvert.DeserializeObject<JsonJoinedGroup>(data.Json.ToJsonString());
                OnGroupJoined(null, new GroupJoinedEventArgs() {  GroupName = message.GroupName, Members = message.Members });
            });

            _socket.Connect();
        }

        public void Close() {
            if(_socket != null)
                _socket.Dispose(); 
        }

        public void SendMessage(string identifier, string message) {
            if(_socket != null)
                _socket.Emit("message", new JsonSendMessage() { ReceiverIdentifier = identifier, Message = message });
        }

        public void SendMessage(List<string> receivers, string message) {
            if (_socket == null)
                receivers.ForEach(receiver => { SendMessage(receiver, message); });
        }

        public void ConfirmUser(string identifier) {
            if(_socket == null)
                _socket.Emit("comfirm-user", identifier);
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

    class JsonCreateGroup {
        [JsonProperty("groupName")]
        public string GroupName { get; set; }
        [JsonProperty("members")]
        public List<string> Members { get; set; }
    }

    class JsonJoinedGroup {
        [JsonProperty("groupName")]
        public string GroupName { get; set; }
        [JsonProperty("members")]
        public List<Model.Client> Members { get; set; }
    }
}