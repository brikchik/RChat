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
using System.Windows.Navigation;
using System.Windows.Shapes;
using RChatShared;

namespace RChatServer
{
    /// <summary>
    /// Логика взаимодействия для RChatServerWindow.xaml
    /// </summary>
    public partial class RChatServerWindow : Window
    {
        public RChatServerWindow()
        {
            InitializeComponent();
			ServerAddress.Text = Constants.ServerAddress + ":" + Constants.ServerPort;
			ConnectedClients = new List<string>();
        }
		private bool Working = false;
		private CommandReceiver Receiver;
		public static List<string> ConnectedClients;
		public static List<string> ChatMessages;
		private void ControlButton_Click(object sender, RoutedEventArgs e)
		{
			if (!Working)
			{
				Receiver = new CommandReceiver();
				Receiver.Start(Constants.ServerAddress, Constants.ServerPort);
				Working = true;
				ControlButton.Content = "Остановить";
			}
			else
			{
				Receiver.Stop();
				Working = false;
				ControlButton.Content = "Запустить";
			}
		}

		private void ServerWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Receiver.Stop();
		}

		/// Append the provided message to the ChatListBox text box.
		/// </summary>
		/// <param name="message"></param>
		private void AppendLineToChatListBox(string message)
		{
			//To ensure we can successfully append to the text box from any thread
			//we need to wrap the append within an invoke action.
			ChatListBox.Dispatcher.BeginInvoke(new Action<string>((messageToAdd) =>
			{
				ChatListBox.Items.Add(messageToAdd);
				ChatListBox.ScrollIntoView(messageToAdd);
			}), new object[] { message });
		}
		/// <summary>
		/// Refresh the messagesFrom text box using the recent message history.
		/// </summary>
		private void RefreshMessagesFromBox()
		{
			//We will perform a lock here to ensure the text box is only
			//updated one thread at  time
			lock (ChatListBox)
			{
				this.messagesFrom.Dispatcher.BeginInvoke(new Action<string[]>((users) =>
				{
					//First clear the text box
					messagesFrom.Text = "";

					//Now write out each username
					foreach (var username in users)
						messagesFrom.AppendText(username + "\n");
				}), new object[] { currentUsers });
			}
		}
	}
}
