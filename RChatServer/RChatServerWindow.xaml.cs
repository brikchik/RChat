using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
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
			NetworkOperator.IS_SERVER = true;
			NetworkOperator._ChatListBox = this.ChatListBox;
			NetworkOperator._ClientCountLabel = this.ClientCountLabel;
			NetworkOperator._Dispatcher = this.Dispatcher;
        }
		private bool Working = false;
		private CommandReceiver Receiver;
		private void ControlButton_Click(object sender, RoutedEventArgs e)
		{
			if (!Working)
			{
				Receiver = new CommandReceiver(Constants.ServerPort);
				Receiver.Start();
				Working = true;
				PowerIndicator.Fill = Brushes.Green;
				ControlButton.Content = "Остановить";
			}
			else
			{
				Receiver.Stop();
				Working = false;
				PowerIndicator.Fill = Brushes.Red;
				ControlButton.Content = "Запустить";
			}
		}
		private void ServerWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Working = false;
			Receiver.Stop();
		}

	}
}
