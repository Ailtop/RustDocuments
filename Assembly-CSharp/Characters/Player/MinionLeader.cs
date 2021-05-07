using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.Player
{
	public class MinionLeader : IDisposable
	{
		public readonly Character player;

		private readonly List<Minion> _minions = new List<Minion>();

		public MinionLeader(Character player)
		{
			this.player = player;
			Singleton<Service>.Instance.levelManager.onMapLoaded += ResetMinionPositions;
		}

		public void Dispose()
		{
			if (!Service.quitting)
			{
				Singleton<Service>.Instance.levelManager.onMapLoaded -= ResetMinionPositions;
			}
		}

		private void ResetMinionPositions()
		{
			foreach (Minion minion in _minions)
			{
				minion.transform.position = player.transform.position;
				if (minion.character.movement != null)
				{
					minion.character.movement.controller.ResetBounds();
				}
			}
		}

		public Minion Summon(Minion minionPrefab, Vector3 position, float duration)
		{
			Minion minion = Summon(minionPrefab, position);
			minion.StartCoroutine(_003CSummon_003Eg__CDespawn_007C5_0(minion, duration));
			return minion;
		}

		public Minion Summon(Minion minionPrefab, Vector3 position)
		{
			_003C_003Ec__DisplayClass6_0 _003C_003Ec__DisplayClass6_ = new _003C_003Ec__DisplayClass6_0();
			_003C_003Ec__DisplayClass6_._003C_003E4__this = this;
			_003C_003Ec__DisplayClass6_.summoned = minionPrefab.Summon(this, position);
			if (_003C_003Ec__DisplayClass6_.summoned.character.health != null)
			{
				_003C_003Ec__DisplayClass6_.summoned.character.health.onDied += _003C_003Ec__DisplayClass6_._003CSummon_003Eg__OnDied_007C0;
			}
			_003C_003Ec__DisplayClass6_.summoned.character.stat.getDamageOverridingStat = player.stat;
			Character character = _003C_003Ec__DisplayClass6_.summoned.character;
			character.onKilled = (Character.OnKilledDelegate)Delegate.Combine(character.onKilled, new Character.OnKilledDelegate(OnKilled));
			_003C_003Ec__DisplayClass6_.summoned.character.onGiveDamage.Add(int.MinValue, OnGiveDamage);
			Character character2 = _003C_003Ec__DisplayClass6_.summoned.character;
			character2.onGaveDamage = (GaveDamageDelegate)Delegate.Combine(character2.onGaveDamage, new GaveDamageDelegate(OnGaveDamage));
			Character character3 = _003C_003Ec__DisplayClass6_.summoned.character;
			character3.onGaveStatus = (Character.OnGaveStatusDelegate)Delegate.Combine(character3.onGaveStatus, new Character.OnGaveStatusDelegate(OnGaveStatus));
			_minions.Add(_003C_003Ec__DisplayClass6_.summoned);
			return _003C_003Ec__DisplayClass6_.summoned;
		}

		private void OnKilled(ITarget target, ref Damage damage)
		{
			player.onKilled?.Invoke(target, ref damage);
		}

		private bool OnGiveDamage(ITarget target, ref Damage damage)
		{
			return player.onGiveDamage.Invoke(target, ref damage);
		}

		private void OnGaveDamage(ITarget target, [In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage gaveDamage, double damageDealt)
		{
			player.onGaveDamage?.Invoke(target, ref originalDamage, ref gaveDamage, damageDealt);
		}

		private void OnGaveStatus(Character target, CharacterStatus.ApplyInfo applyInfo, bool result)
		{
			player.onGaveStatus?.Invoke(target, applyInfo, result);
		}
	}
}
