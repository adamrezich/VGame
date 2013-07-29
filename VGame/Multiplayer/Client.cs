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
		public string Name {
			get {
				return Game.Cmd.Variables["name"].Value.StringData;
			}
		}
		public Thread Thread { get; internal set; }

		// Fields
		protected GameState GameState;
		protected NetClient NetClient;
		private GameStateManager gameStateManager;
		private Stopwatch updateStopwatch;
		private Stopwatch commandStopwatch;
		private bool isExiting = false;

		// Constructor
		public Client(Game game, bool isLocalServer) {
			Game = game;
			IsLocalServer = isLocalServer;
			IsConnected = false;
			if (!IsLocalServer) {
				gameStateManager = new GameStateManager();
				NetPeerConfiguration config = new NetPeerConfiguration(Identifier);
				NetClient = new NetClient(config);
				Thread = new Thread(Loop);
				Thread.Start();
			}
			Local = this;
		}
		~Client() {
			if (updateStopwatch != null)
				updateStopwatch.Stop();
			if (commandStopwatch != null)
			commandStopwatch.Stop();
		}

		// Public methods
		public void Connect() {
			DebugMessage("Connecting...");
			if (IsLocalServer) {
				IsConnected = true;
				Server.Local.AddPlayer(null, Name, UpdateRate);
			}
			else
				throw new Exception("Can't local-connect to remote server.");
		}
		public void Connect(string address, int port) {
			if (!IsLocalServer) {
				NetClient.Start();
				NetOutgoingMessage msg = NetClient.CreateMessage();
				msg.Write((byte)PacketType.Connect);
				msg.Write(Name);
				msg.Write((short)UpdateRate);
				WriteConnectMessage(ref msg);
				NetClient.Connect(address, port, msg);
			}
			else
				throw new Exception("Can't remote-connect to local server.");
		}
		public void Disconnect(string message) {
			isExiting = true;
			if (IsLocalServer) {
				Server.Local.RemovePlayer(0, message);
			}
			else {
				if (IsConnected)
					NetClient.Disconnect(message);
				DebugMessage("Stopping client...");
				Thread.Join();
			}
			IsConnected = false;
			Client.Local = null;
		}
		public void Step() {
			if (updateStopwatch.ElapsedMilliseconds >= 1000 / UpdateRate) {
				CheckIncomingMessages();
				updateStopwatch.Reset();
				updateStopwatch.Start();
			}
			if (commandStopwatch.ElapsedMilliseconds >= 1000 / CommandRate) {
				// Send commands
				commandStopwatch.Reset();
				commandStopwatch.Start();
			}
		}

		// Private methods
		private void Loop() {
			updateStopwatch = Stopwatch.StartNew();
			commandStopwatch = Stopwatch.StartNew();
			while (!isExiting) {
				Step();
			}
			DebugMessage("Client stopped.");
		}
		private void CheckIncomingMessages() {
			NetIncomingMessage incoming;
			while ((incoming = NetClient.ReadMessage()) != null) {
				NetConnectionStatus status = (NetConnectionStatus)incoming.ReadByte();
				switch (status) {
					case NetConnectionStatus.Disconnected:
						OnRemoteDisconnect(incoming.ReadString());
						IsConnected = false;
						isExiting = true;
						return;
				}
				switch (incoming.MessageType) {
					case NetIncomingMessageType.ConnectionApproval:
						OnConnectionApproval();
						break;
					case NetIncomingMessageType.Data:
						switch ((PacketType)incoming.ReadByte()) {
							case PacketType.GameState:
								GameState gs = GameState.NetDeserialize(ref incoming);
								GameStateManager.Add(gs);
								OnReceiveGameState(gs);
								break;
						}
						break;
				}
			}
		}

		// Virtual methods
		protected virtual void WriteConnectMessage(ref NetOutgoingMessage msg) {
		}
		protected virtual void OnRemoteDisconnect(string message) {
			DebugMessage("Disconnected from remost host: " + message);
		}
		protected virtual void OnConnectionApproval() {
			DebugMessage("Connected to remote host.");
		}
		protected virtual void OnReceiveGameState(GameState gameState) {
			DebugMessage(string.Format("Received game state from server with {0} entities.", gameState.Entities.Count));
		}
		protected virtual void DebugMessage(string message) {
			Console.WriteLine("[C] " + message);
		}
	}
}

