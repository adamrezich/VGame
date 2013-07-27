using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using Lidgren.Network;

using VGame.GameStateSystem;

namespace VGame.Multiplayer {
	public abstract class Client {

		// Static
		public static Client Local { get; internal set; }

		// Properties
		public bool IsLocalServer { get; internal set; }
		public GameStateManager GameStateManager {
			get {
				if (IsLocalServer)
					return Server.Local.GameStateManager;
				return gameStateManager;
			}
			internal set {
				gameStateManager = value;
			}
		}
		public Game Game { get; set; }
		public abstract string Identifier { get; }
		public bool IsConnected { get; set; }
		public int UpdateRate {
			get {
				return Game.Cmd.Variables["cl_updaterate"].Value.IntData;
			}
		}
		public int CommandRate {
			get {
				return Game.Cmd.Variables["cl_cmdrate"].Value.IntData;
			}
		}

		// Fields
		protected GameState gameState;
		protected NetClient client;
		private GameStateManager gameStateManager;
		private string identifier;

		// Constructor
		public Client(Game game, bool isLocalServer) {
			Game = game;
			IsLocalServer = isLocalServer;
			IsConnected = false;
			if (!IsLocalServer) {
				gameStateManager = new GameStateManager();
				NetPeerConfiguration config = new NetPeerConfiguration(Identifier);
				client = new NetClient(config);
			}
			Local = this;
		}

		// Public methods
		public void Connect() {
			DebugMessage("Connecting...");
			if (IsLocalServer) {
				IsConnected = true;
				Server.Local.AddPlayer(null, UpdateRate);
			}
			else
				throw new Exception("Can't local-connect to remote server.");
		}
		public void Connect(string address, int port) {
			if (!IsLocalServer) {
				client.Start();
				NetOutgoingMessage msg = client.CreateMessage();
				msg.Write((byte)PacketType.Connect);
				msg.Write((short)UpdateRate);
				WriteConnectMessage(ref msg);
				client.Connect(address, port, msg);
			}
			else
				throw new Exception("Can't remote-connect to local server.");
		}
		public void Disconnect() {
			if (IsLocalServer) {
				Server.Local.RemovePlayer(0);
			}
			else {
				client.Disconnect("peace");
			}
		}
		public void Tick() {
		}

		// Private methods
		private void CheckIncomingMessages() {
			NetIncomingMessage incoming;
			while ((incoming = client.ReadMessage()) != null) {
				NetConnectionStatus status = (NetConnectionStatus)incoming.ReadByte();
				switch (status) {
					case NetConnectionStatus.Disconnected:
						OnRemoteDisconnect();
						break;
				}
			}
		}

		// Virtual methods
		protected virtual void WriteConnectMessage(ref NetOutgoingMessage msg) {
		}
		protected virtual void OnRemoteDisconnect() {
		}
		protected virtual void DebugMessage(string message) {
			Console.WriteLine("[C] " + message);
		}
	}
}

