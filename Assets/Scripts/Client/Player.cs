using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(NetworkView))]
public class Player : MonoBehaviour {

	// That's actually not the owner but the player,
	// the server instantiated the prefab for, where this script is attached
	[HideInInspector]
	public NetworkPlayer owner;
	
	// GUI to be used
	[SerializeField]
	GameObject playerCanvasPrefab;
	
	// Properties of a player.
	public int life;
	public int gold;

	// References to server instances
	private PlayerManager playerManager;
	public GameField gameField;


	// EVENTS
	void Awake() {
		// Disable this by default for now. Just to make sure no one can use 
		// this until we didn't find the right player. (see setOwner())
		if (Network.isClient) {
			enabled = false;
		}
		else if(Network.isServer){
			playerManager = (PlayerManager)GameObject.Find("Server").GetComponent<PlayerManager>();
		}
	}


	// Called from server on clients
	[RPC]
	void SetOwner(NetworkPlayer p) {
		owner = p;
		if(p == Network.player){
			// So it just so happens that WE are the player in question,
			// which means we can enable this control again
			enabled = true;
			
			// Create Canvas and assign events to buttons
			// TODO: add gold en money to canvas. Maybe move this to a seperate function to keep it organized.
			GameObject canvas = (GameObject)Instantiate(playerCanvasPrefab);
			Button btnSpawnCreep = canvas.transform.FindChild("SpawnCreepButton").GetComponent<Button>();
			btnSpawnCreep.onClick.AddListener(() => { networkView.RPC("SpawnEnemy", RPCMode.Server); });

		}
		else {
			// Disable a bunch of other things here that are not interesting
			if (GetComponent<Camera>() != null) {
				GetComponent<Camera>().enabled = false;
			}
			
			if (GetComponent<AudioListener>() != null) {
				GetComponent<AudioListener>().enabled = false;
			}
			
			if (GetComponent<GUILayer>() != null) {
				GetComponent<GUILayer>().enabled = false;
			}
		}
	}
	[RPC]
	void SetCameraTarget(Vector3 v) {
		if (Network.isServer) {
			// Create camera for the client that is positioned
			// at a birdseye view of this gamefield.
			v.y += 6;
			v.x += 4;
			camera.transform.position = v;
			camera.transform.Rotate(45, 0, 0);
		}
	}
	[RPC]
	void SetLife(int amount) {
		life = amount;
	}
	[RPC]
	void SetGold(int amount) {
		gold = amount;
	}
	
	
	// Called from client on server
	[RPC]
	void SpawnEnemy() {
		if (Network.isServer) {
			// Check if player has enough money to buy this unit
			// TODO: support for multiple unit types
			Enemy enemyPrefab = Resources.Load<Enemy>("EnemyPrefab");
			int price = enemyPrefab.price;
			if (gold < price) {
				return;
			}
			gold -= price;
			networkView.RPC ("SetGold", RPCMode.Others, gold);
			
			GameField nextGameField = playerManager.GetNextGamefield(this);
			if(nextGameField != null){
				// Give the function a reference to this player object on the server,
				// so the gamefield can give the spawned enemy an owner.
				nextGameField.SpawnEnemy(this);
			}
			else{
				Debug.Log("ERROR: No next player");
			}
		}
	}
	[RPC]
	void BuildTower(int id, string towerType, NetworkMessageInfo senderInfo) {
		if (Network.isClient) {
			return;		
		}
		
		// Check that this player actually owns the gamefield
		PlayerManager.PlayerInfo info = PlayerManager.GetPlayerInfo(senderInfo.sender);
		if (info == null) {
			Debug.Log ("Error resolving playerinfo!");
			return;
		}
		if (info.gameField != gameField) {
			Debug.Log ("This player does not own the gamefield!");
			return;
		}
		
		Tile[] tiles = gameField.GetComponentsInChildren<Tile>();
		foreach(Tile tile in tiles) {
			if (tile.id == id) {
				// TODO: check if tile is available (should be disabled otherwise)

				// Load tower prefab
				Tower towerPrefab = Resources.Load<Tower>(towerType);
				
				// Check if player has enough money to buy tower
				int price = towerPrefab.buildPrice;
				if (gold < price) {
					return;
				}
				gold -= price;
				networkView.RPC ("SetGold", RPCMode.Others, gold);
				
				// Instantiate the tower on all clients
				Tower tower = (Tower)Network.Instantiate(towerPrefab, tile.transform.position, Quaternion.identity, 0);
				// Make tower gameobject child of the tile it's built on
				tower.transform.parent = tile.transform;
				tower.tileId = id;
				tower.networkView.RPC ("SetTileId", RPCMode.Others, id);
				
				// Update pathfinding to include the tower we just made
				UpdatePathfinding();
			}
		}		
	}
	[RPC]
	void SellTower(int tileId, NetworkMessageInfo senderInfo) {
		if (Network.isClient) {
			return;
		}
		
		// Check that this player actually owns the gamefield
		PlayerManager.PlayerInfo info = PlayerManager.GetPlayerInfo(senderInfo.sender);
		if (info == null) {
			Debug.Log ("Error resolving playerinfo!");
			return;
		}
		if (info.gameField != gameField) {
			Debug.Log ("This player does not own the gamefield!");
			return;
		}

		// Find Tile
		Tile[] tiles = gameField.GetComponentsInChildren<Tile>();
		foreach(Tile tile in tiles) {
			if (tile.id == tileId) {
				if (tile.transform.childCount > 0){
					Tower tower = tile.transform.GetChild(0).gameObject.GetComponent<Tower>();
					
					// Give the player money back
					gold += tower.sellPrice;
					networkView.RPC ("SetGold", RPCMode.Others, gold);
					
					// Now remove the tower on the server and all clients
					Network.Destroy(tower.gameObject);
					
					// Update pathfinding to include the tower we just removed
					UpdatePathfinding();
				}
			}
		}
	}
	
	
	// HELPER FUNCTIONS
	private void UpdatePathfinding() {
		GameObject graphUpdater = gameField.transform.Find("GraphUpdater").gameObject;
		graphUpdater.GetComponent<Pathfinding.GraphUpdateScene>().Apply(); 
		// Iterate through all units and update their current route
		foreach (Enemy enemy in gameField.enemyList) {
			enemy.SetDestination(enemy.vDestination);
		}
	}
	// Used to get the NetworkView for the current player.
	// ONLY works when called on CLIENT.
	public static NetworkView GetNetworkView() {
		if (Network.isClient) {
			// Return the network view of the player with an active camera,
			// which is only one: the active player on the calling client.
			Player[] playerList = (Player[])FindObjectsOfType(typeof(Player));
			foreach (Player p in playerList) {
				if (p.GetComponent<Camera>().enabled == true) {
					return p.GetComponent<NetworkView>();
				}
			}
			return null;
		}
		else {
			Debug.Log("GetNetworkView() only supported on client");
			return null;
		}
	}
	
	void OnGUI() {
		if (Network.isServer) {
			return; // Get lost, this is the client side
		}
			
		GUI.Label(new Rect(0, 0, 400, 200), "Life: " + life);	
		GUI.Label(new Rect(0, 10, 400, 200), "Gold: " + gold);
	}
}
