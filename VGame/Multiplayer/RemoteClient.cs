using System;
using Lidgren.Network;

using VGame.GameStateSystem;

namespace VGame.Multiplayer {
	public class RemoteClient {

		// Properties
		public int PlayerID { get; internal set; }
		public int UpdateRate { get; internal set; }
		public NetConnection Connection { get; internal set; }

		// Constructor
		public RemoteClient(int playerID, NetConnection connection, int updateRate) {
			PlayerID = playerID;
			Connection = connection;
			UpdateRate = updateRate;
		}

		// Internal methods
		internal void SendGameState(GameState gameState) {
			NetOutgoingMessage msg = Server.Local.server.CreateMessage();
			msg.Write((byte)NetConnectionStatus.Connected);
			Server.Local.server.SendMessage(msg, Connection, NetDeliveryMethod.ReliableOrdered);
		}

	}
}
