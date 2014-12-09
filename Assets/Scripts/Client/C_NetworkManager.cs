using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(NetworkView))]
public class C_NetworkManager : MonoBehaviour {

	// Settings
	public float widthPercent = 0.3f;
	public float heightPercent = 0.3f;
	
	// Constants
	private const string typeName = "LW_BETA_SECRET";
	private const string gameName = "Join_me";
	
	// Privates
	private bool playing = false;
	private HostData[] hostList;
	
	
	// MASTER SERVER EVENTS
	void OnMasterServerEvent(MasterServerEvent msEvent) {
		if (msEvent == MasterServerEvent.HostListReceived) {
			Debug.Log("MasterServerEvent: HostListReceived");
			hostList = MasterServer.PollHostList();
		}
	}
	void OnConnectedToServer() {
		Debug.Log("Client: connected to server");
		playing = true;
	}	
	void OnDisconnectedFromServer(NetworkDisconnection info) {
		if (Network.isClient) {
			if (info == NetworkDisconnection.LostConnection)
				Debug.Log("Lost connection to the server");
			else
				Debug.Log("Successfully diconnected from the server");
		}
	}


	// FUNCTIONS
	private void RefreshHostList() {
		MasterServer.RequestHostList(typeName);
	}
	private void JoinServer(HostData hostData) {
		Network.Connect(hostData);
	}
	
	
	// TODO: remove this deprecated GUI, use Canvas object instead
	void OnGUI() {
		if (playing)
			return; 
		
		Rect r = new Rect(Screen.width * (1 - widthPercent) / 2,
		                  Screen.height * (1 - heightPercent) / 2,
		                  Screen.width * widthPercent,
		                  Screen.height * heightPercent);
		
		// Draw menu
		GUILayout.BeginArea(r);
		GUILayout.BeginVertical("box");

		if (GUILayout.Button("Get gamelist"))
			RefreshHostList();
		
		if (hostList != null) {
			for (int i = 0; i < hostList.Length; i++) {
				if (GUILayout.Button(hostList[i].gameName))
					JoinServer(hostList[i]);
			}
		}
		
		if (GUILayout.Button("Quit"))
			Application.Quit();
		
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}
}
