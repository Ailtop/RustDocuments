using System.Collections;
using Characters.Abilities.Constraints;
using Characters.Actions;
using Characters.Movements;
using Unity.Mathematics;
using UnityEngine;

namespace Characters.Gear.Weapons
{
	public class SkulWait : MonoBehaviour
	{
		private const float _waitingTime = 30f;

		[SerializeField]
		private Weapon _weapon;

		[SerializeField]
		private Action _action;

		[SerializeField]
		[Constraint.Subcomponent]
		private Constraint.Subcomponents _constraints;

		private void OnEnable()
		{
			StartCoroutine(CCheckWait());
		}

		private IEnumerator CCheckWait()
		{
			yield return null;
			float waitedTime = 0f;
			while (true)
			{
				Movement movement = _weapon.owner.movement;
				waitedTime += Chronometer.global.deltaTime;
				if (math.abs(movement.moved.x) > 0.0001f || math.abs(movement.moved.y) > 0.0001f)
				{
					waitedTime = 0f;
				}
				if (!_constraints.Pass())
				{
					waitedTime = 0f;
				}
				if (waitedTime > 30f)
				{
					waitedTime = 0f;
					_action.TryStart();
					yield return CWaitForWaitAction();
				}
				yield return null;
			}
		}

		private IEnumerator CWaitForWaitAction()
		{
			Movement movement = _weapon.owner.movement;
			while (_action.running && _constraints.Pass())
			{
				yield return null;
				if (math.abs(movement.moved.x) > 0.0001f || math.abs(movement.moved.y) > 0.0001f)
				{
					break;
				}
			}
			_weapon.owner.CancelAction();
		}
	}
}
