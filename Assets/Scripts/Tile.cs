using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

	// Privates
	private int id;
	
	
	// FUNCTIONS
	public void SetId (int identifier) {
		id = identifier;
	}
	
	public int GetId () {
		return id;
	}
	

	// EVENTS
	void OnMouseDown() {
		if (Network.isClient) {
			transform.parent.gameObject.networkView.RPC("BuildTower", RPCMode.Server, id, "TowerPrefab");
		}
	}
}
