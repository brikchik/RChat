using RChatShared;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media;

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
			// прокрутка к последнему элементу чата
			((INotifyCollectionChanged)ChatListBox.Items).CollectionChanged += ChatListBoxAutoScroll;
			ServerAddress.Text = NetworkOperator.ServerPort.ToString();
			NetworkOperator.IS_SERVER = true;
			NetworkOperator._ChatListBox = this.ChatListBox;
			NetworkOperator._ClientCountLabel = this.ClientCountLabel;
			NetworkOperator._Dispatcher = this.Dispatcher;

			NetworkOperator.EnableTimeoutChecks(); // запускаем проверку подключения клиентов
        }
		private void ChatListBoxAutoScroll(object sender, NotifyCollectionChangedEventArgs e)
		{
			try
			{
				ChatListBox.ScrollIntoView(ChatListBox.Items[ChatListBox.Items.Count - 1]);
			}
			catch { } // если нет элементов / пользователь действует слишком быстро
		}
		private bool Working = false;
		private CommandReceiver Receiver;
		private void ControlButton_Click(object sender, RoutedEventArgs e)
		{
			if (!Working)
			{
				try
				{
					NetworkOperator.ServerPort = int.Parse(ServerAddress.Text);
				}
				catch
				{
					MessageBox.Show("Некорректный адрес");
				}
				Receiver = new CommandReceiver(NetworkOperator.ServerPort);
				Receiver.Start();
				ServerAddress.IsEnabled = false;
				Working = true;
				PowerIndicator.Fill = Brushes.Green;
				ControlButton.Content = "Остановить";
			}
			else
			{
				Receiver.Stop();
				ServerAddress.IsEnabled = true;
				Working = false;
				PowerIndicator.Fill = Brushes.Red;
				ControlButton.Content = "Запустить";
			}
		}
		private void ServerWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			// отключаем активность при закрытии окна
			Working = false;
			if (Receiver!=null)Receiver.Stop();
		}

	}
}
