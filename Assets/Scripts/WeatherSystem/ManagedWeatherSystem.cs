using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ManagedWeatherSystem : WeatherSystem,QuadTreeLib.IHasRect {

	protected int _weatherIndex = -1; 
	public List<WeatherComponent> _influenceObjects = new List<WeatherComponent>();

	public void StartWeather(){
		InitGlobalShaders ();
		weather.Start (this);
	}

	protected override void OnAwake ()
	{
		 
	}

	protected override void OnStart ()
	{
		 
	}

	protected override void OnGUI(){
		if (isMainWeather) {
			base.OnGUI ();
		}
	}

	void OnDrawGizmos(){
		if (isMainWeather) {

			Gizmos.color = Color.yellow;
			Gizmos.DrawRay (weatherCamera.transform.position, weatherCamera.transform.forward * particleOffset.z); 

			float y = Mathf.Tan (Mathf.Deg2Rad * weatherCamera.fieldOfView / 2);
			float x = weatherCamera.aspect * y;
			float degree = Mathf.Atan (x) * Mathf.Rad2Deg; 

			var forward1 = Quaternion.LookRotation(weatherCamera.transform.forward) * Quaternion.Euler (0, degree, 0) * Vector3.forward; 
			var forward2 = Quaternion.LookRotation(weatherCamera.transform.forward) * Quaternion.Euler (0, -degree, 0) * Vector3.forward; 

			Gizmos.DrawRay (weatherCamera.transform.position, forward1 * particleOffset.z); 
			Gizmos.DrawRay (weatherCamera.transform.position, forward2 * particleOffset.z); 

			Gizmos.color = Color.blue;
			Gizmos.DrawLine (weatherCamera.transform.position, _edgePoint);

			#if UNITY_EDITOR
			Gizmos.color = Color.green;
			foreach (var item in this._influenceObjects) { 
				Gizmos.DrawWireCube (item.bounds.center,item.bounds.size);
			}
			#endif

			Gizmos.color = Color.red;
		} else {
			Gizmos.color = Color.cyan;
		} 
		Gizmos.DrawWireCube (transform.position, new Vector3 (influenceRange.x, 100, influenceRange.y));
	}

	public void AddInfluenceObject(WeatherComponent obj){
		_influenceObjects.Add (obj);
		EnableWeather (obj,true);
	}

	public void RemoveInfluenceObject(WeatherComponent obj){
		_influenceObjects.Remove (obj);
		EnableWeather (obj,false);
	}

	public void ResetInfluenceObjects(){
		foreach (var influenceObject in _influenceObjects) {
			EnableWeather (influenceObject,false);
		}
		_influenceObjects.Clear (); 
	}

	public void ApplyInfluenceObjects(){		
		foreach (var influenceObject in _influenceObjects) {
			EnableWeather (influenceObject,true);
		} 
	}


	void EnableWeather(WeatherComponent node,bool enabled){
		if (enabled) {
			node.EnableWeatherSystem (this);
		} else {
			node.DisableWeatherSystem (this);
		}
	}

	private static bool globalWeatherVariblesDirty = true; 

	private static Vector4[] globalWeatherVaribles = new Vector4[4];
	private static Vector4[] globalWeatherRanges = new Vector4[4];

	public static void UpdateWeatherVaribles(bool force){
		if (force || globalWeatherVariblesDirty) {
			//Shader.SetGlobalVectorArray ("_Weathers", globalWeatherVaribles); 
			for (int i = 0; i < globalWeatherVaribles.Length; ++i) {
				Shader.SetGlobalVector ("_Weathers"+i,globalWeatherVaribles[i]);
			}
			globalWeatherVariblesDirty = false;
		}	
	}

	public static void UpdateWeatherRanges(){ 
		//Shader.SetGlobalVectorArray ("_WeatherRanges", globalWeatherRanges);
		for (int i = 0; i < globalWeatherRanges.Length; ++i) {
			Shader.SetGlobalVector ("_WeatherRanges"+i,globalWeatherRanges[i]);
		}
	}

	public override void SetWeatherAmount(float value){
		if (weatherIndex < 0) {
			Debug.LogErrorFormat ("SetWeatherAmount {0} for {1} with index={2} error", value,name,weatherIndex);
			return;
		}
		globalWeatherVaribles [weatherIndex].x =value;
		globalWeatherVariblesDirty = true;
		//UpdateWeatherVaribles ();
	}

	public override void SetWeatherTemperature(float value){
		if (weatherIndex < 0) {
			Debug.LogErrorFormat ("SetWeatherTemperature {0} for {1} with index={2} error", value,name,weatherIndex);
			return;
		}
		globalWeatherVaribles [weatherIndex].y =value;
		globalWeatherVariblesDirty = true;
		//UpdateWeatherVaribles ();
	}

	public override void SetWeatherBolting(float value){
		if (weatherIndex < 0) {
			Debug.LogErrorFormat ("SetWeatherBolting {0} for {1} with index={2} error", value,name,weatherIndex);
			return;
		}
		globalWeatherVaribles [weatherIndex].w =value;
		globalWeatherVariblesDirty = true;
		//UpdateWeatherVaribles ();
	}

	public override void SetWeatherRange(Rect range){	
		if (weatherIndex < 0) {
			Debug.LogErrorFormat ("SetWeatherRange for {1} with index={2} error", range,name,weatherIndex);
			return;
		}	
		globalWeatherRanges [weatherIndex] = new Vector4(range.xMin,range.yMin,range.xMax,range.yMax);
		UpdateWeatherRanges ();
		Debug.LogFormat ("SetWeatherRange {0} box:{1}",weatherIndex,globalWeatherRanges [weatherIndex]);
	}

	public override PointLocation GetCameraLocation(out float factor){
		var pos = weatherCamera.transform.position;
		var box = influenceArea;
		return MathUtility.GetLocation (box, pos, weatherCamera.transform.forward,particleOffset.z, out factor);
	}


	public int weatherIndex{
		set{ 			
			_weatherIndex = value;
			//InitGlobalShaders ();
		}
		get { 
			return _weatherIndex;
		}
	}

	public RectangleF Rectangle {
		get {
			return new RectangleF(this.influenceArea);
		}
	}
}
