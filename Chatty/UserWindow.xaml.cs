﻿using Chatty.Model;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Chatty
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class UserWindow : Window {
        public event EventHandler<ProfileSelectedArgs> ProfileSelected;

        public UserWindow() {
            InitializeComponent();

            List<User> users = ProfileManager.GetUsers();
            if(users != null) 
                listView_Profiles.ItemsSource = users;
        }

        /// <summary>
        /// Called when creating a new profile.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_NewProfile_Click(object sender, RoutedEventArgs e) {
            string username = txtBox_Username.Text;
            if(username != null && username.Length > 0) {
                KeyPair pair = SecurityManager.GenerateKeys();
                User user = new User() { UserName = username, PrivateKey = pair.PrivateKey, PublicKey = pair.PublicKey };
                ProfileManager.WriteUserInfo(user);
                SelectProfile(user);
            }
        }

        /// <summary>
        /// Calls the create user event.
        /// </summary>
        /// <param name="user"></param>
        private void SelectProfile(User user) {
            if(ProfileSelected != null) {
                ProfileSelected(this, new ProfileSelectedArgs() { Profile = user });
                this.Close();
            }
        }

        /// <summary>
        /// Called when double clicked on a profile. This loads the profile and continues to the main window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView_Profiles_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            var item = listView_Profiles.SelectedItem;
            if(item != null && item.GetType() == typeof(User)) {
                SelectProfile(item as User);
            }
        }
    }
}
