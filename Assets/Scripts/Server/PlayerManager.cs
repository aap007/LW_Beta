using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This class resides only on server-side and keeps track
// of all players connected to the server.
[RequireComponent(typeof(NetworkView))]
public class PlayerManager : MonoBehaviour {

	// Settings
	public Player playerPrefab;
	public GameField gameFieldPrefab;

	// Constants
	private const int GAMEFIELD_OFFSET = 15;
	
	// Publics
	public class PlayerInfo {
		// This is the network player on the CLIENT
		public NetworkPlayer networkPlayer;
	
		// SERVER instance of a Player
		public Player player;
		// SERVER instance of a GameField
		public GameField gameField;
		
		public PlayerInfo(NetworkPlayer np, Player p, GameField g) {
			networkPlayer = np;
			player = p;
			gameField = g;
		}
	}
	public static List<PlayerInfo> playerInfoTracker;


	// EVENTS
	void Awake() {
		playerInfoTracker = new List<PlayerInfo>();
	}


	// FUNCTIONS
	
	// Find the player belonging to a certain gamefield
	public static Player GetPlayer(GameField g) {
		foreach (PlayerInfo playerInfo in playerInfoTracker) {
			if (playerInfo.gameField == g) {
				return playerInfo.player;
			}
		}
		return null;
	}
	// Use this function to get player information for the current player.
	// This will ONLY work when called on the SERVER using the
	// Network.player from the CLIENT as argument for this function.
	// To get the client Network.player use NetworkMessageInfo.sender
	// from the RPC call from client to server.
	public static PlayerInfo GetPlayerInfo(NetworkPlayer p) {
		foreach (PlayerInfo playerInfo in playerInfoTracker) {
			if (playerInfo.networkPlayer == p) {
				return playerInfo;
			}
		}
		return null;
	}
	public static PlayerInfo GetPlayerInfo(GameField g) {
		foreach (PlayerInfo playerInfo in playerInfoTracker) {
			if (playerInfo.gameField == g) {
				return playerInfo;
			}
		}
		return null;
	}
		
	public void SpawnPlayer(NetworkPlayer networkPlayer) {
		// Position the camera of the player at a birds-eye view of the gamefield
		Vector3 playerPos = new Vector3(GAMEFIELD_OFFSET*playerInfoTracker.Count, 0, 0);
		playerPos.x += 4;
		playerPos.y += 10;
		playerPos.z -= 4;
		
		// Create a player and a gamefield
		// For each player that joins, a gamefield is created with an offset on the X-axis
		Player player = (Player)Network.Instantiate(playerPrefab, playerPos, playerPrefab.transform.rotation, 0);
		GameField gameField = (GameField)Network.Instantiate(gameFieldPrefab, new Vector3(GAMEFIELD_OFFSET*playerInfoTracker.Count, 0, 0), Quaternion.identity, 0);
		
		// Link player to gamefield, but only on the server; client doesn't need to know
		playerInfoTracker.Add(new PlayerInfo(networkPlayer, player, gameField));
		// Also add a reference in the player object, used in many RPC calls.
		player.gameField = gameField;		
		
		// Set the owner (=owning client) for the created player
		NetworkView clientNetView = player.GetComponent<NetworkView>();
		clientNetView.RPC("SetOwner", RPCMode.All, networkPlayer);
	}
	public void RemovePlayer(NetworkPlayer player) {
		// TODO
	}
	public GameField GetNextGamefield(Player p){
		GameField retVal = null;
		
		for(int i = 0; playerInfoTracker.Count > i; i++){
			if(playerInfoTracker[i].player == p){
				if(playerInfoTracker.Count - 1 == i){
					retVal = playerInfoTracker[0].gameField;
				}
				else{
					retVal = playerInfoTracker[i+1].gameField;
				}
				break;
			}
		}
		return retVal;
	}
	

	// HELPER FUNCTIONS


}
