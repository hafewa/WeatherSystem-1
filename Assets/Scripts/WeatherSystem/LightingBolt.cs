using UnityEngine;
using System.Collections;

public class LightingBolt : MonoBehaviour {

	public Light lightSource;

	public float flashLength  = 0.76f;

	public float lightingStrikeOdds  = 0.955f;

	public float minIntensity  = 0.5f;

	public float maxIntensity = 0.7f;


	public GameObject lightingBolt1;
	public GameObject lightingBolt2;

	public AudioClip[] lightingSounds;

	public float maxShake = 0.05f;

	[Header("Debug Info")]
	public float _lastTime = 0; 
	public float _boltingTime = 0;
	public bool _bolting = false;
	public Vector3 origPos ;

	private WeatherSystem weatherSystem;

	// Use this for initialization
	void Start () { 
		weatherSystem = GetComponent<WeatherSystem> ();
		origPos = Camera.main.transform.position;
		weatherSystem.SetWeatherBolting (0);
	}
	
	// Update is called once per frame
	void Update () { 

		if (!weatherSystem.isMainWeather || Camera.main == null) {			
			return;
		}

		if (_bolting) {
			var passedTime = Time.time - _lastTime;
			if (passedTime >= _boltingTime) {
				_bolting = false;
				_lastTime = Time.time;
				lightSource.enabled = false;
				#if EANBLE_BOLTING_SHAKE
				Camera.main.transform.position = origPos;
				#endif
				weatherSystem.SetWeatherBolting (0);
			} else {				
				if (passedTime <= flashLength) {
					var shake = Random.value ; 
					weatherSystem.SetWeatherBolting (shake); 
					lightSource.intensity = Mathf.Lerp (shake *maxIntensity, 0, passedTime/_boltingTime); 
					shake = shake  - 0.5f;
					#if EANBLE_BOLTING_SHAKE
					Camera.main.transform.position = origPos + Camera.main.transform.right * (shake) * maxShake + Camera.main.transform.up * (shake) * maxShake;
					#endif
				} else {
					lightSource.enabled = false;
					#if EANBLE_BOLTING_SHAKE
					Camera.main.transform.position = origPos;
					#endif
					weatherSystem.SetWeatherBolting (0); 
				}
			}

		} else {
			if (Time.time - _lastTime > flashLength) {
				if (Random.value < lightingStrikeOdds) {
					var bolt1 = GameObject.Instantiate (lightingBolt1);
					var bolt2 = GameObject.Instantiate (lightingBolt2);

					bolt1.AddComponent<RenderBehind> ();
					bolt2.AddComponent<RenderBehind> ();

					bolt1.transform.position = Camera.main.transform.position + Vector3.up  ;
					bolt2.transform.position = Camera.main.transform.position + Vector3.up  ;

					//var noise = Mathf.PerlinNoise (minIntensity, maxIntensity);
					//lightSource.intensity = Mathf.Lerp (minIntensity, maxIntensity, noise);

					lightSource.intensity = maxIntensity;

					lightSource.enabled = true;

					if (lightingSounds.Length > 0) {
						var sound = lightingSounds [Random.Range (0, lightingSounds.Length)];
						var audioSource = GetComponent<AudioSource> ();
						if (audioSource != null)
							audioSource.PlayOneShot (sound);	
						_boltingTime = sound.length;
					} else {
						_boltingTime = 3;
					}


					_lastTime = Time.time;
					_bolting = true;

				}
			}
		}
		

	}
}
