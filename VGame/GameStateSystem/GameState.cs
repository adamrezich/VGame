using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Lidgren.Network;

namespace VGame.GameStateSystem {
	[Serializable]
	public class GameState {

		// Properties
		public uint Tick { get; internal set; }
		public SortedDictionary<int, Entity> Entities { get; internal set; }
		public SortedDictionary<int, Player> Players { get; internal set; }
		public List<int> CreatedEntities { get; internal set; }
		public List<int> DestroyedEntities { get; internal set; }
		public List<int> AddedPlayers { get; internal set; }
		public List<int> RemovedPlayers { get; internal set; }
		public Dictionary<int, Message> Messages { get; internal set; }
		private int lastEntity {
			get {
				return _lastEntity;
			}
			set {
				_lastEntity = value;
				while (_lastEntity >= Int16.MaxValue)
					_lastEntity -= Int16.MaxValue;
			}
		}
		private int lastPlayer {
			get {
				return _lastPlayer;
			}
			set {
				_lastPlayer = value;
				while (_lastPlayer >= Byte.MaxValue)
					_lastPlayer -= Byte.MaxValue;
			}
		}

		// Fields
		private int _lastEntity = 0;
		private int _lastPlayer = 0;


		// Constructor
		public GameState() {
			Entities = new SortedDictionary<int, Entity>();
			Players = new SortedDictionary<int, Player>();
			CreatedEntities = new List<int>();
			DestroyedEntities = new List<int>();
			AddedPlayers = new List<int>();
			RemovedPlayers = new List<int>();
			Messages = new Dictionary<int, Message>();
		}

		// Public methods
		public int AddEntity(Entity entity) {
			int id = lastEntity;
			lastEntity++;
			entity.ID = (ushort)id;
			Entities.Add(id, entity);
			CreatedEntities.Add(id);
			return id;
		}
		public void RemoveEntity(int id) {
			if (Entities.ContainsKey(id)) {
				Entities.Remove(id);
				DestroyedEntities.Add(id);
			}
		}
		public int AddPlayer(Player player) {
			int id = lastPlayer;
			lastPlayer++;
			AddedPlayers.Add(id);
			Players.Add(id, player);
			return id;
		}
		public void RemovePlayer(int id) {
			RemovedPlayers.Add(id);
			RemoveEntity(Players[id].EntityID);
			Players.Remove(id);
		}
		public GameState CreateNext() {
			GameState gs = this.Copy();
			gs.Tick = Tick + 1;
			gs.CreatedEntities.Clear();
			gs.DestroyedEntities.Clear();
			gs.AddedPlayers.Clear();
			gs.RemovedPlayers.Clear();
			/*foreach (KeyValuePair<int, Entity> kvp in gs.Entities) {
			}*/
			return gs;
		}

		// Internal methods
		internal void NetSerialize(ref NetOutgoingMessage msg) {
			msg.Write(Tick);
			msg.Write((ushort)DestroyedEntities.Count);
			foreach (int ent in DestroyedEntities)
				msg.Write((ushort)ent);
			msg.Write((ushort)Entities.Count);
			foreach (KeyValuePair<int, Entity> kvp in Entities)
				kvp.Value.NetSerialize(ref msg);
			/*msg.Write((byte)Players.Count);
			foreach (KeyValuePair<int, Player> kvp in Players)
				kvp.Value.NetSerialize(ref msg);*/
			msg.Write(Messages.Count);
			foreach (KeyValuePair<int, Message> kvp in Messages) {
				//msg.Write(message.ToString());
				msg.Write(kvp.Value.Sender);
				msg.Write(kvp.Value.Contents);
				msg.Write(kvp.Value.Flags);
			}
		}
		static internal GameState NetDeserialize(ref NetIncomingMessage msg) {
			GameState gs = new GameState();

			gs.Tick = msg.ReadUInt32();
			int destroyedEntCount = (int)msg.ReadInt16();
			for (int i = 0; i < destroyedEntCount; i++) {
				gs.DestroyedEntities.Add((int)msg.ReadInt16());
			}
			int entCount = (int)msg.ReadInt16();

			for (int i = 0; i < entCount; i++) {
				gs.AddEntity((Entity)Entity.NetDeserialize(ref msg));
			}

			int msgCount = (int)msg.ReadInt32();
			for (int i = 0; i < msgCount; i++) {
				gs.Messages.Add(gs.Messages.Count, new Message(msg.ReadString(), msg.ReadString(), msg.ReadByte()));
			}

			return gs;
		}

	}
	public enum GameStateObject {
		Entity,
		Player
	}
}