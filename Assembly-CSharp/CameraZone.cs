using System;
using Scenes;
using UI;
using UnityEngine;

public class CameraZone : MonoBehaviour
{
	private enum HorizontalAlign
	{
		Center,
		Left,
		Right
	}

	private enum VerticalAlign
	{
		Center,
		Bottom,
		Top
	}

	[SerializeField]
	[GetComponent]
	private BoxCollider2D _zone;

	[SerializeField]
	private HorizontalAlign _horizontalAlign;

	[SerializeField]
	private VerticalAlign _verticalAlign = VerticalAlign.Bottom;

	[SerializeField]
	private bool _hasCeil;

	[NonSerialized]
	public Bounds bounds;

	public bool hasCeil
	{
		get
		{
			return _hasCeil;
		}
		set
		{
			_hasCeil = value;
		}
	}

	private void Awake()
	{
		if (_zone != null)
		{
			bounds = _zone.bounds;
			UnityEngine.Object.Destroy(_zone);
		}
	}

	public Vector3 GetClampedPosition(Camera camera, Vector3 position)
	{
		float orthographicSize = camera.orthographicSize;
		UIManager uiManager = Scene<GameBase>.instance.uiManager;
		Vector2 sizeDelta = uiManager.rectTransform.sizeDelta;
		Vector2 sizeDelta2 = uiManager.content.sizeDelta;
		float num = sizeDelta2.x / sizeDelta2.y;
		float num2 = orthographicSize * 2f * sizeDelta2.y / sizeDelta.y;
		float num3 = num2 * num;
		Vector3 max = bounds.max;
		max.x -= num3 * 0.5f;
		if (_hasCeil)
		{
			max.y -= num2 * 0.5f;
		}
		else
		{
			max.y = float.PositiveInfinity;
		}
		Vector3 min = bounds.min;
		min.x += num3 * 0.5f;
		min.y += num2 * 0.5f;
		float z = position.z;
		position = Vector3.Max(Vector3.Min(position, max), min);
		position.z = z;
		if (bounds.size.x < num3)
		{
			switch (_horizontalAlign)
			{
			case HorizontalAlign.Center:
				position.x = bounds.center.x;
				break;
			case HorizontalAlign.Left:
				position.x = min.x;
				break;
			case HorizontalAlign.Right:
				position.x = max.x;
				break;
			}
		}
		if (bounds.size.y < num2)
		{
			switch (_verticalAlign)
			{
			case VerticalAlign.Center:
				position.y = bounds.center.y;
				break;
			case VerticalAlign.Bottom:
				position.y = min.y;
				break;
			case VerticalAlign.Top:
				position.y = max.y;
				break;
			}
		}
		return position;
	}

	public void ClampPosition(Camera camera)
	{
		camera.transform.position = GetClampedPosition(camera, camera.transform.position);
	}
}
