using UnityEngine;

namespace Characters.Operations
{
	public class RandomizedTranslateCollider : CharacterOperation
	{
		[SerializeField]
		private Transform _center;

		[SerializeField]
		private Collider2D _targetCollider;

		[SerializeField]
		[Range(0f, 10f)]
		private float _distribution;

		private Vector3 _translate;

		public override void Run(Character owner)
		{
			_translate = _center.position;
			float num = Random.Range(0f - _distribution, _distribution);
			if (Random.value > 0.5f)
			{
				_translate.x += num;
			}
			else
			{
				_translate.x -= num;
			}
			if (Random.value > 0.5f)
			{
				_translate.y += num;
			}
			else
			{
				_translate.y -= num;
			}
			_translate.z = 0f;
			_targetCollider.transform.position = _translate;
		}
	}
}
