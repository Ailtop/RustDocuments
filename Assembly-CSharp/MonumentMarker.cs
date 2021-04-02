using Facepunch;
using UnityEngine;
using UnityEngine.UI;

public class MonumentMarker : MonoBehaviour
{
	public Text text;

	public Image imageBackground;

	public Image image;

	public Color dayColor;

	public Color nightColor;

	public void Setup(LandmarkInfo info)
	{
		text.text = (info.displayPhrase.IsValid() ? info.displayPhrase.translated : info.transform.root.name);
		if (info.mapIcon != null)
		{
			image.sprite = info.mapIcon;
			text.SetActive(false);
			imageBackground.SetActive(true);
		}
		else
		{
			text.SetActive(true);
			imageBackground.SetActive(false);
		}
		SetNightMode(false);
	}

	public void SetNightMode(bool nightMode)
	{
		Color color = (nightMode ? nightColor : dayColor);
		Color color2 = (nightMode ? dayColor : nightColor);
		if (text != null)
		{
			text.color = color;
		}
		if (image != null)
		{
			image.color = color;
		}
		if (imageBackground != null)
		{
			imageBackground.color = color2;
		}
	}
}
