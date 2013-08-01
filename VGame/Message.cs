using System;

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

