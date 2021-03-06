using System;
using System.Collections.Generic;
using System.Linq;

using VGame;
using VGame.Multiplayer;
using VGame.StateSystem;
using VGame.GameStateSystem;
using VGame.CommandSystem;

namespace VGameServerTest {
	public class TestServer : Server {
		public override string Identifier {
			get {
				return "TEST";
			}
		}

		public TestServer(Game game, bool isLocalServer) : base(game, isLocalServer) {
		}

		protected override void OnConnect(RemoteClient client) {
			//System.Threading.Thread.Sleep(1000);
			/*TestEntity ent = new TestEntity();
			ent.TestProperty = "OMG IT WORKS";
			GameStateManager.CurrentGameState.AddEntity(ent);*/
			base.OnConnect(client);
		}
		protected override void OnTick() {
			((TestGame)Game).CurrentTick = (int)GameStateManager.CurrentGameState.Tick;
			((TestGame)Game).DrawGUI();
		}

		protected override void DebugMessage(string message) {
			((TestGame)Game).Buffer.Add(string.Format("[S] {0}", message));
			((TestGame)Game).DrawGUI();
		}
		protected override PlayerEntity CreatePlayerEntity() {
			return new TestPlayerEntity();
		}
	}
	public class TestClient : Client {
		public override string Identifier {
			get {
				return "TEST";
			}
		}

		public TestClient(Game game, bool isLocalServer) : base(game, isLocalServer) {
		}

		protected override void OnReceiveGameState(GameState gameState, bool full) {
			base.OnReceiveGameState(gameState, full);
			((TestGame)Game).CurrentTick = (int)gameState.Tick;
			((TestGame)Game).DrawGUI();
		}

		protected override void DebugMessage(string message) {
			((TestGame)Game).Buffer.Add(string.Format("[C] {0}", message));
			((TestGame)Game).DrawGUI();
		}
	}
	public class TestGame : Game {
		private int BufferHeight = 19;

		public TestGame(bool isSinglePlayer, bool isServer) : base(false) {
			IsSinglePlayer = isSinglePlayer;
			Cmd.Variables["cl_updaterate"].Value = new VGame.CommandSystem.Parameter(10);
			if (IsSinglePlayer) {
				new TestServer(this, true);
				Server.Local.Start();
				new TestClient(this, true);
				Client.Local.Connect();
			}
			else {
				if (isServer) {
					new TestServer(this, false);
					Server.Local.Start();
				}
				else {
					new TestClient(this, false);
					//Client.Local.Connect("localhost", 1337);
				}
			}
		}

		public List<string> Buffer = new List<string>();
		public int CurrentTick = 0;
		public string InputBuffer = "";

		protected override void Initialize() {
			LoadCommands();
			StateManager.AddState(new TestState());
			Entity.Add("ent_player", typeof(TestPlayerEntity));
		}
		public override void DebugMessage(string message) {
			Buffer.Add(message);
			DrawGUI();
		}
		public override void ErrorMessage(string message) {
			Buffer.Add("ERROR: " + message);
			DrawGUI();
		}
		public override void OnExit() {
			if (Client.Local != null)
				Client.Local.Disconnect("Shutting down", false);
			if (Server.Local != null)
				Server.Local.Stop();
			Console.Clear();
		}

