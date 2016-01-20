using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Chatty.Model
{


    class JsonSendMessage
    {
        [JsonProperty("receiver")]
        public string ReceiverIdentifier { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
    }

    class JsonReceivedMessage
    {
        [JsonProperty("sender")]
        public string Sender { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }
    }

    [Serializable]
    class JsonConnectUser
    {
        [JsonProperty("userName")]
        public string UserName { get; set; }
        [JsonProperty("publicKey")]
        public string PublicKey { get; set; }
    }

    class JsonCreateGroup
    {
        [JsonProperty("groupName")]
        public string GroupName { get; set; }
        [JsonProperty("members")]
        public List<string> Members { get; set; }
    }

    class JsonJoinedGroup
    {
        [JsonProperty("groupName")]
        public string GroupName { get; set; }
        [JsonProperty("groupHash")]
        public string GroupHash { get; set; }
        [JsonProperty("members")]
        public List<Model.Client> Members { get; set; }
    }
}
