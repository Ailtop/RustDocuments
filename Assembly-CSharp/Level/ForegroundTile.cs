using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Level
{
	public class ForegroundTile : MonoBehaviour
	{
		[SerializeField]
		private Tilemap _tilemap;

		[SerializeField]
		private float _alphaChangeTime = 0.3f;

		private Coroutine _alphaChange;

		private int _triggerCount;

		private float _alpha
		{
			get
			{
				return _tilemap.color.a;
			}
			set
			{
				Color color = _tilemap.color;
				color.a = value;
				_tilemap.color = color;
			}
		}

		private void Active()
		{
			if (_alphaChange != null)
			{
				StopCoroutine(_alphaChange);
			}
			_alphaChange = StartCoroutine(CAlphaChange(1f, _alphaChangeTime));
		}

		private void Inactive()
		{
			if (_alphaChange != null)
			{
				StopCoroutine(_alphaChange);
			}
			_alphaChange = StartCoroutine(CAlphaChange(0f, _alphaChangeTime));
		}

		private IEnumerator CAlphaChange(float targetAlpha, float time)
		{
			float startTime = Time.time;
			float startAlpha = _alpha;
			float num = Mathf.Abs(targetAlpha - startAlpha);
			float newTime = time * num;
			float spendTime = Time.time - startTime;
			while (spendTime < newTime)
			{
				_alpha = startAlpha + (targetAlpha - startAlpha) * (spendTime / newTime);
				spendTime = Time.time - startTime;
				yield return null;
			}
			_alpha = targetAlpha;
		}

		private void OnTriggerEnter2D(Collider2D collision)
		{
			if (collision.gameObject.layer == 9)
			{
				_triggerCount++;
				if (_triggerCount > 0)
				{
					Inactive();
				}
			}
		}

		private void OnTriggerExit2D(Collider2D collision)
		{
			if (collision.gameObject.layer == 9)
			{
				_triggerCount--;
				if (_triggerCount <= 0)
				{
					Active();
				}
			}
		}
	}
}
