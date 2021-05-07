using System.Collections;
using Scenes;
using TMPro;
using UnityEngine;

public class LineText : MonoBehaviour
{
	[SerializeField]
	private TextMeshPro _text;

	[SerializeField]
	private SpriteRenderer _speechBubble;

	[SerializeField]
	private float _minWidth = 2f;

	[SerializeField]
	private float _maxWidth = 40f;

	[SerializeField]
	private float _minHeight = 25f / 32f;

	private CoroutineReference? _displayCoroutine;

	public bool finished { get; private set; }

	private void Awake()
	{
		Hide();
	}

	public void Display(string text, float duration)
	{
		_displayCoroutine?.Stop();
		if (!Scene<GameBase>.instance.uiManager.npcConversation.visible)
		{
			_displayCoroutine = this.StartCoroutineWithReference(CDisplay(text, duration));
		}
	}

	public IEnumerator CDisplay(string text, float duration)
	{
		if (!Scene<GameBase>.instance.uiManager.npcConversation.visible)
		{
			Show(text);
			yield return Chronometer.global.WaitForSeconds(duration);
			Hide();
		}
	}

	private void Show(string text)
	{
		finished = false;
		_text.text = text;
		_speechBubble.size = ResizeDisplayField(Mathf.Clamp(_minWidth, _text.preferredWidth + 0.5f, _maxWidth), Mathf.Max(_minHeight, _text.preferredHeight + 0.5f));
		_text.gameObject.SetActive(true);
		_speechBubble.gameObject.SetActive(true);
	}

	public void Hide()
	{
		finished = true;
		_text.text = "";
		_text.gameObject.SetActive(false);
		_speechBubble.gameObject.SetActive(false);
	}

	private void OnDisable()
	{
		Hide();
	}

	private Vector2 ResizeDisplayField(float width, float height)
	{
		return new Vector2(width, height);
	}
}
