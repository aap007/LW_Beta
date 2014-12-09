using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This class implements the gameplay for a certain gametype
public class GameLogic : MonoBehaviour {

	private PlayerManager playerManager;
	
	private const int GOLD_PER_SEC = 1;
	private const int STARTING_GOLD = 100;
	private const int STARTING_LIFE = 20;
	
	void Awake () {
		// Get reference to PlayerManager, which we need for gameplay.
		playerManager = (PlayerManager)FindObjectOfType(PlayerManager);
	}
	
	void Update () {
		// XXX: is this check necessary?
		if (Network.isClient)
			return;
			
		// TODO: Check for loser and victor
		
		// TODO: Supply all players with gold
		List<C_PlayerManager> playerList = playerManager.GetPlayers();
		foreach (C_PlayerManager player in playerList) {
			// Okay, so the C_PlayerManager object exists on both client and server
			// So what is the difference between increasing gold directly on the object
			// or callling the RPC function on the class? 
			player.IncreaseGold(1);
			//NetworkView net = (NetworkView)player.GetComponent<NetworkView>();
			//net.RPC("SetGold", RPCMode.All, 100);
		}
	}
}
