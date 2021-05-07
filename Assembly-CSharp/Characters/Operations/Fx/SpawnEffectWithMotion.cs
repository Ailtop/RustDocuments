using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FX;
using UnityEngine;

namespace Characters.Operations.Fx
{
	public class SpawnEffectWithMotion : CharacterOperation
	{
		[SerializeField]
		private Transform _spawnPosition;

		[SerializeField]
		private EffectInfo _info;

		private Character _owner;

		private ReusableChronoSpriteEffect _spawned;

		private void Awake()
		{
			if (_spawnPosition == null)
			{
				_spawnPosition = base.transform;
			}
			_info.loop = true;
		}

		public override void Run(Character owner)
		{
			_owner = owner;
			float duration = _info.duration;
			_info.duration = 0f;
			_spawned = _info.Spawn(_spawnPosition.position, owner);
			owner.health.onTookDamage += Health_onTookDamage;
		}

		private void Health_onTookDamage([In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
		{
			_spawned.reusable.Despawn();
			_owner.health.onTookDamage -= Health_onTookDamage;
		}
	}
}
