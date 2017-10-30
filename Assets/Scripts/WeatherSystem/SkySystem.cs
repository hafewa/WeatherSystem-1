using UnityEngine;
using System.Collections;

public interface SkyInfoProvider{
	float GetWeatherSkyIntensity (WeatherSystem weatherSystem);
	float GetWeatherSunIntensity (WeatherSystem weatherSystem);
}

public class SkySystem {	

	private WeatherSystem _weatherSystem;
	 
	private Material _skyMaterial;

	private float _sunnyIntensity = 0; 

	private float _skyIntensity = 0;

	public void Update(WeatherSystem weatherSystem){
		_weatherSystem = weatherSystem;
		var skyInfoProvider = weatherSystem.skyWeather;
		_sunnyIntensity = skyInfoProvider.GetWeatherSunIntensity (weatherSystem);
		_skyIntensity =  skyInfoProvider.GetWeatherSkyIntensity (weatherSystem);
		UpdateSky (weatherSystem,skyInfoProvider);
		if (weatherSystem.useLighting) {
			UpdateSunAndMoon (weatherSystem,skyInfoProvider); 
		}
	}


	protected void UpdateSky(WeatherSystem weatherSystem,SkyInfoProvider skyInfoProvider){ 
		this.skyMaterial = weatherSystem.skyMaterial;
		this.skyMaterial.SetFloat ("_CloudyIntensity", 1 - Mathf.Clamp(skyInfoProvider.GetWeatherSkyIntensity(_weatherSystem),0,1));
		this.skyMaterial.SetFloat ("_DayIntensity", (weatherSystem.dayIntensity));
		this.skyMaterial.SetFloat("_AMultiplier",_skyIntensity);
	}

	protected void UpdateSunAndMoon(WeatherSystem weatherSystem,SkyInfoProvider skyInfoProvider){
		var sunLight = weatherSystem.sunLight;
		var moonLight = weatherSystem.moonLight;
		var sunAndMoon = weatherSystem.sunAndMoon;
		var stars = weatherSystem.stars;
		var moon = weatherSystem.moon;

		var isDay = weatherSystem.isMorning || weatherSystem.isDay || weatherSystem.isEvening;  

		if (isDay) {			
			var sunIntensity = weatherSystem.maxSunIntensity * weatherSystem.dayIntensity * Mathf.Clamp (_sunnyIntensity, 0.0f, 1);
			if (sunLight.enabled) {
				//sunLight.intensity = Mathf.Lerp (sunLight.intensity,sunIntensity , Time.deltaTime * weatherSystem.weatherCrossSpeed);
				sunLight.intensity = sunIntensity;
			} else {
				sunLight.enabled = true;
				sunLight.intensity = sunIntensity;
			} 
			if (weatherSystem.isMorning || weatherSystem.isEvening) {
				var eveningSunIntensity = weatherSystem.eveningSunIntensity * Mathf.Clamp (_sunnyIntensity, 0.0f, 1);
				sunLight.color = Color.Lerp (weatherSystem.lightSource.color, weatherSystem.eveningSunColor, 1 - weatherSystem.dayIntensity); 
				sunLight.intensity = Mathf.Lerp(eveningSunIntensity,sunIntensity,Mathf.Abs( (weatherSystem.dayIntensity - 0.5f) * 2));
			} else {
				sunLight.color = weatherSystem.lightSource.color;
			}
			moonLight.enabled = false;
		} else { 
			sunLight.enabled = false; 
			moonLight.enabled = true; 
			moonLight.intensity = weatherSystem.nightSunIntensity * weatherSystem.nightIntensity * Mathf.Clamp (_sunnyIntensity, 0.0f, 1);
		}


		stars.SetActive (!isDay);
		moon.SetActive (!isDay);

		float angle = (float)(weatherSystem.startTime * 360);

		weatherSystem._moonAngle = angle;

		if (weatherSystem.startTime > 0.6) {
			angle = Mathf.Min (292, angle);
		} else {
			angle = Mathf.Min (292, 360 - angle);
		}

		switch (weatherSystem.eastDirection) {
		case WorldDirection.X_POSITIVE:
			sunAndMoon.transform.localRotation = Quaternion.Euler (0,0,angle);
			break;
		case WorldDirection.Z_POSITIVE:
			sunAndMoon.transform.localRotation = Quaternion.Euler (-angle,0,0);
			break;
		case WorldDirection.X_NEGATIVE:
			sunAndMoon.transform.localRotation = Quaternion.Euler (0,0,-angle);
			break;
		case WorldDirection.Z_NEGATIVE:
			sunAndMoon.transform.localRotation = Quaternion.Euler (angle,0,0);
			break;
		}


		var fogFactor = Mathf.Clamp (1 - weatherSystem.dayIntensity, 0, 1);
		RenderSettings.fogColor = Color.Lerp (weatherSystem.dayFogColor,weatherSystem.nightFogColor, fogFactor);
		RenderSettings.fogStartDistance = Mathf.Lerp(weatherSystem.dayFogStartDistance,weatherSystem.nightFogStartDistance,fogFactor);
		RenderSettings.fogEndDistance = Mathf.Lerp(weatherSystem.dayFogEndDistance,weatherSystem.nightFogEndDistance,fogFactor);

		if (weatherSystem.autoSunAngle) {
			if (!weatherSystem.sunAngleSyncWithMoon) {
				sunLight.transform.rotation = Quaternion.Euler (weatherSystem.sunAngle.x, weatherSystem.sunAngleOfDay, weatherSystem.sunAngle.z); 
			}
		} else {
			sunLight.transform.rotation = Quaternion.Euler (weatherSystem.sunAngle.x, weatherSystem.sunAngle.y,weatherSystem.sunAngle.z); 
		}
	}


	public Material skyMaterial{
		set { 
			if (_skyMaterial != value) { 
				_skyMaterial = value;
				if (_weatherSystem.skyMesh != null) {
					_weatherSystem.skyMesh.material = _skyMaterial;
					RenderSettings.skybox = null;
				} else {
					RenderSettings.skybox = value;
				}
			}
		}

		get { 
			return _skyMaterial;
		}
	}

	public float skyIntensity{
		set { 
			_skyIntensity = value;
			_skyMaterial.SetFloat("_AMultiplier",_skyIntensity);
			_skyMaterial.SetFloat ("_Exposure", _skyIntensity);
		}
		get { 
			return _skyIntensity;
		}
	}	

	public WeatherSystem weatherSystem{
		set { 
			_weatherSystem = value; 
		}
	}
}
