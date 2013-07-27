using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using Lidgren.Network;

using VGame.CommandSystem;
using VGame.GameStateSystem;

namespace VGame.Multiplayer {
	public abstract class Server {

		// Static
		public static Server Local { get; internal set; }

		// Properties
		public GameStateManager GameStateManager { get; internal set; }
		public SortedDictionary<int, RemoteClient> RemoteClients { get; internal set; }
		public Game Game { get; set; }
		public virtual bool Joinable {
			get {
				return true;
			}
			set {
			}
		}
		public bool IsLocalServer { get; internal set; }
		protected List<int> ClientsToRemove { get; set; }
		public abstract string Identifier { get; }
		protected int MinRate {
			get {
				return Game.Cmd.Variables["sv_minrate"].Value.IntData;
			}
		}
		protected int MaxRate {
			get {
				return Game.Cmd.Variables["sv_maxrate"].Value.IntData;
			}
		}
		protected int MinUpdateRate {
			get {
				return Game.Cmd.Variables["sv_minupdaterate"].Value.IntData;
			}
		}
		protected int MaxUpdateRate {
			get {
				return Game.Cmd.Variables["sv_maxupdaterate"].Value.IntData;
			}
		}
		protected int TickRate {
			get {
				return 15;
			}
		}
		public Thread Thread { get; internal set; }
		public bool Started { get; internal set; }

		// Fields
		private bool isExiting = false;
		internal NetServer server;
		protected NetPeerConfiguration config;
		protected NetIncomingMessage incoming;

		// Constructor
		public Server(Game game, bool isLocalServer, int port) {
			Game = game;
			GameStateManager = new GameStateManager();
			RemoteClients = new SortedDictionary<int, RemoteClient>();
			ClientsToRemove = new List<int>();
			IsLocalServer = isLocalServer;
			if (!IsLocalServer) {
				config = new NetPeerConfiguration(Identifier);
				config.Port = port;
				config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
				GetConfig(ref config);
			}
			Started = false;
			Server.Local = this;
		}
		~Server() {
			if (Started)
				Stop();
		}

		// Public methods
		public void Start() {
			if (!IsLocalServer) {
				server = new NetServer(config);
				server.Start();
			}
			Started = true;
			Thread = new Thread(MainLoop);
			Thread.IsBackground = true;
			Thread.Start();
		}
		public void Stop() {
			isExiting = true;
			Thread.Join();
			server.Shutdown("Server is shutting down.");
		}
		public void Tick() {
			if (!IsLocalServer) {
				CheckIncomingMessages();
			}
		}
		public int AddPlayer(NetConnection connection, int updateRate) {
			GameState currentGameState = GameStateManager.CurrentGameState;
			int newClientID = 0;
			if (RemoteClients.Count > 0)
				newClientID = RemoteClients.Last().Key + 1;
			RemoteClients.Add(newClientID, new RemoteClient(newClientID, connection, updateRate));
			int entID = currentGameState.AddEntity();
			currentGameState.Players.Add(newClientID, new Player(entID));
			if (IsLocalServer) {
				OnConnect(RemoteClients[newClientID]);
			}
			return newClientID;
		}
		public void RemovePlayer(int id) {
			GameState currentGameState = GameStateManager.CurrentGameState;
			RemoteClient rc = RemoteClients[id];
			if (rc.Connection != null) {
				//rc.Connection.Disconnect("");
			}
			currentGameState.Entities.Remove(currentGameState.Players[id].EntityID);
			currentGameState.Players.Remove(id);
			//RemoteClients.Remove(id);
		}

		// Protected methods
		protected RemoteClient GetRemoteClientByConnection(NetConnection connection) {
			foreach (KeyValuePair<int, RemoteClient> kvp in RemoteClients) {
				if (kvp.Value.Connection == connection)
					return kvp.Value;
			}
			return null;
		}
		
		// Private methods
		private void MainLoop() {
			while (!isExiting) {
				Tick();
			}
		}
		private void CheckIncomingMessages() {
			GameState currentGameState = GameStateManager.CurrentGameState;
			List<RemoteClient> clients = new List<RemoteClient>(RemoteClients.Values.ToList());
			if ((incoming = server.ReadMessage()) != null) {
				switch (incoming.MessageType) {
					case NetIncomingMessageType.ConnectionApproval:
						if (incoming.ReadByte() == (byte)PacketType.Connect) {
							if (server.ConnectionsCount >= server.Configuration.MaximumConnections) {
								incoming.SenderConnection.Deny("Server full.");
							}
							else if (!Joinable) {
								incoming.SenderConnection.Deny("Server is currently unjoinable.");
							}
							else {
								incoming.SenderConnection.Approve();

								int newClientID = AddPlayer(incoming.SenderConnection, incoming.ReadInt16());
								ReadConnectMessage(ref incoming, RemoteClients[newClientID]);
								OnConnect(RemoteClients[newClientID]);
							}
						}
						break;

					case NetIncomingMessageType.StatusChanged:
						if (incoming.SenderConnection.Status == NetConnectionStatus.Disconnected) {
							RemoteClient disconnectedClient = GetRemoteClientByConnection(incoming.SenderConnection);
							ClientsToRemove.Add(disconnectedClient.PlayerID);
							OnDisconnect(disconnectedClient);
						}
						break;

					case NetIncomingMessageType.Data:
						OnData(incoming);
						break;
				}
				server.Recycle(incoming);
			}
			foreach (int i in ClientsToRemove) {
				RemovePlayer(i);
			}
			ClientsToRemove.Clear();
		}

		// Virtual methods
		protected virtual void OnConnect(RemoteClient client) {
			DebugMessage("Player connected!");
		}
		protected virtual void OnDisconnect(RemoteClient client) {
			DebugMessage("Player disconnected!");
		}
		protected virtual void OnData(NetIncomingMessage msg) {
		}
		protected virtual void GetConfig(ref NetPeerConfiguration config) {
			config.ConnectionTimeout = 10;
			config.MaximumConnections = 16;
		}
		protected virtual void ReadConnectMessage(ref NetIncomingMessage msg, RemoteClient remoteClient) {
		}
		protected virtual void DebugMessage(string message) {
			Console.WriteLine("[S] " + message);
		}

	}

	public enum PacketType {
		Connect,
		Disconnect,
		GameState,
		Input
	}
}

