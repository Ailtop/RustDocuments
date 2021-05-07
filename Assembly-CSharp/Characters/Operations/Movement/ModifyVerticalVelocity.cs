using System.Collections;
using UnityEngine;

namespace Characters.Operations.Movement
{
	public class ModifyVerticalVelocity : CharacterOperation
	{
		private enum Method
		{
			Add,
			Set
		}

		[SerializeField]
		private float _amount;

		[SerializeField]
		private Method _method;

		[Information("For Add method only", InformationAttribute.InformationType.Info, false)]
		[SerializeField]
		private Curve _curve;

		private Character _owner;

		private CoroutineReference _coroutineReference;

		public override void Run(Character owner)
		{
			_owner = owner;
			if (_curve.duration > 0f)
			{
				switch (_method)
				{
				case Method.Add:
					_coroutineReference = owner.StartCoroutineWithReference(CRun(owner));
					break;
				case Method.Set:
					_coroutineReference = owner.StartCoroutineWithReference(CRunWithIgnoreGravity(owner));
					break;
				}
			}
			else
			{
				switch (_method)
				{
				case Method.Add:
					owner.movement.verticalVelocity += _amount;
					break;
				case Method.Set:
					owner.movement.verticalVelocity = _amount;
					break;
				}
			}
		}

		private IEnumerator CRunWithIgnoreGravity(Character character)
		{
			character.movement.ignoreGravity.Attach(this);
			yield return CRun(character);
			character.movement.ignoreGravity.Detach(this);
		}

		private IEnumerator CRun(Character character)
		{
			float t = 0f;
			float normAmountBefore = 0f;
			for (; t < _curve.duration; t += character.chronometer.animation.deltaTime)
			{
				float num = _curve.Evaluate(t);
				switch (_method)
				{
				case Method.Add:
					character.movement.verticalVelocity += _amount * (num - normAmountBefore);
					break;
				case Method.Set:
					character.movement.verticalVelocity = _amount;
					break;
				}
				normAmountBefore = num;
				yield return null;
			}
		}

		public override void Stop()
		{
			if (_owner == _coroutineReference.monoBehaviour)
			{
				_coroutineReference.Stop();
			}
			if (_method == Method.Set)
			{
				_owner?.movement.ignoreGravity.Detach(this);
			}
		}
	}
}
