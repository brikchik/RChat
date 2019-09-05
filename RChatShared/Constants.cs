namespace RChatShared
{
	public class Constants
	{
		// Коды команд.
		public const byte HelloCommand = 1; // makes server aware of the client
		public const byte EndCommand = 2; // removes client from server
		public const byte MessageTransferCommand = 3; // for 1 message
		public const byte Get10MessagesCommand = 4; // for up to 10 messages
		public const byte Send10MessagesCommand = 5; // for up to 10 messages
		public const byte ACK = 6; // command received successfully
		public const byte WrongCommand = 7; // for wrong commands or tokens
		public const byte CheckConnectionCommand = 8; // checks if clients are still connected to the server
	}
}