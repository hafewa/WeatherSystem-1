using UnityEngine;
using System.Collections;

public class WindZone : Weather {

	protected override GameObject GetWeatherPrefab (WeatherSystem weatherSystem)
	{
		return weatherSystem.wind;
	}

	protected override AudioClip GetWeatherSound (WeatherSystem weatherSystem)
	{
		return weatherSystem.windSound;
	}

	protected override float GetWeatherSoundVolume (WeatherSystem weatherSystem){
		return weatherSystem.windSoundVolume;
	}

	private Vector4 windDir = new Vector4(0.1f,0,0.05f,0.2f);

	protected override void OnStart (WeatherSystem weatherSystem)
	{ 
		windDir.x = weatherSystem.windDir.x;
		windDir.y = weatherSystem.windDir.y;
		windDir.z = weatherSystem.windDir.z;
		windDir.w = 0;
		weatherSystem.SetGlobalVector ("_WindDir",windDir); 
	}

	protected override void OnStarting (WeatherSystem weatherSystem, float factor)
	{
		windDir.w = factor * weatherSystem.windPower;
		weatherSystem.SetGlobalVector ("_WindDir",windDir);
	}

	protected override void OnStoping (WeatherSystem weatherSystem, float factor)
	{
		windDir.w = (1- factor) * weatherSystem.windPower; 
		weatherSystem.SetGlobalVector ("_WindDir",windDir); 
	}
	 
	protected override void OnClear (WeatherSystem weatherSystem)
	{ 
		windDir.w = 0;
		weatherSystem.SetGlobalVector ("_WindDir",windDir);
	}

	protected override void UpdateParticle(WeatherSystem weatherSystem){ 
		var forward = weatherSystem.weatherCamera.transform.forward;
		var right = weatherSystem.weatherCamera.transform.right;
		forward.y = 0;
		forward.Normalize ();
		right.y = 0;
		right.Normalize ();

		Vector3 offset = forward * weatherSystem.windParticleOffset.z + right * weatherSystem.windParticleOffset.x + Vector3.up * weatherSystem.windParticleOffset.y; 
		weatherObject.transform.position = weatherSystem.weatherCamera.transform.position + offset; 
		var direction = (weatherSystem.weatherCamera.transform.position + forward * weatherSystem.windParticleOffset.z - weatherObject.transform.position);
		direction.y = 0;
		direction.Normalize ();
		weatherObject.transform.rotation = Quaternion.LookRotation(direction);
	}
}
