using System.Collections.Generic;
using UnityEngine;

public class LineTextManager : MonoBehaviour
{
	[SerializeField]
	private LineText _floatingTextPrefab;

	private Queue<LineText> _lineTexts = new Queue<LineText>(20);

	private const int _maxFloats = 20;

	private HashSet<GameObject> _locked;

	public HashSet<GameObject> locked => _locked;

	private void Awake()
	{
		_locked = new HashSet<GameObject>();
	}

	public LineText Spawn(string text, Vector3 position, float duration)
	{
		LineText lineText;
		if ((_lineTexts.Count > 0 && _lineTexts.Peek().finished) || _lineTexts.Count > 20)
		{
			lineText = _lineTexts.Dequeue();
			while (lineText == null && _lineTexts.Count > 0)
			{
				lineText = _lineTexts.Dequeue();
			}
			if (lineText == null)
			{
				lineText = Object.Instantiate(_floatingTextPrefab);
			}
		}
		else
		{
			lineText = Object.Instantiate(_floatingTextPrefab);
		}
		lineText.transform.position = position;
		lineText.Display(text, duration);
		_lineTexts.Enqueue(lineText);
		return lineText;
	}
}
