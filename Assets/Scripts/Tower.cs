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
	
	public Renderer muzzleFlash;
	public Light muzzleLight;
	
	public AudioClip firesound;
	
	// Privates
	private float timeLeft = 0.0f;
	private Enemy target = null;
	private GameField gameField = null;

	
	// EVENTS
	void Start() {
		muzzleFlash.enabled = false;
		muzzleLight.enabled = false;
		
		// Get Gamefield through following relation: GameField -> Tile -> Tower
		gameField = transform.parent.parent.gameObject.GetComponent<GameField>();
		if (gameField == null) {
			Debug.Log ("Tower: error resolving gamefield");
		}
		
		StartCoroutine(BuildEffect());	
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
					Fire(transform.position, target.transform);	
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
	
	
	// FUNCTIONS
	void Fire(Vector3 startPos, Transform target) {
		Projectile p = (Projectile)Network.Instantiate(bulletPrefab, startPos, Quaternion.identity, 0);
		p.damage = bulletDamage;
		p.destination = target.transform;
		
		// Inform clients that we are shooting, so they can display effects
		networkView.RPC("ClientFire", RPCMode.Others);
	}
	
	
	// SERVER -> CLIENT RPC
	[RPC]
	void ClientFire() {
		audio.PlayOneShot(firesound, 0.4F);
		StartCoroutine(FireEffect());
	}
	
	
	// HELPER FUNCTIONS
	private Enemy findClosestTarget() {
		lock(gameField.enemyList) {
			Vector3 pos = transform.position;
			Enemy closest = (gameField.enemyList.Count == 0 ? null : gameField.enemyList[0]);
			foreach(Enemy enemy in gameField.enemyList) {
				float cur = Vector3.Distance(pos, enemy.transform.position);
				float old = Vector3.Distance(pos, closest.transform.position);
				if (cur < old) {
					closest = enemy;
				}
			}
			return closest;
		}
	}
	
	
	// CO-ROUTINES
	IEnumerator BuildEffect() {
		for (float f = 0.0f; f < 1.0f; f += 0.05f) {
			foreach (MeshRenderer r in GetComponentsInChildren(typeof(MeshRenderer))) {
				Color c = r.material.color;
				c.a = f;
				r.material.color = c;
			}
			yield return null;
		}
	}
	IEnumerator FireEffect () {
		muzzleFlash.renderer.enabled = true;
		muzzleLight.enabled = true;
		yield return new WaitForSeconds(0.08f);	
		muzzleFlash.renderer.enabled = false;
		muzzleLight.enabled = false;
	}
}
