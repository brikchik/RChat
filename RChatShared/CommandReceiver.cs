using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace RChatShared
{
	public class CommandReceiver
	{

		private readonly TcpListener TcpServer;
		private Thread CommandReceiverThread;
		private bool Working = true;

		public CommandReceiver(int port)
		{
			TcpServer = new TcpListener(IPAddress.Any, port);
		}
		public void Start()
		{
			CommandReceiverThread = new Thread(ListenForClients);
			CommandReceiverThread.IsBackground = true;
			CommandReceiverThread.Start();
		}
		private void ListenForClients()
		{
			TcpServer.Start();
			while (Working)
			{
				TcpClient client = TcpServer.AcceptTcpClient();
				Thread clientThread = new Thread(ProcessClient);
				clientThread.Start(client);
			}
			TcpServer.Stop();
		}
		private void ProcessClient(object clientObject)
		{
			TcpClient client = (TcpClient)clientObject;
			BinaryFormatter formatter = new BinaryFormatter();
			Command command = null;
			try
			{
				command = (Command)formatter.Deserialize(client.GetStream());
			}
			catch { Console.WriteLine("Получена некорректная команда"); }
			NetworkOperator.ProcessCommand(command, client);
			
			client.Close();
		}

		public void Stop()
		{
			Working = false;
			TcpServer.Stop();
			CommandReceiverThread.Abort();
		}
	}
}
