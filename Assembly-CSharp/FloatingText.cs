using System.Collections;
using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
	private const float duration = 0.5f;

	private static readonly WaitForSeconds waitForDuration = new WaitForSeconds(0.5f);

	[SerializeField]
	[GetComponent]
	private PoolObject _poolObject;

	[SerializeField]
	private TextMeshPro _text;

	public PoolObject poolObject => _poolObject;

	public Color color
	{
		get
		{
			return _text.color;
		}
		set
		{
			_text.color = value;
		}
	}

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

	public int sortingOrder
	{
		get
		{
			return _text.sortingOrder;
		}
		set
		{
			_text.sortingOrder = value;
		}
	}

	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}

	public FloatingText Spawn()
	{
		return _poolObject.Spawn().GetComponent<FloatingText>();
	}

	public void Despawn()
	{
		_poolObject.Despawn();
	}

	public void Despawn(float seconds)
	{
		StartCoroutine(CDespawn(seconds));
	}

	private IEnumerator CDespawn(float seconds)
	{
		yield return Chronometer.global.WaitForSeconds(seconds);
		Despawn();
	}

	public void FadeOut(float duration)
	{
		StartCoroutine(CFadeOut(duration));
	}

	private IEnumerator CFadeOut(float duration)
	{
		float t = duration;
		SetFadeAlpha(1f);
		yield return null;
		while (t > 0f)
		{
			SetFadeAlpha(t / duration);
			yield return null;
			t -= Chronometer.global.deltaTime;
		}
		SetFadeAlpha(0f);
	}

	private void SetFadeAlpha(float alpha)
	{
		Color color = this.color;
		color.a = alpha;
		this.color = color;
	}

	public void Initialize(string text, Vector3 position)
	{
		StopAllCoroutines();
		_text.text = text;
		base.transform.position = position;
		base.transform.localScale = Vector3.one;
		base.gameObject.SetActive(true);
	}

	public void Initialize(string text, Vector3 position, Color color)
	{
		Initialize(text, position);
		_text.color = color;
	}
}
