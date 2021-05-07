using System.Collections;
using Characters;
using Data;
using Level;
using Scenes;
using Services;
using Singletons;
using UnityEngine;

public class CastleCameraController : MonoBehaviour
{
	private enum State
	{
		Inside,
		OutsideDiving,
		Outside
	}

	[SerializeField]
	private CameraZone _inside;

	[SerializeField]
	private BoxCollider2D _outsideDiving;

	[SerializeField]
	private CameraZone _outsideDivingCameraZone;

	[SerializeField]
	private BoxCollider2D _outside;

	[SerializeField]
	private CameraZone _outsideCameraZone;

	[SerializeField]
	private Collider2D _portal;

	[SerializeField]
	private AnimationCurve _curve;

	[SerializeField]
	private SpriteRenderer _cover;

	private State _state;

	private CameraController _cameraController;

	private Color _globalLight;

	private void Awake()
	{
		_cameraController = Scene<GameBase>.instance.cameraController;
	}

	private IEnumerator CLerpGlobalLight()
	{
		yield return null;
		Map map = Map.Instance;
		Character player = Singleton<Service>.Instance.levelManager.player;
		while (true)
		{
			float t = player.transform.position.y / -30f;
			map.globalLight.color = Color.Lerp(map.originalLightColor, Color.white, t);
			map.globalLight.intensity = Mathf.Lerp(map.originalLightIntensity, 1f, t);
			yield return null;
		}
	}

	private IEnumerator CUpdate()
	{
		yield return null;
		while (true)
		{
			Vector3 position = Singleton<Service>.Instance.levelManager.player.transform.position;
			Color color;
			if (_inside.bounds.Contains(position))
			{
				if (_state != 0)
				{
					_state = State.Inside;
					float time3 = 0f;
					color = _cover.color;
					_cameraController.pause = true;
					_cameraController.zone = null;
					Vector3 originalPosition2 = _cameraController.transform.position;
					Vector3 targetPosition2 = _inside.GetClampedPosition(_cameraController.camera, _cameraController.transform.position);
					for (; time3 < 1f; time3 += Time.unscaledDeltaTime * 1.5f)
					{
						yield return null;
						_cameraController.transform.position = Vector3.Lerp(originalPosition2, targetPosition2, _curve.Evaluate(time3));
					}
					for (; time3 < 1f; time3 += Time.unscaledDeltaTime * 1.5f)
					{
						yield return null;
						color.a = time3;
						_cover.color = color;
						_cameraController.transform.position = Vector3.Lerp(originalPosition2, targetPosition2, time3);
					}
					_cameraController.pause = false;
					_cameraController.zone = _inside;
					color = default(Color);
				}
			}
			else if (_outsideDiving.bounds.Contains(position))
			{
				if (_state != State.OutsideDiving)
				{
					_state = State.OutsideDiving;
					float time3 = 0f;
					color = _cover.color;
					_cameraController.pause = true;
					_cameraController.zone = null;
					Vector3 targetPosition2 = _cameraController.transform.position;
					Vector3 originalPosition2 = _outsideDivingCameraZone.GetClampedPosition(_cameraController.camera, _cameraController.transform.position);
					for (; time3 < 1f; time3 += Time.unscaledDeltaTime * 1.5f)
					{
						yield return null;
						color.a = time3;
						_cover.color = color;
						_cameraController.transform.position = Vector3.Lerp(targetPosition2, originalPosition2, _curve.Evaluate(time3));
					}
					_cameraController.pause = false;
					_cameraController.zone = _outsideDivingCameraZone;
					color = default(Color);
				}
			}
			else if (_outside.bounds.Contains(position))
			{
				_state = State.Outside;
				_cameraController.zone = _outsideCameraZone;
				if (_portal.bounds.Contains(position))
				{
					for (float time3 = 0f; time3 < 1f; time3 += Time.unscaledDeltaTime)
					{
						yield return null;
					}
					if (GameData.Generic.tutorial.isPlayed())
					{
						Singleton<Service>.Instance.levelManager.Load(Chapter.Type.Chapter1);
					}
					else
					{
						Singleton<Service>.Instance.levelManager.Load(Chapter.Type.Tutorial);
					}
				}
			}
			yield return null;
		}
	}

	private void OnEnable()
	{
		StartCoroutine(CUpdate());
		StartCoroutine(CLerpGlobalLight());
	}
}
