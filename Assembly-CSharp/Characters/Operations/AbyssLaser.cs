using Characters.Actions;
using FX;
using UnityEngine;

namespace Characters.Operations
{
	public class AbyssLaser : CharacterOperation, IAttackDamage
	{
		[SerializeField]
		private ChargeAction _chargeAction;

		[Header("Effect")]
		[SerializeField]
		private float _yScaleMin;

		[SerializeField]
		private float _yScaleMax;

		[Space]
		[SerializeField]
		private Transform _spawnPosition;

		[SerializeField]
		private bool _attachToSpawnPosition;

		[SerializeField]
		private EffectInfo _info;

		[Header("Attack")]
		[SerializeField]
		private AttackDamage _attackDamage;

		private float _damageMultiplier;

		[Space]
		[SerializeField]
		private float _damageMultiplierMin;

		[SerializeField]
		private float _damageMultiplierMax;

		public float amount => _damageMultiplier * _attackDamage.amount;

		private void Awake()
		{
			if (_spawnPosition == null)
			{
				_spawnPosition = base.transform;
			}
		}

		public override void Run(Character owner)
		{
			_damageMultiplier = (_damageMultiplierMax - _damageMultiplierMin) * _chargeAction.chargedPercent + _damageMultiplierMin;
			ReusableChronoSpriteEffect reusableChronoSpriteEffect = _info.Spawn(_spawnPosition.position, owner, _spawnPosition.rotation.eulerAngles.z);
			if (_attachToSpawnPosition)
			{
				reusableChronoSpriteEffect.transform.parent = _spawnPosition;
			}
			float y = (_yScaleMax - _yScaleMin) * _chargeAction.chargedPercent + _yScaleMin;
			reusableChronoSpriteEffect.transform.localScale = new Vector3(1f, y, 1f);
		}

		public override void Stop()
		{
			_info.DespawnChildren();
		}
	}
}
