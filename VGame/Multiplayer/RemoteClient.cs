using System;
using System.Diagnostics;
using Lidgren.Network;

using VGame.GameStateSystem;

namespace VGame.Multiplayer {
	public class RemoteClient {

		// Properties
		public int PlayerID { get; internal set; }
		public int UpdateRate { get; internal set; }
		public NetConnection Connection { get; internal set; }
		public Stopwatch Stopwatch { get; internal set; }
		public int LastTickSent { get; internal set; }

		// Constructor
		public RemoteClient(int playerID, NetConnection connection, int updateRate) {
			PlayerID = playerID;
			Connection = connection;
			UpdateRate = updateRate;
			Stopwatch = Stopwatch.StartNew();
			LastTickSent = -1;
		}
		~RemoteClient() {
			Stopwatch.Stop();
		}

		// Internal methods
		internal void SendGameState(GameState gameState) {
			SendGameState(gameState, false);
		}
		internal void SendGameState(GameState gameState, bool full) {
			NetOutgoingMessage msg = Server.Local.CreateMessage();
			msg.Write((byte)(full ? PacketType.FullGameState : PacketType.GameState));
			gameState.NetSerialize(ref msg);
			Server.Local.SendMessage(msg, Connection, NetDeliveryMethod.ReliableOrdered);
			LastTickSent = (int)gameState.Tick;
		}

	}
}

