using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
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
		// отличие клиента от сервера
		public static bool IS_SERVER = false;
		// SERVER ONLY SECTION
		public static Dictionary<string, IPEndPoint> Clients = new Dictionary<string, IPEndPoint>();
		public static Dictionary<string, string> ClientNames = new Dictionary<string, string>();
		public static List<Message> Messages = new List<Message>();
		// CLIENT ONLY SECTION
		public static string ServerAddress = "127.0.0.1"; //default
		public static int ServerPort = 5000; //default
		public static IPEndPoint ClientAddress = null;
		public static string ClientToken = null;
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
					client.ReceiveTimeout = 1000;
					client.SendTimeout = 1000;
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
		public static CommandReceiver clientReceiver = null; // для прослушивания клиентом команд сервера
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
								ClientNames.Add(clientToken, command.Messages[0].Text);
							}
							else // токен остается за клиентом при смене адреса
							{
								clientToken = command.ClientToken;
								Clients[clientToken] = clientPoint;
								ClientNames[clientToken] = command.Messages[0].Text;
							}
							_Dispatcher.Invoke(()=>_ChatListBox.Items.Add("Клиент "+clientToken + " подключился с адреса " + clientPoint.ToString()));
							_Dispatcher.Invoke(() => _ClientCountLabel.Text = Clients.Count.ToString());
							// ответить клиенту, передав ему созданный токен
							Command helloResponseCommand = new Command(Constants.HelloCommand, clientToken);
							formatter.Serialize(client.GetStream(), helloResponseCommand);
						}
						else // для клиента
						{
							ClientAddress = (IPEndPoint)client.Client.LocalEndPoint;
							ClientToken = command.ClientToken;
							Console.WriteLine("New client token received: " + ClientToken + " -> " + ClientAddress.ToString());
							clientReceiver = new CommandReceiver(ClientAddress.Port);
							clientReceiver.Start();
							// клиент должен начать слушать команды сервера до отключения
						}
						break;
					case Constants.EndCommand:
						if (IS_SERVER)
						{
							bool clientExists = Clients.ContainsKey(command.ClientToken);
							if (clientExists)
							{
								Clients.Remove(command.ClientToken);
								ClientNames.Remove(command.ClientToken);
							}
							_Dispatcher.Invoke(() => _ChatListBox.Items.Add("Клиент " + command.ClientToken + " отключился"));
							_Dispatcher.Invoke(() => _ClientCountLabel.Text = Clients.Count.ToString());
							formatter.Serialize(client.GetStream(), new Command(Constants.EndCommand));
						}
						else
						{
							if (clientReceiver != null) clientReceiver.Stop();
						}
						break;
					case Constants.MessageTransferCommand: // передача сообщения
						if (IS_SERVER)
						{
							if (Clients.ContainsKey(command.ClientToken))
							{
								command.Messages[0].SenderName = ClientNames[command.ClientToken];
								Messages.Add(command.Messages[0]);
								_Dispatcher.Invoke(() => _ChatListBox.Items.Add(
									"[" + command.Messages[0].Date + "]    " + command.Messages[0].SenderName + ":   "+command.Messages[0].Text));
								// время отправки сообщений показывается на сервере, но обычно это лишняя информация
								Command transferResponseCommand = new Command(Constants.ACK, command.ClientToken);
								formatter.Serialize(client.GetStream(), transferResponseCommand);

								// сервер присылает клиенту сообщение (клиентам)
								foreach (IPEndPoint clientAddress in Clients.Values)
								{
									SendCommand(clientAddress.Address.ToString(), clientAddress.Port, command);
									Thread.Sleep(20); // чтобы потоки действительно успели закрыться
									// позволяет избежать непредсказуемых ошибок времени выполнения
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
							_Dispatcher.Invoke(() => _ChatListBox.Items.Add("[" + command.Messages[0].Date + "]    " + command.Messages[0].SenderName + ":   " + command.Messages[0].Text));
							Command transferResponseCommand = new Command(Constants.ACK, command.ClientToken);
							formatter.Serialize(client.GetStream(), transferResponseCommand);
						}
						break;
					case Constants.Get10MessagesCommand: // приходит на сервер
						Message[] messages = Messages.Skip(Math.Max(0, Messages.Count() - 10)).ToArray(); // берем последние 10 сообщений
						Command oldMessagesSendCommand = new Command(Constants.Send10MessagesCommand, messages, command.ClientToken);
						formatter.Serialize(client.GetStream(), oldMessagesSendCommand);
						break;
					case Constants.Send10MessagesCommand: // приходит на клиент
						// очищаем чат
						_Dispatcher.Invoke(() => _ChatListBox.Items.Clear());
						foreach (Message message in command.Messages)
						{
							// все сообщения уникальны, т.к. содержат время отправки
							_Dispatcher.Invoke(() => _ChatListBox.Items.Add("[" + command.Messages[0].Date +
								"]    " + message.SenderName + ":   " + message.Text));
						}
						break;
					case Constants.ACK:// ответ на приём сообщения - ничего
						break;
					case Constants.CheckConnectionCommand: // клиент должен сообщить о своём присутствии
						if (IS_SERVER)
						{
							// ничего не делать, клиент всё еще подключён
						}
						else
						{
							formatter.Serialize(client.GetStream(), command);
							// клиент просто отправляет эту же команду обратно
						}
						break;
					default:
						Console.WriteLine("Неопознанная команда");
						break;
				}
			}
			catch (Exception exception) { Console.WriteLine(exception.Message); }
		}
		/*
		 * Проверка на присутствие подключённых клиентов осуществляется периодической отправкой запросов
		 * Если запрос остался без ответа, клиент отключён
		 */
		public static void EnableTimeoutChecks()
		{
			TimeoutCheckerThread.IsBackground = true;
			TimeoutCheckerThread.Start();
		}
		private static Thread TimeoutCheckerThread = new Thread(CheckClientTimeouts);
		private static Command TimeoutCheckCommand = new Command(Constants.CheckConnectionCommand);
		private static void CheckClientTimeouts()
		{
			try
			{
				while (true) // в простое будет просто ждать, поэтому остановка не требуется
				{
					for (int i = 0; i < Clients.Count; i++)
					{
						{
							TimeoutCheckCommand.ClientToken = Clients.Keys.ElementAt(i);
							bool success = SendCommand(Clients.Values.ElementAt(i).Address.ToString(),
								Clients.Values.ElementAt(i).Port, TimeoutCheckCommand);
							// если отправка не удалась (нет соединения/не пришёл ответ), клиент уже недоступен
							if (!success)
							{
								ClientNames.Remove(Clients.Keys.ElementAt(i));
								_Dispatcher.Invoke(() => _ChatListBox.Items.Add("Клиент " + Clients.Keys.ElementAt(i) + " отключился"));
								Clients.Remove(Clients.Keys.ElementAt(i));
								_Dispatcher.Invoke(() => _ClientCountLabel.Text = Clients.Count.ToString());
							}
						}
						Thread.Sleep(5000); // интервал проверки
					}
				}
			}
			catch (Exception exception) { Console.WriteLine(exception.Message); }
		} 
	}
}
