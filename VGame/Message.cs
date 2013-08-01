using System;

using Lidgren.Network;

namespace VGame {
	public struct Message {
		public string Contents;
		public string Sender;
		public DateTime Timestamp;
		public MessageType Type;
		public byte Flags;
		
		public Message(string sender, string contents, byte flags) {
			Sender = sender;
			Contents = contents;
			Flags = flags;
			Timestamp = DateTime.UtcNow;
			Type = MessageType.Chat;
		}
		public Message(string contents) {
			Sender = null;
			Contents = contents;
			Flags = 0;
			Timestamp = DateTime.UtcNow;
			Type = MessageType.System;
		}
		public Message(string contents, MessageType type) {
			Sender = null;
			Contents = contents;
			Flags = 0;
			Timestamp = DateTime.UtcNow;
			Type = type;
		}
		public void NetSerialize(ref NetOutgoingMessage msg) {
			msg.Write(Sender == null ? "" : Sender);
			msg.Write(Contents);
			msg.Write((byte)Type);
			msg.Write(Flags);
			// TODO: Timestamp
		}

		public static Message NetDeserialize(ref NetIncomingMessage msg) {
			Message message = new Message();
			string sender = msg.ReadString();
			if (sender == "")
				sender = null;
			message.Sender = sender;
			message.Contents = msg.ReadString();
			message.Type = (MessageType)msg.ReadByte();
			message.Flags = msg.ReadByte();
			return message;
		}

		public override string ToString() {
			switch (Type) {
				case MessageType.Chat:
					return string.Format("<{0}> {1}", Sender, Contents);
				case MessageType.System:
					return string.Format("* {0} *", Contents);
			}
			return string.Format("[ChatMessage]");
		}
	}
	public enum MessageType {
		Chat,
		System
	}
}

