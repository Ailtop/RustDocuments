using System.Collections;
using Characters;
using Characters.Abilities;
using Characters.Actions;
using Characters.Operations.Attack;
using UnityEngine;

namespace Level.Traps
{
	public class ClericTrap : MonoBehaviour
	{
		[SerializeField]
		private Character _character;

		[SerializeField]
		private SweepAttack2 _sweepAttack;

		[SerializeField]
		private float _explosionTime;

		[SerializeField]
		private Action _attackAction;

		[SerializeField]
		[AbilityAttacher.Subcomponent]
		private AbilityAttacher _abilityAttacher;

		private void Awake()
		{
			_sweepAttack.Initialize();
			_abilityAttacher.Initialize(_character);
			_abilityAttacher.StartAttach();
		}

		private void OnEnable()
		{
			_sweepAttack.Run(_character);
			StartCoroutine(CAttack());
		}

		private IEnumerator CAttack()
		{
			yield return Chronometer.global.WaitForSeconds(_explosionTime);
			_sweepAttack.Stop();
			_attackAction.TryStart();
			while (_attackAction.running)
			{
				yield return null;
			}
			_character.gameObject.SetActive(false);
		}

		private void OnDisable()
		{
			_sweepAttack.Stop();
			_character.CancelAction();
			_abilityAttacher.StopAttach();
		}
	}
}
