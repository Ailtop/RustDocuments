using System;
using System.Collections;
using Characters;
using Services;
using Singletons;
using UnityEngine;

namespace CutScenes.SpecialMap
{
	public class FollowMovement : MonoBehaviour
	{
		[Serializable]
		private class Target
		{
			public enum Type
			{
				Player,
				Character,
				Transform
			}

			[SerializeField]
			private Type _type;

			[SerializeField]
			private Character _character;

			[SerializeField]
			private Transform _transform;

			internal Transform GetTransform()
			{
				switch (_type)
				{
				case Type.Transform:
					return _transform;
				case Type.Player:
					return Singleton<Service>.Instance.levelManager.player.transform;
				case Type.Character:
					return _character.transform;
				default:
					return Singleton<Service>.Instance.levelManager.player.transform;
				}
			}
		}

		[SerializeField]
		private bool _floatingOnStop;

		[SerializeField]
		private Transform _body;

		[SerializeField]
		private bool _startOnAwake;

		[SerializeField]
		private Target _target;

		[SerializeField]
		private CustomFloat _offsetX;

		[SerializeField]
		private CustomFloat _offsetY;

		[SerializeField]
		private float _trackSpeed;

		private void Awake()
		{
			if (_startOnAwake)
			{
				Run();
			}
		}

		public void Run()
		{
			StartCoroutine("CChase");
			if (_floatingOnStop)
			{
				StartCoroutine("CFloat");
			}
		}

		public void Stop()
		{
			StopCoroutine("CChase");
			if (_floatingOnStop)
			{
				StopCoroutine("CFloat");
			}
		}

		private IEnumerator CChase()
		{
			float elapsed = 0f;
			Transform targetTransform = _target.GetTransform();
			float offsetX = _offsetX.value;
			float offsetY = _offsetY.value;
			while (true)
			{
				float deltaTime = Chronometer.global.deltaTime;
				elapsed += deltaTime;
				Vector2 vector = new Vector2(targetTransform.position.x + offsetX, targetTransform.position.y + offsetY);
				base.transform.position = Vector3.Lerp(base.transform.position, vector, deltaTime * _trackSpeed);
				yield return null;
			}
		}

		private IEnumerator CFloat()
		{
			float t = 0f;
			float _floatAmplitude = 0.5f;
			float _floatFrequency = 0.8f;
			while (true)
			{
				Vector3 zero = Vector3.zero;
				t += Chronometer.global.deltaTime;
				zero.y = Mathf.Sin(t * (float)Math.PI * _floatFrequency) * _floatAmplitude;
				_body.localPosition = zero;
				yield return null;
			}
		}
	}
}
