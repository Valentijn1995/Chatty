using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Chatty {
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window {
        public bool AppClosing { get; set; }

        public SettingsWindow() {
            InitializeComponent();
            AppClosing = true;
        }

        private void btn_Submit_Click(object sender, RoutedEventArgs e) {
            int id;
            string userName = txtBox_UserName.Text;

            if (userName != null && userName.Length >= 3 && int.TryParse(txtBox_Id.Text, out id)) {
                KeyPair pair = SecurityManager.GenerateKeys(512);
                IOManager.WriteUserInfo(new Model.User() { UserName = userName, PrivateKey = pair.PrivateKey, PublicKey = pair.PublicKey }, id);
                AppClosing = false;
                this.Close();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (AppClosing)
                Environment.Exit(1);
        }
    }
}
