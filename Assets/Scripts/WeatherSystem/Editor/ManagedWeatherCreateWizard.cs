using UnityEngine;
using UnityEditor;
using System.Collections;

public class ManagedWeatherCreateWizard : WeatherSystemCreateWizard<ManagedWeatherSystem> {
	protected override void OnCreatePosted(ManagedWeatherSystem weatherSystem){
		var sceneManagement = GameObject.FindObjectOfType<SceneManagement> ();
		if (sceneManagement == null) {
			sceneManagement = new GameObject ("SceneManagement").AddComponent<SceneManagement>(); 
		}
		weatherSystem.transform.parent = sceneManagement.transform;
		weatherSystem.transform.localScale = Vector3.one;
		weatherSystem.transform.localRotation = Quaternion.identity;
		weatherSystem.transform.localPosition = Vector3.one;
	}
}
