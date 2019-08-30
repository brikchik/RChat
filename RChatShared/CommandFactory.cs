using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RChatShared
{
	public abstract class CommandFactory
	{
		public static Command CreateCommand() { return new Command(0); }
	}
}
