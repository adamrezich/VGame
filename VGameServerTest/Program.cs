using System;
using System.Collections.Generic;

using VGame;
using VGame.Multiplayer;
using VGame.StateSystem;
using VGame.GameStateSystem;

namespace VGameServerTest {
	public class TestServer : Server {
		public override string Identifier {
			get {
				return "TEST";
			}
		}

		public TestServer(Game game, bool isLocalServer) : base(game, isLocalServer, 1337) {
		}

		protected override void OnConnect(RemoteClient client) {
			System.Threading.Thread.Sleep(1000);
			TestEntity ent = new TestEntity();
			ent.TestProperty = "OMG IT WORKS";
			GameStateManager.CurrentGameState.AddEntity(ent);
			base.OnConnect(client);
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
			//SingleplayerTest();
			MultiplayerTest();
			Game.Exit();
		}

		private void SingleplayerTest() {
			Console.WriteLine("Singleplayer test");
			Console.WriteLine("-----------------");
			new TestServer(Game, true);
			new TestClient(Game, true);
			Server.Local.Start();
			TestClient.Local.Connect();
			FakeDraw();
			TestServer.Local.Stop();
			Console.WriteLine("");
		}
		private void MultiplayerTest() {
			Console.WriteLine("Multiplayer test");
			Console.WriteLine("----------------");
			new TestServer(Game, false);
			new TestClient(Game, false);
			Server.Local.Start();
			System.Threading.Thread.Sleep(1000);
			TestClient.Local.Connect("localhost", 1337);
			System.Threading.Thread.Sleep(3000);
			TestClient.Local.Disconnect("Peace");
			System.Threading.Thread.Sleep(3000);
			TestServer.Local.Stop();
		}

		private void FakeDraw() {
			foreach (KeyValuePair<int, Entity> kvp2 in Client.Local.GameStateManager.CurrentGameState.Entities) {
				foreach (KeyValuePair<string, object> kvp in kvp2.Value.Serialize())
					Console.WriteLine(string.Format("{0,16}{2,8}{1,16}", kvp.Key, kvp.Value, kvp.Value.GetType().Name));
				Console.WriteLine("");
			}
		}
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
			TestGame game = new TestGame(false);
			game.Run();
			Console.ReadLine();
		}
	}
}
