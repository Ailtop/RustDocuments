using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters.Actions;
using Characters.Operations;
using Services;
using Singletons;
using UnityEditor;
using UnityEngine;

namespace Characters.Gear.Quintessences
{
	public class RunActionAndOwnerOperation : UseQuintessence
	{
		[SerializeField]
		private Character _character;

		[SerializeField]
		[Subcomponent(typeof(OperationInfo))]
		private OperationInfo.Subcomponents _operations;

		[SerializeField]
		private Characters.Actions.Action _action;

		protected override void Awake()
		{
			base.Awake();
			_quintessence.onEquipped += _operations.Initialize;
			_character.gameObject.SetActive(false);
			_character.transform.parent = null;
		}

		private void AttachEvents()
		{
			Character character = _character;
			character.onKilled = (Character.OnKilledDelegate)Delegate.Combine(character.onKilled, new Character.OnKilledDelegate(OnKilled));
			_character.onGiveDamage.Add(int.MinValue, OnGiveDamage);
			Character character2 = _character;
			character2.onGaveDamage = (GaveDamageDelegate)Delegate.Combine(character2.onGaveDamage, new GaveDamageDelegate(OnGaveDamage));
			Character character3 = _character;
			character3.onGaveStatus = (Character.OnGaveStatusDelegate)Delegate.Combine(character3.onGaveStatus, new Character.OnGaveStatusDelegate(OnGaveStatus));
		}

		private void DetachEvents()
		{
			Character character = _character;
			character.onKilled = (Character.OnKilledDelegate)Delegate.Remove(character.onKilled, new Character.OnKilledDelegate(OnKilled));
			_character.onGiveDamage.Remove(OnGiveDamage);
			Character character2 = _character;
			character2.onGaveDamage = (GaveDamageDelegate)Delegate.Remove(character2.onGaveDamage, new GaveDamageDelegate(OnGaveDamage));
			Character character3 = _character;
			character3.onGaveStatus = (Character.OnGaveStatusDelegate)Delegate.Remove(character3.onGaveStatus, new Character.OnGaveStatusDelegate(OnGaveStatus));
		}

		private void Disable()
		{
			if (!(_character == null))
			{
				DetachEvents();
				_operations.StopAll();
				_character.gameObject.SetActive(false);
			}
		}

		private void OnEnable()
		{
			Singleton<Service>.Instance.levelManager.onMapLoadedAndFadedIn += Disable;
		}

		private void OnDisable()
		{
			Disable();
			Singleton<Service>.Instance.levelManager.onMapLoadedAndFadedIn -= Disable;
		}

		protected override void OnUse()
		{
			if (!_character.gameObject.activeSelf)
			{
				_quintessence.owner.StartCoroutine(CUse());
			}
		}

		private IEnumerator CUse()
		{
			_character.stat.getDamageOverridingStat = _quintessence.owner.stat;
			AttachEvents();
			_character.ForceToLookAt(_quintessence.owner.desiringLookingDirection);
			_character.transform.position = base.transform.position;
			_character.gameObject.SetActive(true);
			StartCoroutine(_operations.CRun(_quintessence.owner));
			_action.TryStart();
			while (_action.running)
			{
				yield return null;
			}
			Disable();
		}

		private void OnKilled(ITarget target, ref Damage damage)
		{
			_quintessence.owner.onKilled?.Invoke(target, ref damage);
		}

		private bool OnGiveDamage(ITarget target, ref Damage damage)
		{
			return _quintessence.owner.onGiveDamage.Invoke(target, ref damage);
		}

		private void OnGaveDamage(ITarget target, [In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage gaveDamage, double damageDealt)
		{
			_quintessence.owner.onGaveDamage?.Invoke(target, ref originalDamage, ref gaveDamage, damageDealt);
		}

		private void OnGaveStatus(Character target, CharacterStatus.ApplyInfo applyInfo, bool result)
		{
			_quintessence.owner.onGaveStatus?.Invoke(target, applyInfo, result);
		}

		private void OnDestroy()
		{
			if (!Service.quitting)
			{
				UnityEngine.Object.Destroy(_character.gameObject);
			}
		}
	}
}
