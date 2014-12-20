using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

	// Unique ID for each tile in a gamefield
	[HideInInspector]
	public int id;


	// EVENTS
	void OnMouseDown() {
		if (Network.isClient) {
			// Ask the server to build us a tower. The server will check if this player also
			// owns the gamefield for which a tower is requested.
			Player.GetNetworkView().RPC("BuildTower", RPCMode.Server, id, "TowerPrefab");
			
			Debug.Log ("Player click on tile: "+id);
		}
	}
}
