using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NetworkView))]
public class Tower : MonoBehaviour {

	// Settings
	public Projectile bulletPrefab;
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
	
	// Reference to the part of the model that will be
	// rotated towards enemies, i.e. the turret barrel.
	public GameObject rotator;
	
	[HideInInspector]
	public int tileId;
	
	// Privates
	private float timeLeft = 0.0f;
	private Enemy target = null;
	private GameField gameField = null;
	
	// EVENTS
	void Start() {
		muzzleFlash.enabled = false;
		muzzleLight.enabled = false;
		
		if (Network.isServer) {
			// Get Gamefield through following relation: GameField -> Tile -> Tower
			gameField = transform.parent.parent.gameObject.GetComponent<GameField>();
			if (gameField == null) {
				Debug.Log ("Tower: error resolving gamefield");
			}
		}
		if (Network.isClient) {
			StartCoroutine(BuildEffect());
		}
	}
	void Update() {
		// Server checks for nearest target and stores this on client and server
		// Then the server spawns the projectile and informs the client via ClientFire.
		if (Network.isServer) {
			// Check for new target, if found, inform client
			// TODO: reduce interval of scanning for enemies,
			// every frame seems a bit too much.
			if (target == null) {
				target = FindClosestTarget();
				if (target != null) {
					networkView.RPC ("SetTarget", RPCMode.Others, target.networkView.viewID);
				}
			}
			// Handle firing logic based on interval settings
			if (target != null) {
				timeLeft -= Time.deltaTime;
				if (timeLeft <= 0.0f) {
					// Target is within range, start firing
					if (Vector3.Distance (transform.position, target.transform.position) <= range) {
						Fire();
						timeLeft = interval;
					}
				}
			}
		}
		// Both client and server update turret rotation
		// This is also done on server, because the server spawns the projectiles
		// and needs to have an updates located of the turret to use as starting
		// location for those projectiles.
		if (target == null) {
			return; // Wait for server to find a target
		}
		if (Vector3.Distance(transform.position, target.transform.position) <= rangeTurn) {
			// Determine the rotation, only use the yaw-component.
			Quaternion targetRotation = Quaternion.LookRotation(transform.position - target.transform.position);
			targetRotation.x = 0;
			targetRotation.z = 0;
			Debug.DrawLine(transform.position,target.transform.position);
			// Smoothly rotate towards the target point.
			rotator.transform.rotation = Quaternion.Slerp(rotator.transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
		}
	}
	void OnMouseDown() {
		if (Network.isClient) {
			// Ask the server to sell this tower
			// TODO: make upgrade/sell menu
			Player.GetNetworkView().RPC("SellTower", RPCMode.Server, tileId);
		}
	}
	
	
	// FUNCTIONS
	void Fire() {
		// TODO: spawning from muzzleflash position doesnt seem to work yet
		Projectile p = (Projectile)Network.Instantiate(bulletPrefab, muzzleFlash.gameObject.transform.position, Quaternion.identity, 0);
		p.damage = bulletDamage;
		// Set destination on server projectile
		p.destination = target.transform;
		// Set destination on client projectile
		// TODO: the client tower also knows the current target,
		// can we somehow optimize setting this target on the projectile?
		p.networkView.RPC ("SetTarget", RPCMode.Others, target.networkView.viewID);
		// Inform clients that we are shooting, so they can display effects
		networkView.RPC("ClientFire", RPCMode.Others);
	}
	
	
	// SERVER -> CLIENT RPC
	[RPC]
	void SetTarget(NetworkViewID id) {
		NetworkView view = NetworkView.Find(id);
		if (view == null) {
			Debug.Log ("Couldn't find tower target with view ID: " + view);
			return;
		}
		target = view.gameObject.GetComponent<Enemy>();
	}
	[RPC]
	void ClientFire() {
		audio.PlayOneShot(firesound, 0.4F);
		StartCoroutine(FireEffect());
	}
	[RPC]
	void SetTileId(int id) {
		tileId = id;
	}
	
	
	// HELPER FUNCTIONS
	private Enemy FindClosestTarget() {
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
		for (float f = 0.0f; f < 1.0f; f += 0.03f) {
			foreach (MeshRenderer r in GetComponentsInChildren(typeof(MeshRenderer))) {
				if(r.material.HasProperty("_Color")){
					Color c = r.material.color;
					c.a = f;
					r.material.color = c;
				}
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
