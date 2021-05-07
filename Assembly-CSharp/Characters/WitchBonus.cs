using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Characters.Abilities;
using Characters.Gear.Items;
using Characters.Gear.Quintessences;
using Characters.Gear.Weapons;
using Data;
using FX.SpriteEffects;
using PhysicsUtils;
using UnityEngine;

namespace Characters
{
	public class WitchBonus
	{
		public abstract class Bonus
		{
			public Tree tree;

			public int indexInTree;

			protected readonly Character _owner;

			protected readonly int[] _costs;

			protected readonly string _key;

			protected readonly IntData _data;

			public int level
			{
				get
				{
					return _data.value;
				}
				set
				{
					if (value == 0 && _data.value > 0)
					{
						Detach();
					}
					else if (value > 0 && _data.value == 0)
					{
						Attach();
					}
					_data.value = value;
					Update();
				}
			}

			public int maxLevel => _costs.Length;

			public int levelUpCost => _costs[level];

			public bool ready
			{
				get
				{
					if (indexInTree != 0)
					{
						return tree.list[indexInTree - 1].level > 0;
					}
					return true;
				}
			}

			public string displayName => Lingua.GetLocalizedString(_key);

			public Bonus(string key, Character owner, IntData data, int[] costs)
			{
				_key = key;
				_owner = owner;
				_data = data;
				_costs = costs;
			}

			public virtual void Initialize()
			{
				if (level > 0)
				{
					Attach();
				}
				Update();
			}

			public bool LevelUp()
			{
				if (!ready || level == maxLevel || !GameData.Currency.darkQuartz.Consume(levelUpCost))
				{
					return false;
				}
				level++;
				GameData.Currency.SaveAll();
				GameData.Progress.SaveAll();
				return true;
			}

			public abstract void Attach();

			public abstract void Detach();

			protected virtual void Update()
			{
			}

			public override string ToString()
			{
				return GetDescription(level);
			}

			public virtual string GetDescription(int level)
			{
				return Lingua.GetLocalizedString(_key + "/desc");
			}
		}

		public class StatBonus : Bonus
		{
			public readonly Stat.Values stat;

			protected readonly Stat.Value _statPerLevel;

			public StatBonus(string key, Character owner, IntData data, int[] costs, Stat.Value statPerLevel)
				: base(key, owner, data, costs)
			{
				stat = new Stat.Values(statPerLevel.Clone());
				_statPerLevel = statPerLevel;
			}

			protected override void Update()
			{
				base.Update();
				if (Stat.Kind.values[_statPerLevel.kindIndex].valueForm == Stat.Kind.ValueForm.Product)
				{
					stat.values[0].value = 1.0 - (double)base.level * _statPerLevel.value;
				}
				else if (_statPerLevel.categoryIndex == Stat.Category.Percent.index)
				{
					stat.values[0].value = 1.0 + (double)base.level * _statPerLevel.value;
				}
				else
				{
					stat.values[0].value = (double)base.level * _statPerLevel.value;
				}
				_owner.stat.SetNeedUpdate();
			}

			public override string GetDescription(int level)
			{
				return string.Format(Lingua.GetLocalizedString(_key + "/desc"), _statPerLevel.value * (double)level);
			}

			public override void Attach()
			{
				_owner.stat.AttachValues(stat);
				Update();
			}

			public override void Detach()
			{
				_owner.stat.DetachValues(stat);
			}
		}

		public class StatBonusByWeaponCategory : Bonus
		{
			public readonly Stat.Values stat;

			protected readonly Stat.Values _statPerLevel;

			protected readonly Weapon.Category _weaponCategory;

			public StatBonusByWeaponCategory(string key, Character owner, IntData data, int[] costs, Weapon.Category weaponCategory, Stat.Value statPerLevel)
				: base(key, owner, data, costs)
			{
				_statPerLevel = new Stat.Values(statPerLevel);
				stat = _statPerLevel.Clone();
				_weaponCategory = weaponCategory;
			}

			public StatBonusByWeaponCategory(string key, Character owner, IntData data, int[] costs, Weapon.Category weaponCategory, Stat.Values statsPerLevel)
				: base(key, owner, data, costs)
			{
				_statPerLevel = statsPerLevel;
				stat = statsPerLevel.Clone();
				_weaponCategory = weaponCategory;
			}

