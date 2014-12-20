using UnityEngine;
using System.Collections;

//Note this line, if it is left out, the script won't know that the class 'Path' exists and it will throw compiler errors
//This line should always be present at the top of scripts which use pathfinding
using Pathfinding;

public class AstarAI : MonoBehaviour {
	//The point to move to
	public Vector3 targetPosition;
	
	private Seeker seeker;
	private CharacterController controller;
	public Transform body;
	public Transform compass;
	
	//The calculated path
	public Path path;
	
	//The AI's speed per second
	public float speed = 100;
	public float turnSpeed = 6.0f;
	
	//The max distance from the AI to a waypoint for it to continue to the next waypoint
	public float nextWaypointDistance = 1f;
	
	//The waypoint we are currently moving towards
	private int currentWaypoint = 0;
	

	public void SetDestination(Vector3 destination) {
		seeker = GetComponent<Seeker>();
		controller = GetComponent<CharacterController>();
				
		//Start a new path to the targetPosition, return the result to the OnPathComplete function
		targetPosition = destination;
		seeker.StartPath (transform.position, targetPosition, OnPathComplete);
	}
	
	public void OnPathComplete (Path p) {
		if (!p.error) {
			path = p;
			currentWaypoint = 0;
		}
	}
	
	public void FixedUpdate () {
		if (path == null) {
			return;
		}
		
		if (currentWaypoint >= path.vectorPath.Count) {
			return;
		}
		
		//Direction to the next waypoint
		Vector3 dir = (path.vectorPath[currentWaypoint]-transform.position).normalized;
		dir *= speed * Time.fixedDeltaTime;
		controller.SimpleMove (dir);

		//Rotate front to the next waypoint
		Vector3 point = path.vectorPath[currentWaypoint];
		point.y = compass.position.y;
		compass.LookAt(point);
		// We need to update transform.rotation, because this is what is synced to clients.
		transform.rotation = Quaternion.Lerp(transform.rotation, compass.rotation, Time.deltaTime * turnSpeed);
		
		// Check if we are close enough to the next waypoint
		// If we are, proceed to follow the next waypoint
		// Removed the y axis for more precise smooting (we don't use the y-axis in pathfinding anyway) 
		Vector3 distance = transform.position - path.vectorPath[currentWaypoint];
		distance.y = 0;
		if (distance.magnitude < nextWaypointDistance) {
			currentWaypoint++;
		}
	}
} 