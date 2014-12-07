using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

	public Tower towerPrefab = null;
	private Tower tower = null;
	private bool isBuild = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnMouseDown() {
		if (!isBuild) {
			tower = (Tower)Instantiate (towerPrefab, transform.position, Quaternion.identity);
			
			GameObject graphUpdater = GameObject.Find ("GraphUpdater");
			graphUpdater.GetComponent<Pathfinding.GraphUpdateScene>().Apply(); 

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
