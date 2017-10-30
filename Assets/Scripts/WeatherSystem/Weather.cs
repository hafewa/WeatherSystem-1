using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Weather   { 
	
	public WeatherType weatherType;

	private WeatherState _state = WeatherState.IDLE;

	private float _startTime = 0;  

	private float _stopTime = 0; 

	private float _enterCrossTime = 1;

	private float _exitCrossTime = 1;

	protected GameObject weatherObject;
	 
	protected List<ParticleSystem> weatherParticles = new List<ParticleSystem>();

	private AudioSource _audioSource;
	  
	public virtual void Init(WeatherSystem weatherSystem){
		this.weatherType = weatherSystem.weatherType;

		var weatherPrefab = GetWeatherPrefab (weatherSystem);
		if (weatherPrefab && !weatherSystem.dontShowParticles) {
			weatherObject = GameObject.Instantiate<GameObject> (weatherPrefab);
			var weatherParticle = weatherObject.GetComponent<ParticleSystem> ();
			if (weatherParticle == null) {
				for (int i = 0; i < weatherObject.transform.childCount; ++i) {
					weatherParticle = weatherObject.transform.GetChild(i).GetComponent<ParticleSystem> ();
					if (weatherParticle != null) {
						weatherParticle.playOnAwake = false;
						weatherParticle.Pause ();
						weatherParticles.Add (weatherParticle);
					}
				}

			}
			weatherObject.transform.parent = weatherSystem.transform;
			weatherObject.transform.localScale = Vector3.one;
			weatherParticle.playOnAwake = false;
			weatherParticle.Pause ();
			UpdateParticle (weatherSystem);
		}		   

		var weatherSound = GetWeatherSound (weatherSystem);
		if (weatherSound != null && !weatherSystem.soundMute) {
			_audioSource = weatherSystem.gameObject.AddComponent<AudioSource> ();
			_audioSource.playOnAwake = false;
			_audioSource.loop = true;
			_audioSource.clip = weatherSound;
			_audioSource.volume = GetWeatherSoundVolume (weatherSystem);
		}

		_state = WeatherState.IDLE;
		_enterCrossTime = weatherSystem.weatherEnterCrossTime;
		_exitCrossTime = weatherSystem.weatherExitCrossTime;
		 
	}



	public void Start(WeatherSystem weatherSystem){
		if (_state != WeatherState.IDLE)
			return;
			
		foreach (var weatherParticle in weatherParticles) {
			weatherParticle.Play (); 
		}
		 
		if (audioSource != null) {
			audioSource.Play ();
		}
		_startTime = Time.time;
		_state = WeatherState.STARTING;
		OnStart (weatherSystem);
	}

	public virtual void Update(WeatherSystem weatherSystem){ 
		UpdateWeather (weatherSystem); 
	}

	public virtual void LateUpdate(WeatherSystem weatherSystem){
		if (_state == WeatherState.IDLE || _state == WeatherState.OVER)
			return; 
		if (weatherObject) {
			UpdateParticle (weatherSystem);
		} 
	}

	public void Stop(WeatherSystem weatherSystem,float crossTime ){ 
		foreach (var weatherParticle in weatherParticles) {
			weatherParticle.Stop (); 
		}
		if (crossTime >= 0) {
			_exitCrossTime = crossTime;
		}
		_stopTime = Time.time;
		_state = WeatherState.STOPPING;
		OnStop (weatherSystem);
	}


	public void Destroy(WeatherSystem weatherSystem){
		if (_state == WeatherState.OVER) {
			Debug.LogFormat ("{0} Destroy dup time", weatherType);
			return;
		}			
		
		if (weatherObject) {
			GameObject.Destroy (weatherObject);
			weatherObject = null;
		}

		if (_audioSource != null) {
			Object.Destroy (_audioSource);
		}
		_state = WeatherState.OVER;
		OnClear (weatherSystem); 
	}	  
	 
	protected void UpdateWeather(WeatherSystem weatherSystem){
		switch (state) {
		case WeatherState.STARTING:
			OnStarting (weatherSystem, (Time.time - _startTime) / _enterCrossTime);
			if (Time.time - _startTime >= _enterCrossTime) {
				_state = WeatherState.RUNNING;
			}
			break;
		case WeatherState.RUNNING:
			OnRunning (weatherSystem);
			break;
		case WeatherState.STOPPING:
			var factor = Mathf.Clamp ((Time.time - _stopTime) / _exitCrossTime, 0, 1);
			if (_audioSource != null) {
				_audioSource.volume = 1 - factor;
			}
			OnStoping (weatherSystem,factor);
			break;
		}
	}	 
	 
	protected virtual void UpdateParticle(WeatherSystem weatherSystem){ 
		float factor = 0;
		var location = weatherSystem.GetCameraLocation (out factor);
		var pos = weatherObject.transform.position;
		var cameraPos = weatherSystem.weatherCamera.transform.position;
		var cameraForward = weatherSystem.weatherCamera.transform.forward; 

		if (location == PointLocation.OUT_RANGE) { 	
			Vector3 interectPoint;
			if (MathUtility.GetSimpleNearestInterectPoint (weatherSystem.influenceArea, cameraPos, weatherSystem.weatherCamera.transform.forward * 1000, out interectPoint)) {				 
				weatherSystem._edgePoint = interectPoint;
				cameraForward = (interectPoint - cameraPos).normalized;
				pos = new Vector3 (interectPoint.x, cameraPos.y, interectPoint.z) + cameraForward * weatherSystem.particleOffset.z + Vector3.up * weatherSystem.particleOffset.y;
			}
		}else if (location == PointLocation.IN_RANGE) {
			#if UNITY_EDITOR
			Vector3 interectPoint;
			if (MathUtility.GetInterectPoint (weatherSystem.influenceArea, cameraPos, weatherSystem.weatherCamera.transform.forward* 1000, out interectPoint)) {				 
				weatherSystem._edgePoint = interectPoint;
			}
			#endif
			pos = cameraPos + cameraForward * weatherSystem.particleOffset.z + Vector3.up * weatherSystem.particleOffset.y; 
		}else if (location == PointLocation.IN_EDGE) { 
			Vector3 interectPoint;
			if (MathUtility.GetInterectPoint (weatherSystem.influenceArea, cameraPos, weatherSystem.weatherCamera.transform.forward* 1000, out interectPoint)) {				 
				weatherSystem._edgePoint = interectPoint;
				pos = new Vector3(interectPoint.x,cameraPos.y,interectPoint.z) - cameraForward * weatherSystem.particleOffset.z + Vector3.up * weatherSystem.particleOffset.y;
			}
		}

		weatherObject.transform.position = pos;
	}	 

	private bool isActive{
		get { 
			return _state == WeatherState.STARTING || _state == WeatherState.RUNNING || _state == WeatherState.STOPPING;
		}
	}

	public void BecomeMainWeather(){
		if (!isActive) {
			return;
		}
		if (_audioSource != null) {
			_audioSource.Play ();
		}
		weatherObject.SetActive(true);
		foreach (var weatherParticle in weatherParticles) {
			weatherParticle.Play (); 
		}
	}

	public void BecomeSlaveWeather(){
		if (!isActive) {
			return;
		}
		if (_audioSource != null) {
			_audioSource.Pause ();
		}
		weatherObject.SetActive(false);
		foreach (var weatherParticle in weatherParticles) {
			weatherParticle.Play (); 
		}
	}

	public bool isOver{
		get { 

			if (_state == WeatherState.STOPPING) {
				if (Time.time - _stopTime >= _exitCrossTime) {
					return weatherParticles.Count == 0 || !weatherParticles[0].IsAlive();
				}
			} 

			if (_state == WeatherState.OVER)
				return true;
			
			return false;
		}
	}

	public WeatherState state {
		set { 
			this._state = value;
		}
		get {
			return this._state;
		}
}

	public float startTime {
		get {
			return this._startTime;
		}
	}

	public float stopTime {
		get {
			return this._stopTime;
		}
	}

	public AudioSource audioSource {
		get {
			return this._audioSource;
		}
	}
	 
	 
	protected virtual GameObject GetWeatherPrefab(WeatherSystem weatherSystem){return null;}

	protected virtual AudioClip GetWeatherSound(WeatherSystem weatherSystem){return null;} 

	protected virtual float GetWeatherSoundVolume (WeatherSystem weatherSystem){return 1;}

	protected virtual float GetWindProbability(WeatherSystem weatherSystem) {return 0.95f;}

	protected virtual void OnStart(WeatherSystem weatherSystem){} 

	protected virtual void OnStarting(WeatherSystem weatherSystem,float factor){}

	protected virtual void OnRunning(WeatherSystem weatherSystem){}

	protected virtual void OnStop(WeatherSystem weatherSystem){} 

	protected virtual void OnStoping(WeatherSystem weatherSystem,float factor){}

	protected virtual void OnClear(WeatherSystem weatherSystem){}
}
