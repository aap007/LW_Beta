using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

	// Unique ID for each tile in a gamefield
	[HideInInspector]
	public int id;
	
	// Reference to the Tower that is built on this tile
	[HideInInspector]
	public Tower tower;
	

	// EVENTS
	void OnMouseDown() {
		if (Network.isClient) {
			// Ask the server to build us a tower. The server will check if this player also
			// owns the gamefield for which a tower is requested.
			transform.parent.gameObject.networkView.RPC("BuildTower", RPCMode.Server, id, "TowerPrefab");
		}
	}
}
