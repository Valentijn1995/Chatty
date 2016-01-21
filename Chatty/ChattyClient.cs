using Chatty.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quobject.SocketIoClientDotNet.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Chatty
{
    public class ChattyClient {
        public event EventHandler<UserSearchEventArgs> OnUserSearch;
        public event EventHandler<UserConfirmEventArgs> OnUserConfirm;
        public event EventHandler<GroupJoinedEventArgs> OnGroupJoined;
        public event EventHandler<MessageReceivedEventArgs> OnMessageReceived;

        private Socket _socket;

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
                OnMessageReceived(this, new MessageReceivedEventArgs() { Identifier = message.Sender, Message = message.Message, TimeStamp = message.Timestamp, GroupHash = message.GroupHash });
            });
            _socket.On("user-search", (data) => {
                List<Client> clients = JsonConvert.DeserializeObject<List<Client>>(data.ToString());
                OnUserSearch(this, new UserSearchEventArgs() { FoundMembers = clients });
            });
            _socket.On("user-confirm", (data) => {
                var client = JsonConvert.DeserializeObject<Client>(data.ToString());
                OnUserConfirm(this, new UserConfirmEventArgs() { PublicKey = client.PublicKey, UserName = client.UserName });
            });
            _socket.On("joined-group", (data) => {
                var message = JsonConvert.DeserializeObject<JsonJoinedGroup>(data.ToString());
                OnGroupJoined(this, new GroupJoinedEventArgs() { GroupName = message.GroupName, Members = message.Members, GroupHash = message.GroupHash });
            });

            _socket.Connect();
        }

        public void Disconnect() {
            if(_socket != null)
                _socket.Disconnect();
        }

        public void Register(string username, string publicKey) {
            if(_socket != null)
                _socket.Emit("register", JObject.FromObject(new JsonConnectUser() { UserName = username, PublicKey = publicKey }));
        }

        private void SendMessage(string identifier, string message) {
            if(_socket != null)
                _socket.Emit("message", JObject.FromObject(new JsonSendMessage() { ReceiverIdentifier = identifier, Message = message }));
        }

        public void SendMessage(Client client, string message) {
            SendMessage(client.PublicKeyHash, SecurityManager.EncryptText(message, client.PublicKey));
        }

        public void SendGroupMessage(Group group, string message) {
            group.ClientList.ForEach(client => { SendMessage(client.PublicKeyHash, SecurityManager.EncryptText(message, client.PublicKey)); });
        }

        public void ConfirmUser(string identifier) {
            if(_socket != null)
                _socket.Emit("user-confirm", identifier);
        }

        public void SearchUser(string userName) {
            if(_socket != null)
                _socket.Emit("user-search", userName);
        }

        public void CreateGroup(string groupName, List<Client> members) {
            if(_socket != null)
                _socket.Emit("create-group", JObject.FromObject(new JsonCreateGroup() { GroupName = groupName, Members = members.Select(member => member.PublicKeyHash).ToList() }));
        }
    }
}