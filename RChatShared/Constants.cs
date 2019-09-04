namespace RChatShared
{
	public class Constants
	{
		// Различные порты, чтобы клиенты не спамили сообщениями друг другу,
		// если клиент и сервер работают на одной машине
		public const string ServerAddress = "127.0.0.1";
		public const int ServerPort = 5000;
		// Коды команд. Один байт в начале сообщения определяет состав последующей части
		public const byte HelloCommand = 1; // makes server aware of the client
		public const byte EndCommand = 2; // removes client from server
		public const byte MessageTransferCommand = 3; // for 1 message
		public const byte Get10MessagesCommand = 4; // for up to 10 messages
		public const byte Send10MessagesCommand = 5; // for up to 10 messages
		public const byte ACK = 6; // command received successfully
		public const byte WrongCommand = 7; // for wrong commands or tokens
	}
}