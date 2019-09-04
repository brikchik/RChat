using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RChatShared
{
	[Serializable]
	public class Command
	{
		public int Type; // назначение команды
		public Message[] Messages; // для сообщений
		public string ClientToken; // для разделения клиентов.
		public Command(int type, string clientToken = null) {
			Type = type;
			ClientToken = clientToken;
		}
		public Command(int type, Message message)
		{
			Type = type;
			Messages = new Message[] { message };
		}
		public Command(int type, Message message, string clientToken)
		{
			Type = type;
			Messages = new Message[] { message };
			ClientToken = clientToken;
		}
		public Command(int type, Message[] messages)
		{
			Type = type;
			Messages = messages;
		}
	}
}
