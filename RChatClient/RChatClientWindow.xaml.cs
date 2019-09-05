using RChatShared;
using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
			// прокрутка к последнему элементу чата
			((INotifyCollectionChanged)ChatListBox.Items).CollectionChanged += ChatListBoxAutoScroll;
			ServerAddress.Text = NetworkOperator.ServerAddress + ":" + NetworkOperator.ServerPort;
			NetworkOperator._ChatListBox = this.ChatListBox;
			NetworkOperator._Dispatcher = this.Dispatcher;
		}
		private void ChatListBoxAutoScroll(object sender, NotifyCollectionChangedEventArgs e)
		{
			try
			{
				ChatListBox.ScrollIntoView(ChatListBox.Items[ChatListBox.Items.Count - 1]);
			}
			catch { } // если нет элементов / пользователь действует слишком быстро
		}
		private void SendMessage()
		{
			if (MessageTextBox.Text == "") return;
			Command command = new Command(Constants.MessageTransferCommand, new Message(MessageTextBox.Text), NetworkOperator.ClientToken);
			bool success = NetworkOperator.SendCommand(NetworkOperator.ServerAddress, NetworkOperator.ServerPort, command);
			if (!success) ConnectionIndicator.Fill = Brushes.Red; // ошибка отправки
			MessageTextBox.Text = "";
			MessageTextBox.Focus();
		}
		private void SendButton_Click(object sender, RoutedEventArgs e)
		{
			SendMessage();
		}

		private bool Connected = false;
		private void ConnectButton_Click(object sender, RoutedEventArgs e)
		{
			if (Connected)
			{
				// отключиться
				Command command = new Command(Constants.EndCommand, NetworkOperator.ClientToken);
				NetworkOperator.SendCommand(NetworkOperator.ServerAddress, NetworkOperator.ServerPort, command);
				ConnectButton.Content = "Подключиться";
				ServerAddress.IsEnabled = true;
				Connected = false;
				ConnectionIndicator.Fill = Brushes.Red;

				// лучше отключить принимающий поток здесь, в основном потоке
				// предотвращает появление труднообъяснимой ошибки
				// для этого пришлось сделать поток доступным извне NetworkOperator
				NetworkOperator.clientReceiver.Stop(); 
			}
			else
			{
				// соединиться
				Command command = new Command(Constants.HelloCommand, new Message(ClientNameTextBox.Text), null); //передача отображаемого имени клиента
				ClientNameTextBox.IsEnabled = false;
				if (NetworkOperator.ClientToken != null) command.ClientToken = NetworkOperator.ClientToken;
				try
				{
					NetworkOperator.ServerAddress = ServerAddress.Text.Split(':')[0];
					NetworkOperator.ServerPort = int.Parse(ServerAddress.Text.Split(':')[1]);
				}
				catch
				{
					MessageBox.Show("Некорректный порт");
					return;
				}
				bool success = NetworkOperator.SendCommand(NetworkOperator.ServerAddress, NetworkOperator.ServerPort, command);
				if (success)
				{
					ConnectButton.Content = "Отключиться";
					ServerAddress.IsEnabled = false;
					Connected = true;
					ConnectionIndicator.Fill = Brushes.Green;
					// сделать индикатор зеленым
					// клиент запрашивает последние сообщения
					Command requestLastMessagesCommand = new Command(Constants.Get10MessagesCommand, NetworkOperator.ClientToken);
					NetworkOperator.SendCommand(NetworkOperator.ServerAddress, NetworkOperator.ServerPort, requestLastMessagesCommand);
				}
				else
				{
					Connected = false;
					ConnectionIndicator.Fill = Brushes.Red;
					// сделать индикатор красным
				}
			}
		}

		private void RChatClient_Closed(object sender, EventArgs e)
		{
			// при выходе сообщить об этом серверу
			NetworkOperator.SendCommand(NetworkOperator.ServerAddress, NetworkOperator.ServerPort, 
				new Command(Constants.EndCommand, NetworkOperator.ClientToken));
			Environment.Exit(0);
		}

		private void MessageTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.Enter & (sender as TextBox).AcceptsReturn == false) SendMessage();
		}
	}
}
