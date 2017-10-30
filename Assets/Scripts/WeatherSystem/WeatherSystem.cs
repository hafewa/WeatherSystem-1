using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum WeatherType{
	Clear_Blue = 1,
	Clear ,
	Cloudy , 
	Heavy_Cloudy ,
	Rain ,
	Heavy_Rain,
	Snow ,
	Heavy_Snow,
	Foggy 
}

public enum WeatherState{
	IDLE = 0,
	STARTING = 1,
	RUNNING ,
	STOPPING,
	OVER,
}

public enum WorldDirection{
	X_POSITIVE = 0,
	Z_POSITIVE = 1,
	X_NEGATIVE = 2,
	Z_NEGATIVE = 3,
}


public enum WeatherQuantity{
	FULL_QUANTITY,
	GOOD_QUANTITY,
	FAST_QUANTITY,
}



public class WeatherSystem : MonoBehaviour {

	private static WeatherSystem _instance;

	public static WeatherSystem instance{
		get {  
			return _instance;
		}
	}

	public static void UpdateWeather(WeatherType WeatherType){
		var current = instance;
		if (current != null) {
			current.weatherType = WeatherType;
			current.UpdateWeather (true);
		}
	}

	[Header("Basic Settings")] 
	public WeatherQuantity weatherQuantity = WeatherQuantity.FULL_QUANTITY;
	public WeatherType weatherType = WeatherType.Clear_Blue;
	public float weatherEnterCrossTime = 0.3f;
	public float weatherExitCrossTime = 5.0f;
	public float temperature = 20;
	public Vector2 influenceRange = new Vector2(100,100);


	protected Weather weather; 
	protected List<Weather> activeWeathers = new List<Weather>();  
	protected WindZone windZone = null;

	[Header("Time Settings")]
	public float dayLength = 1;
	public int year = 2017;
	[Range(1,12)]
	public int month = 5;
	[Range(1,30)]
	public int days = 26;
	[Range(0,23)]
	public int hours = 10;
	[Range(0,59)]
	public int minutes = 45;

	public double startTime = 0;   
	public bool timeUpdate = true;
	public bool timeControl = false;
	public bool autoChangeWeather = false;
	public bool autoSunAngle = true;
	public bool sunAngleSyncWithMoon = false;

	[Header("Sky Settings")] 
	public MeshRenderer skyMesh; 
	public Material skyMaterial; 
	public GameObject starPrefab; 
	public GameObject moonPrefab; 
	public WorldDirection eastDirection = WorldDirection.X_POSITIVE;  
	public float moonDistance = 800;
	public float seasonAngle = 60;

	[HideInInspector]
	public GameObject sunAndMoon;
	[HideInInspector]
	public GameObject sun;
	[HideInInspector]
	public GameObject moon;
	[HideInInspector]
	public GameObject stars;

	[Header("Wind Settings")]
	public Vector3 windDir = new Vector3(0.1f,0.05f,0.2f); 
	[Range(0,2)]
	public float windPower = 0.1f;  
	public Vector3 windParticleDirection = new Vector3(0,90,0);
	public Vector3 windParticleOffset = new Vector3(20,0,5);

	[Header("Light Settings")]
	public bool useLighting = false;
	public Light lightSource; 
	public Light sunLight; 
	public Light moonLight; 
	public float maxSunIntensity = 3; 
	public Vector3 sunAngle;
	public Color eveningSunColor = Color.red; 
	public float eveningSunIntensity = 1;
	public Color nightSunColor = Color.red; 
	public float nightSunIntensity = 1;

	[Header("Weather Intensity")]
	[Range(0.1f,1.1f)]
	public float clearSkyIntensity = 1.09f;
	[Range(0.1f,1.1f)]
	public float clearSunIntensity = 1.09f;
	[Range(0.1f,1.1f)]
	public float cloudySkyIntensity = 0.5f;
	[Range(0.1f,1.1f)]
	public float cloudySunIntensity = 0.5f;
	[Range(0.1f,1.1f)]
	public float rainSkyIntensity = 1.0f;
	[Range(0.1f,1.1f)]
	public float rainSunIntensity = 0.1f;
	[Range(0.1f,1.1f)]
	public float snowSkyIntensity = 1.0f;
	[Range(0.1f,1.1f)]
	public float snowSunIntensity = 0.5f; 
	public float maxBloomIntensity = 0.4f;
	public float minBloomIntensity = 0;

