using UnityEngine;
using System.Collections;

public class GameField : MonoBehaviour {

	public float interval = 3.0f;
	float timeLeft = 0.0f;

	public Enemy enemyPrefab = null;
	
	private Transform spawnPoint;
	private Transform endPoint;
	
	void Start () {
		spawnPoint = transform.FindChild("SpawnPoint");
		endPoint = transform.FindChild("EndPoint");
	}
	
	void Update () {
		timeLeft -= Time.deltaTime;
		if (timeLeft <= 0.0f) {
			Enemy enemy = (Enemy)Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
			enemy.SetDestination(endPoint.position);			
			timeLeft = interval;
		}
		
		// XXX: Is this efficient? Better let objects check themselves if they collide with endPoint.
		Enemy[] enemies = (Enemy[])FindObjectsOfType(typeof(Enemy));
		float range = endPoint.collider.bounds.size.x / 4;
		for (int i = 0; i < enemies.Length; ++i) {
			if (Vector3.Distance(endPoint.position, enemies[i].transform.position) <= range) {			
				Destroy(enemies[i].gameObject);
			}
		}
	}
	
	public Vector3 GetEndpoint() {
		return endPoint.position;
	}
}