			protected override void Update()
			{
				base.Update();
				for (int i = 0; i < stat.values.Length; i++)
				{
					Stat.Value value = stat.values[i];
					if (_owner.playerComponents.inventory.weapon.current.category == _weaponCategory)
					{
						Stat.Value value2 = _statPerLevel.values[i];
						if (Stat.Kind.values[value2.kindIndex].valueForm == Stat.Kind.ValueForm.Product)
						{
							value.value = 1.0 - (double)base.level * value2.value;
						}
						else if (value2.categoryIndex == Stat.Category.Percent.index)
						{
							value.value = 1.0 + (double)base.level * value2.value;
						}
						else
						{
							value.value = (double)base.level * value2.value;
						}
					}
					else if (value.IsCategory(Stat.Category.Percent.index))
					{
						value.value = 1.0;
					}
					else
					{
						value.value = 0.0;
					}
				}
				_owner.stat.SetNeedUpdate();
			}

			public override string GetDescription(int level)
			{
				object[] args = ((IEnumerable<Stat.Value>)_statPerLevel.values).Select((Func<Stat.Value, object>)((Stat.Value stat) => stat.value * (double)level)).ToArray();
				return string.Format(Lingua.GetLocalizedString(_key + "/desc"), args);
			}

			public override void Attach()
			{
				_owner.stat.AttachValues(stat);
				_owner.playerComponents.inventory.weapon.onSwap += Update;
				_owner.playerComponents.inventory.weapon.onChanged += OnWeaponChanged;
				Update();
			}

			public override void Detach()
			{
				_owner.stat.DetachValues(stat);
				_owner.playerComponents.inventory.weapon.onSwap -= Update;
				_owner.playerComponents.inventory.weapon.onChanged -= OnWeaponChanged;
			}

			private void OnWeaponChanged(Weapon old, Weapon @new)
			{
				Update();
			}
		}

		public class GenericBonus : Bonus
		{
			protected readonly float _bonusPerLevel;

			public float bonus => _bonusPerLevel * (float)base.level;

			public GenericBonus(string key, Character owner, IntData data, int[] costs, float bonusPerLevel)
				: base(key, owner, data, costs)
			{
				_bonusPerLevel = bonusPerLevel;
			}

			public override string GetDescription(int level)
			{
				return string.Format(Lingua.GetLocalizedString(_key + "/desc"), _bonusPerLevel * (float)level);
			}

			public override void Attach()
			{
			}

			public override void Detach()
			{
			}
		}

		public class GetShieldOnSwap : GenericBonus
		{
			private readonly Characters.Abilities.Shield _shield;

			private readonly float _duration;

			private readonly float _swapCooldownReduction;

			public GetShieldOnSwap(string key, Character owner, IntData data, int[] costs, float bonusPerLevel, float duration, float swapCooldownReduction)
				: base(key, owner, data, costs, bonusPerLevel)
			{
				GetShieldOnSwap getShieldOnSwap = this;
				_duration = duration;
				_swapCooldownReduction = swapCooldownReduction;
				_shield = new Characters.Abilities.Shield(base.bonus)
				{
					duration = _duration
				};
				_shield.onDetach += delegate(Shield.Instance instance)
				{
					if (instance.amount > 0.0)
					{
						owner.playerComponents.inventory.weapon.ReduceSwapCooldown(getShieldOnSwap._swapCooldownReduction);
					}
				};
			}

			private void AttachShieldAbility()
			{
				_shield.amount = base.bonus;
				_owner.ability.Add(_shield);
			}

			public override void Attach()
			{
				_owner.playerComponents.inventory.weapon.onSwap += AttachShieldAbility;
			}

			public override void Detach()
			{
				_owner.playerComponents.inventory.weapon.onSwap -= AttachShieldAbility;
			}

			public override string GetDescription(int level)
			{
				return string.Format(Lingua.GetLocalizedString(_key + "/desc"), _bonusPerLevel * (float)level, _duration, _swapCooldownReduction);
			}
		}

		public class ReviveOnce : Bonus, IAbility, IAbilityInstance
		{
			private readonly float _remainhealthPercent;

			private static readonly TargetLayer _layer = new TargetLayer(0, false, true, false, false);

			private NonAllocOverlapper _overlapper;

			Character IAbilityInstance.owner => _owner;

