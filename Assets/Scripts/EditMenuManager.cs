using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EditMenuManager : MonoBehaviour
{

	[SerializeField]
	private GameObject _titlePanel;

	[SerializeField]
	private GameObject _cropImagePanel;


	void Start ()
	{
		_titlePanel.SetActive (true);
		_cropImagePanel.SetActive (false);
	}

	public void ShowDifficulties ()
	{
		_titlePanel.SetActive (false);
		_cropImagePanel.SetActive (true);
	}

    public void EditGameNewScene()
    {
        SceneManager.LoadScene("editGameScene");
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
