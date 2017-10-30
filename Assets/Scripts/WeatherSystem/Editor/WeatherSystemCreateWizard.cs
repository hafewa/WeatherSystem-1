using UnityEngine;
using System.Collections;
using UnityEditor;

public class WeatherSystemCreateWizard <T> : ScriptableWizard  where T:WeatherSystem {
	[Header("Base Settings")] 
	public WeatherType weatherType = WeatherType.Clear; 

	[Header("Lightings")]
	public Light lightSource;

	[Header("Sky settings")]
	public MeshRenderer skyMesh;
	public Material skyMaterial;
	public GameObject starsPrefab;
	public GameObject moonPrefab;

	[Header("Prefabs Settings")]
	public GameObject snowPrefab;
	public GameObject rainPrefab;
	public GameObject windPrefab;
	public GameObject lightingBolt1;
	public GameObject lightingBolt2;

	[Header("Sounds settings")]
	public AudioClip snowSound;
	public AudioClip rainSound;
	public AudioClip thunderSound;
	public AudioClip windSound; 

	[Header("Snow settings")]
	public Texture snowTexture; 
	public float snowStartHeight = -50.0f;

	[Header("Rain settings")]
	public Texture rainBump;  

	protected virtual void OnWizardUpdate(){
		isValid = true;
		errorString = "";

		if (lightSource == null) {
			isValid = false;
			errorString = "light source is null";
			return;
		}		 

		skyMaterial = FindAsset<Material> (skyMaterial,"WeatherSystem/Materials_Textures/Materials/Sky_Weather.mat");
		if (skyMaterial == null) {  
			isValid = false;
			errorString = "sky material is null";
			return;
		}

		starsPrefab = FindAsset<GameObject> (starsPrefab,"Res/Effect/particle/Scene/changlefang01/changlefang01_xingxing_eff01.prefab");
		if (starsPrefab == null) {
			isValid = false;
			errorString = "stars prefab is null";
			return;
		}

		moonPrefab = FindAsset<GameObject> (moonPrefab,"Res/Effect/particle/Scene/changlefang01/changlefang01_yueliang_eff01.prefab");
		if (moonPrefab == null) {
			isValid = false;
			errorString = "moon prefab is null";
			return;
		}

		snowPrefab = FindAsset<GameObject> (snowPrefab,"Res/Effect/particle/Scene/tianqi/eff_Snow_01.prefab");
		if (snowPrefab == null) {
			isValid = false;
			errorString = "snow prefab is null";
			return;
		}
		 
		rainPrefab = FindAsset<GameObject> (rainPrefab,"Res/Effect/particle/Scene/tianqi/eff_rain_01.prefab");
		if (rainPrefab == null) {
			isValid = false;
			errorString = "rain prefab is null";
			return;
		}

		windPrefab = FindAsset<GameObject> (windPrefab,"Res/Effect/particle/Scene/tianqi/eff_Wind_01.prefab");
		if (windPrefab == null) {
			isValid = false;
			errorString = "wind prefab is null";
			return;
		}

		lightingBolt1 = FindAsset<GameObject> (lightingBolt1,"WeatherSystem/Prefabs/Particles/LightningBolt1.prefab");
		if (lightingBolt1 == null) {
			isValid = false;
			errorString = "lightingBolt1 is null";
			return;
		}
		lightingBolt2 = FindAsset<GameObject> (lightingBolt2,"WeatherSystem/Prefabs/Particles/LightningBolt2.prefab");
		if (lightingBolt2 == null) {
			isValid = false;
			errorString = "lightingBolt2 is null";
			return;
		}

		snowSound = FindAsset<AudioClip> (snowSound,"WeatherSystem/Sounds/Wind/SnowWindLooped.mp3");
		if (snowSound == null) {
			isValid = false;
			errorString = "snow sound is null";
			return;
		}

		rainSound = FindAsset<AudioClip> (rainSound,"WeatherSystem/Sounds/Precipitation/RainNew.wav");
		if (rainSound == null) {
			isValid = false;
			errorString = "rain sound is null";
			return;
		}
		thunderSound = FindAsset<AudioClip> (thunderSound,"WeatherSystem/Sounds/Thunder/Thunder 1 (New).wav");
		if (thunderSound == null) {
			isValid = false;
			errorString = "thunder sound is null";
			return;
		}

		windSound = FindAsset<AudioClip> (windSound,"WeatherSystem/Sounds/Wind/Wind1(New).wav");
		if (windSound == null) {
			isValid = false;
			errorString = "wind sound is null";
			return;
		}


		snowTexture = FindAsset<Texture> (snowTexture,"WeatherSystem/Materials_Textures/Snow/Snow0103_1_M.tif");
		if (snowTexture == null) {
			isValid = false;
			errorString = "snow texture is null";
			return;
		}

		rainBump = FindAsset<Texture> (rainBump,"WeatherSystem/Materials_Textures/Snow/Waterbump.jpg");
		if (rainBump == null) {
			isValid = false;
			errorString = "rain bump is null";
			return;
		}
	}

	protected virtual void OnWizardCreate(){
		var go = new GameObject ("WeatherSystem");
		var audioSource = go.AddComponent<AudioSource> ();
		var lightingBolt = go.AddComponent<LightingBolt> ();

		lightingBolt.lightSource = lightSource;
		lightingBolt.lightingBolt1 = lightingBolt1;
		lightingBolt.lightingBolt2 = lightingBolt2;
		lightingBolt.lightingSounds = new AudioClip[]{thunderSound};


		var weatherSystem = go.AddComponent<T> ();

		weatherSystem.weatherType = weatherType;
		weatherSystem.maxSunIntensity = lightSource.intensity+0.1f;
		weatherSystem.lightSource = lightSource;
		weatherSystem.skyMesh = skyMesh;
		weatherSystem.skyMaterial = skyMaterial;
		weatherSystem.starPrefab = starsPrefab;
		weatherSystem.moonPrefab = moonPrefab;
		weatherSystem.rain = rainPrefab;
		weatherSystem.snow = snowPrefab;
		weatherSystem.wind = windPrefab;
		weatherSystem.rainSound = rainSound;
		weatherSystem.snowSound = snowSound;
		weatherSystem.windSound = windSound;
		weatherSystem.thunderSound = thunderSound;
		weatherSystem.snowTexture = snowTexture;
		weatherSystem.snowStartHeight = snowStartHeight;
		weatherSystem.rainBump = rainBump;



		OnCreatePosted (weatherSystem);
	}

	protected virtual void OnCreatePosted(T weatherSystem){
	}

	static T FindAsset<T>(T value,string path) where T :Object{
		if (value != null)
			return value;
		return AssetDatabase.LoadAssetAtPath<T> ("Assets/"+path);
	}
}
