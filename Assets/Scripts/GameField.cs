using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

[RequireComponent(typeof(NetworkView))]
public class GameField : MonoBehaviour {

	// Privates
	private float interval = 0.5f;
	private float timeLeft = 0.0f;
	
	// Settings
	public Enemy enemyPrefab = null;
	public GameObject[] spawnPoints;
	public Transform endPoint;
	
	// List with enemies currently on this gamefield
	[HideInInspector]
	public List<Enemy> enemyList;
	
	// References
	[HideInInspector]
	public Player player = null;
	
	
	// EVENTS
	void Start () {
		// These variables hold the relative position of the
		// lower left and upper right corners of this gamefield.
		// This is used to create a GridGraph for pathfinding.
		float startX = 0f;
		float startZ = 0f;
		float endX = 0f;
		float endZ = 0f;
		float radiusX = 1f;
		float radiusZ = 1f;
		
		// Give the tiles an identifier (0-based)
		Tile[] tiles = gameObject.GetComponentsInChildren<Tile>();
		for(int i = 0; i < tiles.Length; i++) {
			tiles[i].id = i;
			
			if(i==0){
				radiusX = tiles[i].GetComponent<MeshFilter>().mesh.bounds.size.x / 2;
				radiusZ = tiles[i].GetComponent<MeshFilter>().mesh.bounds.size.z / 2;
				
				startX = tiles[i].transform.position.x - radiusX;
				startZ = tiles[i].transform.position.z - radiusZ;
				endX = tiles[i].transform.position.x + radiusX;
				endZ = tiles[i].transform.position.z + radiusZ;
			}
			else{
				if(tiles[i].transform.position.x - radiusX < startX){
					startX = tiles[i].transform.position.x - radiusX;
				}
				if(tiles[i].transform.position.x + radiusX > endX){
					endX = tiles[i].transform.position.x + radiusX;
				}
				if(tiles[i].transform.position.z - radiusZ < startZ){
					startZ = tiles[i].transform.position.z - radiusZ;
				}
				if(tiles[i].transform.position.z + radiusZ > endZ){
					endZ = tiles[i].transform.position.z + radiusZ;
				}
			}
		}
		
		if (Network.isServer) {
			enemyList = new List<Enemy>();

			// Create a GridGraph with the same dimensions as this gamefield
			// Be sure to position it BELOW the gamefield (required for A* pathfinding)
			// Only the server has the A* pathfinding gameobject			
			AstarData data = AstarPath.active.astarData;
			GridGraph gg = data.AddGraph(typeof(GridGraph)) as GridGraph;
			gg.width = 8;
			gg.depth = 16;
			gg.nodeSize = 1;
			// Below settings are for collision detection
			gg.cutCorners = false;
			gg.erodeIterations = 0;
			gg.collision.type = ColliderType.Capsule;
			gg.collision.diameter = 0;
			gg.collision.height = 2;
			gg.collision.mask = 0;
			gg.collision.mask |= (1 << LayerMask.NameToLayer("Obstacles"));
			gg.collision.heightMask = 0;
			gg.collision.heightMask |= (1 << LayerMask.NameToLayer("Ground"));
			gg.center = new Vector3 ((endX - startX) / 2 + transform.position.x,-1, (endZ - startZ) / 2 + transform.position.z);
			// Updates internal size from the above values
			gg.UpdateSizeFromWidthDepth();
			// Scans all graphs, do not call gg.Scan(), that is an internal method
			AstarPath.active.Scan();
		}
	}
	void Update () {
		if (Network.isClient) {
			return;
		}
		
		timeLeft -= Time.deltaTime;
		if (timeLeft <= 0.0f) {
			// XXX: Is this efficient? Better let objects check themselves if they collide with endPoint.
			float range = endPoint.collider.bounds.size.x / 4;
			// Make a copy of the enemy list, because we remove elements from it while looping
			// TODO: can this be done more efficiently?
			List<Enemy> tempEnemyList = new List<Enemy>(enemyList);
			foreach (Enemy enemy in tempEnemyList) {
				if (Vector3.Distance(endPoint.position, enemy.transform.position) <= range) {		
					// This player has "leaked", so remove a life
					// TODO: combine this in a single function which updates life on server + all clients
					player.life -= 1;
					player.networkView.RPC("SetLife", RPCMode.Others, player.life);
					
					// Add a life for the player that owned the enemy
					enemy.owner.life += 1;
					enemy.owner.networkView.RPC("SetLife", RPCMode.Others, enemy.owner.life);
					
					// Remove from server and all clients
					RemoveEnemy(enemy);
				}
			}
			timeLeft = interval;
		}
	}
	
	
	// FUNCTIONS
	public void SpawnEnemy(Player originator) {
		// Spawn enemy at random spawnpoint
		Enemy enemy = (Enemy)Network.Instantiate(enemyPrefab, spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position, Quaternion.identity, 0);
		// Update values on server
		enemy.owner = originator;
		enemy.gameField = this;
		enemyList.Add(enemy);
		// Set endpoint for A* pathfinding of this enemy
		enemy.SetDestination(endPoint.position);
	}
	public void RemoveEnemy(Enemy enemy) {
		lock (enemyList) {
			enemyList.Remove(enemy);
			Network.Destroy(enemy.gameObject.networkView.viewID);
		}
	}
}





















