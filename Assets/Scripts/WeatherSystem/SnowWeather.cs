using UnityEngine;
using System.Collections;

public class SnowWeather : SkyWeather {

	private float _snowSpeed = 0.12f; 
	private float _snowAmount  = 0;
	
	protected override GameObject GetWeatherPrefab (WeatherSystem weatherSystem)
	{
		return weatherSystem.snow;
	}

	protected override AudioClip GetWeatherSound (WeatherSystem weatherSystem)
	{
		return weatherSystem.snowSound;
	}

	protected override float GetWeatherSoundVolume (WeatherSystem weatherSystem){
		return weatherSystem.snowSoundVolume;
	}

	public override float GetWeatherSkyIntensity (WeatherSystem weatherSystem)
	{
		return weatherSystem.snowSkyIntensity;
	}

	public override float GetWeatherSunIntensity (WeatherSystem weatherSystem)
	{
		if (weatherType == WeatherType.Heavy_Snow)
			return weatherSystem.snowSunIntensity * 0.1f;
		
		return weatherSystem.snowSunIntensity;
	} 

	protected override float GetWindProbability (WeatherSystem weatherSystem)
	{
		if (weatherType == WeatherType.Snow)
			return 0.85f;

		return base.GetWindProbability(weatherSystem);
	}

	protected override void OnStart (WeatherSystem weatherSystem)
	{
		_snowAmount = weatherSystem._snowAmount;
		weatherSystem.Temperature = -10; 
		 
	}

	protected override void OnStarting (WeatherSystem weatherSystem, float factor)
	{
		var sunLight = weatherSystem.sunLight;
		sunLight.shadowStrength = Mathf.Clamp01(Mathf.Lerp(sunLight.shadowStrength, weatherSystem.snowSkyIntensity , factor));
		snowAmount += _snowSpeed * Time.deltaTime; 
		weatherSystem._snowAmount = snowAmount;
		weatherSystem.SetWeatherAmount (snowAmount); 

		/*var bloom = Camera.main.GetComponent<UnityStandardAssets.ImageEffects.Bloom> ();
		if(bloom!=null)
			bloom.bloomIntensity = (1-factor) * (weatherSystem.maxBloomIntensity - weatherSystem.minBloomIntensity) + weatherSystem.minBloomIntensity;*/
	}

	protected override void OnRunning (WeatherSystem weatherSystem)
	{
		snowAmount += _snowSpeed * Time.deltaTime; 
		weatherSystem._snowAmount = snowAmount;
		weatherSystem.SetWeatherAmount (snowAmount);  
	}

	protected override void OnStop (WeatherSystem weatherSystem)
	{
		weatherSystem._snowAmount = _snowAmount;
	}

	protected override void OnStoping (WeatherSystem weatherSystem, float factor)
	{ 
		var sunLight = weatherSystem.sunLight;
		sunLight.shadowStrength = Mathf.Lerp(sunLight.shadowStrength,weatherSystem.lightSource.shadowStrength , factor);

		snowAmount = Mathf.LinearToGammaSpace(Mathf.Lerp (_snowAmount,0,factor)); 
		weatherSystem.SetWeatherAmount (_snowAmount);

		/*var bloom = Camera.main.GetComponent<UnityStandardAssets.ImageEffects.Bloom> ();
		if(bloom!=null)
			bloom.bloomIntensity = factor * (weatherSystem.maxBloomIntensity - weatherSystem.minBloomIntensity) + weatherSystem.minBloomIntensity;*/
	}

	protected override void OnClear (WeatherSystem weatherSystem)
	{
		_snowAmount = 0;
		weatherSystem.SetWeatherAmount ( _snowAmount);
		if (weatherSystem.weatherType != WeatherType.Snow && weatherSystem.weatherType != WeatherType.Heavy_Snow) {
			weatherSystem._snowAmount = _snowAmount;
		}
	}

	public float snowSpeed {
		get {
			return this._snowSpeed;
		}
		set {
			_snowSpeed = value;
		}
	}

	public float snowAmount {
		get {
			return this._snowAmount;
		}
		set {
			_snowAmount = Mathf.Clamp( value,0,weatherType==WeatherType.Heavy_Snow?1.0f:0.9f); 
		}
	}
	 
}
