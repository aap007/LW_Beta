using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour {
	public GUISkin skin = null;

	public float widthPercent = 0.3f;
	public float heightPercent = 0.3f;

	public Texture2D logo = null;
	
	private const string typeName = "LW_BETA_SECRET";
	private const string gameName = "Join_me";
	private const int maxPlayers = 8;
	bool playing = false;

	private HostData[] hostList;
	
	public GameField gameFieldPrefab;
	
	private List<NetworkPlayer> playerList;
	

	void OnCreate() {
		playerList = new List<NetworkPlayer>();
	}

	void OnMasterServerEvent(MasterServerEvent msEvent) {
		if (msEvent == MasterServerEvent.RegistrationSucceeded) {
			Debug.Log("MasterServerEvent: RegistrationSucceeded");
			playing = true;
		}
		if (msEvent == MasterServerEvent.HostListReceived) {
			Debug.Log("MasterServerEvent: HostListReceived");
			hostList = MasterServer.PollHostList();
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
		playing = true;
	}
	void OnPlayerConnected(NetworkPlayer player) {
		//Debug.Log("Player " + playersConnected + " connected from " + player.ipAddress + ":" + player.port);
		//Debug.Log ("Spawning field on server at X-coordinate: " + 25*playersConnected);
		GameField gameField = (GameField)Network.Instantiate(gameFieldPrefab, new Vector3(25*(playerList.Count), 0, 0), Quaternion.identity, 0);
		playerList.Add(player);
	}
	void OnDisconnectedFromServer(NetworkDisconnection info) {
		if (Network.isServer)
			Debug.Log("Local server connection disconnected");
		else
			if (info == NetworkDisconnection.LostConnection)
				Debug.Log("Lost connection to the server");
		else
			Debug.Log("Successfully diconnected from the server");
	}
	void OnPlayerDisconnected(NetworkPlayer player) {
		Debug.Log("Clean up after player " + player);
		Network.RemoveRPCs(player);
		Network.DestroyPlayerObjects(player);
	}

	private void RefreshHostList() {
		MasterServer.RequestHostList(typeName);
	}
	private void JoinServer(HostData hostData) {
		Network.Connect(hostData);
	}
	private void StartServer() {
		Network.InitializeServer(maxPlayers, 25000, !Network.HavePublicAddress());
	}

	
	void OnGUI() {
		if (playing)
			return;
	
		GUI.skin = skin; 

		Rect r = new Rect(Screen.width * (1 - widthPercent) / 2,
			Screen.height * (1 - heightPercent) / 2,
			Screen.width * widthPercent,
			Screen.height * heightPercent);

		// draw logo, centered at the top right of the menu
		/*if (logo != null) {
			Rect l = new Rect(r.x + r.width,
				r.y - logo.height,
				logo.width,
				logo.height);
			GUI.DrawTexture(l, logo);
		}*/

		// Draw menu
		GUILayout.BeginArea(r);
		GUILayout.BeginVertical("box");

		if (!Network.isClient && !Network.isServer)
		{
			if (GUILayout.Button("Start Server"))
				StartServer();
			if (GUILayout.Button("Refresh"))
				RefreshHostList();
			
			if (hostList != null) {
				for (int i = 0; i < hostList.Length; i++) {
					if (GUILayout.Button(hostList[i].gameName))
						JoinServer(hostList[i]);
				}
			}
		}

		//if (GUILayout.Button("Local game"))
		//	Application.LoadLevel("Scene_Main");

		if (GUILayout.Button("Quit"))
			Application.Quit();

		GUILayout.EndVertical();
		GUILayout.EndArea();
	}
}
