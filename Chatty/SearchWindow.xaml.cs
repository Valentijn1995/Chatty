using Chatty.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace Chatty
{
    /// <summary>
    /// Interaction logic for SearchWindow.xaml
    /// </summary>
    public partial class SearchWindow : Window
    {
        private ChattyClient _client;
        public event EventHandler<GroupJoinedEventArgs> GroupCreated;
        public event EventHandler<UserConfirmEventArgs> PrivateChatCreated;


        public SearchWindow(ChattyClient client) {
            _client = client;
            _client.OnUserSearch += OnUserSearch;

            InitializeComponent();
        }

        /// <summary>
        /// Displays the result of a user search.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUserSearch(object sender, UserSearchEventArgs e) {
            if (e.FoundMembers != null && e.FoundMembers.Count > 0) {
                List<Client> uniqueNewClients = UniqueClients(e.FoundMembers, GetSearchedClients());
                OnDispatcher( new Action(() => {
                    listView_results.Items.Clear();
                    foreach (Client client in e.FoundMembers) {
                        listView_results.Items.Add(client);
                    }
                }));
            }
        }

        /// <summary>
        /// Starts searching for a user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Search_Click(object sender, RoutedEventArgs e) {
            string searchValue = Txtbox_Search.Text;
            if(searchValue != null && searchValue.Length > 0) {
                _client.SearchUser(searchValue);
            }
        }

        /// <summary>
        /// Creates a private conversation with the client selected. Closes the searchWindow.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_PrivateCon_Click(object sender, RoutedEventArgs e) {
            if(listView_results.SelectedItems != null && listView_results.SelectedItems.Count > 0 && PrivateChatCreated != null) {
                foreach(var item in listView_results.SelectedItems) {
                    Client client = item as Client;
                    PrivateChatCreated(this, new UserConfirmEventArgs() { UserName = client.UserName, PublicKeyHash = client.PublicKeyHash });
                }
                this.Close();
            }
        }

        /// <summary>
        /// Adds the selected member to the group creation list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_AddMember_Click(object sender, RoutedEventArgs e) {
            if(listView_results.SelectedItems != null && listView_results.SelectedItems.Count > 0) {
                List<Client> uniqueNewClients = UniqueClients(GetSelectedSearchedClients(), GetGroupClients());
                OnDispatcher(new Action(() => {
                    foreach(Client client in uniqueNewClients) {
                        listView_group.Items.Add(client);
                    }
                }));
            }
        }

        /// <summary>
        /// Removes the selected member from the group creation list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_RemoveMember_Click(object sender, RoutedEventArgs e) {
            if(listView_group.SelectedItems != null && listView_group.SelectedItems.Count > 0) {
                foreach(var item in listView_group.SelectedItems) {
                    OnDispatcher(new Action(() => {
                        listView_group.Items.Remove(item as Client);
                    }));
                }
            }
        }

        /// <summary>
        /// Creates a group conversation with the clients selected in the group list. Closes the searchWindow.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_GroupCon_Click(object sender, RoutedEventArgs e) {
            StartSearch(Txtbox_Search.Text);
        }

        private void StartSearch(string value) {
            string groupName = TxtBox_groupName.Text;
            if(groupName == null || groupName.Length <= 0)
                return;

            if(listView_group.Items.Count >= 2 && GroupCreated != null) {
                List<Client> clients = new List<Client>();
                foreach(var item in listView_group.Items) {
                    clients.Add(item as Client);
                }
                GroupCreated(this, new GroupJoinedEventArgs() { GroupName = groupName, Members = clients });
                this.Close();
            }
        }
        
        private void Txtbox_Search_KeyUp(object sender, System.Windows.Input.KeyEventArgs e) {
            if(e.Key == System.Windows.Input.Key.Enter)
                StartSearch(Txtbox_Search.Text);
        }

        /// <summary>
        /// Enables different threads to perform actions on the main GUI Thread.
        /// </summary>
        /// <param name="action"></param>
        private void OnDispatcher(Action action) {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
        }

        private List<Client> UniqueClients(List<Client> newClients, List<Client> currentClients) {
            List<Client> uniqueNewClients = new List<Client>();
            foreach(Client client in newClients) {
                if(!currentClients.Any(clientObj => clientObj.PublicKeyHash.Equals(client.PublicKeyHash)))
                    uniqueNewClients.Add(client);
            }
            return uniqueNewClients;
        }

        private List<Client> GetSearchedClients() => ConvertToTypedList<Client>(listView_results.Items);

        private List<Client> GetSelectedSearchedClients() => ConvertToTypedList<Client>(listView_results.SelectedItems);

        private List<Client> GetGroupClients() => ConvertToTypedList<Client>(listView_group.Items);

        private List<Client> GetSelectedGroupClients() => ConvertToTypedList<Client>(listView_group.SelectedItems);

        private List<T> ConvertToTypedList<T>(IList list) {
            List<T> clients = new List<T>();
            foreach(var item in list) {
                clients.Add((T)item);
            }
            return clients;
        }
    }
}