			public IAbility ability => this;

			public float remainTime { get; set; }

			public bool attached => true;

			public Sprite icon { get; }

			public float iconFillAmount => 0f;

			public bool iconFillInversed => false;

			public bool iconFillFlipped => false;

			public int iconStacks => 0;

			public bool expired => GameData.Progress.reassembleUsed;

			public float duration { get; set; }

			public int iconPriority => 100;

			public bool removeOnSwapWeapon => false;

			public IAbilityInstance CreateInstance(Character owner)
			{
				return this;
			}

			public ReviveOnce(string key, Character owner, IntData data, int[] costs, float remainHealthPercent)
				: base(key, owner, data, costs)
			{
				_remainhealthPercent = remainHealthPercent;
				icon = Resource.instance.reassembleIcon;
			}

			public override void Attach()
			{
				_owner.ability.Add(this);
			}

			public override void Detach()
			{
				_owner.ability.Remove(this);
			}

			public void UpdateTime(float deltaTime)
			{
			}

			public void Refresh()
			{
			}

			private void Revive()
			{
				if (GameData.Progress.reassembleUsed || _owner.ability.GetInstance<Revive>() != null)
				{
					return;
				}
				Chronometer.global.AttachTimeScale(this, 0.2f, 0.5f);
				_owner.health.Heal(_owner.health.maximumHealth * (double)_remainhealthPercent * (double)base.level);
				GameData.Progress.reassembleUsed = true;
				Resource.instance.reassembleParticle.Emit(_owner.transform.position, _owner.collider.bounds, _owner.movement.push);
				_owner.CancelAction();
				_owner.chronometer.master.AttachTimeScale(this, 0.01f, 0.5f);
				_owner.spriteEffectStack.Add(new ColorBlend(int.MaxValue, Color.clear, 0.5f));
				GetInvulnerable getInvulnerable = new GetInvulnerable();
				getInvulnerable.duration = 2 + base.level;
				_owner.spriteEffectStack.Add(new Invulnerable(0, 0.2f, getInvulnerable.duration));
				_owner.ability.Add(getInvulnerable);
				CircleCaster circleCaster = new CircleCaster();
				circleCaster.origin = _owner.transform.position;
				circleCaster.radius = 8f;
				circleCaster.contactFilter.SetLayerMask(_layer.Evaluate(_owner.gameObject));
				List<Target> components = circleCaster.Cast().GetComponents<Target>();
				for (int i = 0; i < components.Count; i++)
				{
					Character character = components[i].character;
					if (!(character == null))
					{
						Damage damage = new Damage(_owner, 50.0, MMMaths.RandomPointWithinBounds(character.collider.bounds), Damage.Attribute.Fixed, Damage.AttackType.Additional, Damage.MotionType.Item, base.level, 0.5f);
						character.movement.push.ApplyKnockback(_owner, new Vector2(0f, 5f), new Curve(AnimationCurve.Linear(0f, 0f, 1f, 1f), 0.3f, 0.5f));
						_owner.AttackCharacter(new TargetStruct(character), ref damage);
					}
				}
			}

			void IAbilityInstance.Attach()
			{
				_owner.health.onDie += Revive;
			}

			void IAbilityInstance.Detach()
			{
				_owner.health.onDie -= Revive;
			}

			public override string GetDescription(int level)
			{
				return string.Format(Lingua.GetLocalizedString(_key + "/desc"), _remainhealthPercent * (float)level);
			}
		}

		public class OverHealToShield : GenericBonus
		{
			private readonly Characters.Abilities.Shield _shield;

			public OverHealToShield(string key, Character owner, IntData data, int[] costs, float bonusPerLevel)
				: base(key, owner, data, costs, bonusPerLevel)
			{
				_shield = new Characters.Abilities.Shield();
			}

			private void OnHealed(double healed, double overHealed)
			{
				double num = overHealed / _owner.health.maximumHealth;
				_shield.amount = (float)(num * (double)base.bonus);
				_owner.ability.Add(_shield);
			}

			public override void Attach()
			{
				_owner.health.onHealed += OnHealed;
			}

			public override void Detach()
			{
				_owner.health.onHealed -= OnHealed;
			}
		}

		public class Alchemy : Bonus
		{
			private readonly RarityPrices _itemRarityGoldsPerLevel;

