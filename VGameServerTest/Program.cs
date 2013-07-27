using System;

using VGame;
using VGame.Multiplayer;
using VGame.StateSystem;

namespace VGameServerTest {
	public class TestServer : Server {
		public override string Identifier {
			get {
				return "TEST";
			}
		}

		public TestServer(Game game, bool isLocalServer) : base(game, isLocalServer, 1337) {
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
	}
	public class TestGame : Game {
		public TestGame(bool initializeRenderer) : base(initializeRenderer) {
		}
		protected override void Initialize() {
			StateManager.AddState(new TestState());
		}
	}
	public class TestState : State {
		public override void Initialize() {
			Console.WriteLine("VGame multiplayer test");
			Console.WriteLine("----------------------");
			new TestServer(Game, false); // true = is local server (single player)
			new TestClient(Game, false); // true = is local server (single player)
			Server.Local.Start();
			System.Threading.Thread.Sleep(1000);
			TestClient.Local.Connect("localhost", 1337);
			System.Threading.Thread.Sleep(3000);
			//TestClient.Local.Disconnect();
		}
	}
	class MainClass {
		public static void Main(string[] args) {
			TestGame game = new TestGame(false);
			game.Run();
		}
	}
}
