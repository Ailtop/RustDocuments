using System;
using Characters.Player;
using Data;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class CameraController : MonoBehaviour
{
	private const int _shakeBaseFps = 60;

	private const float _maxShakeInterval = 0.0166666675f;

	[NonSerialized]
	public bool pause;

	[SerializeField]
	[GetComponent]
	private Camera _camera;

	[SerializeField]
	[GetComponent]
	private PixelPerfectCamera _pixelPerfectCamera;

	[SerializeField]
	private Vector3 _offset;

	[SerializeField]
	private EasingFunction.Method _moveEaseMethod = EasingFunction.Method.Linear;

	[SerializeField]
	private float _moveSpeed = 1f;

	private float _moveTime;

	private EasingFunction _moveEase;

	[SerializeField]
	private EasingFunction.Method _zoomEaseMethod = EasingFunction.Method.Linear;

	[SerializeField]
	private float _zoomSpeed = 1f;

	private float _zoomTime;

	private float _formerZoom = 1f;

	private float _targetZoom = 1f;

	private EasingFunction _zoomEase;

	[SerializeField]
	private float _trackSpeed = 1f;

	private Transform _targetToTrack;

	private Vector3 _targetPosition;

	private Vector3 _position;

	private Vector3 _delta;

	private Vector3 _shakeAmount;

	private float _timeToNextShake;

	private PlayerCameraController _playerCameraController;

	public readonly MaxOnlyTimedFloats shake = new MaxOnlyTimedFloats();

	public CameraZone zone;

	public float trackSpeed
	{
		get
		{
			return _trackSpeed;
		}
		set
		{
			_trackSpeed = value;
		}
	}

	public Vector3 delta
	{
		get
		{
			if (!pause)
			{
				return _delta;
			}
			return Vector3.zero;
		}
	}

	public PixelPerfectCamera pixelPerfectcamera => _pixelPerfectCamera;

	public Camera camera => _camera;

	public float zoom => _pixelPerfectCamera.zoom;

	private void Awake()
	{
		_camera.transparencySortMode = TransparencySortMode.Orthographic;
		_position = base.transform.position;
		_moveEase = new EasingFunction(_moveEaseMethod);
		_zoomEase = new EasingFunction(_zoomEaseMethod);
	}

	public void Update()
	{
		if (pause)
		{
			_position = base.transform.position;
			return;
		}
		Vector3 vector;
		if ((bool)_targetToTrack)
		{
			vector = ((!(_playerCameraController == null)) ? Vector3.Lerp(_position, _playerCameraController.trackPosition, Time.unscaledDeltaTime * _playerCameraController.trackSpeed) : Vector3.Lerp(_position, _targetToTrack.position + _offset, Time.unscaledDeltaTime * _trackSpeed));
		}
		else
		{
			_moveTime += Time.unscaledDeltaTime * _moveSpeed;
			if (_moveTime >= 1f)
			{
				vector = _targetPosition;
			}
			else
			{
				float t = _moveEase.function(0f, 1f, _moveTime);
				vector = Vector3.LerpUnclamped(_position, _targetPosition, t);
			}
		}
		if (_pixelPerfectCamera != null)
		{
			_zoomTime += Time.unscaledDeltaTime * _zoomSpeed;
			if (_zoomTime >= 1f)
			{
				_pixelPerfectCamera.zoom = _targetZoom;
			}
			else
			{
				_pixelPerfectCamera.zoom = _zoomEase.function(_formerZoom, _targetZoom, _zoomTime);
			}
		}
		vector.z = _position.z;
		Vector3 position = _position;
		_position = zone?.GetClampedPosition(_camera, vector) ?? vector;
		_delta = _position - position;
		_timeToNextShake -= Time.deltaTime;
		if (_timeToNextShake < 0f)
		{
			_shakeAmount = UnityEngine.Random.insideUnitSphere * shake.value * GameData.Settings.cameraShakeIntensity;
			_shakeAmount *= 2f;
			_timeToNextShake = 0.0166666675f;
		}
		base.transform.position = _position + _shakeAmount;
		shake.Update();
	}

	public void StartTrack(Transform target)
	{
		_playerCameraController = target.GetComponent<PlayerCameraController>();
		_targetToTrack = target;
	}

	public void StopTrack()
	{
		_targetToTrack = null;
	}

	public void Move(Vector3 position)
	{
		position += _offset;
		position.z = _position.z;
		_position = (_targetPosition = position);
		_moveTime = 0f;
	}

	public void Zoom(float percent, float zoomSpeed = 1f)
	{
		_targetZoom = percent;
		_zoomSpeed = zoomSpeed;
		_zoomTime = 0f;
		_formerZoom = _pixelPerfectCamera.zoom;
	}

	public void RenderEndingScene()
	{
		_playerCameraController.RenderDeathCamera();
	}

	public void Shake(float amount, float duration)
	{
		shake.Attach(this, amount, duration);
	}
}
