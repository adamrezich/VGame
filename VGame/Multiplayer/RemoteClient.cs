using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lidgren.Network;

using VGame.CommandSystem;
using VGame.GameStateSystem;

namespace VGame.Multiplayer {
	public class RemoteClient {

		// Properties
		public int PlayerID { get; internal set; }
		public int UpdateRate {
			get {
				return Math.Max(Variables["cl_updaterate"].Value.IntData, 1);
			}
		}
		public NetConnection Connection { get; internal set; }
		public Stopwatch Stopwatch { get; internal set; }
		public int LastTickSent { get; internal set; }
		public Dictionary<string, Variable> Variables { get; private set; }

		// Constructor
		public RemoteClient(int playerID, NetConnection connection, int updateRate) {
			PlayerID = playerID;
			Variables = new Dictionary<string, Variable>();
			Connection = connection;
			Variables.Add("cl_updaterate", new Variable(VariableDefinition.List["cl_updaterate"], new Parameter(updateRate)));
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
		internal void SendMessage(Message message) {
			NetOutgoingMessage msg = Server.Local.CreateMessage();
			msg.Write((byte)PacketType.Message);
			message.NetSerialize(ref msg);
			Server.Local.SendMessage(msg, Connection, NetDeliveryMethod.ReliableOrdered);
		}

	}
}

