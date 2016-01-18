using Chatty.Model;
using Chatty.Model.INotify;
using System.Windows;

namespace Chatty
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ChattyClient _client;
        private ClientManager _manager;

        public MainWindow() {
            InitializeComponent();

            _client = new ChattyClient();
            _client.SetMessageListener(new SocketIOListener("....Adress...."));
            _client.Listener.OnMessageReceived += OnMessageReceived;
            _client.Listener.OnGroupMessageReceived += OnGroupMessageReceived;

            _manager = new ClientManager();
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e) {
            if(_manager.GetClient(e.Identifier) == null) {
                _manager.AddClient(new Client() { PublicKey = e.Identifier, UserName = e.UserName });
                listView_Clients.Items.Add(new ChatItem() { Identifier = e.Identifier, Value = e.UserName });
            }
            
            //Push to chat
            Highlight(e.Identifier, true);
        }

        private void OnGroupMessageReceived(object sender, GroupMessageReceivedEventArgs e) {
            if(_manager.GetGroup(e.Identifier) == null) {
                _manager.AddGroup(new Group() { GroupId = e.Identifier, ClientList = e.Clients });
                listView_Clients.Items.Add(new ChatItem() { Identifier = e.Identifier, Value = e.Identifier });
            }

            //Push to chat
            Highlight(e.Identifier, true);
        }

        private void Button_Send_Click(object sender, RoutedEventArgs e) {
            string message = Textbox_Message.Text;
            if(/*isGroupMessage*/ true) {
                _client.SendGroupMessage(null, message);
            }
            else {
                _client.SendMessage(null, message);
            }
            //Push to chat
        }

        
        private void Highlight(string identifier, bool value) {
            foreach(object item in listView_Clients.Items) {
                if((item as ChatItem).Identifier == identifier) {
                    (item as ChatItem).Highlighted = value;
                }
            }
        }
    }
}
