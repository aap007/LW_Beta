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
	void OnCreate() {
		playerTracker = new List<C_PlayerManager>();
	}
	void Update () {
		if (Network.isClient) {
			return; //Get lost, this is the server-side!
		}	
	}


	// PUBLICS
	public void SpawnPlayer(NetworkPlayer player) {
		C_PlayerManager client = (C_PlayerManager)Network.Instantiate(playerPrefab, transform.position, Quaternion.identity, (int)NetworkGroup.PLAYER);
		playerTracker.Add(client);
		NetworkView clientNetView = GetPlayerNetworkView(client);
		clientNetView.RPC("SetOwner", RPCMode.All, player);
		
		Debug.Log ("Spawning field on server at X-coordinate: " + (25 * GetPlayerCount ()));
		GameField gameField = (GameField)Network.Instantiate(gameFieldPrefab, new Vector3(GAMEFIELD_OFFSET*GetPlayerCount(), 0, 0), Quaternion.identity, 0);
		clientNetView.RPC("SetGameField", RPCMode.All, gameField);
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
