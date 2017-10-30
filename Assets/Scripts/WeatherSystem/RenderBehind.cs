using UnityEngine;
using System.Collections;

public class RenderBehind : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GetComponent<Renderer> ().material.renderQueue = 2900;
	}
	 
}
