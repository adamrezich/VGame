using System;
using System.Collections.Generic;
using System.Linq;

namespace VGame.GameStateSystem {
	public class GameStateManager {

		// Properties
		public List<GameState> States { get; internal set; }
		public int StatesToKeep { get; internal set; }
		public GameState CurrentGameState {
			get {
				if (States.Count == 0)
					return null;
				return States.Last();
			}
		}

		// Constructor
		public GameStateManager() {
			States = new List<GameState>();
			StatesToKeep = 10;
			Add(new GameState());
		}

		// Static constructor
		static GameStateManager() {
			Entity.Add("ent_player_base", typeof(PlayerEntity));
		}

		// Public methods
		public void Add(GameState state) {
			States.Add(state);
			while (States.Count > StatesToKeep) {
				States.RemoveAt(0);
			}
		}
		public void Add() {
			if (States.Count == 0)
				throw new Exception("Tried to create a new GameState from an existing one, but there was no existing one.");
			Add(CurrentGameState.CreateNext());

		}
		public GameState GetDeltaToCurrent(int tick) {
			if (tick >= CurrentGameState.Tick)
				throw new Exception("Player is updating faster than the server. This probably shouldn't be happening.");
			if (tick > (int)CurrentGameState.Tick - States.Count) {
				int startIndex = StatesToKeep - ((int)CurrentGameState.Tick - tick);
				int endIndex = StatesToKeep;
				System.Diagnostics.Debug.WriteLine(endIndex - startIndex);
				GameState gs = new GameState();
				gs.Tick = CurrentGameState.Tick;

				// Players first

				// Check for players that were added
				/*List<int> addedPlayers = new List<int>();
				for (int i = startIndex; i < endIndex; i++) {
					if (States[i].AddedPlayers.Count > 0) {
						foreach (int pl in States[i].AddedPlayers) {
							addedPlayers.Add(pl);
						}
					}
				}
				List<int> removedPlayers = new List<int>();
				for (int i = startIndex; i < endIndex; i++) {
					if (States[i].RemovedPlayers.Count > 0) {
						foreach (int pl in States[i].RemovedPlayers) {
							removedPlayers.Add(pl);
							if (addedPlayers.Contains(pl))
								addedPlayers.Remove(pl);
						}
					}
				}
				foreach (int i in removedPlayers) {
				}*/

				List<int> createdEntities = new List<int>();
				for (int i = startIndex; i < endIndex; i++) {
					if (States[i].CreatedEntities.Count > 0) {
						foreach (int ent in States[i].CreatedEntities) {
							createdEntities.Add(ent);
						}
					}
				}
				List<int> destroyedEntities = new List<int>();
				for (int i = startIndex; i < endIndex; i++) {
					if (States[i].DestroyedEntities.Count > 0) {
						foreach (int ent in States[i].DestroyedEntities) {
							destroyedEntities.Add(ent);
							if (createdEntities.Contains(ent))
								createdEntities.Remove(ent);
						}
					}
				}
				// "Remove" entity for all entities that have been destroyed
				foreach (int i in destroyedEntities) {
					gs.DestroyedEntities.Add(i);
				}
				// Add entire entity for all entities that have been created since we last checked
				foreach (int i in createdEntities) {
					gs.Entities.Add(i, CurrentGameState.Entities[i]);
				}
				// Add delta entity for all other entities
				foreach (KeyValuePair<int, Entity> kvp in CurrentGameState.Entities) {
					if (!gs.Entities.ContainsKey(kvp.Key))
						gs.Entities.Add(kvp.Key, kvp.Value);
				}


				/*foreach (KeyValuePair<int, Entity> kvp in state2.Entities) {
					gs.Entities.Add(kvp.Key, kvp.Value);
				}
				foreach (KeyValuePair<int, Player> kvp in state2.Players) {
					gs.Players.Add(kvp.Key, kvp.Value);
				}
				foreach (KeyValuePair<int, Message> kvp in state2.Messages) {
					gs.Messages.Add(kvp.Key, kvp.Value);
				}*/
				// TODO: Delta compression
				return gs;
			}
			return null;
		}

	}
}

