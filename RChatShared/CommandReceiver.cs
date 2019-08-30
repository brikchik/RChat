using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Collections.ObjectModel;

namespace RChatShared
{
	public class CommandReceiver
	{
		public List<string> Clients = new List<string>();
		public List<string> Messages = new List<string>();
		public CommandReceiver() { } // конструктор для клиентов 

		Thread thread;
		public void Start(string address, int port) {
			IPAddress localAddr = IPAddress.Parse(address);
			if (receiver == null)
				receiver = new TcpListener(localAddr, port);
			receiver.Start();
			if (thread == null) thread = new Thread(Loop);
			thread.Start();
		}
		public void Stop() {

			if (receiver != null)
			{
				receiver.Stop();
			}
			if (thread != null)
			{
				thread.Abort();
				thread = null;
			}
			receiver = null;
		}

		TcpListener receiver = null;
		TcpClient client;
		private string ReadOneString(NetworkStream stream)
		{
			byte[] length = new byte[4];
			stream.Read(length, 0, 4);
			int messageLength = BitConverter.ToInt32(length, 0);
			Console.WriteLine("Message length = " + messageLength);
			string receivedMessage = null;
			if (messageLength > 0)
			{
				byte[] message = new byte[messageLength];
				stream.Read(message, 0, messageLength);
				receivedMessage = Encoding.UTF8.GetString(message);
			}
			return receivedMessage;
		}
		private void Loop()
		{
			while (true)
			{
				/*
				 * Команда может иметь разную длину: 1 байт для простых команд, 
				 * 1 байт + 4 байта на значение размера сообщения + сообщение этого размера
				 */
				client = receiver.AcceptTcpClient();
				Console.WriteLine("Client connection received from " + client.Connected);
				NetworkStream stream = client.GetStream();
				byte[] commandType = new byte[1];
				stream.Read(commandType, 0, 1);
				Console.WriteLine(commandType[0].ToString());
				switch (commandType[0])
				{
					// общие
					case Constants.MessageTransferCommand:
						Console.WriteLine("Received a new message");
						// приём сообщения для добавления в общий чат
						string receivedMessage = ReadOneString(stream);
						Messages.Add(receivedMessage);
						// #### добавление в форму
						break;
					// только для клиента
					case Constants.HelloCommand:
						Console.WriteLine("Received command type: hello command");
						string clientAddress = ReadOneString(stream);
						Clients.Add(clientAddress);
						Console.WriteLine("Client added: "+clientAddress);
						// #### сервер запоминает клиента и начинает отправлять новые сообщения
						// а также инициирует передачу 10 последних сообщений
						break;
					case Constants.EndCommand:
						Console.WriteLine("Received disconnection command");
						// #### отключение клиента от сервера
						break;
					case Constants.Get10MessagesCommand:
						for (int i=0; i<10; i++)
						{

						}
						// #### принимать 10 сообщений
						break;
					default: Console.WriteLine("Incorrect command"); break;
				}

				stream.Close();
				// в этом нет особенной необходимости, но стоит дать потоку время на успешное закрытие
				// иногда сталкивался с ошибкой на перегруженном компьютере
				Thread.Sleep(50);
			}
		}
	}
}
