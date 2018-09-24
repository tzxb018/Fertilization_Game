using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using System.Linq;

public class HintHelper : MonoBehaviour
{
	private Dictionary<string, Sprite> cache = new Dictionary<string, Sprite> ();


	private Barrels loadedBarrels = new Barrels ();

	private static HintHelper _manager = null;

	public static HintHelper manager {
		get {
			if (_manager)
				return _manager;

			Debug.Log ("No Hint Helper in scene... Creating");
			GameObject newObj = new GameObject ("Hint Helper");

			_manager = newObj.AddComponent<HintHelper> () as HintHelper;
			return _manager;
		}
	}

	void Awake ()
	{
		if (_manager == null) {
			_manager = this;
			loadedBarrels = LoadBarrels ("hints_bball.xml");
		} else {
			Destroy (gameObject);
		}
	}



	Barrels LoadBarrels (string path)
	{
		string filePath = System.IO.Path.Combine (Application.streamingAssetsPath, path);
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
			return Barrels.LoadFromText (result);
		} else {
			Debug.Log ("File Not Found: " + filePath);
			return new Barrels ();
		}
	}

	public Sprite LoadSpriteByName (string name)
	{

		if (cache.ContainsKey (name)) {
			return cache [name];
		}

		string basePath = System.IO.Path.Combine (Application.streamingAssetsPath, "hintImages");	
		string fullPath = System.IO.Path.Combine (basePath, name);

		#if UNITY_EDITOR || !UNITY_ANDROID
		fullPath = "file://" + fullPath;
		#endif

		WWW reader = new WWW (fullPath);
		while (!reader.isDone) {
		}

		if (!string.IsNullOrEmpty (reader.error)) {
			Debug.Log (reader.error);
			return null;
		}

		Texture texture = reader.texture;

		Sprite sprite = Sprite.Create (texture as Texture2D, new Rect (0, 0, texture.width, texture.height), Vector2.zero);

		cache.Add (name, sprite);

		return sprite;
	}

	public Barrel GetRandomBarrelFromDifficulty (int difficulty, int minPool, List<string> matchTypes)
	{
		Barrels matchingHints = new Barrels ();

		int currentDifficultyMatch = difficulty;
		while (currentDifficultyMatch > 0 && matchingHints.barrels.Count < minPool) {
			List<Barrel> currentMatch = loadedBarrels.barrels.Where (p => p.difficulty == currentDifficultyMatch && matchTypes.Contains (p.fertilizerType)).ToList ();

			foreach (Barrel b in currentMatch) {
				matchingHints.barrels.Add (b);
			}
			currentDifficultyMatch--;
		}

		if (matchingHints.barrels.Count <= 0)
			return null;

		return matchingHints.barrels [Random.Range (0, matchingHints.barrels.Count)];
	}

}


[XmlRoot ("BarrelHolder")]
public class Barrels
{
	[XmlArray ("Barrels")]
	[XmlArrayItem ("Barrel")]
	public List<Barrel> barrels = new List<Barrel> ();


	public void Save (string path)
	{
		try {
			XmlSerializer serializer = new XmlSerializer (typeof(Barrels));
			using (FileStream stream = new FileStream (path, FileMode.Create)) {
				serializer.Serialize (stream, this);
			}
		} catch (System.Exception e) {
			Debug.LogError (e);
			Debug.LogError ("Unable to save xml to file");
			return;
		}
	}

	public static Barrels Load (string path)
	{
		try {
			XmlSerializer serializer = new XmlSerializer (typeof(Barrels));
			using (FileStream stream = new FileStream (path, FileMode.Open)) {
				return serializer.Deserialize (stream) as Barrels;
			}
		} catch (System.Exception e) {
			Debug.LogError (e);
			Debug.LogError ("Unable to load xml from file");
			return new Barrels ();
		}
	}

	public static Barrels LoadFromText (string text)
	{
		try {
			XmlSerializer serializer = new XmlSerializer (typeof(Barrels));
			return serializer.Deserialize (new StringReader (text)) as Barrels;
		} catch (System.Exception e) {
			Debug.LogError (e);
			Debug.LogError ("Unable to load xml from string");
			return new Barrels ();
		}
	}
}

[XmlRoot ("Barrel")]
public class Barrel
{
	[XmlElement ("FerilizerType")]
	public string fertilizerType;

	[XmlElement ("HintDifficulty")]
	public int difficulty;

	[XmlElement ("HintText")]
	public string hintText;

	[XmlElement ("HintImage")]
	public string imagePath;
}
