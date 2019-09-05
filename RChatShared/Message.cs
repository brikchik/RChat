using System;

namespace RChatShared
{
	[Serializable]
	public class Message // класс для хранения сообщений на сервере
	{
		public string Text; // текст сообщения
		public string SenderName; // имя для отображения в чате, задаётся сервером
		public DateTime Date; // время отправки/получения сообщения
		public Message(string text)
		{
			Text = text;
			Date = DateTime.Now;
		}
	}
}
