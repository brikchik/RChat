using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RChatShared
{
	public class Command
	{
		private byte[] command; // текст команды, задаваемый в конструкторе
		// Возможно, стоило бы добавить класс для каждой команды, но это избыточно для 5 простых команд
		public Command(int type) {
			switch (type)
			{
				case Constants.EndCommand:
					command = new byte[1];
					command[0] = Constants.EndCommand;
					break;
				default:
					break;
			}
		}
		public Command(int type, string message)
		{
			byte[] messageTextInBytes = Encoding.UTF8.GetBytes(message);
			byte[] messageLengthInBytes = BitConverter.GetBytes(messageTextInBytes.Length);
			command = new byte[1 + 4 + messageTextInBytes.Length];
			command[0] = (byte)type;
			messageLengthInBytes.CopyTo(command, 1);
			messageTextInBytes.CopyTo(command, 5);
		}
		public Command(int type, string[] messages)
		{
			if (type == Constants.Get10MessagesCommand)
			{
				// #### TODO
			}
		}
		public bool Send(string address, int port) {
			try
			{
				TcpClient client = new TcpClient(address, port);
				NetworkStream stream = client.GetStream();
				stream.Write(command, 0, command.Length);
				stream.Close();
				return true;
			}
			catch
			{
				Console.WriteLine("Unable to connect!");
				return false;
			}
		}
		
	}
}
