using UnityEngine;
using System.Collections;

public class SkyWeather : Weather,SkyInfoProvider  {
	
	public override void Update(WeatherSystem weatherSystem){
		if (state == WeatherState.STARTING || state == WeatherState.RUNNING) {
			//_sunnyIntensity = Mathf.Lerp(_sunnyIntensity, GetWeatherSunIntensity(weatherSystem),Time.deltaTime * weatherSystem.weatherCrossSpeed);
			//_skyIntensity = Mathf.Lerp(_skyIntensity, GetWeatherSkyIntensity(weatherSystem),Time.deltaTime * weatherSystem.weatherCrossSpeed);
		}
		base.Update (weatherSystem);
	}	

	public virtual float GetWeatherSkyIntensity (WeatherSystem weatherSystem)
	{
		throw new System.NotImplementedException ();
	}

	public virtual float GetWeatherSunIntensity (WeatherSystem weatherSystem)
	{
		throw new System.NotImplementedException ();
	}
	 
}
