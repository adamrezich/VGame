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
		protected List<Tuple<int, string>> ClientsToRemove { get; set; }
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
				return 66;
			}
		}
		protected int Timeout {
			get {
				return Game.Cmd.Variables["sv_timeout"].Value.IntData;
			}
		}
		public Thread Thread { get; internal set; }
		public bool Started { get; internal set; }

		// Fields
		private bool isExiting = false;
		protected NetServer NetServer;
		protected NetPeerConfiguration config;
		protected NetIncomingMessage incoming;
		private Stopwatch tickStopwatch;

		// Constructor
		public Server(Game game, bool isLocalServer) {
			Game = game;
			GameStateManager = new GameStateManager();
			RemoteClients = new SortedDictionary<int, RemoteClient>();
			ClientsToRemove = new List<Tuple<int, string>>();
			IsLocalServer = isLocalServer;
			if (!IsLocalServer) {
				config = new NetPeerConfiguration(Identifier);
				config.Port = Game.Cmd.Variables["hostport"].Value.IntData;
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
			DebugMessage("Starting server...");
			if (!IsLocalServer) {
				NetServer = new NetServer(config);
				NetServer.Start();
			}
			Started = true;
			Thread = new Thread(Loop);
			Thread.IsBackground = true;
			Thread.Start();
		}
		public void Stop() {
			isExiting = true;
			Thread.Join();
			if (IsLocalServer) {
				if (Client.Local != null)
					Client.Local.Disconnect("Local server stopping.");
			}
			else
				NetServer.Shutdown("Server is shutting down.");
			Started = false;
			DebugMessage("Server stopped.");
			Server.Local = null;
		}
		public void Tick() {
			GameStateManager.Add();
			if (!IsLocalServer) {
				CheckIncomingMessages();
			}
			OnTick();
		}
		public int AddPlayer(NetConnection connection, string name, int updateRate) {
			GameState currentGameState = GameStateManager.CurrentGameState;
			/*int newClientID = 0;
			if (RemoteClients.Count > 0)
				newClientID = RemoteClients.Last().Key + 1;
			RemoteClient rc = new RemoteClient(0, connection, updateRate);
			RemoteClients.Add(newClientID, rc);
			int entID = currentGameState.AddEntity(new PlayerEntity());
			Player p = new Player(name, entID);
			rc.PlayerID = currentGameState.AddPlayer(p);
			if (IsLocalServer) {
				OnConnect(RemoteClients[newClientID]);
			}*/
			int entID = currentGameState.AddEntity(CreatePlayerEntity());
			Player p = new Player(name, entID);
			int newClientID = currentGameState.AddPlayer(p);
			RemoteClient rc = new RemoteClient(newClientID, connection, updateRate);
			RemoteClients.Add(newClientID, rc);
			if (IsLocalServer) {
				OnConnect(RemoteClients[newClientID]);
			}
			return newClientID;
		}
		public void RemovePlayer(int id, string message) {
			GameState currentGameState = GameStateManager.CurrentGameState;
			RemoteClient rc = RemoteClients[id];
			OnDisconnect(rc, message);
			if (rc.Connection != null) {
				rc.Connection.Disconnect(message);
			}
			currentGameState.RemovePlayer(id);
			RemoteClients.Remove(id);
		}
		public Player GetPlayer(int id) {
			if (GameStateManager.CurrentGameState.Players.ContainsKey(id))
				return GameStateManager.CurrentGameState.Players[id];
			else
				return null;
		}
		public void BroadcastMessage(Message msg) {
			DebugMessage(msg.ToString());
			GameStateManager.CurrentGameState.Messages.Add(GameStateManager.CurrentGameState.Messages.Count, msg);
			if (IsLocalServer) {
				//Client.Local.ReceiveMessage(msg);
			}
			else {
				foreach (RemoteClient rc in AllClients()) {
					rc.SendMessage(msg);
				}
			}
			OnReceiveMessage(msg);
		}

		// Protected methods
		protected RemoteClient GetRemoteClientByConnection(NetConnection connection) {
			foreach (KeyValuePair<int, RemoteClient> kvp in RemoteClients) {
				if (kvp.Value.Connection == connection)
					return kvp.Value;
			}
			return null;
		}
		protected List<RemoteClient> AllClientsButOne(int one) {
			Dictionary<int, RemoteClient> dict = new Dictionary<int, RemoteClient>(RemoteClients);
			if (dict.ContainsKey(one)) dict.Remove(one);
			return dict.Values.ToList<RemoteClient>();
		}
		protected List<RemoteClient> AllClients() {
			return RemoteClients.Values.ToList<RemoteClient>();
		}
		
		// Private methods
		private void Loop() {
			tickStopwatch = Stopwatch.StartNew();
			while (!isExiting) {
				Step();
			}
		}
		private void Step() {
			if (tickStopwatch.ElapsedMilliseconds >= 1000 / TickRate) {
				Tick();
				tickStopwatch.Reset();
				tickStopwatch.Start();
			}
			if (!IsLocalServer) {
				foreach (RemoteClient rc in AllClients()) {
					if (rc.Stopwatch.ElapsedMilliseconds >= ClampUpdateRate(rc.UpdateRate)) {
						GameState stateToSend = GameStateManager.GetDeltaToCurrent(rc.LastTickSent);
						if (stateToSend == null) // full update
							rc.SendGameState(GameStateManager.CurrentGameState, true);
						else
							rc.SendGameState(stateToSend);
						rc.Stopwatch.Reset();
						rc.Stopwatch.Start();
					}
				}
			}
		}
		private void CheckIncomingMessages() {
			GameState currentGameState = GameStateManager.CurrentGameState;
			List<RemoteClient> clients = new List<RemoteClient>(RemoteClients.Values.ToList());
			if ((incoming = NetServer.ReadMessage()) != null) {
				switch (incoming.MessageType) {
					case NetIncomingMessageType.ConnectionApproval:
						if (incoming.ReadByte() == (byte)PacketType.Connect) {
							if (NetServer.ConnectionsCount >= NetServer.Configuration.MaximumConnections) {
								incoming.SenderConnection.Deny("Server full.");
							}
							else if (!Joinable) {
								incoming.SenderConnection.Deny("Server is currently unjoinable.");
							}
							else {
								NetOutgoingMessage msg = NetServer.CreateMessage();
								msg.Write((byte)PacketType.Connect);
								incoming.SenderConnection.Approve(msg);
								int newClientID = AddPlayer(incoming.SenderConnection, incoming.ReadString(), incoming.ReadInt16());
								ReadConnectMessage(ref incoming, RemoteClients[newClientID]);
								OnConnect(RemoteClients[newClientID]);
							}
						}
						break;

					case NetIncomingMessageType.StatusChanged:
						if (incoming.SenderConnection.Status == NetConnectionStatus.Disconnected) {
							incoming.ReadByte(); // TODO: Figure out why we need this
							RemoteClient disconnectedClient = GetRemoteClientByConnection(incoming.SenderConnection);
							if (disconnectedClient != null) {
								ClientsToRemove.Add(new Tuple<int, string>(disconnectedClient.PlayerID, incoming.ReadString()));
							}
						}
						break;

					case NetIncomingMessageType.Data:
						OnData(incoming);
						break;
				}
				NetServer.Recycle(incoming);
			}
			foreach (Tuple<int, string> t in ClientsToRemove) {
				RemovePlayer(t.Item1, t.Item2);
			}
			ClientsToRemove.Clear();
		}
		private int ClampUpdateRate(int clientUpdateRate) {
			clientUpdateRate = Math.Min(MathHelper.Clamp(clientUpdateRate, MinUpdateRate, MaxUpdateRate), TickRate);
			return 1000 / clientUpdateRate;
		}

		// Virtual methods
		protected virtual void OnConnect(RemoteClient client) {
			string str = string.Format("Player {0} connected.", GameStateManager.CurrentGameState.Players[client.PlayerID].Name);
			BroadcastMessage(new Message(str));
		}
		protected virtual void OnDisconnect(RemoteClient client, string message) {
			string str = string.Format("Player {0} disconnected: {1}", GameStateManager.CurrentGameState.Players[client.PlayerID].Name, message);
			BroadcastMessage(new Message(str));
		}
		protected virtual void OnData(NetIncomingMessage msg) {
			switch ((PacketType)msg.ReadByte()) {
				case PacketType.Input:
					OnUserInput(ref msg);
					break;
			}
		}
		protected virtual void OnUserInput(ref NetIncomingMessage msg) {
			RemoteClient rc = GetRemoteClientByConnection(msg.SenderConnection);
			if (rc == null)
				return;
			int userInfoCount = (int)msg.ReadByte();
			if (userInfoCount > 0) {
				DebugMessage(string.Format("Got {0} UserInfo vars from client.", userInfoCount));
				for (int i = 0; i < userInfoCount; i++) {
					string ui_k = msg.ReadString();
					if (!VariableDefinition.List.ContainsKey(ui_k)) {
						DebugMessage(string.Format("Got invalid UserInfo var {0} from client!", ui_k));
						continue;
					}
					VariableDefinition ui_def = VariableDefinition.List[ui_k];
					Parameter ui_v = new Parameter();
					switch (ui_def.Type) {
						case ParameterType.Bool:
							ui_v = new Parameter(msg.ReadBoolean());
							break;
						case ParameterType.Float:
							ui_v = new Parameter(msg.ReadFloat());
							break;
						case ParameterType.Int:
							ui_v = new Parameter(msg.ReadInt32());
							break;
						case ParameterType.String:
							ui_v = new Parameter(msg.ReadString());
							break;
					}
					if (rc.Variables.ContainsKey(ui_k))
						rc.Variables[ui_k].Value = ui_v;
					else
						rc.Variables.Add(ui_k, new Variable(ui_def, ui_v));
				}
			}
			int commandCount = (int)msg.ReadByte();
			if (commandCount > 0) {
				//DebugMessage(string.Format("Got {0} commands from client.", commandCount));
				for (int i = 0; i < commandCount; i++) {
					Game.Cmd.Run(msg.ReadString(), GetRemoteClientByConnection(msg.SenderConnection));
				}
			}
		}
		protected virtual void OnTick() {
		}
		protected virtual void OnReceiveMessage(Message message) {
		}
		protected virtual PlayerEntity CreatePlayerEntity() {
			return new PlayerEntity();
		}
		protected virtual void GetConfig(ref NetPeerConfiguration config) {
			config.ConnectionTimeout = Timeout;
			config.MaximumConnections = 16;
		}
		protected virtual void ReadConnectMessage(ref NetIncomingMessage msg, RemoteClient remoteClient) {
		}
		protected virtual void DebugMessage(string message) {
			Console.WriteLine("[S] " + message);
			//Debug.Write("[S]" + message + "\n");
		}

		// Internal methods
		internal NetOutgoingMessage CreateMessage() {
			return NetServer.CreateMessage();
		}
		internal void SendMessage(NetOutgoingMessage msg, NetConnection recipient, NetDeliveryMethod method) {
			NetServer.SendMessage(msg, recipient, method);
		}

	}

	public enum PacketType {
		Connect,
		Disconnect,
		GameState,
		FullGameState,
		Input,
		Message
	}
}

