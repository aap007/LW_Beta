using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

	// Unique ID for each tile in a gamefield
	[HideInInspector]
	public int id;
	

	// EVENTS
	void OnMouseDown() {
		if (Network.isClient) {
			// TODO: this also works for gamefields not owned by this player!
			transform.parent.gameObject.networkView.RPC("BuildTower", RPCMode.Server, id, "TowerPrefab");
		}
	}
}