	[Header("Camera Settings")] 
	public Vector3 particleOffset = new Vector3(0,5,12);

	[Header("Snow Settings")]
	public Texture snowTexture;
	public float snowStartHeight = -80;

	[Header("Rain Settings")]   
	public Texture rainBump; 
	public Color rainSpecColor = Color.white;
	[Range(0,32)]
	public float rainShininess = 16;

	[Header("Fog Settings")]
	public Color dayFogColor = new Color(1,1,1,0);
	public float dayFogStartDistance = 30;
	public float dayFogEndDistance = 500;

	public Color nightFogColor = new Color(1,1,1,0);
	public float nightFogStartDistance = 60;
	public float nightFogEndDistance = 1000;

	[Header("Weather Prefabs")]
	public bool dontShowParticles = false;
	public GameObject rain; 
	public GameObject snow; 
	public GameObject wind;  


	[Header("Sounds")]
	public bool soundMute = false;
	public AudioClip rainSound; 
	public float rainSoundVolume = 0.5f; 
	public AudioClip snowSound; 
	public float snowSoundVolume = 0.5f;  
	public AudioClip windSound; 
	public float windSoundVolume = 1;  
	public AudioClip thunderSound; 

	[Header("Debug Info")]
	private bool _isMainWeather = true;
	private bool _inited = false;
	public float _snowAmount = 0;
	public float _dayIntensity; 
	public float _nightIntensity; 
	public LightingBolt lightingBolt;
	public float _moonAngle;
	public Vector3 _edgePoint;

	private SkySystem _skySystem;
	private Camera _camera;

	private List<TimeControlObject> dayObjects = new List<TimeControlObject> ();
	private List<TimeControlObject> nightObjects = new List<TimeControlObject> ();

	void Awake(){
		Debug.Log ("WeatherSystem Init "+name);
		InitSky ();
		if (useLighting) {
			InitLights (); 
		}
		UpdateWeather (false);
		OnAwake ();
		_instance = this;
	}

	protected virtual void OnAwake(){
		InitGlobalShaders ();
		_skySystem = new SkySystem (); 
	}
	 
	void Start(){
		Debug.Log ("WeatherSystem Start "+name);
		lightingBolt = GetComponent<LightingBolt> ();
		lightingBolt.enabled = weatherType == WeatherType.Rain || weatherType == WeatherType.Heavy_Rain; 
		OnStart ();
	}

	protected virtual void OnStart(){
		weather.Start (this);
	}

	protected void Update(){ 
		if (Input.GetKeyDown (KeyCode.F12)) {
			timeControl = !timeControl;
		}
		if (timeUpdate) {
			startTime += Time.deltaTime / dayLength / 60; 
		}
		UpdateTime ();  
		foreach (var activeWeather in activeWeathers) {		
			if(activeWeather!=weather)	
				activeWeather.Update (this);
		}
		weather.Update (this);  
		if (_skySystem != null)
			_skySystem.Update (this);
	}


	protected void LateUpdate(){

		ShowNightObjects (isNight,false);
		ShowDayObjects (isDay,false);
		 
		var dirtyObjects = new List<Weather> ();

		foreach (var activeWeather in activeWeathers) {	
			if(activeWeather!=weather)	
				activeWeather.LateUpdate (this);	
			if (activeWeather.isOver) {
				dirtyObjects.Add (activeWeather);
			}
		}

		weather.LateUpdate (this); 

		foreach (var dirtyObject in dirtyObjects) {
			dirtyObject.Destroy (this);
			activeWeathers.Remove (dirtyObject);
		}		 
	}

	protected void OnDestroy(){
		foreach (var activeWeather in activeWeathers) {	
			if(activeWeather!=weather)	
				weather.Destroy (this);
		}
		weather.Destroy (this);
		activeWeathers.Clear ();
		ClearGlobalShaders ();
		if (_instance == this) {
			_instance = null;
		}
	}

