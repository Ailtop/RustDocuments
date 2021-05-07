using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class FreeParallaxElement
{
	internal readonly List<Renderer> GameObjectRenderers = new List<Renderer>();

	[Tooltip("Game objects to parallax. These will be cycled in sequence, which allows a long rolling background or different individual objects. If there is only one, and the reposition logic specifies to wrap, a second object will be added that is a clone of the first. It is recommended that these all be the same size.")]
	public List<GameObject> GameObjects;

	[Tooltip("The speed at which this object moves in relation to the speed of the parallax.")]
	[Range(-3f, 3f)]
	[FormerlySerializedAs("SpeedRatio")]
	public float SpeedRatioX;

	[Range(-3f, 3f)]
	public float SpeedRatioY;

	public float AutoScrollX;

	public Vector2 Translated;

	[Tooltip("Contains logic on how this object repositions itself when moving off screen.")]
	public FreeParallaxElementRepositionLogic RepositionLogic;

	[HideInInspector]
	public FreeParallaxElementRepositionLogicFunction RepositionLogicFunction;

	public void SetupState(FreeParallax p, Camera c, int index)
	{
		if (RepositionLogic.PositionMode != FreeParallaxPositionMode.IndividualStartOffScreen && RepositionLogic.PositionMode != FreeParallaxPositionMode.IndividualStartOnScreen && GameObjects.Count == 1)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(GameObjects[0]);
			gameObject.transform.parent = GameObjects[0].transform.parent;
			gameObject.transform.position = GameObjects[0].transform.position;
			GameObjects.Add(gameObject);
		}
		if (GameObjectRenderers.Count != 0)
		{
			return;
		}
		foreach (GameObject gameObject2 in GameObjects)
		{
			Renderer component = gameObject2.GetComponent<Renderer>();
			if (component == null)
			{
				Debug.LogError("Null renderer found at element index " + index + ", each game object in the parallax must have a renderer");
				break;
			}
			GameObjectRenderers.Add(component);
		}
	}

	public void SetupScale(FreeParallax p, Camera c, int index)
	{
		Vector3 vector = c.ViewportToWorldPoint(Vector3.zero);
		for (int i = 0; i < GameObjects.Count; i++)
		{
			GameObject gameObject = GameObjects[i];
			Renderer renderer = GameObjectRenderers[i];
			Bounds bounds = renderer.bounds;
			if (RepositionLogic.ScaleHeight > 0f)
			{
				gameObject.transform.localScale = Vector3.one;
				float num;
				if (p.IsHorizontal)
				{
					Vector3 vector2 = c.WorldToViewportPoint(new Vector3(0f, vector.y + bounds.size.y, 0f));
					num = RepositionLogic.ScaleHeight / vector2.y;
				}
				else
				{
					Vector3 vector3 = c.WorldToViewportPoint(new Vector3(vector.x + bounds.size.x, 0f, 0f));
					num = RepositionLogic.ScaleHeight / vector3.x;
				}
				gameObject.transform.localScale = new Vector3(num, num, 1f);
				bounds = renderer.bounds;
			}
			if (RepositionLogic.PositionMode == FreeParallaxPositionMode.IndividualStartOffScreen || RepositionLogic.PositionMode == FreeParallaxPositionMode.IndividualStartOnScreen || !(SpeedRatioX > 0f))
			{
				continue;
			}
			if (p.IsHorizontal)
			{
				float x = c.WorldToViewportPoint(new Vector3(vector.x + bounds.size.x, 0f, 0f)).x;
				if (x < 1.1f)
				{
					Debug.LogWarning("Game object in element index " + index + " did not fit the screen width but was asked to wrap, so it was stretched. This can be fixed by making sure any parallax graphics that wrap are at least 1.1x times the largest width resolution you support.");
					Vector3 localScale = gameObject.transform.localScale;
					if (x != 0f)
					{
						localScale.x = localScale.x * (1f / x) + 0.1f;
					}
					gameObject.transform.localScale = localScale;
				}
				continue;
			}
			float y = c.WorldToViewportPoint(new Vector3(0f, vector.y + bounds.size.y, 0f)).y;
			if (y < 1.1f)
			{
				Debug.LogWarning("Game object in element index " + index + " did not fit the screen height but was asked to wrap, so it was stretched. This can be fixed by making sure any parallax graphics that wrap are at least 1.1x times the largest height resolution you support.");
				Vector3 localScale2 = gameObject.transform.localScale;
				if (y != 0f)
				{
					localScale2.y = localScale2.y * (1f / y) + 0.1f;
				}
				gameObject.transform.localScale = localScale2;
			}
		}
	}

	public void SetupPosition(FreeParallax p, Camera c, int index)
	{
		Vector3 vector = c.ViewportToWorldPoint(Vector3.zero);
		Vector3 vector2 = c.ViewportToWorldPoint(Vector3.one);
		float num;
		float num2;
		if (p.IsHorizontal)
		{
			num = vector2.y + 1f;
			num2 = (vector.x + vector2.x + GameObjectRenderers[0].bounds.size.x) / 2f;
		}
		else
		{
			num = vector2.x + 1f;
			num2 = (vector.y + vector2.y + GameObjectRenderers[0].bounds.size.y) / 2f;
		}
		for (int i = 0; i < GameObjects.Count; i++)
		{
			GameObject gameObject = GameObjects[i];
			Renderer renderer = GameObjectRenderers[i];
			if (RepositionLogic.SortingOrder != 0)
			{
				renderer.sortingOrder = RepositionLogic.SortingOrder;
			}
			if (RepositionLogic.PositionMode == FreeParallaxPositionMode.IndividualStartOffScreen || RepositionLogic.PositionMode == FreeParallaxPositionMode.IndividualStartOnScreen)
			{
				float x;
				float y;
				if (p.IsHorizontal)
				{
					x = ((RepositionLogic.PositionMode == FreeParallaxPositionMode.IndividualStartOnScreen) ? renderer.bounds.min.x : 0f);
					y = ((RepositionLogic.PositionMode == FreeParallaxPositionMode.IndividualStartOnScreen) ? renderer.bounds.min.y : (num + renderer.bounds.size.y));
				}
				else
				{
					x = ((RepositionLogic.PositionMode == FreeParallaxPositionMode.IndividualStartOnScreen) ? renderer.bounds.min.x : (num + renderer.bounds.size.x));
					y = ((RepositionLogic.PositionMode == FreeParallaxPositionMode.IndividualStartOnScreen) ? renderer.bounds.min.y : 0f);
				}
				FreeParallax.SetPosition(gameObject, renderer, x, y);
				continue;
			}
			num2 = ((!p.IsHorizontal) ? (num2 - (renderer.bounds.size.y - p.WrapOverlap)) : (num2 - (renderer.bounds.size.x - p.WrapOverlap)));
			gameObject.transform.rotation = Quaternion.identity;
			if (RepositionLogic.PositionMode == FreeParallaxPositionMode.WrapAnchorTop)
			{
				if (p.IsHorizontal)
				{
					FreeParallax.SetPosition(gameObject, renderer, num2, c.ViewportToWorldPoint(new Vector3(0f, 1f, 0f)).y - renderer.bounds.size.y);
				}
				else
				{
					FreeParallax.SetPosition(gameObject, renderer, c.ViewportToWorldPoint(new Vector3(1f, 0f, 0f)).x - renderer.bounds.size.x, num2 + renderer.bounds.size.y);
				}
			}
			else if (RepositionLogic.PositionMode == FreeParallaxPositionMode.WrapAnchorBottom)
			{
				if (p.IsHorizontal)
				{
					FreeParallax.SetPosition(gameObject, renderer, num2, vector.y);
				}
				else
				{
					FreeParallax.SetPosition(gameObject, renderer, vector.x, num2);
				}
			}
			else if (p.IsHorizontal)
			{
				FreeParallax.SetPosition(gameObject, renderer, num2, renderer.bounds.min.y);
			}
			else
			{
				FreeParallax.SetPosition(gameObject, renderer, renderer.bounds.min.x, num2);
			}
			GameObjects.RemoveAt(i);
			GameObjects.Insert(0, gameObject);
			GameObjectRenderers.RemoveAt(i);
			GameObjectRenderers.Insert(0, renderer);
		}
	}

	public void Randomize(FreeParallax p, Camera c)
	{
		if (p.IsHorizontal)
		{
			if (SpeedRatioX != 0f)
			{
				float num = 0f;
				for (int i = 0; i < GameObjects.Count; i++)
				{
					Bounds bounds = GameObjectRenderers[i].bounds;
					num += Math.Abs(bounds.max.x - bounds.min.x);
				}
				Update(p, new Vector2(UnityEngine.Random.Range(0f - num, num) / SpeedRatioX, 0f), c);
			}
		}
		else if (SpeedRatioY != 0f)
		{
			float num2 = 0f;
			for (int j = 0; j < GameObjects.Count; j++)
			{
				Bounds bounds2 = GameObjectRenderers[j].bounds;
				num2 += Math.Abs(bounds2.max.y - bounds2.min.y);
			}
			Update(p, new Vector2(0f, UnityEngine.Random.Range(0f - num2, num2) / SpeedRatioY), c);
		}
	}

	public void Update(FreeParallax p, Vector2 delta, Camera c)
	{
		if (GameObjects == null || GameObjects.Count == 0 || GameObjects.Count != GameObjectRenderers.Count)
		{
			return;
		}
		delta.x += AutoScrollX;
		Translated += delta;
		foreach (GameObject gameObject2 in GameObjects)
		{
			gameObject2.transform.Translate(delta.x * SpeedRatioX, delta.y * SpeedRatioY, 0f);
		}
		bool flag = RepositionLogic.PositionMode != FreeParallaxPositionMode.IndividualStartOffScreen && RepositionLogic.PositionMode != FreeParallaxPositionMode.IndividualStartOnScreen;
		float num = (flag ? 0f : 1f);
		float num2;
		float num3;
		if (p.IsHorizontal)
		{
			num2 = c.rect.x - num;
			num3 = c.rect.width + num;
		}
		else
		{
			num2 = c.rect.y - num;
			num3 = c.rect.height + num;
		}
		int num4 = GameObjects.Count;
		for (int i = 0; i < num4; i++)
		{
			GameObject gameObject = GameObjects[i];
			Renderer renderer = GameObjectRenderers[i];
			Bounds bounds = renderer.bounds;
			Vector3 vector = ((delta.x > 0f) ? c.WorldToViewportPoint(bounds.min) : c.WorldToViewportPoint(bounds.max));
			float num5 = (p.IsHorizontal ? vector.x : vector.y);
			if (flag)
			{
				if (delta.x > 0f && num5 >= num3)
				{
					if (p.IsHorizontal)
					{
						float x = GameObjectRenderers[0].bounds.min.x - renderer.bounds.size.x + p.WrapOverlap;
						FreeParallax.SetPosition(gameObject, renderer, x, renderer.bounds.min.y);
					}
					else
					{
						float y = GameObjectRenderers[0].bounds.min.y - renderer.bounds.size.y + p.WrapOverlap;
						FreeParallax.SetPosition(gameObject, renderer, renderer.bounds.min.x, y);
					}
					GameObjects.RemoveAt(i);
					GameObjects.Insert(0, gameObject);
					GameObjectRenderers.RemoveAt(i);
					GameObjectRenderers.Insert(0, renderer);
				}
				else if (delta.x < 0f && num5 <= num2)
				{
					if (p.IsHorizontal)
					{
						float x2 = GameObjectRenderers[GameObjects.Count - 1].bounds.max.x - p.WrapOverlap;
						FreeParallax.SetPosition(gameObject, renderer, x2, renderer.bounds.min.y);
					}
					else
					{
						float y2 = GameObjectRenderers[GameObjects.Count - 1].bounds.max.y - p.WrapOverlap;
						FreeParallax.SetPosition(gameObject, renderer, renderer.bounds.min.x, y2);
					}
					GameObjects.RemoveAt(i);
					GameObjects.Add(gameObject);
					GameObjectRenderers.RemoveAt(i--);
					GameObjectRenderers.Add(renderer);
					num4--;
				}
			}
			else if (p.IsHorizontal)
			{
				if (delta.x > 0f && (vector.y >= c.rect.height || num5 >= num3))
				{
					if (RepositionLogicFunction != null)
					{
						RepositionLogicFunction(p, this, delta.x, gameObject, renderer);
						continue;
					}
					Vector3 vector2 = c.ViewportToWorldPoint(Vector3.zero);
					float x3 = UnityEngine.Random.Range(RepositionLogic.MinXPercent, RepositionLogic.MaxXPercent);
					float y3 = UnityEngine.Random.Range(RepositionLogic.MinYPercent, RepositionLogic.MaxYPercent);
					Vector3 vector3 = c.ViewportToWorldPoint(new Vector3(x3, y3));
					FreeParallax.SetPosition(gameObject, renderer, vector2.x - vector3.x, vector3.y);
				}
				else if (delta.x < 0f && (vector.y >= c.rect.height || vector.x < num2))
				{
					if (RepositionLogicFunction != null)
					{
						RepositionLogicFunction(p, this, delta.x, gameObject, renderer);
						continue;
					}
					Vector3 vector4 = c.ViewportToWorldPoint(Vector3.one);
					float x4 = UnityEngine.Random.Range(RepositionLogic.MinXPercent, RepositionLogic.MaxXPercent);
					float y4 = UnityEngine.Random.Range(RepositionLogic.MinYPercent, RepositionLogic.MaxYPercent);
					Vector3 vector5 = c.ViewportToWorldPoint(new Vector3(x4, y4));
					FreeParallax.SetPosition(gameObject, renderer, vector4.x + vector5.x, vector5.y);
				}
			}
			else if (delta.x > 0f && (vector.x >= c.rect.width || num5 >= num3))
			{
				if (RepositionLogicFunction != null)
				{
					RepositionLogicFunction(p, this, delta.x, gameObject, renderer);
					continue;
				}
				Vector3 vector6 = c.ViewportToWorldPoint(Vector3.zero);
				float x5 = UnityEngine.Random.Range(RepositionLogic.MinXPercent, RepositionLogic.MaxXPercent);
				float y5 = UnityEngine.Random.Range(RepositionLogic.MinYPercent, RepositionLogic.MaxYPercent);
				Vector3 vector7 = c.ViewportToWorldPoint(new Vector3(x5, y5));
				FreeParallax.SetPosition(gameObject, renderer, vector7.x, vector6.y - vector7.y);
			}
			else if (delta.x < 0f && (vector.x >= c.rect.width || vector.y < num2))
			{
				if (RepositionLogicFunction != null)
				{
					RepositionLogicFunction(p, this, delta.x, gameObject, renderer);
					continue;
				}
				Vector3 vector8 = c.ViewportToWorldPoint(Vector3.one);
				float x6 = UnityEngine.Random.Range(RepositionLogic.MinXPercent, RepositionLogic.MaxXPercent);
				float y6 = UnityEngine.Random.Range(RepositionLogic.MinYPercent, RepositionLogic.MaxYPercent);
				Vector3 vector9 = c.ViewportToWorldPoint(new Vector3(x6, y6));
				FreeParallax.SetPosition(gameObject, renderer, vector9.x, vector8.y + vector9.y);
			}
		}
	}
}
