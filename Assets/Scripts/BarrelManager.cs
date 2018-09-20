using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarrelManager : MonoBehaviour
{

	public string correctAnswer = "";

	[SerializeField]
	private Text hintField;

	[SerializeField]
	private GameObject _textHint;

	[SerializeField]
	private Image _background;

	[SerializeField]
	private Image _imageHint;

	public void SetHintText (string hints)
	{
		hintField.text = hints;

	}

	public void TextHint ()
	{
		_textHint.SetActive (true);
		_imageHint.gameObject.SetActive (false);
		_background.enabled = true;
	}

	public void SetHintImage (Sprite image)
	{
		_imageHint.sprite = image;
	}

	public void ImageHint ()
	{
		_textHint.SetActive (false);
		_imageHint.gameObject.SetActive (true);
		_background.enabled = false;
	}


}
