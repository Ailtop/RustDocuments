using System.Collections;
using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Hero.LightSwords
{
	public class LightSwordStuck : MonoBehaviour
	{
		private static short _currentSortingOrder = 1;

		private static int[] _points = new int[5] { 45, 75, 90, 105, 135 };

		[SerializeField]
		private SpriteRenderer[] _bodyContainer;

		[SerializeField]
		private Transform[] _trailEffectTransformContainer;

		[SerializeField]
		private Transform _trailEffectTransform;

		private SpriteRenderer _body;

		[SerializeField]
		private Color _startColor;

		[SerializeField]
		private Color _endColor;

		[SerializeField]
		private Curve _hitColorCurve;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _onStuck;

		private int _order;

		private void Awake()
		{
			_order = _currentSortingOrder++;
			_onStuck.Initialize();
		}

		public void OnStuck(Character owner, Vector2 position, float angle)
		{
			Hide();
			int num = Evaluate(angle);
			_body = _bodyContainer[num];
			_trailEffectTransform.SetParent(_trailEffectTransformContainer[num]);
			_body.sortingOrder = _order;
			base.transform.position = position;
			_onStuck.gameObject.SetActive(true);
			_onStuck.Run(owner);
			Show();
		}

		public void Despawn()
		{
			Hide();
		}

		public void Sign()
		{
			StartCoroutine(CEaseColor());
		}

		private int Evaluate(float angle)
		{
			angle -= 180f;
			int result = 0;
			float num = float.MaxValue;
			for (int i = 0; i < _points.Length; i++)
			{
				float num2 = Mathf.Abs(angle - (float)_points[i]);
				if (num2 < num)
				{
					result = i;
					num = num2;
				}
			}
			return result;
		}

		private void Show()
		{
			if (_body != null)
			{
				_body.gameObject.SetActive(true);
			}
		}

		private void Hide()
		{
			if (_body != null)
			{
				_body.color = _startColor;
				_body.gameObject.SetActive(false);
			}
		}

		private IEnumerator CEaseColor()
		{
			float duration = _hitColorCurve.duration;
			for (float time = 0f; time < duration; time += Chronometer.global.deltaTime)
			{
				_body.color = Color.Lerp(_startColor, _endColor, _hitColorCurve.Evaluate(time));
				yield return null;
			}
			_body.color = _endColor;
		}
	}
}
