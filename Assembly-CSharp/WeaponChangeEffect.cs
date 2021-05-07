using System.Collections;
using UnityEngine;

public class WeaponChangeEffect : MonoBehaviour
{
	[SerializeField]
	private Color _startColor = Color.white;

	[SerializeField]
	private Color _endColor = Color.white;

	[SerializeField]
	private float _duration = 0.2f;

	[SerializeField]
	private SpriteRenderer _spriteRenderer;

	[SerializeField]
	private EasingFunction.Method _runEaseMethod = EasingFunction.Method.Linear;

	private EasingFunction _runEase;

	private Material tempMaterial;

	private Material defaultMaterial;

	private static float _endTime;

	private bool _isActive;

	private const string shader = "GUI/Text Shader";

	private void Start()
	{
		_isActive = true;
		tempMaterial = new Material(Shader.Find("GUI/Text Shader"));
		tempMaterial.hideFlags = HideFlags.None;
		_spriteRenderer.material = tempMaterial;
		_spriteRenderer.color = _startColor;
	}

	private static void Initialize()
	{
	}

	private IEnumerator ChangeColor(Chronometer chronometer)
	{
		float t = 0f;
		do
		{
			yield return null;
			_spriteRenderer.color = Color.Lerp(_startColor, _endColor, t / _duration);
			t += chronometer.deltaTime;
		}
		while (!(t > _duration));
		_spriteRenderer.material = defaultMaterial;
		_spriteRenderer.color = Color.white;
		_isActive = false;
	}
}
