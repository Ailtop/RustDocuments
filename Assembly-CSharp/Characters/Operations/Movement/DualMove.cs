using Characters.Controllers;
using UnityEngine;

namespace Characters.Operations.Movement
{
	public class DualMove : CharacterOperation
	{
		private const float directionThreshold = 0.66f;

		[SerializeField]
		private bool _useDashDistanceStat;

		[SerializeField]
		private float _movementSpeedFactor1;

		[SerializeField]
		private Force _force1;

		[SerializeField]
		private Curve _curve1;

		[SerializeField]
		private float _movementSpeedFactor2;

		[SerializeField]
		private Force _force2;

		[SerializeField]
		private Curve _curve2;

		[SerializeField]
		private bool _needDirectionInput = true;

		private CoroutineReference _coroutineReference1;

		private CoroutineReference _coroutineReference2;

		public override void Run(Character owner)
		{
			_003C_003Ec__DisplayClass11_0 _003C_003Ec__DisplayClass11_ = default(_003C_003Ec__DisplayClass11_0);
			_003C_003Ec__DisplayClass11_.owner = owner;
			_003C_003Ec__DisplayClass11_._003C_003E4__this = this;
			if (_003C_003Ec__DisplayClass11_.owner.movement == null)
			{
				return;
			}
			if (_needDirectionInput)
			{
				PlayerInput component = _003C_003Ec__DisplayClass11_.owner.GetComponent<PlayerInput>();
				if (component != null && ((_003C_003Ec__DisplayClass11_.owner.lookingDirection == Character.LookingDirection.Left && component.direction.x >= -0.66f) || (_003C_003Ec__DisplayClass11_.owner.lookingDirection == Character.LookingDirection.Right && component.direction.x <= 0.66f)))
				{
					return;
				}
			}
			float extraPower = 0f;
			float extraPower2 = 0f;
			if (_movementSpeedFactor1 > 0f || _movementSpeedFactor2 > 0f)
			{
				float num = Mathf.Abs((float)_003C_003Ec__DisplayClass11_.owner.stat.GetFinal(Stat.Kind.MovementSpeed));
				float num2 = Mathf.Abs((float)_003C_003Ec__DisplayClass11_.owner.stat.Get(Stat.Category.Constant, Stat.Kind.MovementSpeed));
				float num3 = Mathf.Max(0f, num - num2);
				extraPower = num3 * _curve1.duration * _movementSpeedFactor1;
				extraPower2 = num3 * _curve2.duration * _movementSpeedFactor2;
			}
			_003CRun_003Eg__TriggerMove_007C11_0(_force1, _curve1, extraPower, ref _coroutineReference1, ref _003C_003Ec__DisplayClass11_);
			_003CRun_003Eg__TriggerMove_007C11_0(_force2, _curve2, extraPower2, ref _coroutineReference2, ref _003C_003Ec__DisplayClass11_);
		}

		public override void Stop()
		{
			_coroutineReference1.Stop();
			_coroutineReference2.Stop();
		}
	}
}
