using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
namespace Shared
{
	public class Constants
	{
		public static int Port = 4000;
		public static byte HelloCommand = 1;
		public static byte EndCommand = 2;
		public static byte MessageTransferCommand = 3;
		public static byte Get10MessagesCommand = 4;
	}
}
