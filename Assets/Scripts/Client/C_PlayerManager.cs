using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NetworkView))]
public class C_PlayerManager : MonoBehaviour {

	//That's actually not the owner but the player,
	//the server instantiated the prefab for, where this script is attached
	private NetworkPlayer owner;
	
	public GameField gameField = null;
	
	// Properties of a client. Made private because they
	// can only be changed by RPC functies call by the server.
	private string name = "HENK";
	private int life;
	private int gold;


	[RPC]
	void SetOwner(NetworkPlayer p) {
		if (Network.isServer) {
			owner = p;
		}
	}
	
	[RPC]
	void SetGameField(GameField g) {
		if (Network.isServer) {
			gameField = g;
			// Set main camera for this user
			Camera cam = g.GetComponent<Camera>();
			cam.enabled = true;
		}
	}
	
	[RPC]
	void SetName(string s) {
		if (Network.isServer) {
			name = s;
		}
	}
	
	[RPC]
	public void IncreaseGold(int i) {
		if (Network.isServer) {
			gold += i;
		}
	}
	
}
