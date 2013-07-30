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

		// Constructor
		public GameState() {
			Entities = new SortedDictionary<int, Entity>();
			Players = new SortedDictionary<int, Player>();
			CreatedEntities = new List<int>();
			DestroyedEntities = new List<int>();
			AddedPlayers = new List<int>();
			RemovedPlayers = new List<int>();
		}

		// Public methods
		public int AddEntity(Entity entity) {
			int id = Entities.Count;
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
		public void AddPlayer(int id, Player player) {
			AddedPlayers.Add(id);
			Players.Add(id, player);
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

		// Static methods
		public static GameState GetDelta(GameState state1, GameState state2) {
			// TODO: Delta compression
			return state2;
		}

		// Internal methods
		internal void NetSerialize(ref NetOutgoingMessage msg) {
			msg.Write(Tick);
			msg.Write((byte)Entities.Count);
			foreach (KeyValuePair<int, Entity> kvp in Entities) {
				kvp.Value.NetSerialize(ref msg);
			}
		}
		static internal GameState NetDeserialize(ref NetIncomingMessage msg) {
			GameState gs = new GameState();

			gs.Tick = msg.ReadUInt32();
			int entCount = (int)msg.ReadByte();

			for (int i = 0; i < entCount; i++) {
				gs.AddEntity((Entity)Entity.NetDeserialize(ref msg));
			}

			return gs;
		}

	}
	public enum GameStateObject {
		Entity,
		Player
	}
}