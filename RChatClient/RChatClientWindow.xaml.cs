using System.Windows;
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
		}

		private void SendButton_Click(object sender, RoutedEventArgs e)
		{
			Command command = new Command(Constants.MessageTransferCommand, MessageTextBox.Text);
			bool success = command.Send(Constants.ServerAddress, Constants.ServerPort);
			// #### check success
		}

		private bool Connected = false;
		private void ConnectButton_Click(object sender, RoutedEventArgs e)
		{
			if (Connected)
			{
				// отключиться
				Command command = new Command(Constants.EndCommand);
				bool success = command.Send(Constants.ServerAddress, Constants.ServerPort);
				ConnectButton.Content = "Подключиться";
				Connected = false;
			}
			else
			{
				// соединиться
				Command command = new Command(Constants.HelloCommand, "client1");
				bool success = command.Send(Constants.ServerAddress, Constants.ServerPort);
				if (success)
				{
					ConnectButton.Content = "Отключиться";
					Connected = true;
					// #### сделать индикатор зеленым
				}
				else
				{
					// #### сделать индикатор красным 
				}
			}
		}
	}
}
