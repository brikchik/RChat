using System;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using RChatShared;
namespace RChatClient
{
	/// <summary>
	/// Логика взаимодействия для RChatClientWindow.xaml
	/// </summary>
	public partial class RChatClientWindow : Window
    {
        public RChatClientWindow()
        {
            InitializeComponent();
			ServerAddress.Text = Constants.ServerAddress + ":" + Constants.ServerPort;
			NetworkOperator._ChatListBox = this.ChatListBox;
			NetworkOperator._Dispatcher = this.Dispatcher;
			// #### TODO: брать адрес сервера из строки подключения вместо константы
		}

		private void SendButton_Click(object sender, RoutedEventArgs e)
		{
			Command command = new Command(Constants.MessageTransferCommand, new Message(MessageTextBox.Text, null), NetworkOperator.ClientToken);
			NetworkOperator.SendCommand(Constants.ServerAddress, Constants.ServerPort, command);
			// #### check success
		}

		private bool Connected = false;
		private void ConnectButton_Click(object sender, RoutedEventArgs e)
		{
			if (Connected)
			{
				// отключиться
				Command command = new Command(Constants.EndCommand, NetworkOperator.ClientToken);
				NetworkOperator.SendCommand(Constants.ServerAddress, Constants.ServerPort, command);
				ConnectButton.Content = "Подключиться";
				Connected = false;
				ConnectionIndicator.Fill = Brushes.Red;
			}
			else
			{
				// соединиться
				Command command = new Command(Constants.HelloCommand);
				bool success = NetworkOperator.SendCommand(Constants.ServerAddress, Constants.ServerPort, command);
				if (success)
				{
					ConnectButton.Content = "Отключиться";
					Connected = true;
					ConnectionIndicator.Fill = Brushes.Green;
					// сделать индикатор зеленым
				}
				else
				{
					Connected = false;
					ConnectionIndicator.Fill = Brushes.Red;
					// сделать индикатор красным
				}
			}
		}

		private void RChatClient_Closed(object sender, System.EventArgs e)
		{
			Environment.Exit(0);
		}
	}
}
