using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine;

[System.Serializable]
public class SettingsManager : MonoBehaviour
{
	public float stagesToMax = 5f;
	[System.NonSerialized]
	public float currentStage = 0f;
	[System.NonSerialized]
	public float currentScore = 0f;

	[System.NonSerialized]
	private int preferedFertilizerCount = 3;

	public int fertilizerCount {
		get {
			return Mathf.Min (preferedFertilizerCount, fertilizerTypeNames.Count);
		}
	}

	public List<string> fertilizerTypeNames = new List<string> ();

	public float startScoreModifier = 100f;
	public float endScoreModifier = 100f;

	public float currentScoreModifier {
		get {
			return (currentStage / stagesToMax) * (endScoreModifier - startScoreModifier) + startScoreModifier;
		}
	}

	public float startGameSpeed = 1f;
	public float endGameSpeed = 2f;

	public float currentGameSpeed {
		get {
			return (currentStage / stagesToMax) * (endGameSpeed - startGameSpeed) + startGameSpeed;
		}
	}

	public float startBarrelSpawnDelay = 1f;
	public float endBarrelSpawnDelay = 2f;

	public float currentBarrelSpawnDelay {
		get {
			return (currentStage / stagesToMax) * (endBarrelSpawnDelay - startBarrelSpawnDelay) + startBarrelSpawnDelay;
		}
	}

	public float startBarrelsRequired = 0.2f;
	public float endBarrelsRequired = 0.3f;

	public float currentBarrelsRequired {
		get {
			return (currentStage / stagesToMax) * (endBarrelsRequired - startBarrelsRequired) + startBarrelsRequired;
		}
	}

	public float startMaxLives = 0.5f;
	public float endMaxLives = 0.6f;

	public float currentMaxLives {
		get {
			return (currentStage / stagesToMax) * (endMaxLives - startMaxLives) + startMaxLives;
		}
	}

	public float startMaxSimultaneousBarrels = 0.2f;
	public float endMaxSimultaneousBarrels = 0.2f;

	public float currentMaxSimultaneousBarrels {
		get {
			return (currentStage / stagesToMax) * (endMaxSimultaneousBarrels - startMaxSimultaneousBarrels) + startMaxSimultaneousBarrels;
		}
	}




	private static SettingsManager _manager = null;

	public static SettingsManager manager {
		get {
			if (_manager)
				return _manager;

			Debug.Log ("No Settings Manager in scene... Creating");
			GameObject newObj = new GameObject ("Setting Manager");

			_manager = newObj.AddComponent<SettingsManager> () as SettingsManager;
			return _manager;
		}
	}

	void Awake ()
	{
		if (_manager == null) {
			_manager = this;
			DontDestroyOnLoad (_manager.gameObject);

			_manager.fertilizerTypeNames.Add ("Example");
			_manager.LoadEasy ();
		} else {
			Destroy (gameObject);
		}
	}

	public void SaveToFile ()
	{
		string json = JsonUtility.ToJson (_manager);

		Debug.Log (Application.persistentDataPath);
		System.IO.File.WriteAllText (Application.persistentDataPath + "/preferences.json", json);
	}

	public void LoadEasy ()
	{
		preferedFertilizerCount = 3;
		ReadFromFile ("gameSettings.json");
	}

	public void LoadMedium ()
	{
		preferedFertilizerCount = 4;
		ReadFromFile ("gameSettings.json");
	}

	public void LoadHard ()
	{
		preferedFertilizerCount = 5;
		ReadFromFile ("gameSettings.json");
	}

	public void LogScore (float score)
	{
		System.IO.File.AppendAllText (Application.persistentDataPath + "/scores.log", System.DateTime.Now.ToLongTimeString () + " - " + System.DateTime.Now.ToLongDateString () + " - " + score.ToString () + System.Environment.NewLine);
	}

	public void ReadFromFile (string file)
	{
		currentScore = 0f;
		currentStage = 0f;

		string filePath = System.IO.Path.Combine (Application.streamingAssetsPath, file);
		string result = "";
		if (filePath.Contains ("://")) {
			WWW reader = new WWW (filePath);
			while (!reader.isDone) {
			}

			result = reader.text;
		} else {
			if (File.Exists (filePath)) {
				result = System.IO.File.ReadAllText (filePath);
			} 
		}

		if (!string.IsNullOrEmpty (result)) {
			JsonUtility.FromJsonOverwrite (result, _manager);
		} else {
			Debug.Log ("File Not Found: " + filePath);
		}
	}

	public void IncrementDifficultyAndRestart ()
	{
		currentStage += 1f;
		currentStage = Mathf.Min (currentStage, stagesToMax);
		SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
	}
}
	