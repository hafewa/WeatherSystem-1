using UnityEngine;
using System.Collections;

public class RainWeather : SkyWeather { 

	private Material rainWaveMaterial;

	private float _rainSpeed = 1.2f; 
	private float _rainAmount  = 0;

	protected override GameObject GetWeatherPrefab (WeatherSystem weatherSystem)
	{
		return weatherSystem.rain;
	}

	protected override AudioClip GetWeatherSound (WeatherSystem weatherSystem)
	{
		return weatherSystem.rainSound;
	}

	protected override float GetWeatherSoundVolume (WeatherSystem weatherSystem){
		return weatherSystem.rainSoundVolume;
	}

	public override float GetWeatherSkyIntensity (WeatherSystem weatherSystem)
	{
		return weatherSystem.rainSkyIntensity;
	}

	public override float GetWeatherSunIntensity (WeatherSystem weatherSystem)
	{
		return weatherSystem.rainSunIntensity;
	} 

	protected override void OnStart (WeatherSystem weatherSystem)
	{
		if (weatherType == WeatherType.Rain) {
			foreach (var weatherParticle in weatherParticles) {
				weatherParticle.maxParticles = weatherParticle.maxParticles * 3 / 4;
			}

			//weatherParticle.emission.rate.constantMin = weatherParticle.emission.rate.constantMin * 3 / 4;
		}
		if (weatherSystem.lightingBolt != null) {
			weatherSystem.lightingBolt.enabled = true; 
		} 
		weatherSystem.Temperature = 20;
		rainAmount = 0;

	}

	protected override void OnStarting (WeatherSystem weatherSystem, float factor)
	{
		base.OnStarting (weatherSystem, factor); 
		rainAmount += _rainSpeed * Time.deltaTime; 
		weatherSystem.SetWeatherAmount (_rainAmount);
		UpdateRainWave (weatherSystem);
	}

	protected override void OnRunning (WeatherSystem weatherSystem)
	{
		UpdateRainWave(weatherSystem);
		weatherSystem.SetWeatherAmount (_rainAmount);
	}

	protected override void OnStop (WeatherSystem weatherSystem)
	{
		if (weatherSystem.lightingBolt != null) {
			weatherSystem.lightingBolt.enabled = weatherSystem.weatherType == WeatherType.Rain || weatherSystem.weatherType == WeatherType.Heavy_Rain; 
		}
	}

	protected override void OnStoping (WeatherSystem weatherSystem, float factor)
	{
		base.OnStoping (weatherSystem, factor); 
		rainAmount = Mathf.LinearToGammaSpace(Mathf.Lerp (_rainAmount,0,factor)); 
		weatherSystem.SetWeatherAmount (_rainAmount);
		UpdateRainWave (weatherSystem);
	}

	protected override void OnClear (WeatherSystem weatherSystem)
	{
		rainAmount = 0;
		weatherSystem.SetWeatherAmount (_rainAmount);
	}

	void UpdateRainWave(WeatherSystem weatherSystem){
		/*int a = (int) (Time.time * weatherSystem.framesPerSecond);
		a = a % weatherSystem.rainTexture.Length; 
		weatherSystem.SetGlobalTexture ("_RainTexture",weatherSystem.rainTexture[a]);*/
	}	 
	 

	public float rainAmount{
		get {
			return _rainAmount;
		}
		set {
			#if UNITY_EDITOR
			_rainAmount = Mathf.Clamp (value, 0, 0.2f);
			#else
			_rainAmount = Mathf.Clamp (value, 0, 0.2f);
			#endif

		}
	}

	  
}
