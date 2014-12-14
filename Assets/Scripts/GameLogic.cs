using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This class implements the gameplay for a certain gametype
// It only exists on the server
public class GameLogic : MonoBehaviour {

	// Privates
	private PlayerManager playerManager = null;
	
	// Constants
	private const int GOLD_PER_SEC = 1;
	private const int STARTING_GOLD = 100;
	private const int STARTING_LIFE = 20;
	
	private float interval = 5.0f;
	private float timeLeft = 0.0f;
	
	// EVENTS
	void OnAwake () {
		// Get reference to PlayerManager, which we need for gameplay.
		playerManager = (PlayerManager)GameObject.Find("Server").GetComponent<PlayerManager>();
	}
	void Update () {			
		// TODO: Check for loser and victor
		
		timeLeft -= Time.deltaTime;
		if (timeLeft <= 0.0f) {
			// Supply all players with gold
			Player[] playerList = FindObjectsOfType<Player>();
			foreach (Player player in playerList) {
			  player.gold += 1;
			  player.networkView.RPC ("SetGold", RPCMode.All, player.gold);
			}
			
			timeLeft = interval;
		}
	}
}
