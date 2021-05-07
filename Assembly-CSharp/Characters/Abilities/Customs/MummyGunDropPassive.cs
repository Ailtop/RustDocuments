using System;
using System.Linq;
using Characters.Abilities.Constraints;
using Level;
using UnityEngine;
using UnityEngine.Serialization;

namespace Characters.Abilities.Customs
{
	[Serializable]
	public class MummyGunDropPassive : Ability, IAbilityInstance
	{
		[Serializable]
		private class DroppedGuns : ReorderableArray<DroppedGuns.Property>
		{
			[Serializable]
			internal class Property
			{
				[SerializeField]
				private float _weight;

				[SerializeField]
				private DroppedMummyGun _droppedGun;

				public float weight => _weight;

				public DroppedMummyGun droppedGun => _droppedGun;
			}
		}

		[SerializeField]
		[Constraint.Subcomponent]
		private Constraint.Subcomponents _constraints;

		[SerializeField]
		private MummyPassiveComponent _mummyPassive;

		[SerializeField]
		[FormerlySerializedAs("_possibility")]
		[Range(1f, 100f)]
		private int _gunDropPossibilityByKill;

		[Header("Supply")]
		[Space]
		[SerializeField]
		[FormerlySerializedAs("_supply")]
		private DroppedMummyGunSupply _supplyPrefab;

		[SerializeField]
		[Information("보급 주기, 0이면 보급되지 않습니다.", InformationAttribute.InformationType.Info, false)]
		private float _supplyInterval;

		private float _remainSupplyTime;

		[SerializeField]
		private CustomFloat _supplyWidth = new CustomFloat(-3f, 5f);

		[SerializeField]
		private CustomFloat _supplyHeight = new CustomFloat(6.5f, 7.5f);

		[Header("Weights")]
		[SerializeField]
		[FormerlySerializedAs("_guns")]
		private DroppedGuns _gunsByKill;

		[SerializeField]
		private DroppedGuns _gunsByPeriodicSupply;

		[SerializeField]
		private DroppedGuns _gunsBySwapSupply;

		public Character owner { get; set; }

		public IAbility ability => this;

		public float remainTime { get; set; }

		public bool attached => true;

		public Sprite icon => _defaultIcon;

		public float iconFillAmount
		{
			get
			{
				if (_supplyInterval != 0f)
				{
					return 1f - _remainSupplyTime / _supplyInterval;
				}
				return 0f;
			}
		}

		public int iconStacks => 0;

		public bool expired => false;

		public override IAbilityInstance CreateInstance(Character owner)
		{
			this.owner = owner;
			return this;
		}

		public void SupplyGunBySwap()
		{
			SupplyGun(_gunsBySwapSupply);
		}

		public void UpdateTime(float deltaTime)
		{
			if (_supplyInterval != 0f && _constraints.Pass())
			{
				_remainSupplyTime -= deltaTime;
				if (!(_remainSupplyTime > 0f))
				{
					_remainSupplyTime = _supplyInterval;
					SupplyGun(_gunsByPeriodicSupply);
				}
			}
		}

		private bool IsInTerrain(Vector3 position)
		{
			position.y += 0.5f;
			return !Physics2D.OverlapPoint(position, Layers.terrainMask);
		}

		private void SupplyGun(DroppedGuns guns)
		{
			Vector3 vector = owner.transform.position;
			Collider2D lastStandingCollider = owner.movement.controller.collisionState.lastStandingCollider;
			if (lastStandingCollider != null)
			{
				vector.y = lastStandingCollider.bounds.max.y;
			}
			Vector3 one = Vector3.one;
			for (int i = 0; i < 10; i++)
			{
				one.x = _supplyWidth.value;
				RaycastHit2D raycastHit2D = Physics2D.Raycast(vector + one, Vector2.down, 5f, Layers.groundMask);
				if ((bool)raycastHit2D && IsInTerrain(raycastHit2D.point))
				{
					vector = raycastHit2D.point;
					break;
				}
			}
			float y = vector.y;
			vector.y += _supplyHeight.value;
			DroppedMummyGun droppedMummyGun = DropGun(guns, Vector3.one);
			if (!(droppedMummyGun == null))
			{
				_supplyPrefab.Spawn(droppedMummyGun, vector, y);
			}
		}

		private void OnOwnerKilled(ITarget target, ref Damage damage)
		{
			if (!(target.character == null) && target.character.type != Character.Type.Dummy && target.character.type != Character.Type.Trap && MMMaths.PercentChance(_gunDropPossibilityByKill))
			{
				DropGun(_gunsByKill, damage.hitPoint);
			}
		}

		private DroppedMummyGun GetRandomGun(DroppedGuns guns)
		{
			DroppedGuns.Property[] values = guns.values;
			float num = UnityEngine.Random.Range(0f, values.Sum((DroppedGuns.Property a) => a.weight));
			for (int i = 0; i < values.Length; i++)
			{
				num -= values[i].weight;
				if (num <= 0f)
				{
					return values[i].droppedGun;
				}
			}
			return null;
		}

		private DroppedMummyGun DropGun(DroppedGuns guns, Vector3 position)
		{
			DroppedMummyGun randomGun = GetRandomGun(guns);
			if (randomGun == null)
			{
				return null;
			}
			return randomGun.Spawn(position, _mummyPassive.baseAbility);
		}

		public void Attach()
		{
			remainTime = 0f;
			Character character = owner;
			character.onKilled = (Character.OnKilledDelegate)Delegate.Combine(character.onKilled, new Character.OnKilledDelegate(OnOwnerKilled));
		}

		public void Detach()
		{
			Character character = owner;
			character.onKilled = (Character.OnKilledDelegate)Delegate.Remove(character.onKilled, new Character.OnKilledDelegate(OnOwnerKilled));
		}

		public void Refresh()
		{
		}
	}
}