	protected void InitSky(){
		var season = new GameObject ("Season");
		season.transform.parent = this.transform;
		season.transform.localScale = Vector3.one;
		season.transform.localPosition = Vector3.zero;
		season.transform.localRotation = Quaternion.Euler(0,0,0);


		if (sunAndMoon == null) {
			sunAndMoon = new GameObject ("SunAndMoon");
			sunAndMoon.transform.parent = season.transform;
			sunAndMoon.transform.localScale = Vector3.one;
			sunAndMoon.transform.localPosition = Vector3.zero;
			sunAndMoon.transform.localRotation = Quaternion.identity;
		}

		sun = new GameObject ("Sun");  
		sun.transform.parent = sunAndMoon.transform;
		sun.transform.localScale = Vector3.one;
		sun.transform.localRotation = Quaternion.LookRotation(Vector3.up);
		sun.transform.localPosition = new Vector3(0,-moonDistance,0);


		moon = GameObject.Instantiate<GameObject> (this.moonPrefab);
		moon.name = "Moon";
		moon.transform.parent = sunAndMoon.transform;
		moon.transform.localScale = Vector3.one;
		moon.transform.localRotation =  Quaternion.LookRotation(Vector3.down);
		moon.transform.localPosition = new Vector3(0,moonDistance,230);

		stars = GameObject.Instantiate<GameObject> (this.starPrefab);
		stars.name = "Stars";
		stars.transform.parent = this.transform;
		stars.transform.localScale = Vector3.one;
		stars.transform.localPosition = Vector3.zero;
		stars.transform.localRotation = Quaternion.identity;  
	}

	protected void InitLights(){ 
		if (sunLight == null) {	
			//sun.transform.position = lightSource.transform.position;
			//sun.transform.rotation = lightSource.transform.rotation;

			sunLight = sun.AddComponent<Light> ();
			sunLight.color = lightSource.color;
			sunLight.intensity = lightSource.intensity;
			sunLight.bounceIntensity = lightSource.bounceIntensity;
			sunLight.shadows = lightSource.shadows;
			sunLight.shadowStrength = lightSource.shadowStrength;
			sunLight.type = lightSource.type; 
			sunLight.renderMode = lightSource.renderMode;
			sunLight.bakedIndex = lightSource.bakedIndex;
			sunLight.cullingMask = lightSource.cullingMask;
			sunLight.flare = lightSource.flare;   
		}

		if (moonLight == null) {
			var moon = new GameObject ("MoonLight");

			moon.transform.parent = this.transform;
			moon.transform.localScale = Vector3.one;
			moon.transform.localPosition = Vector3.zero;
			moon.transform.localRotation = Quaternion.identity; 

			moon.transform.rotation = lightSource.transform.rotation;

			moonLight = moon.AddComponent<Light> ();
			moonLight.color = this.nightSunColor;
			moonLight.intensity = this.nightSunIntensity;
			moonLight.bounceIntensity = lightSource.bounceIntensity;
			moonLight.shadows = lightSource.shadows;
			moonLight.shadowStrength = lightSource.shadowStrength;
			moonLight.type = lightSource.type; 
			moonLight.renderMode = lightSource.renderMode;
			moonLight.bakedIndex = lightSource.bakedIndex;
			moonLight.cullingMask = lightSource.cullingMask; 
			moonLight.enabled = false;
		}
		 

		if (lightSource) {
			lightSource.enabled = false;
			sunAngle = lightSource.transform.rotation.eulerAngles; 
			maxSunIntensity = lightSource.intensity; 

			//#if UNITY_EDITOR
			if(autoSunAngle){
				startTime = (90 + sunAngle.y) / 180.0f;
			}
			//#endif
		} 
	}

	protected void InitGlobalShaders(){
		 
		Shader.SetGlobalTexture ("_SnowTexture", this.snowTexture);
		Shader.SetGlobalFloat ("_SnowStartHeight", this.snowStartHeight); 

		Shader.SetGlobalTexture ("_RainBump", this.rainBump);
		Shader.SetGlobalColor ("_RainSpecularColor", this.rainSpecColor);
		Shader.SetGlobalFloat ("_RainShininess", this.rainShininess); 
		Shader.SetGlobalFloat ("_Weather0", 0); 
		Shader.SetGlobalFloat ("_Weather1", 0); 

		SetWeatherRange (this.influenceArea);
	}

	protected void ClearGlobalShaders(){

		Shader.SetGlobalTexture ("_SnowTexture", null);
		Shader.SetGlobalFloat ("_SnowStartHeight", 0); 

		Shader.SetGlobalTexture ("_RainBump", null);
		Shader.SetGlobalColor ("_RainSpecularColor", Color.white);
		Shader.SetGlobalFloat ("_RainShininess",0); 

		SetWeatherRange (new Rect(0,0,0,0));

		SetWeatherAmount (0);
		SetWeatherBolting (0);
		SetWeatherTemperature (0); 
	}

