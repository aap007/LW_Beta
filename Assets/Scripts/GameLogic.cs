using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This class implements the gameplay for a certain gametype
// It only exists on the server
public class GameLogic : MonoBehaviour {

	// Constants
	private const int GOLD_PER_SEC = 1;
	private const int STARTING_GOLD = 100;
	private const int STARTING_LIFE = 20;
	
	// Privates
	private float interval = 5.0f;
	private float timeLeft = 0.0f;
	
	// EVENTS
	void Update() {
		// TODO: Check for loser and victor
		
		timeLeft -= Time.deltaTime;
		if (timeLeft <= 0.0f) {
			// Supply all players with gold
			Player[] playerList = FindObjectsOfType<Player>();
			foreach (Player player in playerList) {
			  player.gold += GOLD_PER_SEC;
			  player.networkView.RPC ("SetGold", RPCMode.All, player.gold);
			}
			
			timeLeft = interval;
		}
	}
}
