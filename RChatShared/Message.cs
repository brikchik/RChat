using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RChatShared
{
	[Serializable]
	public class Message // класс для хранения сообщений на сервере
	{
		public string Text; // текст сообщения
		public string SenderName; // имя для отображения в чате
		public DateTime Date; // время отправки/получения сообщения
		public Message(string text, string sendername)
		{
			Text = text;
			Date = DateTime.Now;
			SenderName = sendername;
		}
	}
}
