using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {
	
	// Settings
	public float speed = 10.0f;
	public int damage = 1;
	public GameObject hitEffect;	
	
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
		
		// Move towards the destination and apply correct rotation
		float stepSize = Time.deltaTime * speed;
		transform.position = Vector3.MoveTowards(transform.position, destination.position, stepSize);
		// TODO: use correct rotation for projectile.
		//transform.rotation = Quaternion.LookRotation(destination.position - transform.position);
		
		if (transform.position.Equals(destination.position)) {
			if (Network.isClient) {
				// TODO: use auto-destroy function of particle system
				GameObject effect = (GameObject)Instantiate(hitEffect, transform.position, Quaternion.identity);
				Destroy(effect, effect.GetComponent<ParticleSystem>().duration);
			}
			if (Network.isServer) {
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
