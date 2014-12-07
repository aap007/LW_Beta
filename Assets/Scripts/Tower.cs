using UnityEngine;
using System.Collections;

public class Tower : MonoBehaviour {

	public Projectile bulletPrefab = null;
	public int bulletDamage = 1;
	
	// interval
	public float interval = 2.0f;
	float timeLeft = 0.0f;
	
	// attack range
	public float range = 10.0f;
	public float rangeTurn = 15.0f;
	public float turnSpeed = 4.0f;
	
	// Build information
	public int buildPrice = 1;
	
	private Enemy target = null;
	
	private Enemy findClosestTarget() {
		Enemy closest = null;
		Vector3 pos = transform.position;
		Enemy[] enemies = (Enemy[])FindObjectsOfType(typeof(Enemy));
		if ((enemies != null) && (enemies.Length > 0)) {
			closest = enemies[0];
			for (int i = 1; i < enemies.Length; ++i) {
				float cur = Vector3.Distance(pos, enemies[i].transform.position);
				float old = Vector3.Distance(pos, closest.transform.position);
				if (cur < old) {
					closest = enemies[i];
				}
			}
		}
		return closest;
	}
	
	void Update() {
		if (target == null) {
			target = findClosestTarget();
		}
		else {
			timeLeft -= Time.deltaTime;
			if (timeLeft <= 0.0f) {		
				// Target is within range, start firing
				if (Vector3.Distance (transform.position, target.transform.position) <= range) {
					GameObject g = (GameObject)Instantiate (bulletPrefab.gameObject, transform.position, Quaternion.identity);
					Projectile b = g.GetComponent<Projectile> ();
					b.damage = bulletDamage;
					// TODO: this always homes to target
					b.SetDestination (target.transform);
					
					timeLeft = interval;
				}
				// Previous target is too far away, find new target
				// TODO: fix
				else {
					target = findClosestTarget();
				}
			}
			
			// is it close enough to turn?
			if (Vector3.Distance (transform.position, target.transform.position) <= rangeTurn) {
				// Determine the rotation.
				Quaternion targetRotation = Quaternion.LookRotation(transform.position - target.transform.position);
				targetRotation.x = 0;
				targetRotation.z = 0;
				
				// Smoothly rotate towards the target point.
				transform.FindChild("Top").rotation = Quaternion.Slerp(transform.FindChild("Top").rotation, targetRotation, turnSpeed * Time.deltaTime);
				
			}	
		}
	}
}
