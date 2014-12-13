using UnityEngine;
using System.Collections;

public class Main : MonoBehaviour {

	// Settings
	public float widthPercent = 0.3f;
	public float heightPercent = 0.3f;

	void OnStart() {
		Screen.SetResolution(360, 640, false);
	}

	// TODO: remove this deprecated GUI, use Canvas object instead
	void OnGUI() {
		Rect r = new Rect(Screen.width * (1 - widthPercent) / 2,
		                  Screen.height * (1 - heightPercent) / 2,
		                  Screen.width * widthPercent,
		                  Screen.height * heightPercent);
		
		// Draw menu
		GUILayout.BeginArea(r);
		GUILayout.BeginVertical("box");
		
		if (GUILayout.Button ("Server"))
		{
			Application.LoadLevel("Scene_Server");
		}
		if (GUILayout.Button ("Client"))
		{
			Application.LoadLevel("Scene_Client");
		}
		
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}
}
