using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeParallax : MonoBehaviour
{
	[Tooltip("Camera to use for the parallax. Defaults to main camera.")]
	public Camera parallaxCamera;

	[Tooltip("The speed at which the parallax moves, which will likely be opposite from the speed at which your character moves. Elements can be set to move as a percentage of this value.")]
	public Vector2 Speed;

	[Tooltip("Randomize position on initialize")]
	public bool randomize = true;

	[Tooltip("The elements in the parallax.")]
	public List<FreeParallaxElement> Elements;

	[Tooltip("Whether the parallax moves horizontally or vertically. Horizontal moves left and right, vertical moves up and down.")]
	public bool IsHorizontal = true;

	[Tooltip("The overlap in world units for wrapping elements. This can help fix rare one pixel gaps.")]
	public float WrapOverlap;

	public CameraController cameraController;

	private float _originHeight;

	private SpriteRenderer[] _spriteRenderers;

	private void Awake()
	{
		_spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
	}

	private void SetFadeAlpha(float t)
	{
		SpriteRenderer[] spriteRenderers = _spriteRenderers;
		foreach (SpriteRenderer obj in spriteRenderers)
		{
			Color color = obj.color;
			Color color3 = (obj.color = new Color(color.r, color.g, color.b, t));
		}
	}

	public void FadeIn()
	{
		StartCoroutine(CFadeIn());
	}

	public IEnumerator CFadeIn()
	{
		float t = 0f;
		SetFadeAlpha(1f);
		yield return null;
		for (; t < 1f; t += Time.unscaledDeltaTime * 0.3f)
		{
			SetFadeAlpha(1f - t);
			yield return null;
		}
		SetFadeAlpha(0f);
	}

	public void FadeOut()
	{
		StartCoroutine(CFadeOut());
	}

	public IEnumerator CFadeOut()
	{
		float t = 0f;
		SetFadeAlpha(0f);
		yield return null;
		for (; t < 1f; t += Time.unscaledDeltaTime * 0.3f)
		{
			SetFadeAlpha(t);
			yield return null;
		}
		SetFadeAlpha(1f);
	}

	private void SetupElementAtIndex(int i)
	{
		FreeParallaxElement freeParallaxElement = Elements[i];
		if (freeParallaxElement.GameObjects == null || freeParallaxElement.GameObjects.Count == 0)
		{
			Debug.LogError("No game objects found at element index " + i + ", be sure to set at least one game object for each element in the parallax");
			return;
		}
		foreach (GameObject gameObject in freeParallaxElement.GameObjects)
		{
			if (gameObject == null)
			{
				Debug.LogError("Null game object found at element index " + i);
				return;
			}
		}
		freeParallaxElement.SetupState(this, parallaxCamera, i);
		freeParallaxElement.SetupScale(this, parallaxCamera, i);
		freeParallaxElement.SetupPosition(this, parallaxCamera, i);
	}

	public void Reset()
	{
		SetupElements(false);
	}

	public void SetupElements(bool randomize)
	{
		if (parallaxCamera == null)
		{
			parallaxCamera = Camera.main;
			if (parallaxCamera == null)
			{
				Debug.LogError("Cannot run parallax without a camera");
				return;
			}
		}
		if (Elements == null || Elements.Count == 0)
		{
			return;
		}
		for (int i = 0; i < Elements.Count; i++)
		{
			SetupElementAtIndex(i);
			if (randomize)
			{
				Elements[i].Randomize(this, parallaxCamera);
			}
		}
	}

	public void AddElement(FreeParallaxElement e)
	{
		if (Elements == null)
		{
			Elements = new List<FreeParallaxElement>();
		}
		int count = Elements.Count;
		Elements.Add(e);
		SetupElementAtIndex(count);
	}

	public static void SetPosition(GameObject obj, Renderer r, float x, float y)
	{
		Vector3 position = new Vector3(x, y, obj.transform.position.z);
		obj.transform.position = position;
		float num = r.bounds.min.x - obj.transform.position.x;
		if (num != 0f)
		{
			position.x -= num;
			obj.transform.position = position;
		}
		float num2 = r.bounds.min.y - obj.transform.position.y;
		if (num2 != 0f)
		{
			position.y -= num2;
			obj.transform.position = position;
		}
	}

	public void Initialize(float originHeight)
	{
		_originHeight = originHeight;
		SetupElements(randomize);
		Translate(new Vector2(0f, 0f - originHeight));
	}

	public void Translate(Vector2 delta)
	{
		for (int i = 0; i < Elements.Count; i++)
		{
			Elements[i].Update(this, delta, parallaxCamera);
		}
	}

	private void Randomize()
	{
		for (int i = 0; i < Elements.Count; i++)
		{
			Elements[i].Randomize(this, parallaxCamera);
		}
	}

	private void Update()
	{
		Vector3 delta = cameraController.delta;
		delta.x *= Speed.x;
		delta.y *= Speed.y;
		foreach (FreeParallaxElement element in Elements)
		{
			element.Update(this, delta, parallaxCamera);
		}
	}
}
