using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RChatShared
{
	public class NetworkOperator
	{
		// Все операции с сетевым взаимодействием вынесены в NetworkOperator
		// и CommandReceiver, чтобы не загружать представление
		// Тут ссылки на элементы интерфейса для изменения
		public static ListBox _ChatListBox;
		public static TextBlock _ClientCountLabel;
		public static Dispatcher _Dispatcher;
		// SERVER ONLY SECTION
		public static Dictionary<string, IPEndPoint> Clients = new Dictionary<string, IPEndPoint>();
		public static List<Message> Messages = new List<Message>();
		// CLIENT ONLY SECTION
		public static IPEndPoint ClientAddress = null;
		public static string ClientToken = null;
		public static bool IS_SERVER = false;
		//
		//
		private static BinaryFormatter formatter = new BinaryFormatter();
		public static bool SendCommand(string ipAddress, int port, Command command)
		{
			using (var client = new TcpClient())
			{
				// посылаем команду на сервер и ждем ответа
				try
				{
					client.ReceiveTimeout = 10000;
					client.SendTimeout = 10000;
					client.Connect(ipAddress, port);
					NetworkStream networkStream = client.GetStream();
					formatter.Serialize(networkStream, command);
					// ждем ответа и обрабатываем его
					Command response = (Command)formatter.Deserialize(networkStream);
					ProcessCommand(response, client);
					return true;
				}
				catch (Exception exception)
				{
					Console.WriteLine(exception.Message + " " + exception.InnerException);
					return false;
				}
			}
		}
		private static CommandReceiver clientReceiver = null; // для прослушивания клиентом команд сервера
		public static void ProcessCommand(Command command, TcpClient client)
		{
			try
			{
				switch (command.Type)
				{
					case Constants.HelloCommand:
						if (IS_SERVER)
						{
							IPEndPoint clientPoint = (IPEndPoint)client.Client.RemoteEndPoint;
							string clientToken;
							if (command.ClientToken == null) // новый токен
							{
								clientToken = new Random().Next().ToString(); // вероятно, этого достаточно - шанс коллизии крайне низок
								Clients.Add(clientToken, clientPoint);
							}
							else // токен остается за клиентом при смене адреса
							{
								clientToken = command.ClientToken;
								Clients[clientToken] = clientPoint;
							}
							Console.WriteLine(clientToken + " -> " + clientPoint.ToString());
							_Dispatcher.Invoke(()=>_ChatListBox.Items.Add("Клиент "+clientToken + " подключился с адреса " + clientPoint.ToString()));
							_Dispatcher.Invoke(() => _ClientCountLabel.Text = Clients.Count.ToString());
							// ответить клиенту, передав ему созданный токен
							Command helloResponseCommand = new Command(Constants.HelloCommand, clientToken);
							formatter.Serialize(client.GetStream(), helloResponseCommand);
						}
						else // for client
						{
							ClientAddress = (IPEndPoint)client.Client.LocalEndPoint;
							ClientToken = command.ClientToken;
							Console.WriteLine("New client token received: " + ClientToken + " -> " + ClientAddress.ToString());
							clientReceiver = new CommandReceiver(ClientAddress.Port);
							clientReceiver.Start();
							// #### клиент должен начать слушать команды сервера до отключения
						}
						break;
					case Constants.EndCommand:
						if (IS_SERVER)
						{
							bool clientExists = Clients.ContainsKey(command.ClientToken);
							if (clientExists) Clients.Remove(command.ClientToken);
							_Dispatcher.Invoke(() => _ChatListBox.Items.Add("Клиент " + command.ClientToken + " отключился"));
							_Dispatcher.Invoke(() => _ClientCountLabel.Text = Clients.Count.ToString());
							formatter.Serialize(client.GetStream(), new Command(Constants.EndCommand));
						}
						else
						{
							if (clientReceiver != null) clientReceiver.Stop();
							Console.WriteLine("Клиент отключён от сервера");
						}
						break;
					case Constants.MessageTransferCommand: // передача сообщения
						if (IS_SERVER)
						{
							if (Clients.ContainsKey(command.ClientToken))
							{
								Messages.Add(command.Messages[0]);
								_Dispatcher.Invoke(() => _ChatListBox.Items.Add(command.ClientToken + ": "+command.Messages[0].Text));
								Command transferResponseCommand = new Command(Constants.ACK, command.ClientToken);
								formatter.Serialize(client.GetStream(), transferResponseCommand);

								// #### Доработать отправку нескольким клиентам
								// сервер присылает клиенту сообщение (клиентам)
								foreach (IPEndPoint clientAddress in Clients.Values)
								{
									SendCommand(clientAddress.Address.ToString(), clientAddress.Port, command);
								}
							}
							else
							{
								Console.WriteLine("Некорректный токен клиента");
								Command badTransferResponseCommand = new Command(Constants.WrongCommand, command.ClientToken);
								formatter.Serialize(client.GetStream(), badTransferResponseCommand);
							}

						}
						else
						{
							Console.WriteLine("Получено сообщение с сервера: " + command.Messages[0].Text);
							_Dispatcher.Invoke(() => _ChatListBox.Items.Add(command.ClientToken + ": " + command.Messages[0].Text));
							Command transferResponseCommand = new Command(Constants.ACK, command.ClientToken);
							formatter.Serialize(client.GetStream(), transferResponseCommand);
						}
						break;
					case Constants.Get10MessagesCommand:
						// ####
						break;
					case Constants.Send10MessagesCommand:
						// ####
						break;
					case Constants.ACK:// ответ на приём сообщения
						if (IS_SERVER)
						{
							Console.WriteLine("Сообщение доставлено клиенту");
						}
						else
						{
							Console.WriteLine("Сообщение доставлено серверу");
						}
						break;
					default:
						// #### не забыть ответить на все команды
						break;
				}
			}
			catch (Exception exception) { Console.WriteLine(exception.Message); }
		}
	}
}
