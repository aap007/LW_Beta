using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour {

	// Settings
	public int health = 5;
	public int speed = 25;
	public int price = 1;

	
	public Vector3 vDestination;


	// FUNCTIONS
	public void SetDestination(Vector3 destination) {
		vDestination = destination;
		AstarAI ai = (AstarAI)gameObject.GetComponent<AstarAI>();
		ai.SetDestination(destination);
	}
	public void TakeDamage(int damage) {
		health -= damage;
		if (health <= 0) {
			Destroy(gameObject);
		}
	}
}
