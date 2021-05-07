using System;
using System.Collections;
using UnityEngine;

namespace FX.Connections
{
	[RequireComponent(typeof(BezierCurve))]
	public class FloatingBezierConnection : Connection
	{
		[SerializeField]
		[GetComponent]
		private BezierCurve _bezierCurve;

		[SerializeField]
		private float _middleYOffset = 5f;

		[SerializeField]
		private float _floatingRange = 5f;

		[SerializeField]
		private float _floatingSpeed = 0.2f;

		[SerializeField]
		private float _trackingSpeed = 1.3f;

		[SerializeField]
		private float _middleTrackingSpeed = 0.016f;

		private const float _speedCorrection = 6f;

		protected override void Show()
		{
			base.gameObject.SetActive(true);
			StartCoroutine(CShow());
		}

		protected override void Hide()
		{
			StopAllCoroutines();
			base.gameObject.SetActive(false);
		}

		private IEnumerator CShow()
		{
			Vector3 startCurrent = base.startPosition;
			Vector3 endCurrent = base.endPosition;
			int middleCount = _bezierCurve.count - 2;
			Vector2[] middleCurrents = new Vector2[middleCount];
			Vector2[] randomOffsets = new Vector2[middleCount];
			float floatingTime = 0f;
			for (int i = 0; i < middleCount; i++)
			{
				middleCurrents[i] = GetMiddlePosition(i);
				randomOffsets[i] = new Vector2(UnityEngine.Random.Range(0f, 360f), UnityEngine.Random.Range(0f, 360f));
			}
			while (!lostConnection)
			{
				startCurrent = GetTrackingVector(startCurrent, base.startPosition, _trackingSpeed);
				endCurrent = GetTrackingVector(endCurrent, base.endPosition, _trackingSpeed);
				for (int j = 0; j < middleCount; j++)
				{
					Vector2 middlePosition = GetMiddlePosition(j);
					middlePosition.y += _middleYOffset;
					middlePosition += GetFloatingOffset(floatingTime, randomOffsets[j]);
					middleCurrents[j] = GetTrackingVector(middleCurrents[j], middlePosition, _middleTrackingSpeed);
				}
				floatingTime += _floatingSpeed * 360f * Chronometer.global.deltaTime;
				_bezierCurve.SetStart(startCurrent);
				_bezierCurve.SetEnd(endCurrent);
				for (int k = 0; k < middleCount; k++)
				{
					_bezierCurve.SetVector(k + 1, middleCurrents[k]);
				}
				_bezierCurve.UpdateCurve();
				yield return null;
			}
			Disconnect();
		}

		private Vector2 GetTrackingVector(Vector2 current, Vector2 target, float speed)
		{
			Vector2 vector = (target - current) * Mathf.Min(1f, Chronometer.global.deltaTime * 6f * speed);
			return current + vector;
		}

		private Vector2 GetMiddlePosition(int index)
		{
			index++;
			float t = (float)index / (float)(_bezierCurve.count - 1);
			return Vector2.Lerp(base.startPosition, base.endPosition, t);
		}

		private Vector2 GetFloatingOffset(float floatingTime, Vector2 randomOffset)
		{
			Vector2 result = default(Vector2);
			result.x = _floatingRange * Mathf.Sin((floatingTime + randomOffset.x) * ((float)Math.PI / 180f));
			result.y = _floatingRange * Mathf.Sin((floatingTime + randomOffset.y) * ((float)Math.PI / 180f));
			return result;
		}
	}
}
