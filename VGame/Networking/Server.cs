using System;
using System.Collections.Generic;
using System.Linq;
using Lidgren.Network;

using VGame.CommandSystem;
using VGame.GameStateSystem;

namespace VGame.Networking {
	public abstract class Server {

		// Static
		public static Server Local { get; set; }

		// Properties
		public GameStateManager GameStateManager { get; set; }
		public SortedDictionary<int, RemoteClient> RemoteClients { get; set; }
		public Game Game { get; set; }
		public abstract bool Joinable { get; set; }
		protected List<int> ClientsToRemove { get; set; }

		// Fields
		private bool isExiting = false;
		protected NetServer server;
		protected NetPeerConfiguration config;
		protected NetIncomingMessage incoming;

		// Constructor
		public Server(Game game) {
			Game = game;
			GameStateManager = new GameStateManager();
			RemoteClients = new SortedDictionary<int, RemoteClient>();
			ClientsToRemove = new List<int>();
		}

		// Public methods
		public void Start() {
		}
		public void Stop() {
			isExiting = true;
		}
		public void Tick() {
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

								int newClientID = 0;
								if (RemoteClients.Count > 0)
									newClientID = RemoteClients.Last().Key + 1;
								RemoteClients.Add(newClientID, new RemoteClient(newClientID, incoming.SenderConnection));
								int entID = currentGameState.AddEntity();
								currentGameState.Players.Add(newClientID, new Player(entID));

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
			}
			server.Recycle(incoming);
			foreach (int i in ClientsToRemove) {
				RemoteClients.Remove(i);
				currentGameState.Players.Remove(i);
			}
			ClientsToRemove.Clear();
		}

		// Abstract methods
		protected abstract void OnConnect(RemoteClient client);
		protected abstract void OnDisconnect(RemoteClient client);
		protected abstract void OnData(NetIncomingMessage msg);

	}

	public enum PacketType {
		Connect,
		Disconnect,
		GameState,
		Input
	}
}

