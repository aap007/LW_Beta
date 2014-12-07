using UnityEngine;
using System.Collections;

public class PlayerManager : MonoBehaviour {

	private string name;

	private int gold;
	private int life;

	void Start () {
	}


	void Update () {
		if (Network.isClient) {
			return; //Get lost, this is the server-side!
		}	
	}
}