	protected void UpdateWeather(bool autoStart){ 
		Weather newWeather = null;
		
		switch (weatherType) {
		case WeatherType.Clear:
		case WeatherType.Clear_Blue:
			newWeather = new ClearWeather ();
			break;	
		case WeatherType.Rain: 
		case WeatherType.Heavy_Rain: 
			newWeather = new RainWeather ();
			break;	
		case WeatherType.Snow: 
		case WeatherType.Heavy_Snow: 
			newWeather = new SnowWeather ();
			break;	
		case WeatherType.Cloudy: 
		case WeatherType.Heavy_Cloudy: 
			newWeather = new CloudyWeather ();
			break;	   
		}

		if (newWeather!=null) {
			if (weather != null) {
				weather.Stop (this,-1);
				activeWeathers.Add (weather);
			}
			weather = newWeather;
			weather.Init(this);
			if (autoStart) {
				weather.Start (this);
			}
		}
	}

	public WindZone StartWind(){
		var windZone = new WindZone (); 
		windZone.Init (this);
		windZone.Start (this);
		activeWeathers.Add (windZone);
		return windZone;
	}

	public void StopWind(WindZone windZone){
		if (windZone != null) {
			windZone.Stop (this,-1); 
		}
	}

	public void AddTimeControlObject(TimeControlObject obj){
		if (obj.showTime == TimeControl.NIGHT) {
			nightObjects.Add (obj);
		} else {
			dayObjects.Add (obj);
		}

	}

	public void RemoveTimeControlObject(TimeControlObject obj){
		if (obj.showTime == TimeControl.NIGHT) {
			nightObjects.Remove (obj);
		} else {
			dayObjects.Remove (obj);
		}
	}

	void ShowNightObjects(bool show,bool immediately){
		foreach (var nightObject in nightObjects) {
			nightObject.Show (show,immediately);
		}
	}

	void ShowDayObjects(bool show,bool immediately){
		foreach (var dayObject in dayObjects) {
			dayObject.Show (show,immediately);
		}
	}

	protected virtual void OnGUI(){
		if (!timeControl)
			return;
		
		startTime = GUI.HorizontalSlider (new Rect(25,25,200,30),(float)startTime,0.0f,1.0f); 

		float y = 100;

		var weatherString = GUI.TextField(new Rect(10,y,60,40),((int)weatherType).ToString());
		int value;
		if (int.TryParse (weatherString, out value)) {
			var type = (WeatherType)value;
			if (type != weatherType) {
				this.weatherType = type;
				UpdateWeather (true);
			}
		}

		var winding = GUI.Toggle(new Rect(100,y,60,30),windZone!=null,"wind");
		if (winding) {
			if(windZone ==null)
				windZone = StartWind ();
		} else {
			StopWind (windZone);
			windZone = null;
		}

		timeUpdate = GUI.Toggle(new Rect(100,y+30,100,30),timeUpdate,"auto time");

		if (GUI.Toggle (new Rect (180, y, 100, 30), weatherQuantity == WeatherQuantity.FULL_QUANTITY, "full quantity")) {
			WeatherQuantity = WeatherQuantity.FULL_QUANTITY;
		} else {
			WeatherQuantity = WeatherQuantity.FAST_QUANTITY;
		}
	}


	 

	void UpdateTime(){	  
		var minuteOfDay = timeOfDay;
		minutes = minuteOfDay % 60;
		hours = minuteOfDay  / 60;
		if (startTime > 1) {
			startTime = startTime - 1; 
			UpdateDays ();
		}
	}

	int timeOfDay{
		get {
			return Mathf.RoundToInt ((float)(startTime * 60 * 24));
		}
	}

	void UpdateDays(){
		hours = 0;
		days += 1;
		if (days > GetMonthDays(year,month)) {
			days = 1;
			month += 1;
			if (month >= 13) {
				month = 1;
				year += 1;
			}
		}
		if (autoChangeWeather) {
			this.weatherType = (WeatherType)Random.Range ((int)(WeatherType.Clear_Blue), (int)(WeatherType.Foggy));
			UpdateWeather (true);
		}
	}


	int GetMonthDays(int year,int month){
		switch (month) {
		case 1:
		case 3:
		case 5:
		case 7:
		case 8:
		case 10:
		case 12:
			return 31;
		case 2:
			if (year % 4 == 0)
				return 29;
			return 28;
		}

		return 30;
	}


	public Camera weatherCamera{
		get { 
			if (_camera == null) {
				_camera = Camera.main;
			}
			return _camera;
		}

		set {
			_camera = value;
		}
	} 

