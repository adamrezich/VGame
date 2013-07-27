using System;
using Lidgren.Network;

namespace VGame.Networking {
	public class RemoteClient {

		// Properties
		public int PlayerID { get; internal set; }
		public NetConnection Connection { get; internal set; }

		// Constructor
		public RemoteClient(int playerID, NetConnection connection) {
			PlayerID = playerID;
			Connection = connection;
		}



	}
}

