using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour {

	// Settings
	public int health = 5;
	public int speed = 25;

	
	public Vector3 vDestination;

	
	// EVENTS
	void Update () {
		//TextMesh tm = GetComponentInChildren<TextMesh>();
		//tm.text = new string('-', health);
		//tm.renderer.material.color = Color.red;

		// adjust health bar so it always faces the camera
		//tm.transform.forward = Camera.main.transform.forward;
	}


	// FUNCTIONS
	public void SetDestination(Vector3 destination) {
		vDestination = destination;
		AstarAI ai = (AstarAI)gameObject.GetComponent<AstarAI>();
		ai.SetDestination(destination);
	}
	public void TakeDamage(int damage) {
		health -= damage;
		if (health <= 0) {
			Debug.Log("Destroying enemy");
			Destroy(gameObject);
		}
	}
}
