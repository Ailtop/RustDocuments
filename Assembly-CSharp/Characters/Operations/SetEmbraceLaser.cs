using FX;
using UnityEngine;

namespace Characters.Operations
{
	public class SetEmbraceLaser : CharacterOperation
	{
		[SerializeField]
		[MinMaxSlider(0f, 180f)]
		private Vector2 _radianRange;

		[SerializeField]
		private Transform _signContainer;

		[SerializeField]
		private Transform _laserContainer;

		[SerializeField]
		private CompositeCollider2D _range;

		private LineEffect[] _signs;

		private LineEffect[] _lasers;

		private void Awake()
		{
			_signs = new LineEffect[_signContainer.childCount];
			_lasers = new LineEffect[_laserContainer.childCount];
			for (int i = 0; i < _signContainer.childCount; i++)
			{
				_signs[i] = _signContainer.GetChild(i).GetComponent<LineEffect>();
				_lasers[i] = _laserContainer.GetChild(i).GetComponent<LineEffect>();
			}
		}

		public override void Run(Character owner)
		{
			float num = Random.Range(0, 360);
			if (_lasers == null || _lasers.Length == 0)
			{
				Debug.LogError("LineEffects is null or length is lower than or equals zero");
				return;
			}
			_signs[0].transform.rotation = Quaternion.AngleAxis(num, Vector3.forward);
			_lasers[0].transform.rotation = Quaternion.AngleAxis(num, Vector3.forward);
			_lasers[0].gameObject.SetActive(true);
			float num2 = 360 / _lasers.Length;
			for (int i = 1; i < _lasers.Length; i++)
			{
				float num3 = Random.Range(_radianRange.x, _radianRange.y);
				_signs[i].transform.rotation = Quaternion.AngleAxis(num + num3, Vector3.forward);
				_lasers[i].transform.rotation = Quaternion.AngleAxis(num + num3, Vector3.forward);
				num += num2;
				_lasers[i].gameObject.SetActive(true);
			}
			_range.GenerateGeometry();
			LineEffect[] lasers = _lasers;
			for (int j = 0; j < lasers.Length; j++)
			{
				lasers[j].gameObject.SetActive(false);
			}
		}
	}
}
