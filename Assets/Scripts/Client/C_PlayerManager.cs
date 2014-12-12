using UnityEngine;
using System.Collections;

public class C_PlayerManager : MonoBehaviour {

	// That's actually not the owner but the player,
	// the server instantiated the prefab for, where this script is attached
	private NetworkPlayer owner;
	
	// Properties of a client. Made private because they
	// can only be changed by RPC functies call by the server.
	private string name;
	private int life;
	private int gold;


	void Awake() {
		// Disable this by default for now
		// Just to make sure no one can use this until we didn't
		// find the right player. (see setOwner())
		if (Network.isClient) {
			enabled = false;
		}
	}
	void Update() {
		if (Network.isServer) {
			return; // Get lost, this is the client side
		}
	}


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
	
	// Called from server with coordinates of the game field.
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
}
