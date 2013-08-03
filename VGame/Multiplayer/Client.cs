using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using Lidgren.Network;

using VGame.GameStateSystem;
using VGame.CommandSystem;

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
				return Math.Max(Game.Cmd.Variables["cl_updaterate"].Value.IntData, 1);
			}
		}
		public int CommandRate {
			get {
				return Math.Max(Game.Cmd.Variables["cl_cmdrate"].Value.IntData, 1);
			}
		}
		public string Name {
			get {
				return Game.Cmd.Variables["name"].Value.StringData;
			}
		}
		public int Timeout {
			get {
				return Game.Cmd.Variables["cl_timeout"].Value.IntData;
			}
		}
		public Thread Thread { get; internal set; }
		public List<Command> CommandsToSend {
			get {
				if (Game.Cmd != null)
					return Game.Cmd.CommandsToSend;
				else {
					if (commandsToSend == null)
						commandsToSend = new List<Command>();
					return commandsToSend;
				}
			}
		}
		public List<Message> Messages { get; private set; }

		// Fields
		protected GameState GameState;
		protected NetClient NetClient;
		private GameStateManager gameStateManager;
		private Stopwatch commandStopwatch;
		private Stopwatch timeoutStopwatch;
		private bool isExiting = false;
		private List<Command> commandsToSend = null;

		// Constructor
		public Client(Game game, bool isLocalServer) {
			Game = game;
			IsLocalServer = isLocalServer;
			IsConnected = false;
			Messages = new List<Message>();
			if (!IsLocalServer) {
				gameStateManager = new GameStateManager();
				NetPeerConfiguration config = new NetPeerConfiguration(Identifier);
				NetClient = new NetClient(config);
			}
			Local = this;
		}
		~Client() {
			if (commandStopwatch != null)
				commandStopwatch.Stop();
			if (timeoutStopwatch != null)
				timeoutStopwatch.Stop();
		}

		// Public methods
		public void Connect() {
			if (IsLocalServer) {
				DebugMessage("Connecting to local server...");
				IsConnected = true;
				Server.Local.AddPlayer(null, Name, UpdateRate);
			}
			else {
				DebugMessage(string.Format("Connecting to {0}:{1}", Game.Cmd.Variables["ip"].Value.StringData, Game.Cmd.Variables["clientport"].Value.IntData));
				NetClient.Start();
				NetOutgoingMessage msg = NetClient.CreateMessage();
				msg.Write((byte)PacketType.Connect);
				msg.Write(Name);
				msg.Write((short)UpdateRate);
				WriteConnectMessage(ref msg);
				NetClient.Connect(Game.Cmd.Variables["ip"].Value.StringData, Game.Cmd.Variables["clientport"].Value.IntData, msg);
				Thread = new Thread(Loop);
				Thread.Start();
			}
		}
		public void Disconnect(string message) {
			Disconnect(message, true);
		}
		public void Disconnect(string message, bool startAnother) {
			if (IsLocalServer) {
				isExiting = true;
				Server.Local.RemovePlayer(0, message);
			}
			else {
				if (IsConnected) {
					NetClient.Disconnect(message); // Needed?
				}
				NetClient.Shutdown(message);
				isExiting = true;
				if (Thread.CurrentThread != null && Thread != null && Thread.CurrentThread != Thread)
					Thread.Join();
			}
			IsConnected = false;
			DebugMessage("Client disconnected: " + message);
			Client.Local = null;
			if (startAnother)
				Activator.CreateInstance(this.GetType(), Game, IsLocalServer);
		}
		public void Tick() {
			if (!IsLocalServer)
				SendCommands();
		}
		public void Chat(string message, byte flags) {

		}
		public void SendCommand(Command cmd) {
			CommandsToSend.Add(cmd);
		}
		public void ReceiveMessage(Message message) {
			OnReceiveMessage(message);
			Messages.Add(message);
		}

		// Private methods
		private void Loop() {
			commandStopwatch = Stopwatch.StartNew();
			timeoutStopwatch = Stopwatch.StartNew();
			while (!isExiting) {
				Step();
			}
			DebugMessage("Client stopped.");
		}
		private void CheckIncomingMessages() {
			NetIncomingMessage incoming;
			while ((incoming = NetClient.ReadMessage()) != null) {
				timeoutStopwatch.Reset();
				timeoutStopwatch.Start();
				switch (incoming.MessageType) {
					case NetIncomingMessageType.StatusChanged:
						NetConnectionStatus status = (NetConnectionStatus)incoming.SenderConnection.Status;
						switch (status) {
							case NetConnectionStatus.Connected:
								IsConnected = true;
								OnConnect();
								break;
							case NetConnectionStatus.Disconnected:
								OnRemoteDisconnect(incoming.ReadString());
								IsConnected = false;
								isExiting = true;
								return;
						}
						break;
					case NetIncomingMessageType.Data:
						switch ((PacketType)incoming.ReadByte()) {
							case PacketType.Connect:
								OnConnect();
								break;
							case PacketType.GameState:
								GameState gs = GameState.NetDeserialize(ref incoming);
								GameStateManager.Add(gs);
								OnReceiveGameState(gs, false);
								break;
							case PacketType.FullGameState:
								GameState fgs = GameState.NetDeserialize(ref incoming);
								GameStateManager.Add(fgs);
								OnReceiveGameState(fgs, true);
								break;
							case PacketType.Message:
								Message message = Message.NetDeserialize(ref incoming);
								ReceiveMessage(message);
								break;
						}
						break;
				}
			}
			CheckTimeout();
		}
		private void Step() {
			if (isExiting) {
				DebugMessage("EXITING!!");
				return;
			}
			CheckIncomingMessages(); // TODO: Move?
			if (commandStopwatch.ElapsedMilliseconds >= 1000 / CommandRate) {
				// Send commands
				Tick();
				commandStopwatch.Reset();
				commandStopwatch.Start();
			}
		}
		private void SendCommands() {
			if (CommandsToSend.Count == 0 && Game.Cmd.UserInfoToSend.Count == 0)
				return;
			//DebugMessage("Sending commands!");
			NetOutgoingMessage msg = NetClient.CreateMessage();
			msg.Write((byte)PacketType.Input);

			// User info to send
			if (Game.Cmd != null) {
				msg.Write((byte)Game.Cmd.UserInfoToSend.Count);
				foreach (string str in Game.Cmd.UserInfoToSend) {
					Game.Cmd.Variables[str].NetSerialize(ref msg);
				}
				Game.Cmd.UserInfoToSend.Clear();
			}
			else {
				msg.Write((byte)0);
			}
			msg.Write((byte)CommandsToSend.Count);
			foreach (Command cmd in CommandsToSend) {
				//cmd.NetSerialize(ref msg);
				msg.Write(cmd.ToString());
			}
			CommandsToSend.Clear();

			NetClient.SendMessage(msg, NetDeliveryMethod.ReliableOrdered, 0);
		}
		private void CheckTimeout() {
			if (timeoutStopwatch.ElapsedMilliseconds >= 1000 * Timeout) {
				Disconnect("Remote host timed out.");
				OnTimeout();
			}
		}

		// Virtual methods
		protected virtual void WriteConnectMessage(ref NetOutgoingMessage msg) {
		}
		protected virtual void OnRemoteDisconnect(string message) {
			DebugMessage("Disconnected from remote host: " + message);
		}
		protected virtual void OnConnect() {
			DebugMessage("Connected to remote host.");
		}
		protected virtual void OnReceiveGameState(GameState gameState, bool full) {
			//DebugMessage(string.Format("{2}GameState Tick: {0} | Ent: {1} (D: {5}) | Plr: {4} | Msg: {3}", gameState.Tick, gameState.Entities.Count, full ? "** A FULL ** " : "", gameState.Messages.Count, gameState.Players.Count, gameState.DestroyedEntities.Count));
		}
		protected virtual void OnReceiveMessage(Message message) {
			DebugMessage(message.ToString());
		}
		protected virtual void OnTimeout() {

		}
		protected virtual void DebugMessage(string message) {
			Console.WriteLine("[C] " + message);
			//Debug.Write("[C]" + message + "\n");
		}
	}
}

