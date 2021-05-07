using System;
using Scenes;
using UnityEngine;

public class CastleParallax : MonoBehaviour
{
	[Serializable]
	private class Element
	{
		[Serializable]
		internal class Reorderable : ReorderableArray<Element>
		{
		}

		[SerializeField]
		private SpriteRenderer _spriteRenderer;

		[SerializeField]
		private float _verticalScroll;

		[SerializeField]
		private float _hotizontalAutoScroll;

		private Vector2 _spriteSize;

		internal void Initialize()
		{
			if (_hotizontalAutoScroll != 0f)
			{
				_spriteRenderer.drawMode = SpriteDrawMode.Tiled;
				_spriteRenderer.tileMode = SpriteTileMode.Continuous;
				_spriteSize = _spriteRenderer.sprite.bounds.size;
				_spriteSize.x *= 2f;
				if (_hotizontalAutoScroll >= 0f)
				{
					_spriteRenderer.size = _spriteSize;
				}
				else
				{
					_spriteRenderer.size = -_spriteSize;
				}
			}
		}

		internal void Update(Vector3 delta, float deltaTime)
		{
			_spriteRenderer.transform.Translate(0f, _verticalScroll * delta.y, 0f);
			if (_hotizontalAutoScroll != 0f)
			{
				Vector2 size = _spriteRenderer.size;
				size.x += _hotizontalAutoScroll * deltaTime;
				if (_hotizontalAutoScroll > 0f && size.x >= _spriteSize.x * 2f)
				{
					size.x = _spriteSize.x;
				}
				if (_hotizontalAutoScroll < 0f && size.x <= _spriteSize.x * 2f)
				{
					size.x = _spriteSize.x * 2f * 2f;
				}
				_spriteRenderer.size = size;
			}
		}
	}

	[SerializeField]
	private Transform _origin;

	[SerializeField]
	private Element.Reorderable _elements;

	private CameraController _cameraController;

	private void Awake()
	{
		Element[] values = _elements.values;
		for (int i = 0; i < values.Length; i++)
		{
			values[i].Initialize();
		}
		_cameraController = Scene<GameBase>.instance.cameraController;
		UpdateElements(_cameraController.transform.position - _origin.position, 0f);
	}

	private void Update()
	{
		UpdateElements(_cameraController.delta, Chronometer.global.deltaTime);
	}

	private void UpdateElements(Vector3 delta, float deltaTime)
	{
		Element[] values = _elements.values;
		for (int i = 0; i < values.Length; i++)
		{
			values[i].Update(delta, deltaTime);
		}
	}
}