		public void DrawGUI() {
			Console.SetCursorPosition(0, 0);
			Console.SetWindowPosition(0, 0);
			Console.Write('╔');
			for (int i = 0; i < 78; i++)
				Console.Write('═');
			Console.Write('╗');
			int count = Math.Min(Buffer.Count, BufferHeight);
			for (int i = 0; i < count; i++) {
				Console.Write('║');
				string str = Buffer[Buffer.Count - count + i];
				if (str.Length > 78)
					str = str.Substring(0, 78);
				Console.Write(str.PadRight(78, ' '));
				Console.Write('║');
			}
			for (int i = 0; i < BufferHeight - count; i++) {
				Console.Write('║');
				for (int j = 0; j < 78; j++)
					Console.Write(' ');
				Console.Write('║');
			}
			Console.Write("╟");
			for (int i = 0; i < 78; i++)
				Console.Write('─');
			Console.Write('╢');
			Console.Write('║');
			Console.Write(InputBuffer);
			for (int j = 0; j < 78 - InputBuffer.Length; j++)
				Console.Write(' ');
			Console.Write('║');
			Console.Write('╚');
			for (int i = 0; i < 78; i++)
				Console.Write('═');
			Console.Write('╝');
			string tickString = "Tick: " + CurrentTick;
			Console.Write(tickString.PadRight(79, ' '));
			Console.SetWindowPosition(0, 0);
			Console.SetCursorPosition(1 + InputBuffer.Length, 2 + BufferHeight);

			if (Client.Local != null) {
				foreach (KeyValuePair<int, Entity> kvp in Client.Local.GameStateManager.CurrentGameState.Entities) {
					if (kvp.Value is TestPlayerEntity) {
						Console.SetCursorPosition(MathHelper.Clamp(((TestPlayerEntity)kvp.Value).X, 0, Console.WindowWidth - 1), MathHelper.Clamp(((TestPlayerEntity)kvp.Value).Y, 0, Console.WindowHeight - 2));
						Console.ForegroundColor = ConsoleColor.Green;
						Console.Write('@');
						Console.ResetColor();
					}
				}
			}
		}
		protected override void Update(GameTime gameTime) {
			if (IsSinglePlayer && Client.Local != null && Client.Local.IsConnected)
				Client.Local.Tick();
			base.Update(gameTime);
		}
		private void LoadCommands() {
			CommandDefinition.Add("n", new CommandDefinition(delegate(CommandManager cmdMan, Command cmd, RemoteClient sender) {
				if (sender == null) {
					if (cmdMan.Game.IsSinglePlayer) {
						TestPlayerEntity plrEnt = ((TestPlayerEntity)Server.Local.GameStateManager.CurrentGameState.Entities[Server.Local.GetPlayer(0).EntityID]);
						plrEnt.Y--;
					}
					else {
						if (cmdMan.Game.IsClient()) {
							cmdMan.SendCommand(cmd);
						}
					}
				}
				else {
					TestPlayerEntity plrEnt = ((TestPlayerEntity)Server.Local.GameStateManager.CurrentGameState.Entities[Server.Local.GetPlayer(sender.PlayerID).EntityID]);
					plrEnt.Y--;
				}
			}));
			CommandDefinition.Add("s", new CommandDefinition(delegate(CommandManager cmdMan, Command cmd, RemoteClient sender) {
				if (sender == null) {
					if (cmdMan.Game.IsSinglePlayer) {
						TestPlayerEntity plrEnt = ((TestPlayerEntity)Server.Local.GameStateManager.CurrentGameState.Entities[Server.Local.GetPlayer(0).EntityID]);
						plrEnt.Y++;
					}
					else {
						if (cmdMan.Game.IsClient()) {
							cmdMan.SendCommand(cmd);
						}
					}
				}
				else {
					TestPlayerEntity plrEnt = ((TestPlayerEntity)Server.Local.GameStateManager.CurrentGameState.Entities[Server.Local.GetPlayer(sender.PlayerID).EntityID]);
					plrEnt.Y++;
				}
			}));
			CommandDefinition.Add("e", new CommandDefinition(delegate(CommandManager cmdMan, Command cmd, RemoteClient sender) {
				if (sender == null) {
					if (cmdMan.Game.IsSinglePlayer) {
						TestPlayerEntity plrEnt = ((TestPlayerEntity)Server.Local.GameStateManager.CurrentGameState.Entities[Server.Local.GetPlayer(0).EntityID]);
						plrEnt.X++;
					}
					else {
						if (cmdMan.Game.IsClient()) {
							cmdMan.SendCommand(cmd);
						}
					}
				}
				else {
					TestPlayerEntity plrEnt = ((TestPlayerEntity)Server.Local.GameStateManager.CurrentGameState.Entities[Server.Local.GetPlayer(sender.PlayerID).EntityID]);
					plrEnt.X++;
				}
			}));
			CommandDefinition.Add("w", new CommandDefinition(delegate(CommandManager cmdMan, Command cmd, RemoteClient sender) {
				if (sender == null) {
					if (cmdMan.Game.IsSinglePlayer) {
						TestPlayerEntity plrEnt = ((TestPlayerEntity)Server.Local.GameStateManager.CurrentGameState.Entities[Server.Local.GetPlayer(0).EntityID]);
						plrEnt.X--;
					}
					else {
						if (cmdMan.Game.IsClient()) {
							cmdMan.SendCommand(cmd);
						}
					}
				}
				else {
					TestPlayerEntity plrEnt = ((TestPlayerEntity)Server.Local.GameStateManager.CurrentGameState.Entities[Server.Local.GetPlayer(sender.PlayerID).EntityID]);
					plrEnt.X--;
				}
			}));
		}
	}
	public class TestState : State {
		private ConsoleKeyInfo consoleKey;
		public override void Initialize() {
			//Console.BackgroundColor = ConsoleColor.Cyan;
			//SingleplayerTest();
			//MultiplayerTest();
			Console.SetWindowSize(80, 25);
			((TestGame)Game).DrawGUI();
		}
		public override void Update(GameTime gameTime) {
			consoleKey = Console.ReadKey();
			if (InputManager.IsValidUnicode(consoleKey.KeyChar) && ((TestGame)Game).InputBuffer.Length < 78)
				((TestGame)Game).InputBuffer += consoleKey.KeyChar.ToString();
			if (consoleKey.Key == ConsoleKey.Backspace && ((TestGame)Game).InputBuffer.Length > 0)
				((TestGame)Game).InputBuffer = ((TestGame)Game).InputBuffer.Substring(0, ((TestGame)Game).InputBuffer.Length - 1);
			if (consoleKey.Key == ConsoleKey.Enter) {
				((TestGame)Game).Buffer.Add("> " + ((TestGame)Game).InputBuffer);
				Game.Cmd.Run(((TestGame)Game).InputBuffer, null);
				((TestGame)Game).InputBuffer = "";
			}
			if (Game.IsClient()) {
				if (consoleKey.Key == ConsoleKey.UpArrow) {
					Game.Cmd.Run("n", null);
				}
				if (consoleKey.Key == ConsoleKey.DownArrow) {
					Game.Cmd.Run("s", null);
				}
				if (consoleKey.Key == ConsoleKey.RightArrow) {
					Game.Cmd.Run("e", null);
				}
				if (consoleKey.Key == ConsoleKey.LeftArrow) {
					Game.Cmd.Run("w", null);
				}
			}
			((TestGame)Game).DrawGUI();
		}

