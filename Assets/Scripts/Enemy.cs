using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour {

	// Settings
	public int health = 5;
	public int speed = 25;
	public int price = 1;

	// Keep track of the player that spawned this enemy
	// This is the SERVER reference of the Player
	[HideInInspector]
	public Player owner;
	
	// SERVER reference to the gamefield this enemy is on
	[HideInInspector]
	public GameField gameField;
	
	// Destination for pathfinding
	[HideInInspector]
	public Vector3 vDestination;
	
	// EVENTS
	void Start (){
		CharacterController c = GetComponent<CharacterController>();
		c.detectCollisions = false;
	}

	// FUNCTIONS
	// TODO: this function is ugly.
	public void SetDestination(Vector3 destination) {
		vDestination = destination;
		AstarAI ai = (AstarAI)gameObject.GetComponent<AstarAI>();
		ai.SetDestination(destination);
	}
	public void TakeDamage(int damage) {
		health -= damage;
		if (health <= 0) {
			gameField.EnemyKilled(this);
		}
	}
}
