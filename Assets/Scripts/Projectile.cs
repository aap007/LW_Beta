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
		// Destroy bullet if destination does not exist anymore
		if (destination == null) {
			if (Network.isServer) {
				Network.Destroy(gameObject);
			}
			return;
		}
		
		// Move towards the destination
		float stepSize = Time.deltaTime * speed;
		transform.position = Vector3.MoveTowards(transform.position, destination.position, stepSize);
		
		// TODO: Use correct rotation for projectile
		
		// TODO: fixme
		if (Network.isServer) {
			if (transform.position.Equals(destination.position)) {
				Enemy enemy = destination.GetComponent<Enemy>();
				enemy.TakeDamage(damage);
				
				// Destroy this projectile
				Network.Destroy(gameObject);
			}
		}
	}
	
	// SERVER -> CLIENT RPC
	[RPC]
	void SetTarget(NetworkViewID id) {
		NetworkView view = NetworkView.Find(id);
		if (view == null) {
			Debug.Log ("Couldn't find projectile target with view ID: " + view);
			return;
		}
		destination = view.gameObject.transform;
	}
}
