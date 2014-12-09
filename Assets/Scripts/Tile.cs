using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

	// Settings
	public Tower towerPrefab = null;
	
	// Privates
	private Tower tower = null;
	private bool isBuild = false;


	// EVENTS
	void OnMouseDown() {
		if (!isBuild) {
			tower = (Tower)Instantiate (towerPrefab, transform.position, Quaternion.identity);
			
			// Trigger A* pathfinding route update
			GameObject graphUpdater = GameObject.Find ("GraphUpdater");
			graphUpdater.GetComponent<Pathfinding.GraphUpdateScene>().Apply(); 
			// Iterate through all units and update their current route
			Enemy[] enemies = (Enemy[])FindObjectsOfType(typeof(Enemy));
			for (int i = 0; i < enemies.Length; ++i) {
				enemies[i].SetDestination(enemies[i].vDestination);
			}
		}
		else {
			Destroy(tower.gameObject);
		}
		isBuild = !isBuild;
	}
}
