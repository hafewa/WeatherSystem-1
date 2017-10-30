using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum WeatherBlendPolicy{
	SIMPLE,
	BLEND
}

[RequireComponent(typeof(MeshRenderer))]
public class WeatherComponent : MonoBehaviour {

	public WeatherBlendPolicy blendPolicy = WeatherBlendPolicy.SIMPLE; 

	private ManagedWeatherSystem[] weatherSystems = new ManagedWeatherSystem[2];

	private MeshRenderer _meshRenderer;  

	void Awake(){
		_meshRenderer = GetComponent<MeshRenderer> ();  
	}

	void OnDestroy(){
		foreach (var weatherSystem in weatherSystems) {
			if(weatherSystem!=null)
				weatherSystem.RemoveInfluenceObject (this);
		}
		weatherSystems = null;
	}

	public void EnableWeatherSystem(ManagedWeatherSystem weather){
		for (int i = 0; i < weatherSystems.Length; ++i) {
			var weatherSystem = weatherSystems [i];
			if (weatherSystem == null) {
				weatherSystems [i] = weather;
				EnableWeather (i, weather.weatherIndex);
				break;
			}else if(weatherSystem == weather){
				break;
			}
		}
	}

	public void DisableWeatherSystem(ManagedWeatherSystem weather){
		for (int i = 0; i < weatherSystems.Length; ++i) {
			var weatherSystem = weatherSystems [i];
			if(weatherSystem == weather){
				EnableWeather (i, -1);
				break;
			}
		}
	}

	void ApplyWeathers(){
		for (int i = 0; i < weatherSystems.Length; ++i) {
			var weatherSystem = weatherSystems [i];
			if (weatherSystem != null) {
				EnableWeather (i, weatherSystem.weatherIndex);
			} else {
				EnableWeather (i, -1);
			}
		}
	}

	void EnableWeather(int index,int weatherIndex){ 
		foreach (var mat in _meshRenderer.materials) { 
			mat.SetFloat ("_Weather" + index, weatherIndex+1);
		}
	}

	public Bounds bounds{
		get { 
			return _meshRenderer.bounds;
		}
	}
}
