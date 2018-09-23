using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	[Header ("Game Variable configuration")]
	public float speedModifier = 1f;
	public float scoreModifier = 400f;
	public float barrelSpawnDelay = 3f;
	// -1 represents infinite
	public int maxBarrels = 4;
	public int maxLives = 5;
	public int barrelsRequired = 10;

	[Range (0, 1)]
	public float correctSoundVolume = .5f;

	[Range (0, 1)]
	public float incorrectSoundVolume = .5f;

	[Header ("Color Configuration")]
	public Color endGameLoseColor;
	public Color endGameWinColor;

	[Header ("Game Objects")]
	[SerializeField]
	private Camera _mainCamera;

	[SerializeField]
	private Image _endgameScreen;

	[SerializeField]
	private GameObject _failScreen;
	[SerializeField]
	private Text _failScore;

	[SerializeField]
	private GameObject _successScreen;
	[SerializeField]
	private Text _successScore;

	[SerializeField]
	private Text _score;

	[SerializeField]
	private GameObject _barrelPrefab;

	[SerializeField]
	private GameObject _answerButtonPrefab;

	[SerializeField]
	private GameObject _answerButtonHolder;

	[SerializeField]
	private GameObject _explosionPrefab;

	[SerializeField]
	private AudioClip _correctSound;

	[SerializeField]
	private AudioClip _incorrectSound;

	[SerializeField]
	private Slider _healthSlider;

	private AudioSource _audioPlayer;

	private Queue<BarrelManager> barrels;

	private List<string> allowedFertilizers = new List<string> ();

	private float score = 0f;
	private int lives;

	private int barrelIndex = 0;

	private int correctBarrelCount = 0;

	private float barrelSpawn = 0f;

	private bool gameOver = false;

	// Initialize some basic parameters
	void Awake ()
	{
		_audioPlayer = GetComponent<AudioSource> ();
		lives = maxLives;
		barrels = new Queue<BarrelManager> (10);
	}

	// Generate GUI response buttons based on input from json and difficulty selected
	void InitializeAnswerButtons ()
	{
		int count = 0;
		foreach (string type in SettingsManager.manager.fertilizerTypeNames) {
			GameObject newAnswerButton = Instantiate (_answerButtonPrefab);
			newAnswerButton.SetActive (true);
			newAnswerButton.GetComponentInChildren<Text> (true).text = type;

			newAnswerButton.GetComponent<Button> ().onClick.RemoveAllListeners ();
			newAnswerButton.GetComponent<Button> ().onClick.AddListener (delegate {
				ButtonClicked (type);
			});

			newAnswerButton.gameObject.SetActive (true);
			newAnswerButton.transform.SetParent (_answerButtonHolder.transform);

			allowedFertilizers.Add (type);
			count++;
			if (count >= SettingsManager.manager.fertilizerCount)
				return;
		}
	}

	// Load persistant settings from the settings manager
	void RetrieveSettings ()
	{
		Debug.Log ("Loading Settings");
		scoreModifier = SettingsManager.manager.currentScoreModifier;
		speedModifier = SettingsManager.manager.currentGameSpeed;
		barrelsRequired = Mathf.CeilToInt (SettingsManager.manager.currentBarrelsRequired);
		maxLives = Mathf.CeilToInt (SettingsManager.manager.currentMaxLives);
		barrelSpawnDelay = SettingsManager.manager.currentBarrelSpawnDelay;
		maxBarrels = Mathf.CeilToInt (SettingsManager.manager.currentMaxSimultaneousBarrels);


		score = SettingsManager.manager.currentScore;
	}

	// Set up first frame conditions
	void Start ()
	{
		gameOver = false;
		RetrieveSettings ();
		InitializeAnswerButtons ();
		InitializeHealth ();
		CalculateScore ();
		MoveBarrels ();
		CalculateBarrelCrash ();
		DetermineBarrelSpawnConditions ();
		CreateBarrel ();
	}

	// Called every frame to update game visuals and check conditions
	void LateUpdate ()
	{
		if (!gameOver) {
			CalculateScore ();
			MoveBarrels ();
			CalculateBarrelCrash ();
			DetermineBarrelSpawnConditions ();
		}
		DisplayHealth ();
	}

	// Set slider to intitial conditions
	void InitializeHealth ()
	{
		_healthSlider.maxValue = maxLives;
		_healthSlider.value = maxLives;
	}

	// Display health slider based on life variable
	void DisplayHealth ()
	{
		_healthSlider.value = lives;
	}

	// Called when too many lives are lost.  Shows end game screen and stops game.
	void FailConditionMet ()
	{
		_endgameScreen.gameObject.SetActive (true);
		_endgameScreen.color = endGameLoseColor;
		_failScreen.SetActive (true);
		_successScreen.SetActive (false);
		gameOver = true;
	}

	// Called when enough barrels are answered correctly.  Shows success screen and stops game.
	void SuccessConditionMet ()
	{
		_endgameScreen.gameObject.SetActive (true);
		_endgameScreen.color = endGameWinColor;
		_failScreen.SetActive (false);
		_successScreen.SetActive (true);
		gameOver = true;
	}


	// When a correct answer is chosen by button press. Plays audio, increments score, increments progress, destroys barrel, and checks success conditions.
	void HandleCorrectAnswer (BarrelManager barrel)
	{
		_audioPlayer.PlayOneShot (_correctSound, correctSoundVolume);
		score += scoreModifier;
		correctBarrelCount++;
		Destroy (barrel.gameObject);
		if (correctBarrelCount >= barrelsRequired)
			SuccessConditionMet ();
	}

	// When an incorrect answer is chosen by button press or bottom of screen being reached. Plays audio, destroys barrel, decreases lives, and checks fail conditions.
	void HandleIncorrectAnswer (BarrelManager barrel)
	{
		Instantiate (_explosionPrefab).transform.position = barrel.transform.position;

		_audioPlayer.PlayOneShot (_incorrectSound, incorrectSoundVolume);

		if (lives <= 1) {
			FailConditionMet ();
			lives = 0;
		} else {
			lives -= 1;
		}

		//score -= scoreModifier;
		Destroy (barrel.gameObject);
	}

	// Logs score to file, clears persisted score, and returns to main menu.
	public void LoadMenu ()
	{
		SettingsManager.manager.LogScore (score);
		SettingsManager.manager.currentScore = 0;
		SceneManager.LoadScene ("menuScene");
	}

	// Logs score to file, clears persisted score, and restarts level.
	public void RestartLevel ()
	{
		SettingsManager.manager.LogScore (score);
		SettingsManager.manager.currentScore = 0;
		SettingsManager.manager.currentStage = 0f;
		SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
	}

	// Persists score and increments stage while restarting.
	public void NextStage ()
	{
		SettingsManager.manager.currentScore = score;
		SettingsManager.manager.IncrementDifficultyAndRestart ();	
	}

	// Handles a button click
	public void ButtonClicked (string type)
	{
		// Debug.Log (type);
		if (gameOver)
			return;
		if (barrels.Count == 0)
			return;

		BarrelManager nextBarrel = barrels.Dequeue ();

		if (nextBarrel.correctAnswer == type) {
			HandleCorrectAnswer (nextBarrel);
		} else {
			HandleIncorrectAnswer (nextBarrel);
		}
	}

	// Displays score in text format on multiple screens.
	void CalculateScore ()
	{
		_failScore.text = _successScore.text = _score.text = "Score: " + score.ToString ("N0");
	}

	// Determines if a barrel has reached the bottom of the screen
	void CalculateBarrelCrash ()
	{
		if (barrels.Count == 0)
			return;

		BarrelManager nextBarrel = barrels.Peek ();
		if (nextBarrel.gameObject.transform.position.y <= -_mainCamera.orthographicSize * 0.87f) {
			do {
				barrels.Dequeue ();
				HandleIncorrectAnswer (nextBarrel);

				if (barrels.Count == 0)
					break;
				nextBarrel = barrels.Peek ();
			} while(nextBarrel.gameObject.transform.position.y <= -_mainCamera.orthographicSize);
		}
	}

	// Moves barrel downwards in accordance with delta time and speed modifier
	void MoveBarrels ()
	{
		foreach (BarrelManager barrel in barrels) {
			Vector3 newPos = barrel.gameObject.transform.position;
			newPos.y -= Time.deltaTime * speedModifier;
			barrel.gameObject.transform.position = newPos;
		}

	}

	// Calculate whether a barrel should spawn
	void DetermineBarrelSpawnConditions ()
	{
		barrelSpawn += speedModifier * Time.deltaTime;
		if (barrelSpawn >= barrelSpawnDelay && (maxBarrels == -1 || barrels.Count <= maxBarrels)) {
			barrelSpawn = 0f;
			CreateBarrel ();
		}
	}

	// Creates a barrel from xml file and difficulty
	void CreateBarrel ()
	{
		GameObject newBarrel = Instantiate (_barrelPrefab);
		BarrelManager newBarrelScript = newBarrel.GetComponent<BarrelManager> ();
		// Initialize barrel physical state

		float xPos = Random.Range (-_mainCamera.orthographicSize * Screen.width / Screen.height + newBarrel.transform.localScale.x / 2, _mainCamera.orthographicSize * Screen.width / Screen.height - newBarrel.transform.localScale.x / 2);

		newBarrel.transform.position = new Vector3 (xPos, _mainCamera.orthographicSize + newBarrel.transform.localScale.y / 2, barrelIndex * 0.0001f);



		// Initialize barrel script state (Choose barrel hint and correct answer here)
	
		int preferredDifficulty = Mathf.FloorToInt (SettingsManager.manager.currentStage + 1f);
		Barrel barrelData = HintHelper.manager.GetRandomBarrelFromDifficulty (preferredDifficulty, 1, allowedFertilizers);


		if (barrelData == null) {
			Destroy (newBarrel);
			return;
		}

		bool success = false;
		if (success == false && !string.IsNullOrEmpty (barrelData.imagePath)) {
			Sprite imageHint = HintHelper.manager.LoadSpriteByName (barrelData.imagePath);
			if (imageHint != null) {
				newBarrelScript.SetHintImage (imageHint);
				newBarrelScript.ImageHint ();
				success = true;
			}
		}
		if (success == false && !string.IsNullOrEmpty (barrelData.hintText)) {
			newBarrelScript.SetHintText (barrelData.hintText);
			newBarrelScript.TextHint ();
			success = true;
		}

		if (success == false) {
			Destroy (newBarrel);
			return;
		}

		//newBarrelScript.SetHintText ("- A: " + SettingsManager.manager.fertilizerTypeNames [selectedType]);
		//newBarrelScript.TextHint ();

		newBarrelScript.correctAnswer = barrelData.fertilizerType;


		// Show barrel
		newBarrel.SetActive (true);

		barrels.Enqueue (newBarrelScript);

		barrelIndex++;
	}



}
