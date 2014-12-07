using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameLogic : MonoBehaviour {

	public List<GameField> gameFields;
	
	const int GOLD_PER_SEC = 1;
	const int STARTING_GOLD = 100;
	const int STARTING_LIFE = 20;
	
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		// Check for loser and victor
		
		// Supply all players with gold
		
	}
	
	public void AddPlayer() {
		/*for (int i=0; i<gameFields.Count; i++) {
			if (gameFields[i].player == null) {
				gameFields[i].player = new Player();
				return;
			}
		}*/
	}
}
