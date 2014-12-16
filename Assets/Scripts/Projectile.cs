using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {
	
	// Settings
	public float speed = 10.0f;
	public int damage = 1;
	
	// Destination set by Tower when creating the bullet
	[HideInInspector]
	public Transform destination;
	
	
	// EVENTS
	void Update () {
		if (Network.isClient) {
			return;
		}
		
		// Destroy bullet if destination does not exist anymore
		if (destination == null) {
			Network.Destroy(gameObject);
			return;
		}
		// Move towards the destination
		float stepSize = Time.deltaTime * speed;
		transform.position = Vector3.MoveTowards(transform.position, destination.position, stepSize);
		
		// TODO: Use correct rotation for projectile
		
		// TODO: fixme
		if (transform.position.Equals(destination.position)) {
			Enemy enemy = destination.GetComponent<Enemy>();
			enemy.TakeDamage(damage);
			
			// Destroy this projectile
			Network.Destroy(gameObject);
		}
	}
}
