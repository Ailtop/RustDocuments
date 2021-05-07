using FX;
using UnityEngine;

namespace Characters.Operations
{
	public class ResizeEffect : CharacterOperation
	{
		[SerializeField]
		private Transform _start;

		[SerializeField]
		private Transform _end;

		[SerializeField]
		private EffectInfo _info;

		[SerializeField]
		private float _originSize;

		public override void Run(Character owner)
		{
			_info.scaleX = new CustomFloat(GetScaleX());
			_info.Spawn(_end.position, owner, _start.rotation.eulerAngles.z);
		}

		private float GetScaleX()
		{
			return Mathf.Abs((_end.transform.position.x - _start.transform.position.x) / _originSize);
		}
	}
}
