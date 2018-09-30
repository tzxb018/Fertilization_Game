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

    [SerializeField]
    private GameObject _button1;

    [SerializeField]
    private GameObject _button2;


	void Start ()
	{
        _titlePanel.SetActive(true);
        _cropImagePanel.SetActive(false);
    }

    public void ShowEditImagePanel ()
	{
		_titlePanel.SetActive (false);
		_cropImagePanel.SetActive (true);
	}

    public void EditGameNewScene()
    {
        SceneManager.LoadScene("editGameScene");
    }

	public void StartScene ()
	{
		SceneManager.LoadScene ("gameScene");
	}
}
