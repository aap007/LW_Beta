using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	// That's actually not the owner but the player,
	// the server instantiated the prefab for, where this script is attached
	public NetworkPlayer owner;
	
	// Properties of a player.
	public string name;
	public int life;
	public int gold;

	// Reference to PlayerManager for server
	private PlayerManager playerManager; 

	void Awake() {
		// Disable this by default for now
		// Just to make sure no one can use this until we didn't
		// find the right player. (see setOwner())
		if (Network.isClient) {
			enabled = false;
		}
		else if(Network.isServer){
			playerManager = (PlayerManager)GameObject.Find("Server").GetComponent<PlayerManager>();
			Debug.Log(playerManager);
		}
	}


	// Called from server on clients
	[RPC]
	void SetOwner(NetworkPlayer p) {
		owner = p;
		if(p == Network.player){
			// So it just so happens that WE are the player in question,
			// which means we can enable this control again
			enabled = true;
		}
		else {
			// Disable a bunch of other things here that are not interesting
			if (GetComponent<Camera>() != null) {
				GetComponent<Camera>().enabled = false;
			}
			
			if (GetComponent<AudioListener>() != null) {
				GetComponent<AudioListener>().enabled = false;
			}
			
			if (GetComponent<GUILayer>() != null) {
				GetComponent<GUILayer>().enabled = false;
			}
		}
	}
	[RPC]
	void SetCameraTarget(Vector3 v) {
		if (Network.isServer) {
			// Create camera for the client that is positioned
			// at a birdseye view of this gamefield.
			v.y += 6;
			v.x += 4;
			camera.transform.position = v;
			camera.transform.Rotate(45, 0, 0);
		}
	}
	[RPC]
	void SetLife(int amount) {
		life = amount;
	}
	[RPC]
	void SetGold(int amount) {
		gold = amount;
	}
	
	
	// Called from client on server
	[RPC]
	void SpawnEnemy() {
		if (Network.isServer) {
			GameField nextGameField = playerManager.GetNextGamefield(this);
			
			if(nextGameField != null){
				nextGameField.SpawnEnemy();
			}
			else{
				Debug.Log("ERROR: No next player");
			}
		}
	}
	
	
	void OnGUI() {
		if (Network.isServer) {
			return; // Get lost, this is the client side
		}
			
		GUI.Label(new Rect(0, 0, 400, 200), "Life: " + life);	
		GUI.Label(new Rect(0, 10, 400, 200), "Gold: " + gold);
		
		if (GUI.Button (new Rect(200, 20, 200, 100), "Spawn creep O_O")){
			networkView.RPC("SpawnEnemy", RPCMode.Server);
		}
	}
}
