using System;

namespace RChatShared
{
	[Serializable]
	public class Command
	{
		public int Type; // назначение команды
		public Message[] Messages = null; // для сообщений
		public string ClientToken; // для разделения клиентов.
		public Command(int type, string clientToken = null) {
			Type = type;
			ClientToken = clientToken;
		}
		public Command(int type, Message message, string clientToken)
		{
			Type = type;
			Messages = new Message[] { message };
			ClientToken = clientToken;
		}
		public Command(int type, Message[] messages, string clientToken)
		{
			Type = type;
			Messages = messages;
			ClientToken = clientToken;
		}
	}
}
