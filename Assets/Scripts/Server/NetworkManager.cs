using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(NetworkView))]
public class NetworkManager : MonoBehaviour {

	// Settings
	public PlayerManager playerManager;

	// Private stuff
	private enum NetworkGroup {
		DEFAULT = 0,
		PLAYER  = 1,
		SERVER  = 2
	};

	// TEMP GUI stuff
	public float widthPercent = 0.3f;
	public float heightPercent = 0.3f;

	// CONSTANTS
	private const int NETWORK_PORT = 25000;
	private const int MAX_PLAYERS = 8;	
	private const string typeName = "LW_BETA_SECRET";
	private const string gameName = "Join_me";
	
	
	// MASTER SERVER EVENTS
	void OnMasterServerEvent(MasterServerEvent msEvent) {
		if (msEvent == MasterServerEvent.RegistrationSucceeded) {
			Debug.Log("MasterServerEvent: RegistrationSucceeded");
		}
	}
	void OnFailedToConnectToMasterServer(NetworkConnectionError info) {
		Debug.Log("MasterServerEvent: couldn't connect: " + info);
	}

	void OnServerInitialized() {
		Debug.Log("Network: OnServerInitialized");
		MasterServer.RegisterHost(typeName, gameName);
	}
	void OnConnectedToServer() {
		Debug.Log("Client: connected to server");
	}
	void OnDisconnectedFromServer(NetworkDisconnection info) {
		if (Network.isServer)
			Debug.Log("Local server connection disconnected");
	}
	
	void OnPlayerConnected(NetworkPlayer player) {
		Debug.Log ("Player connected from " + player.ipAddress + ":" + player.port);
		playerManager.SpawnPlayer (player);
	}

	void OnPlayerDisconnected(NetworkPlayer player) {
		Debug.Log("Player disconnected, cleanup");
		playerManager.RemovePlayer (player);
		Network.RemoveRPCs(player);
		Network.DestroyPlayerObjects(player);
	}


	// FUNCTIONS
	private void StartServer() {
		Network.InitializeServer(MAX_PLAYERS, NETWORK_PORT, !Network.HavePublicAddress());
		Instantiate(playerManager);
	}


	// TODO: remove this deprecated style GUI, use Canvas object instead
	void OnGUI() {
		Rect r = new Rect(Screen.width * (1 - widthPercent) / 2,
			Screen.height * (1 - heightPercent) / 2,
			Screen.width * widthPercent,
			Screen.height * heightPercent);

		// Draw menu
		GUILayout.BeginArea(r);
		GUILayout.BeginVertical("box");
		
		if (GUILayout.Button("Start Server"))
			StartServer();
		if (GUILayout.Button("Quit"))
			Application.Quit();

		GUILayout.EndVertical();
		GUILayout.EndArea();
	}
}
