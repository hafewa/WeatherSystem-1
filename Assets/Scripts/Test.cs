using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Test : MonoBehaviour {

	public int nextScene = 1;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnGUI(){
		if(GUI.Button(new Rect(100,200,100,100),"Change Scene")){
			ChangeScene ();
		}
	}

	void ChangeScene(){ 
		SceneManager.LoadScene (nextScene);
	}
}
