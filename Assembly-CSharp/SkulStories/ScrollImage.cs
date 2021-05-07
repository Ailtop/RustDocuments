using System.Collections;
using Scenes;
using UnityEngine;
using UnityEngine.UI;

namespace SkulStories
{
	public class ScrollImage : Sequence
	{
		private enum Type
		{
			Scene,
			OverlayScene
		}

		[SerializeField]
		private Type _type;

		[SerializeField]
		private float _amountX;

		[SerializeField]
		private float _amountY = 1f;

		[SerializeField]
		private float _speed;

		[SerializeField]
		private float _startDelay;

		[SerializeField]
		private Curve curve;

		private Image _scene;

		private NarrationScene _narrationScene;

		private void Start()
		{
			_narrationScene = Scene<GameBase>.instance.uiManager.narrationScene;
		}

		private void CheckType()
		{
			switch (_type)
			{
			case Type.Scene:
				_scene = _narrationScene.scene;
				break;
			case Type.OverlayScene:
				_scene = _narrationScene.overlayScene;
				break;
			}
		}

		public override IEnumerator CRun()
		{
			if (_narration.skipped)
			{
				yield break;
			}
			CheckType();
			Vector3 startPosition = _scene.transform.localPosition;
			float moveTime = 0f;
			Vector2 targetPosition = new Vector2(startPosition.x * _amountX, startPosition.y * _amountY);
			yield return Chronometer.global.WaitForSeconds(_startDelay);
			while (moveTime < 1f)
			{
				moveTime += Chronometer.global.deltaTime / _speed;
				if (_narrationScene.changed)
				{
					break;
				}
				_scene.transform.localPosition = Vector2.Lerp(startPosition, targetPosition, curve.Evaluate(moveTime));
				yield return null;
			}
			_narrationScene.scene.rectTransform.pivot = _scene.rectTransform.pivot;
			_narrationScene.scene.transform.localPosition = _scene.transform.localPosition;
		}
	}
}
