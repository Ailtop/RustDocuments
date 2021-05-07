using Characters;
using Characters.Abilities;
using Characters.Operations;
using Services;
using Singletons;
using UnityEditor;
using UnityEngine;

namespace Level.Objects
{
	public class DivineShield : MonoBehaviour
	{
		[SerializeField]
		private Character _target;

		[SerializeField]
		private Prop _prop;

		[SerializeField]
		[AbilityComponent.Subcomponent]
		private AbilityComponent _bigShield;

		[SerializeField]
		[AbilityComponent.Subcomponent]
		private AbilityComponent _smallShield;

		[SerializeField]
		[Subcomponent(typeof(OperationInfos))]
		private OperationInfos _hitOperation;

		public Character target => _target;

		private void Start()
		{
			Initialize();
			AttachShield();
			_prop.onDestroy += DetachShield;
			_target.onDie += InstantDestroy;
		}

		private void Initialize()
		{
			GetShield(_target.sizeForEffect).Initialize();
			_hitOperation.Initialize();
		}

		private void AttachShield()
		{
			_target.ability.Add(GetShield(_target.sizeForEffect).ability);
			_target.health.onTakeDamage.Add(int.MaxValue, OnTakeDamage);
		}

		private void DetachShield()
		{
			if (!(_target == null) && _target.liveAndActive)
			{
				_target.ability.Remove(GetShield(_target.sizeForEffect).ability);
				_target.health.onTakeDamage.Remove(OnTakeDamage);
			}
		}

		private bool OnTakeDamage(ref Damage damage)
		{
			_hitOperation.gameObject.SetActive(true);
			_hitOperation.Run(_target);
			return true;
		}

		private AbilityComponent GetShield(Character.SizeForEffect size)
		{
			switch (size)
			{
			case Character.SizeForEffect.Small:
				return _smallShield;
			case Character.SizeForEffect.Medium:
				return _smallShield;
			case Character.SizeForEffect.Large:
				return _bigShield;
			case Character.SizeForEffect.ExtraLarge:
				return _bigShield;
			case Character.SizeForEffect.None:
				return _smallShield;
			default:
				return _smallShield;
			}
		}

		private void InstantDestroy()
		{
			if (!_prop.destroyed)
			{
				Character player = Singleton<Service>.Instance.levelManager.player;
				Damage damage = new Damage(player, 10000.0, Vector2.zero, Damage.Attribute.Fixed, Damage.AttackType.Additional, Damage.MotionType.Basic);
				_prop.Hit(player, ref damage);
			}
		}
	}
}
