# RChat

A simple realtime chat bot. Uses raw TCP connections.

Contains "Server" and "Client" application using shared "RChatShared" library. Different clients can connect to a single server simultaneously.

Entities: Message[Text, SenderName, DateTime], Command[Type, Message, ClientToken]

All entities are serializable which makes it easy to transfer them as one object in binary code.

CommandReceiver class represents a server thread with infinite connection listening loop

NetworkOperator contains other operations including command transferring and UI updates.
