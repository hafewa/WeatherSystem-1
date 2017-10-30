using UnityEngine;
using System.Collections;

public enum TimeControl{
	DAY,
	NIGHT
}
 
public enum TimeControlState{
	IDLE,
	SHOWING,
	HIDDING
}

public class TimeControlObject : MonoBehaviour { 
	public TimeControl showTime = TimeControl.NIGHT;

	[Range(0,10)]
	public float crossingTime  = 1;

	public TimeControlState _state = TimeControlState.IDLE;

	private Light _light;

	private ParticleSystem _particleSystem;

	private float _maxLightIntensity = 1;

	public float _timer = 0 ; 

	void Awake(){
		_light = GetComponent<Light> ();
		if (_light != null) {
			_maxLightIntensity = _light.intensity;
		}
		_particleSystem = GetComponent<ParticleSystem> ();
	}

	void Start(){
		WeatherSystem.instance.AddTimeControlObject (this);
	}

	void OnDestroy(){
		if(WeatherSystem.instance!=null)
			WeatherSystem.instance.RemoveTimeControlObject (this);
	} 

	void LateUpdate(){
		if (_state == TimeControlState.IDLE)
			return;

		_timer += Time.deltaTime;
		var factor = crossingTime> 0 ? _timer / crossingTime:1;
		if (_state == TimeControlState.SHOWING) {
			if (_light != null) { 
				_light.intensity = Mathf.Lerp (0,_maxLightIntensity,factor);
			}
			if (factor >= 1) {
				_state = TimeControlState.IDLE;
			}

		}else if (_state == TimeControlState.HIDDING) {
			if (_light != null) { 
				_light.intensity = Mathf.Lerp (_light.intensity,0,factor);
			}
			if (factor >= 1) {
				_state = TimeControlState.IDLE;
				gameObject.SetActive (false);
			}
		}
	}

	public void Show(bool show,bool immediately){
		if (crossingTime <= 0 || immediately) {
			gameObject.SetActive (show);
		} else {
			if (show) {
				if (!gameObject.gameObject.activeInHierarchy) {
					_timer = 0; 
					_state = TimeControlState.SHOWING;
					gameObject.SetActive (true); 
					if (_light != null) {
						_light.intensity = 0;
					}
					if (_particleSystem != null) {
						_particleSystem.Play ();
					}
				}
			}else if(gameObject.gameObject.activeInHierarchy){
				_timer = 0; 
				_state = TimeControlState.HIDDING;
				if (_particleSystem != null) {
					_particleSystem.Stop ();
				}
			}
		}
	}
	 
	 
}
