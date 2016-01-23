using Chatty.Model;
using Chatty.Model.INotify;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace Chatty
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private User _user;
        private ChattyClient _client;
        private UserManager _manager;

        public MainWindow() {
            _client = new ChattyClient();
            _client.OnMessageReceived += OnMessageReceived;
            _client.OnUserConfirm += OnUserConfirm;
            _client.OnGroupJoined += OnGroupJoined;
            _manager = new UserManager();

            InitializeComponent();
            
            UserWindow window = new UserWindow();
            window.ProfileSelected += ProfileSelected;
            window.ShowDialog();
        }

        

        /// <summary>
        /// Initializes the Socket and sets-up all the events.
        /// </summary>
        /// <param name="adress"></param>
        private void Connect() {
            string adress = TxtBox_ServerAdress.Text;
            _client.Initialize(adress, Menu_IgnoreServerCertificateValidation.IsChecked);
            _client.Register(_user.UserName, _user.PublicKey);
        }

        #region Events

        /// <summary>
        /// Called when a profile is selected in the profile select menu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProfileSelected(object sender, ProfileSelectedArgs e) {
            _user = e.Profile;
            this.Title = $"Chatty - {_user.UserName}";
            Connect();
        }

        /// <summary>
        /// Called when user joins a group. Creates the group and chatHistory for the group.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGroupJoined(object sender, GroupJoinedEventArgs e) {
            if (_manager.GetGroup(e.GroupHash) != null)
                return;

            Group group = new Group() { GroupHash = e.GroupHash, GroupName = e.GroupName, ClientList = e.Members };
            group.ClientList = FilterOwnUser(group.ClientList);
            _manager.AddGroup(group);
            _manager.AddClient(e.Members);
            OnDispatcher(new Action(() => {
                listView_Clients.Items.Add(new ChatItem() { Identifier = group.GroupHash, Value = group.GroupName });
            }));
        }

        /// <summary>
        /// Called when a User is confirmed. This gives us the publicKey and username from the server.
        /// Creates a new Client and chatHistory for the client.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUserConfirm(object sender, UserConfirmEventArgs e) {
            if (_manager.GetClient(e.PublicKeyHash) != null)
                return;

            Client client = new Client() { PublicKey = e.PublicKey, UserName = e.UserName };
            OnDispatcher(new Action(() => {
                listView_Clients.Items.Add(new ChatItem() { Identifier = client.PublicKeyHash, Value = client.UserName });
            }));
            _manager.AddClient(client);
            _manager.GetChatHistory(client.PublicKeyHash).PushMessage(_manager.RetrieveMessages(client.PublicKeyHash), client.UserName);
        }

        /// <summary>
        /// Called when a message is received. Checks what kind of message it is and who it belongs to.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMessageReceived(object sender, MessageReceivedEventArgs e) {
            long convertedTimestamp = ConvertTicks(e.TimeStamp);
            string decryptedMessage = SecurityManager.DecryptText(e.Message, _user.PrivateKey);
            if (e.GroupMessage) {
                if(_manager.GetGroup(e.GroupHash) != null) {                       //Known Group
                    Client client = _manager.GetClient(e.Identifier);
                    _manager.GetChatHistory(e.GroupHash).PushMessage(decryptedMessage, client.UserName, convertedTimestamp);
                }
            }
            else {
                if (_manager.GetClient(e.Identifier) != null) {                     //Known Client
                    Client client = _manager.GetClient(e.Identifier);
                    _manager.GetChatHistory(e.Identifier).PushMessage(decryptedMessage, client.UserName, convertedTimestamp);
                    if(!DoesChatItemExist(e.Identifier)) {
                        OnDispatcher(new Action(() => {
                            listView_Clients.Items.Add(new ChatItem() { Identifier = client.PublicKeyHash, Value = client.UserName });
                        }));
                    }
                }
                else {                                                              //Unknown Client
                    _client.ConfirmUser(e.Identifier);
                    _manager.SaveMessage(e.Identifier, decryptedMessage, convertedTimestamp);
                }
            }
        }
        
        private void Button_Send_Click(object sender, RoutedEventArgs e) {
            SendMessage(Textbox_Message.Text);
        }

        private void Menu_Reconnect_Click(object sender, RoutedEventArgs e) {
            _client.Disconnect();
            Connect();
        }

        private void Textbox_Message_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            if(e.Key == System.Windows.Input.Key.Enter) 
                SendMessage(Textbox_Message.Text);
        }

        /// <summary>
        /// Opens the window to search/create a new user or group
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Search_Click(object sender, RoutedEventArgs e) {
            SearchWindow window = new SearchWindow(_client);
            window.GroupCreated += (senderObj, args) => { _client.CreateGroup(args.GroupName, args.Members); };
            window.PrivateChatCreated += (senderObj, args) => { _client.ConfirmUser(args.PublicKeyHash); };
            window.ShowDialog();
        }

        /// <summary>
        /// Opens the corresponding chat.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView_Clients_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            var item = listView_Clients.SelectedItem;
            if(item != null && item.GetType() == typeof(ChatItem)) {
                if(_manager.GetGroup((item as ChatItem).Identifier) != null) {
                    Group group = _manager.GetGroup((item as ChatItem).Identifier);
                    if(group != null) {
                        OnDispatcher(new Action(() => {
                            listView_Chat.ItemsSource = _manager.GetChatHistory(group.GroupHash);
                            Label_Chatname.Text = $"Currently chatting in {group.GroupName}";
                        }));
                    }
                }
                else {
                    Client client = _manager.GetClient((item as ChatItem).Identifier);
                    if(client != null) {                                                
                        OnDispatcher(new Action(() => {
                            listView_Chat.ItemsSource = _manager.GetChatHistory(client.PublicKeyHash);
                            Label_Chatname.Text = $"Currently chatting with {client.UserName}";
                        }));
                    }
                }
            }
        }

        #endregion Events

        #region GUI Helpers

        /// <summary>
        /// Checks wether the current open chat is a group chat.
        /// </summary>
        /// <returns></returns>
        private bool IsCurrentChatGroup() {
            if(listView_Chat.ItemsSource != null && listView_Chat.ItemsSource.GetType() == typeof(ChatHistory)) {
                return (listView_Chat.ItemsSource as ChatHistory).IsGroup;
            }
            return false;
        }
        
        /// <summary>
        /// Removes own client from the clientList. This prevents messages from being send to yourself.
        /// </summary>
        /// <param name="clientList"></param>
        /// <returns></returns>
        private List<Client> FilterOwnUser(List<Client> clientList) {
            Client clientToRemove = clientList.Find(client => client.PublicKey == _user.PublicKey);
            clientList.Remove(clientToRemove);
            return clientList;
        }

        #endregion GUI Helpers

        /// <summary>
        /// Sends the message to the server and pushes it (locally) to the correct chatHistory.
        /// </summary>
        /// <param name="message"></param>
        private void SendMessage(string message) {
            if(listView_Chat.ItemsSource != null && listView_Chat.ItemsSource.GetType() == typeof(ChatHistory)
                    && message != null && message.Length > 0) {
                if(IsCurrentChatGroup()) {
                    Group group = _manager.GetGroup((listView_Chat.ItemsSource as ChatHistory).Identifier);
                    _client.SendGroupMessage(group, message);
                }
                else {
                    Client client = _manager.GetClient((listView_Chat.ItemsSource as ChatHistory).Identifier);
                    _client.SendMessage(client, message);
                }
                (listView_Chat.ItemsSource as ChatHistory).PushMessage(message, _user.UserName, DateTime.Now.Ticks);
            }
        }

        /// <summary>
        /// Allows other threads to use the main GUI thread.
        /// </summary>
        /// <param name="action"></param>
        private void OnDispatcher(Action action) {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
        }

        /// <summary>
        /// Converts timestamp to .NET format.
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        private long ConvertTicks(long timeStamp) => new DateTime(1970, 1, 1, 1, 0, 0, DateTimeKind.Utc).AddMilliseconds(timeStamp).Ticks;

        private List<ChatItem> GetChatItems() => ConvertToTypedList<ChatItem>(listView_Clients.Items);

        /// <summary>
        /// Checks wether the ChatItem already exists.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        private bool DoesChatItemExist(string identifier) {
            List<ChatItem> items = GetChatItems();
            if(items.Any(item => item.Identifier.Equals(identifier)))
                return true;

            return false;
        }

        private List<T> ConvertToTypedList<T>(IList list) {
            List<T> clients = new List<T>();
            foreach(var item in list) {
                clients.Add((T)item);
            }
            return clients;
        }
    }
}