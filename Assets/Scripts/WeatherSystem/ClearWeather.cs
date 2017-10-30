using UnityEngine;
using System.Collections;

public class ClearWeather : SkyWeather { 
	 
	 
	public override float GetWeatherSkyIntensity (WeatherSystem weatherSystem)
	{
		return weatherSystem.clearSkyIntensity;
	}

	public override float GetWeatherSunIntensity (WeatherSystem weatherSystem)
	{
		return weatherSystem.clearSunIntensity;
	}


	protected override float GetWindProbability (WeatherSystem weatherSystem)
	{
		if (weatherType == WeatherType.Clear)
			return 0.85f;

		return 0;
	}
	 
	protected override void OnStop (WeatherSystem weatherSystem)
	{
		 

		var flare = GameObject.FindObjectOfType<LensFlare> ();
		if (flare != null) {
			flare.gameObject.SetActive (false);
		}
	}  
	 
}
