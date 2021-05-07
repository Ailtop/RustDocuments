using System.Collections;
using FX;
using Level;
using UI;
using UnityEngine;

namespace Scenes
{
	public class GameBase : Scene<GameBase>
	{
		[SerializeField]
		private UIManager _uiManager;

		[SerializeField]
		private Camera _camera;

		[SerializeField]
		private CameraController _cameraController;

		[SerializeField]
		private CameraController _minimapCameraController;

		[SerializeField]
		private GameFadeInOut _gameFadeInOut;

		private ParallaxBackground _background;

		public UIManager uiManager => _uiManager;

		public Camera camera => _camera;

		public CameraController cameraController => _cameraController;

		public CameraController minimapCameraController => _minimapCameraController;

		public GameFadeInOut gameFadeInOut => _gameFadeInOut;

		public void SetBackground(ParallaxBackground background, float originHeight)
		{
			if (_background != null)
			{
				Object.Destroy(_background.gameObject);
			}
			if (!(background == null))
			{
				_background = Object.Instantiate(background, _cameraController.transform);
				_background.transform.localPosition = new Vector3(0f, 0f, 0f - cameraController.transform.localPosition.z);
				StartCoroutine(CInitialize(originHeight));
			}
		}

		public void ChangeBackgroundWithFade(ParallaxBackground background, float originHeight)
		{
			if (_background == null)
			{
				SetBackground(background, originHeight);
				return;
			}
			StartCoroutine(_003CChangeBackgroundWithFade_003Eg__CDestroy_007C17_0(_background));
			_background = Object.Instantiate(background, _cameraController.transform);
			_background.transform.localPosition = new Vector3(0f, 0f, 0f - cameraController.transform.localPosition.z);
			StartCoroutine(CInitialize(originHeight));
			_background.FadeOut();
		}

		private IEnumerator CInitialize(float originHeight)
		{
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			_background.Initialize(originHeight);
		}
	}
}
