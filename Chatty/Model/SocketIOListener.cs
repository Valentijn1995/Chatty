using SocketIOClient;
using System;

namespace Chatty.Model
{
    public class SocketIOListener : IMessageListener
    {
        public event EventHandler<MessageReceivedEventArgs> OnMessageReceived;
        public event EventHandler<GroupMessageReceivedEventArgs> OnGroupMessageReceived;

        private SocketIOClient.Client _socket;

        public SocketIOListener(string adress) {
            Initialize(adress);
        }

        public void Initialize(string adress) {
            _socket = new SocketIOClient.Client(adress);
            _socket.Error += ThrowSocketError;
            
            _socket.On("connect", (data) => {
                Console.WriteLine("Connected with Server");
            });

            _socket.On("message", (data) => {
                Console.WriteLine("Message received: {0}", data.Json.ToJsonString());

                if(OnMessageReceived != null)
                    OnMessageReceived(this, new MessageReceivedEventArgs() { Identifier = "clientId", Message = "hi" });
            });
            
            _socket.On("groupMessage", (data) => {
                Console.WriteLine("Group Message received: {0}", data.Json.ToJsonString());

                if(OnGroupMessageReceived != null)
                    OnGroupMessageReceived(this, new GroupMessageReceivedEventArgs() { Identifier = "groupId", Message = "hi" });
            });

            _socket.Connect();
        }

        public void Close() {
            if(_socket == null)
                return;
            
            _socket.Error -= ThrowSocketError;
            _socket.Dispose(); 
        }

        private void ThrowSocketError(object sender, ErrorEventArgs e) {
            Console.WriteLine("Socket threw an Exception: {0}", e.Message);
        }

        public void SendMessage(string identifier, string message) {
            if(_socket == null)
                return;

            _socket.Emit("message", new JsonMessage() { Identifier = identifier, Message = message });
        }

        public void SendGroupMessage(string identifier, string message) {
            if(_socket == null)
                return;

            _socket.Emit("groupMessage", new JsonMessage() { Identifier = identifier, Message = message });
        }
    }

    class JsonMessage
    {
        public string Message { get; set; }
        public string Identifier { get; set; }
    }
}
