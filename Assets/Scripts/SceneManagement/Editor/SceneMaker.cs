using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using QuadTreeLib;

public class SceneMaker  {

	[MenuItem("SceneManagement/Generate Quad Tree",false,1)]
	public static void MakeQuadTree(){

		var sceneManagment = GameObject.FindObjectOfType<SceneManagement> ();

		if (sceneManagment == null) {
			var go = new GameObject ("SceneManagment");
			sceneManagment = go.AddComponent<SceneManagement> ();
		}
		sceneManagment.CalculateBounds ();
		sceneManagment.Build ();  
		 
	}	 
	 
	 
}
