using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NetworkView))]
public class Tower : MonoBehaviour {

	// Settings
	public Projectile bulletPrefab = null;
	public int bulletDamage = 1;
	public float interval = 2.0f;
	
	public float range = 10.0f;
	public float rangeTurn = 15.0f;
	public float turnSpeed = 4.0f;
	
	public int buildPrice = 1;
	public int sellPrice = 1;
	
	// Privates
	private float timeLeft = 0.0f;
	private Enemy target = null;

	
	// EVENTS
	void Start() {
		StartCoroutine(FadeIn());	
	}
	void Update() {
		if (Network.isClient) {
			return;
		}
		
		if (target == null) {
			target = findClosestTarget();
		}
		else {
			timeLeft -= Time.deltaTime;
			if (timeLeft <= 0.0f) {		
				// Target is within range, start firing
				if (Vector3.Distance (transform.position, target.transform.position) <= range) {
					Projectile p = (Projectile)Network.Instantiate(bulletPrefab, transform.position, Quaternion.identity, 0);
					p.damage = bulletDamage;
					// TODO: this always homes to target
					p.destination = target.transform;
					
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
	void OnMouseDown() {
		if (Network.isClient) {
			// TODO: Ask the server to sell this tower
			//Player.GetNetworkView().RPC("SellTower", RPCMode.Server, tileId);
		}
	}
	
	
	// HELPER FUNCTIONS
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
	
	// CO-ROUTINES
	IEnumerator FadeIn() {
		for (float f = 0.0f; f < 1.0f; f += 0.05f) {
			foreach (MeshRenderer r in GetComponentsInChildren(typeof(MeshRenderer))) {
				Color c = r.material.color;
				c.a = f;
				r.material.color = c;
			}
			yield return null;
		}
	}
}
