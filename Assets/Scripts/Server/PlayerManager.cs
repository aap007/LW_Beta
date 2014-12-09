using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(NetworkView))]
public class PlayerManager : MonoBehaviour {

	// Settings
	public C_PlayerManager playerPrefab;
	public GameField gameFieldPrefab;

	// Constants
	private const int GAMEFIELD_OFFSET = 25;
	
	// Privates
	private List<C_PlayerManager> playerTracker;
	
	private enum NetworkGroup {
		DEFAULT = 0,
		PLAYER  = 1,
		SERVER  = 2
	};


	// EVENTS
	void Awake() {
		playerTracker = new List<C_PlayerManager>();
		Debug.Log("Playermanager created");
	}
	void Update () {
		if (Network.isClient) {
			return; //Get lost, this is the server-side!
		}	
	}


	// FUNCTIONS
	public void SpawnPlayer(NetworkPlayer player) {
		//C_PlayerManager client = (C_PlayerManager)Network.Instantiate(playerPrefab, transform.position, Quaternion.identity, (int)NetworkGroup.PLAYER);
		//playerTracker.Add(client);
		//NetworkView clientNetView = GetPlayerNetworkView(client);
		//clientNetView.RPC("SetOwner", RPCMode.All, player);
		
		//Debug.Log ("Spawning field on server at X-coordinate: " + (25 * GetPlayerCount ()));
		//GameField gameField = (GameField)Network.Instantiate(gameFieldPrefab, new Vector3(GAMEFIELD_OFFSET*GetPlayerCount(), 0, 0), Quaternion.identity, (int)NetworkGroup.PLAYER);
		//clientNetView.RPC("SetGameField", RPCMode.All, gameField);
	}
	public void RemovePlayer(NetworkPlayer player) {
		// TODO
	}
	public List<C_PlayerManager> GetPlayers() {
		return playerTracker;
	}


	// HELPER FUNCTIONS
	private NetworkView GetPlayerNetworkView(C_PlayerManager player) {
		return (NetworkView)player.GetComponent<NetworkView>();
	}
	private int GetPlayerCount() {
		return playerTracker.Count;
	}


}
