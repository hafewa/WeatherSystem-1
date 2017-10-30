using UnityEngine;
using System.Collections;
using UnityEditor;

public class WeatherSystemEditor  {
	[MenuItem("WeatherSystem/Create Global WeatherSystem",false)]
	public static void CreateWeatherSystem(){
		ScriptableWizard.DisplayWizard<GlobalWeatherSystemCreateWizard> ("Create Weather System","Create");
	}

	[MenuItem("WeatherSystem/Create Managed WeatherSystem",false)]
	public static void CreateManagedWeatherSystem(){
		ScriptableWizard.DisplayWizard<ManagedWeatherCreateWizard> ("Create Weather System","Create");
	}
}
