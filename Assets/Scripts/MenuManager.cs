using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{

	[SerializeField]
	private GameObject _titlePanel;

	[SerializeField]
	private GameObject _difficultyPanel;


	void Start ()
	{
		_titlePanel.SetActive (true);
		_difficultyPanel.SetActive (false);
	}

	public void ShowDifficulties ()
	{
		_titlePanel.SetActive (false);
		_difficultyPanel.SetActive (true);
	}

	public void StartEasy ()
	{
		SettingsManager.manager.LoadEasy ();
		StartScene ();
	}

	public void StartMedium ()
	{
		SettingsManager.manager.LoadMedium ();
		StartScene ();
	}

	public void StartHard ()
	{
		SettingsManager.manager.LoadHard ();
		StartScene ();
	}

	public void StartScene ()
	{
		SceneManager.LoadScene ("gameScene");
	}
}