			private readonly RarityPrices _eseenceRarityGoldsPerLevel;

			public Alchemy(string key, Character owner, IntData data, int[] costs, RarityPrices itemRarityGoldsPerLevel, RarityPrices essenceRarityGoldsPerLevel)
				: base(key, owner, data, costs)
			{
				_itemRarityGoldsPerLevel = itemRarityGoldsPerLevel;
				_eseenceRarityGoldsPerLevel = essenceRarityGoldsPerLevel;
			}

			public override string GetDescription(int level)
			{
				return string.Format(Lingua.GetLocalizedString(_key + "/desc"), _itemRarityGoldsPerLevel.get_Item((Rarity)3) * level);
			}

			public override void Attach()
			{
			}

			public override void Detach()
			{
			}

			public int GetGoldByDiscard(Item item)
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				return _itemRarityGoldsPerLevel.get_Item(item.rarity) * base.level;
			}

			public int GetGoldByDiscard(Quintessence essence)
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				return _eseenceRarityGoldsPerLevel.get_Item(essence.rarity) * base.level;
			}
		}

		public class Tree
		{
			public ReadOnlyCollection<Bonus> list { get; protected set; }

			protected void InitializeTreeIndex()
			{
				for (int i = 0; i < list.Count; i++)
				{
					list[i].tree = this;
					list[i].indexInTree = i;
				}
			}
		}

		public class Skull : Tree
		{
			private const string _key = "witch/skull";

			public readonly StatBonus marrowImplant;

			public readonly StatBonus fastDislocation;

			public readonly StatBonusByWeaponCategory nutritionSupply;

			public readonly GetShieldOnSwap enhanceExoskeleton;

			public Skull(Character owner)
			{
				marrowImplant = new StatBonus("witch/skull/0", owner, GameData.Progress.witch.skull[0], WitchSettings.instance.골수이식_비용, new Stat.Value(Stat.Category.PercentPoint, Stat.Kind.MagicAttackDamage, (float)WitchSettings.instance.골수이식_마법공격력p * 0.01f));
				marrowImplant.Initialize();
				fastDislocation = new StatBonus("witch/skull/1", owner, GameData.Progress.witch.skull[1], WitchSettings.instance.신속한탈골_비용, new Stat.Value(Stat.Category.PercentPoint, Stat.Kind.SwapCooldownSpeed, WitchSettings.instance.신속한탈골_교대대기시간가속 * 0.01f));
				fastDislocation.Initialize();
				nutritionSupply = new StatBonusByWeaponCategory("witch/skull/2", owner, GameData.Progress.witch.skull[2], WitchSettings.instance.영양공급_비용, Weapon.Category.Balance, new Stat.Value(Stat.Category.PercentPoint, Stat.Kind.SkillCooldownSpeed, WitchSettings.instance.영양공급_스킬쿨다운p * 0.01f));
				nutritionSupply.Initialize();
				enhanceExoskeleton = new GetShieldOnSwap("witch/skull/3", owner, GameData.Progress.witch.skull[3], WitchSettings.instance.외골격강화_비용, WitchSettings.instance.외골격강화_보호막, WitchSettings.instance.외골격강화_보호막지속시간, WitchSettings.instance.외골격강화_교대대기시간감소);
				enhanceExoskeleton.Initialize();
				base.list = new ReadOnlyCollection<Bonus>(new Bonus[4] { marrowImplant, fastDislocation, nutritionSupply, enhanceExoskeleton });
				InitializeTreeIndex();
			}
		}

		public class Body : Tree
		{
			private const string _key = "witch/body";

			public readonly StatBonus strongBone;

			public readonly StatBonus fractureImmunity;

			public readonly StatBonusByWeaponCategory heavyFrame;

			public readonly ReviveOnce reassemble;

			public Body(Character owner)
			{
				strongBone = new StatBonus("witch/body/0", owner, GameData.Progress.witch.body[0], WitchSettings.instance.통뼈_비용, new Stat.Value(Stat.Category.PercentPoint, Stat.Kind.PhysicalAttackDamage, (float)WitchSettings.instance.통뼈_물리공격력p * 0.01f));
				strongBone.Initialize();
				fractureImmunity = new StatBonus("witch/body/1", owner, GameData.Progress.witch.body[1], WitchSettings.instance.골절상면역_비용, new Stat.Value(Stat.Category.Constant, Stat.Kind.Health, WitchSettings.instance.골절상면역_체력증가));
				fractureImmunity.Initialize();
				heavyFrame = new StatBonusByWeaponCategory("witch/body/2", owner, GameData.Progress.witch.body[2], WitchSettings.instance.육중한뼈대_비용, Weapon.Category.Power, new Stat.Value(Stat.Category.Percent, Stat.Kind.TakingDamage, WitchSettings.instance.육중한뼈대_받는피해 * 0.01f));
				heavyFrame.Initialize();
				reassemble = new ReviveOnce("witch/body/3", owner, GameData.Progress.witch.body[3], WitchSettings.instance.재조립_비용, (float)WitchSettings.instance.재조립_체력회복p * 0.01f);
				reassemble.Initialize();
				base.list = new ReadOnlyCollection<Bonus>(new Bonus[4] { strongBone, fractureImmunity, heavyFrame, reassemble });
				InitializeTreeIndex();
			}
		}

		public class Soul : Tree
		{
			private const string _key = "witch/soul";

			public readonly StatBonus soulAcceleration;

			public readonly StatBonus willOfAncestor;

			public readonly StatBonusByWeaponCategory fatalMind;

			public readonly Alchemy ancientAlchemy;

			public Soul(Character owner)
			{
				//IL_0198: Unknown result type (might be due to invalid IL or missing references)
				//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
				//IL_01e1: Expected O, but got Unknown
				//IL_01e1: Expected O, but got Unknown
				soulAcceleration = new StatBonus("witch/soul/0", owner, GameData.Progress.witch.soul[0], WitchSettings.instance.영혼가속_비용, new Stat.Value(Stat.Category.PercentPoint, Stat.Kind.CriticalChance, WitchSettings.instance.영혼가속_치명타확률p * 0.01f));
				soulAcceleration.Initialize();
				willOfAncestor = new StatBonus("witch/soul/1", owner, GameData.Progress.witch.soul[1], WitchSettings.instance.선조의의지_비용, new Stat.Value(Stat.Category.PercentPoint, Stat.Kind.EssenceCooldownSpeed, (float)WitchSettings.instance.선조의의지_정수쿨다운가속p * 0.01f));
				willOfAncestor.Initialize();
				fatalMind = new StatBonusByWeaponCategory("witch/soul/2", owner, GameData.Progress.witch.soul[2], WitchSettings.instance.날카로운정신_비용, Weapon.Category.Speed, new Stat.Values(new Stat.Value(Stat.Category.PercentPoint, Stat.Kind.AttackSpeed, (float)WitchSettings.instance.날카로운정신_공격속도p * 0.01f), new Stat.Value(Stat.Category.PercentPoint, Stat.Kind.MovementSpeed, (float)WitchSettings.instance.날카로운정신_이동속도p * 0.01f)));
				fatalMind.Initialize();
				ancientAlchemy = new Alchemy("witch/soul/3", owner, GameData.Progress.witch.soul[3], WitchSettings.instance.고대연금술_비용, new RarityPrices(new int[4]
				{
					WitchSettings.instance.고대연금술_골드량_커먼,
					WitchSettings.instance.고대연금술_골드량_레어,
					WitchSettings.instance.고대연금술_골드량_유니크,
					WitchSettings.instance.고대연금술_골드량_레전더리
				}), new RarityPrices(new int[4]
				{
					WitchSettings.instance.고대연금술_골드량_정수_커먼,
					WitchSettings.instance.고대연금술_골드량_정수_레어,
					WitchSettings.instance.고대연금술_골드량_정수_유니크,
					WitchSettings.instance.고대연금술_골드량_정수_레전더리
				}));
				ancientAlchemy.Initialize();
				base.list = new ReadOnlyCollection<Bonus>(new Bonus[4] { soulAcceleration, willOfAncestor, fatalMind, ancientAlchemy });
				InitializeTreeIndex();
			}
		}

		public static WitchBonus instance = new WitchBonus();

		public Skull skull { get; protected set; }

		public Body body { get; protected set; }

		public Soul soul { get; protected set; }

		public void Apply(Character character)
		{
			skull = new Skull(character);
			body = new Body(character);
			soul = new Soul(character);
		}
	}
}