	public bool isMorning{
		get { 
			return hours >= sunupTime && hours < sunupTime + 3;
		}
	} 

	public bool isDay{
		get { 
			return hours >= sunupTime + 3 && hours < sunsetTime - 3;
		}
	}

	public bool isEvening{
		get { 
			return hours >= sunsetTime - 3 && hours < sunsetTime ;
		}
	}

	public bool isNight{
		get { 
			return hours >= sunsetTime || hours < sunupTime;
		}
	}

	public int sunupTime{
		get { 
			return 5;
		}
	}

	public int sunsetTime{
		get { 
			return 19;
		}
	}

	public float dayIntensity{
		get { 			 
			_dayIntensity = 1;

			if (isMorning) {
				_dayIntensity = Mathf.Clamp((timeOfDay - 60.0f * sunupTime) / (60 * 3),0,1);
			}

			if (isEvening) {
				_dayIntensity = Mathf.Clamp((60.0f * sunsetTime - timeOfDay) / (60 * 3),0,1);
			}

			if (isNight) {
				_dayIntensity = -1;
			}

			return _dayIntensity;
		}
	}

	public float nightIntensity{
		get { 	
			_nightIntensity = -1;

			if (isNight) { 
				if (hours <=sunupTime && hours >= sunupTime - 2) {
					_nightIntensity = 1 - (timeOfDay - (sunupTime - 2) * 60.0f) / (60 * 2);			
				} else if (hours >=sunsetTime && hours < sunsetTime + 2) {
					_nightIntensity = 1 - ((sunsetTime + 2) * 60.0f - timeOfDay) / (60 * 2);
				} else {
					_nightIntensity = 1;
				}
			}
			return _nightIntensity;
		}
	}
	 

	public float sunAngleOfDay{
		get {
			return 180.0f * ((timeOfDay - 12 * 60.0f) / ((sunsetTime - sunupTime + 1)*60.0f));
		}
	}

	public float weatherCrossSpeed{
		get { 
			if (weatherEnterCrossTime > 0.01f) {
				return 3;
			}
			return float.MaxValue;
		}
	}

	public float Temperature{
		set { 
			this.temperature = value;
			SetWeatherTemperature (this.temperature);
		}
		get{ 
			return this.temperature;
		}
	}

	public WeatherQuantity WeatherQuantity{
		set { 
			this.weatherQuantity = value;

			switch (weatherQuantity) {
			case WeatherQuantity.FULL_QUANTITY: 
				Shader.globalMaximumLOD = 400;
				break;
			case WeatherQuantity.GOOD_QUANTITY:
				Shader.globalMaximumLOD = 300;
				break;
			case WeatherQuantity.FAST_QUANTITY: 
				Shader.globalMaximumLOD = 200;
				break;		
			}
		}
		get { 
			return this.weatherQuantity;
		}
	}

	public SkyWeather skyWeather{
		get { 
			return weather as SkyWeather;
		}
	}
	 

	public virtual void SetWeatherAmount(float value){
		Shader.SetGlobalFloat ("_WeatherAmount",value);
	}

	public virtual void SetWeatherTemperature(float value){
		Shader.SetGlobalFloat ("_Temperature",value);
	}

	public virtual void SetWeatherBolting(float value){
		Shader.SetGlobalFloat ("_Bolting",value);
	}

	public virtual void SetWeatherRange(Rect range){ 
		Shader.SetGlobalVector ("_WeatherRange",new Vector4(range.xMin,range.yMin,range.xMax,range.yMax)); 
	}

	public virtual void SetGlobalVector(string name,Vector4 value){
		Shader.SetGlobalVector (name, value);
	}


	public bool isMainWeather{
		get { 
			return _isMainWeather;
		}

		set { 

			if (_isMainWeather != value || !_inited) {
				_isMainWeather = value;
				stars.SetActive (_isMainWeather);
				sunAndMoon.SetActive (_isMainWeather); 
				moonLight.gameObject.SetActive (_isMainWeather); 
				_inited = true; 
			}

		}
	}

	public Rect influenceArea{
		get { 
			return new Rect (new Vector2 (transform.position.x-influenceRange.x/2, transform.position.z-influenceRange.y/2), new Vector2 (influenceRange.x, influenceRange.y));
		}
	}

	public virtual PointLocation GetCameraLocation(out float factor){
		factor = 2;
		return PointLocation.IN_RANGE;
	}
	 

}
