using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	private string sName;

	private int iGold;
	private int iLife;

	private bool bIsObserver;

	// Use this for initialization
	void Start () {
		bIsObserver = false;
	}
	void OnDestroy () {
		Debug.Log ("Player destroyed");
	}
	// Update is called once per frame
	void Update () {
	
	}
}
