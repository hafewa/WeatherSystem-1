using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using QuadTreeLib;

public class SceneManagement : MonoBehaviour {

	public bool onlyStatic = true;

	public ManagedWeatherSystem[] weatherSystems;

	public SkySystem skySystem;

	public Rect bounds;

	public bool drawTiles = false;

	public bool drawBounds = true;
	  
	private QuadTree<Item> sceneTree;

	private QuadTree<ManagedWeatherSystem> weatherTree;

	private ManagedWeatherSystem mainWeatherSystem; 

	void Start () { 
		Debug.Log ("SceneManagement Init ");
		skySystem = new SkySystem ();
		weatherSystems = GameObject.FindObjectsOfType<ManagedWeatherSystem> ();
		for (int i = 0; i < weatherSystems.Length; ++i) {
			weatherSystems[i].weatherIndex = i;  
		} 
		Build (); 

		ApplyWeathers ();
	}
	 
	void Update () { 
		UpdateMainWeather ();

	}

	void LateUpdate(){
		ManagedWeatherSystem.UpdateWeatherVaribles (false);
	}

	public void ClearObjects(){
		sceneTree = new QuadTree<Item> (new RectangleF(bounds));
	}

	public void AddObject(MeshRenderer obj,bool addWeatherComponent=true){
		if (addWeatherComponent && obj.GetComponent<WeatherComponent> () == null)
			obj.gameObject.AddComponent<WeatherComponent> ();
		sceneTree.Insert (new Item(obj));
	}

	public void RemoveObject(MeshRenderer obj){
		
	}

	private void CalculateBounds(MeshRenderer[] nodes){

		float xMin, yMin, xMax, yMax;
		xMin = float.MaxValue;
		yMin = float.MaxValue;
		xMax = float.MinValue;
		yMax = float.MinValue;
		foreach (var node in nodes) {    
			var nodeBounds = node.bounds;
			xMin = Mathf.Min (xMin,nodeBounds.min.x);
			yMin = Mathf.Min (yMin,nodeBounds.min.z);
			xMax = Mathf.Max (xMax,nodeBounds.max.x);
			yMax = Mathf.Max (yMax,nodeBounds.max.z); 
		}

		Vector2 center = new Vector2 ((xMax+xMin)/2,(yMax+yMin)/2);
		Vector2 size = new Vector2 (xMax-xMin,yMax-yMin);
		if (size.x > 2 * size.y) {
			size.y = size.x;
		}else if (size.y > 2 * size.x) {
			size.x = size.y;
		}
		bounds.center = center;
		bounds.size = size + Vector2.one;

		Debug.LogFormat ("bounds {0} {1};count {2}",bounds.min,bounds.size,nodes.Length);  
	}

	public void CalculateBounds (){
		var nodes = GameObject.FindObjectsOfType<MeshRenderer> (); 
		CalculateBounds (nodes);
	}

	public void Build(bool addWeatherComponent = true){
		var nodes = GameObject.FindObjectsOfType<MeshRenderer> (); 

		sceneTree = new QuadTree<Item> (new RectangleF(bounds));

		foreach (var node in nodes) {  
			#if UNITY_EDITOR
				if ((!onlyStatic || node.gameObject.isStatic)) {  
					AddObject (node,addWeatherComponent);
				}
			#else
				if (node.GetComponent<WeatherComponent> ()!=null) {  
					AddObject (node,addWeatherComponent);
				}
			#endif

		}

		Debug.LogFormat ("treecount {0} ;nodecount {1}",sceneTree.Count,nodes.Length);  
	} 

	public List<Item> Query(RectangleF queryArea){
		return sceneTree.Query (queryArea);
	}
	 

	public void UpdateMainWeather(){ 

		var pos = Camera.main.transform.position;
		var dir = Camera.main.transform.forward * 1000;
		ManagedWeatherSystem mainWeather = mainWeatherSystem == null?weatherSystems[0]:mainWeatherSystem;
		float minDistance = float.MaxValue;
		ManagedWeatherSystem weatherSystem;
		float distance = float.MaxValue;
		Vector3 interectPoint;

		for (int i = 0; i < weatherSystems.Length; ++i) {
			weatherSystem = weatherSystems [i];
			if (weatherSystem != null && weatherSystem.gameObject.activeInHierarchy) { 
				if (MathUtility.GetInterectPoint (weatherSystem.influenceArea, pos, dir, out interectPoint)) {
					distance = (pos - interectPoint).magnitude;
				}
				if (distance < minDistance) {
					if (mainWeather != null && mainWeather != weatherSystem) {
						mainWeather.isMainWeather = false;
					}
					minDistance = distance;
					mainWeather = weatherSystem;
				} else {
					weatherSystem.isMainWeather = false;
				}
			}
		}
		if (mainWeather != null) {
			mainWeather.isMainWeather = true;
		}
		mainWeatherSystem = mainWeather;
		skySystem.Update (mainWeatherSystem);
	}

	public void ApplyWeathers(){ 
		
		foreach (var weatherSystem in weatherSystems) {
			var results = Query (new RectangleF(weatherSystem.influenceArea)); 
			foreach (var result in results) {
				weatherSystem.AddInfluenceObject(result.value.GetComponent<WeatherComponent>());
			}
			weatherSystem.StartWeather ();
		}
	}


	void OnDrawGizmos(){
		if (sceneTree == null)
			return;
		 
		DrawGizmosNode ("root",sceneTree.root);

	}

	void DrawGizmosNode(string name,QuadTreeNode<Item> node){

		if (drawTiles) {
			Gizmos.color = Color.green;
			Gizmos.DrawWireCube (new Vector3(node.Bounds.Center.x,0,node.Bounds.Center.y), new Vector3(node.Bounds.Width,100,node.Bounds.Height));
		}

		if (drawBounds) {
			Gizmos.color = Color.blue;
			foreach (var item in node.Contents) {
				//Gizmos.DrawWireCube (new Vector3(item.Rectangle.Center.x,0,item.Rectangle.Center.y), new Vector3(item.Rectangle.Width,100,item.Rectangle.Height));
				Gizmos.DrawWireCube (item.value.bounds.center,item.value.bounds.size);
			}
		}

		int i = 0;
		foreach (var childNode in node.children) {
			DrawGizmosNode ("child-"+i,childNode);
			++i;
		}
	}
	 
}