		private void SingleplayerTest() {
			Console.WriteLine("Singleplayer test");
			Console.WriteLine("-----------------");
			new TestServer(Game, true);
			new TestClient(Game, true);
			Server.Local.Start();
			Client.Local.Connect();
			FakeDraw();
			Server.Local.Stop();
			Console.WriteLine("");
		}
		private void MultiplayerTest() {
			Console.WriteLine("Multiplayer test");
			Console.WriteLine("----------------");
			new TestServer(Game, false);
			new TestClient(Game, false);
			Server.Local.Start();
			System.Threading.Thread.Sleep(1000);
			Client.Local.Connect();
			System.Threading.Thread.Sleep(3000);
			Client.Local.Disconnect("Intentional disconnect by user");
			System.Threading.Thread.Sleep(3000);
			Server.Local.Stop();
		}

		private void FakeDraw() {
			foreach (KeyValuePair<int, Entity> kvp2 in Client.Local.GameStateManager.CurrentGameState.Entities) {
				foreach (KeyValuePair<string, object> kvp in kvp2.Value.Serialize())
					Console.WriteLine(string.Format("{0,16}{2,8}{1,16}", kvp.Key, kvp.Value, kvp.Value.GetType().Name));
				Console.WriteLine("");
			}
		}
	}

	public class TestPlayerEntity : PlayerEntity {

		// Properties
		[EntityProperty]
		public int X { get; set; }
		[EntityProperty]
		public int Y { get; set; }
		public Vector2 Position { get; set; }

	}

	public class TestEntity : SharedEntity {

		// Properties
		[EntityProperty]
		public string TestProperty { get; set; }
		[EntityProperty]
		public int TestInt { get; set; }
		//[EntityProperty]
		public Vector2 Asdf { get; set; }

		// Constructor
		public TestEntity() : base() {
			TestProperty = "ohai test";
			TestInt = 0;
		}
	}
	class MainClass {
		public static void Main(string[] args) {
			Entity.Add("ent_test", typeof(TestEntity));

			ConsoleKeyInfo cki = new ConsoleKeyInfo();
			bool server = false;
			bool singleplayer = false;
			while (true) {
				Console.Clear();
				Console.Write("[S]erver, [C]lient, or Single[P]layer?");
				cki = Console.ReadKey(true);
				if (cki.KeyChar == 's' || cki.KeyChar == 'S') {
					server = true;
					break;
				}
				if (cki.KeyChar == 'c' || cki.KeyChar == 'C') {
					server = false;
					break;
				}
				if (cki.KeyChar == 'p' || cki.KeyChar == 'P') {
					singleplayer = true;
					break;
				}
			}
			Console.Clear();


			TestGame game = new TestGame(singleplayer, server);
			game.Run();
		}
	}
}
