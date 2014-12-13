using UnityEngine;
using System.Collections;
using Pathfinding;

public class GameField : MonoBehaviour {
	// TODO: use playermanager to resolve player
	public Player player;

	// Settings
	public Enemy enemyPrefab = null;
	public float interval = 3.0f;

	// Privates
	private float timeLeft = 0.0f;
	
	private Transform spawnPoint;
	private Transform endPoint;
	
	
	// EVENTS
	void Start () {
		spawnPoint = transform.FindChild("SpawnPoint");
		endPoint = transform.FindChild("EndPoint");
		
		
		float startX = 0f;
		float startZ = 0f;
		float endX = 0f;
		float endZ = 0f;
		float radiusX = 1f;
		float radiusZ = 1f;
		
		// Give the Tiles there own identifier
		Tile[] tiles = gameObject.GetComponentsInChildren<Tile>();
		for(int i = 0; i < tiles.Length; i++) {
			tiles[i].SetId(i);
			
			
			
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
		Debug.Log("startX: "+startX);
		Debug.Log("endX: "+endX);
		Debug.Log("startZ: "+startZ);
		Debug.Log("endZ: "+endZ);
		
		// This holds all graph data
		AstarData data = AstarPath.active.astarData;
		// This creates a Grid Graph
		GridGraph gg = data.AddGraph(typeof(GridGraph)) as GridGraph;
		// Setup a grid graph with some values
		gg.width = 16;
		gg.depth = 32;
		gg.nodeSize = 0.5f;
		gg.cutCorners = false;
		gg.center = new Vector3 ((endX - startX) / 2 + transform.position.x,-1, (endZ - startZ) / 2 + transform.position.z);
		// Updates internal size from the above values
		gg.UpdateSizeFromWidthDepth();
		// Scans all graphs, do not call gg.Scan(), that is an internal method
		AstarPath.active.Scan();
		
	}
	void Update () {
		if (Network.isClient) {
			return;
		}
	
		timeLeft -= Time.deltaTime;
		if (timeLeft <= 0.0f) {
			//SpawnEnemy();
			timeLeft = interval;
		}
		
		// XXX: Is this efficient? Better let objects check themselves if they collide with endPoint.
		Enemy[] enemies = (Enemy[])FindObjectsOfType(typeof(Enemy));
		float range = endPoint.collider.bounds.size.x / 4;
		for (int i = 0; i < enemies.Length; ++i) {
			if (Vector3.Distance(endPoint.position, enemies[i].transform.position) <= range) {			
				Network.Destroy(enemies[i].gameObject.networkView.viewID);
				
				player.life -= 1;
				player.networkView.RPC("SetLife", RPCMode.All, player.life);
			}
		}
	}
	public void SpawnEnemy() {
		Enemy enemy = (Enemy)Network.Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity, 0);
		enemy.SetDestination(endPoint.position);	
	}
		
	
	// FUNCTIONS
	public Vector3 GetEndpoint() {
		return endPoint.position;
	}
	
	
	// Called from client on server
	[RPC]
	void BuildTower(int id, string towerType) {
		if (Network.isClient) { // TODO: is this required?
			return;		
		}
		
		Tile[] tiles = gameObject.GetComponentsInChildren<Tile>();
		foreach(Tile tile in tiles) {
			if (tile.GetId() == id) {
				// TODO: check if tile is available?? (should be disabled otherwise)
				
				
				// Load tower prefab
				Tower towerPrefab = Resources.Load<Tower>(towerType);
				
				// Check if player has enough money to buy tower
				int price = towerPrefab.buildPrice;
				if (player.gold < price) {
					return;
				}
				player.gold -= price;
				player.networkView.RPC ("SetGold", RPCMode.All, player.gold);

				// Instantiate the tower on all clients
				Tower t = (Tower)Network.Instantiate(towerPrefab, tile.transform.position, tile.transform.rotation, 0);
				tile.enabled = false;
				
				// Update pathfinding to include the tower we just made
				// TODO: should be done on all clients!
				GameObject graphUpdater = GameObject.Find ("GraphUpdater");
				graphUpdater.GetComponent<Pathfinding.GraphUpdateScene>().Apply(); 
				// Iterate through all units and update their current route
				Enemy[] enemies = (Enemy[])FindObjectsOfType(typeof(Enemy));
				foreach (Enemy enemy in enemies) {
					enemy.SetDestination(enemy.vDestination);
				}
				/* // Trigger A* pathfinding route update
			GameObject graphUpdater = GameObject.Find ("GraphUpdater");
			graphUpdater.GetComponent<Pathfinding.GraphUpdateScene>().Apply(); 
			// Iterate through all units and update their current route
			Enemy[] enemies = (Enemy[])FindObjectsOfType(typeof(Enemy));
			// TODO: this is ugly code, please fix
			for (int i = 0; i < enemies.Length; ++i) {
				enemies[i].SetDestination(enemies[i].vDestination);
			}*/
			}
		}		
	}
}





















