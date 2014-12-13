using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(NetworkView))]
public class PlayerManager : MonoBehaviour {

	// Settings
	public Player playerPrefab;
	public GameField gameFieldPrefab;

	// Constants
	private const int GAMEFIELD_OFFSET = 15;
	
	// Privates
	class PlayerInfo {
		public Player player;
		public GameField gameField;
		
		public PlayerInfo(Player p, GameField g){
			player = p;
			gameField = g;
		}
	}
	
	private List<PlayerInfo> playerInfoTracker;

	// EVENTS
	void Awake() {
		playerInfoTracker = new List<PlayerInfo>();
	}
	void Update () {
		if (Network.isClient) {
			return; //Get lost, this is the server-side!
		}
	}


	// FUNCTIONS
	public void SpawnPlayer(NetworkPlayer networkPlayer) {
		Vector3 playerPos = new Vector3(GAMEFIELD_OFFSET*GetPlayerCount(), 0, 0);
		playerPos.x += 4;
		playerPos.y += 13;
		playerPos.z -= 5.5f;
		
		Player player = (Player)Network.Instantiate(playerPrefab, playerPos, playerPrefab.transform.rotation, 0);
		GameField gameField = (GameField)Network.Instantiate(gameFieldPrefab, new Vector3(GAMEFIELD_OFFSET*GetPlayerCount(), 0, 0), Quaternion.identity, 0);
		
		playerInfoTracker.Add(new PlayerInfo(player, gameField));
		NetworkView clientNetView = GetPlayerNetworkView(player);
		clientNetView.RPC("SetOwner", RPCMode.All, networkPlayer);
		
		// Link player to gamefield, but only on the server; client doesn't need to know
		gameField.player = player;
		
	}
	public void RemovePlayer(NetworkPlayer player) {
		// TODO
	}
	public GameField GetNextGamefield(Player p){
		GameField retVal = null;
		
		for(int i = 0; GetPlayerCount() > i; i++){
			if(playerInfoTracker[i].player == p){
				if(GetPlayerCount() - 1 == i){
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
	private NetworkView GetPlayerNetworkView(Player player) {
		return (NetworkView)player.GetComponent<NetworkView>();
	}
	private int GetPlayerCount() {
		return playerInfoTracker.Count;
	}

}
