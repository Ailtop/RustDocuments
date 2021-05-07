using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class TextLayoutElement : MonoBehaviour, ILayoutElement
{
	[SerializeField]
	private TextMeshProUGUI _text;

	[SerializeField]
	private int _layoutPriority = 1;

	[SerializeField]
	private float _padding;

	[SerializeField]
	private float _userMinHeight;

	[SerializeField]
	private float _maxWidth = 100f;

	[SerializeField]
	private float _maxHeight = float.PositiveInfinity;

	private float _minWidth = -1f;

	private float _minHeight = -1f;

	public string text
	{
		get
		{
			return _text.text;
		}
		set
		{
			_text.text = value;
		}
	}

	public float preferredWidth => -1f;

	public float flexibleWidth => -1f;

	public float minWidth => _minWidth;

	public float minHeight => _minHeight;

	public float preferredHeight => -1f;

	public float flexibleHeight => -1f;

	public int layoutPriority => _layoutPriority;

	public void CalculateLayoutInputHorizontal()
	{
		if (_text == null)
		{
			_minWidth = -1f;
		}
		else
		{
			_minWidth = Mathf.Min(_text.preferredWidth, _text.rectTransform.rect.width) + _padding;
		}
	}

	public void CalculateLayoutInputVertical()
	{
		if (_text == null)
		{
			_minHeight = -1f;
			return;
		}
		_minHeight = _text.preferredHeight + _padding;
		if (_userMinHeight > 0f)
		{
			_minHeight = Mathf.Max(_userMinHeight, _minHeight);
		}
		_minHeight = Mathf.Min(_maxHeight, _minHeight);
	}
}
