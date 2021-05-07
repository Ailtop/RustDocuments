using System.Collections;
using Characters.Movements;
using UnityEngine;

namespace Characters.Operations.Movement
{
	public class OverrideMovementConfig : CharacterOperation
	{
		[SerializeField]
		private Characters.Movements.Movement.Config _config;

		[SerializeField]
		private int _priority;

		[SerializeField]
		private float _duration;

		private Character _owner;

		public override void Run(Character owner)
		{
			_owner = owner;
			_owner.movement.configs.Add(_priority, _config);
			if (_config.keepMove)
			{
				_owner.movement.Move((_owner.lookingDirection == Character.LookingDirection.Right) ? Vector2.right : Vector2.left);
			}
			if (_duration > 0f)
			{
				StartCoroutine(CRun());
			}
		}

		private IEnumerator CRun()
		{
			yield return _owner.chronometer.master.WaitForSeconds(_duration);
			Remove();
		}

		private void Remove()
		{
			if (!(_owner == null))
			{
				_owner.movement.configs.Remove(_config);
			}
		}

		public override void Stop()
		{
			base.Stop();
			Remove();
		}
	}
}
