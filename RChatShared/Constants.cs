namespace RChatShared
{
	public class Constants
	{
		// Различные порты, чтобы клиенты не спамили сообщениями друг другу,
		// если клиент и сервер работают на одной машине
		public const string ServerAddress = "127.0.0.1";
		public const int ServerPort = 5000;
		public const int ClientPort = 5001;
		// Коды команд. Один байт в начале сообщения определяет состав последующей части
		public const byte HelloCommand = 1; // makes server aware of the client
		public const byte EndCommand = 2; // removes client from server
		public const byte MessageTransferCommand = 3;
		public const byte Get10MessagesCommand = 4;
	}
}