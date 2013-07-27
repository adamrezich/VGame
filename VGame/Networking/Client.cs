using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading;

using VGame.GameStateSystem;

namespace VGame.Networking {
	public class Client {

		// Static
		public static Client Local { get; set; }

		// Properties
		public bool IsLocalServer { get; set; }
		public GameState GameState {
			get {
				if (IsLocalServer)
					return Server.Local.GameStateManager.CurrentGameState;
				return gameState;
			}
			set {
				gameState = value;
			}
		}
		public Game Game { get; set; }

		// Fields
		private GameState gameState;
		private bool isExiting = false;

		// Constructor
		public Client(Game game) {
			Game = game;
		}

		// Public methods
		public void Start() {
		}
		public void Stop() {
		}
		public void Tick() {
		}

		// Private methods
		private void CheckIncomingMessages() {
			//if ((CheckIncomingMessages = 
		}
	}
}

