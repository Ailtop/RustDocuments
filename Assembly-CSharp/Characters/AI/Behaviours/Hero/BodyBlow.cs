using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters.Actions;
using Characters.AI.Hero;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Hero
{
	public class BodyBlow : Behaviour, IFinish, IComboable
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(ChainAction))]
		private Characters.Actions.Action _startAction;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(ChainAction))]
		private Characters.Actions.Action _readyAction;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(ChainAction))]
		private Characters.Actions.Action _attackAction;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(ChainAction))]
		private Characters.Actions.Action _failAction;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(SlashCombo))]
		private SlashCombo _slashCombo;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(SkipableIdle))]
		private SkipableIdle _skipableIdle;

		private bool _canUseSlashCombo;

		public IEnumerator CTryContinuedCombo(AIController controller, ComboSystem comboSystem)
		{
			comboSystem.Clear();
			yield return CCombat(controller);
		}

		public override IEnumerator CRun(AIController controller)
		{
			_startAction.TryStart();
			while (_startAction.running)
			{
				yield return null;
			}
			yield return CCombat(controller);
		}

		private IEnumerator CCombat(AIController controller)
		{
			Character character = controller.character;
			character.onGaveDamage = (GaveDamageDelegate)Delegate.Combine(character.onGaveDamage, new GaveDamageDelegate(CheckHit));
			_readyAction.TryStart();
			while (_readyAction.running)
			{
				yield return null;
			}
			_attackAction.TryStart();
			while (_attackAction.running)
			{
				yield return null;
			}
			Character character2 = controller.character;
			character2.onGaveDamage = (GaveDamageDelegate)Delegate.Remove(character2.onGaveDamage, new GaveDamageDelegate(CheckHit));
			if (_canUseSlashCombo)
			{
				yield return _slashCombo.CRun(controller);
			}
			else
			{
				_failAction.TryStart();
				while (_failAction.running)
				{
					yield return null;
				}
			}
			_canUseSlashCombo = false;
			yield return _skipableIdle.CRun(controller);
		}

		private void CheckHit(ITarget target, [In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage gaveDamage, double damageDealt)
		{
			_canUseSlashCombo = true;
		}
	}
}
