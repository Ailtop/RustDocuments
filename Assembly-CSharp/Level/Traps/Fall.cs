using System;
using Characters;
using Characters.Operations;
using Services;
using Singletons;
using UnityEngine;

namespace Level.Traps
{
	[RequireComponent(typeof(Character))]
	public class Fall : Trap
	{
		[SerializeField]
		[GetComponent]
		private Character _character;

		[SerializeField]
		[Tooltip("이미 설정된 값으로, 참조용으로 사용중")]
		private double _damage;

		[SerializeField]
		private Transform _escapePoint;

		[SerializeField]
		[CharacterOperation.Subcomponent]
		private CharacterOperation.Subcomponents _onEscape;

		private IAttackDamage _attackDamage;

		private void Awake()
		{
			_onEscape.Initialize();
			_attackDamage = GetComponent<IAttackDamage>();
		}

		private void OnTriggerEnter2D(Collider2D collision)
		{
			Character component = collision.GetComponent<Character>();
			if (component == null)
			{
				return;
			}
			switch (component.type)
			{
			case Character.Type.Player:
			{
				float num = ((_damage == 0.0) ? 0f : _attackDamage.amount);
				Damage damage = new Damage(new Attacker(_character), num, component.transform.position, Damage.Attribute.Fixed, Damage.AttackType.Additional, Damage.MotionType.Basic, 1.0, 0f, 0.0, 1.0, true);
				if (Math.Floor(component.health.currentHealth) <= _damage)
				{
					damage.@base = component.health.currentHealth - 1.0;
				}
				component.health.TakeDamage(ref damage);
				component.transform.position = _escapePoint.position;
				Singleton<Service>.Instance.floatingTextSpawner.SpawnPlayerTakingDamage(damage.amount, component.transform.position);
				_onEscape.Run(component);
				break;
			}
			case Character.Type.TrashMob:
			case Character.Type.Summoned:
				component.health.Kill();
				break;
			}
		}
	}
}
