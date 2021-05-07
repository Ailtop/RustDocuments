using System;
using Characters.Abilities.CharacterStat;
using Characters.Abilities.Customs;
using Characters.Abilities.Enemies;
using UnityEditor;
using UnityEngine;

namespace Characters.Abilities
{
	public abstract class AbilityComponent : MonoBehaviour
	{
		public class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public new static readonly Type[] types;

			public new static readonly string[] names;

			static SubcomponentAttribute()
			{
				types = new Type[101]
				{
					typeof(StatBonusComponent),
					typeof(StackableStatBonusComponent),
					typeof(StackableStatBonusByTimeComponent),
					null,
					typeof(StatBonusByAirTimeComponent),
					typeof(StatBonusByMovingComponent),
					typeof(StatBonusByShieldComponent),
					typeof(StatBonusByIncomeComponent),
					typeof(StatBonusBySkillsInCooldownComponent),
					typeof(StatBonusByOtherStatComponent),
					typeof(StatBonusByGaveDamageComponent),
					typeof(StatBonusByKillComponent),
					null,
					typeof(StatByApplyingStatusCountsWithinRangeComponent),
					typeof(StatByCountsWithinRangeComponent),
					null,
					typeof(StatPerLostHealthComponent),
					typeof(StatBonusPerGearRarityComponent),
					typeof(StatBonusPerGearTagComponent),
					null,
					typeof(AttachAbilityOnGaveDamageComponent),
					typeof(CurseOfLightComponent),
					typeof(AirAndGroundAttackDamageComponent),
					typeof(ExtraDamageToBackComponent),
					typeof(ModifyDamageComponent),
					typeof(ModifyDamageByTargetLayerComponent),
					typeof(ModifyTrapDamageComponent),
					typeof(ChangeTakingDamageToOneComponent),
					typeof(IgnoreTakingDamageByDirectionComponent),
					typeof(IgnoreTrapDamageComponent),
					typeof(CriticalToMaximumHealthComponent),
					null,
					typeof(AddAirJumpCountComponent),
					typeof(AttachAbilityToTargetOnGaveDamageComponent),
					typeof(AddGaugeValueOnGaveDamageComponent),
					typeof(AdditionalHitComponent),
					typeof(AdditionalHitByTargetStatusComponent),
					typeof(AdditionalHitToStatusTakerComponent),
					typeof(ApplyStatusOnGaveDamageComponent),
					typeof(AttachAbilityWithinColliderComponent),
					null,
					typeof(ReduceCooldownByTriggerComponent),
					typeof(ReduceSwapCooldownByTriggerComponent),
					typeof(IgnoreSkillCooldownComponent),
					null,
					typeof(ChangeActionComponent),
					typeof(CurrencyBonusComponent),
					typeof(GetInvulnerableComponent),
					typeof(GiveStatusOnGaveDamageComponent),
					typeof(ModifyTimeScaleComponent),
					typeof(OperationByTriggerComponent),
					typeof(OverrideMovementConfigComponent),
					typeof(PeriodicHealComponent),
					typeof(ShieldComponent),
					typeof(WeaponPolymorphComponent),
					typeof(ReviveComponent),
					typeof(AlchemistGaugeBoostComponent),
					typeof(AlchemistGaugeDeactivateComponent),
					typeof(AlchemistPassiveComponent),
					null,
					typeof(RiderPassiveComponent),
					typeof(RiderSkeletonRiderComponent),
					null,
					typeof(ThiefPassiveComponent),
					typeof(SpawnThiefGoldOnTookDamageComponent),
					typeof(SpawnThiefGoldOnGaveDamageComponent),
					null,
					typeof(MummyPassiveComponent),
					typeof(MummyGunDropPassiveComponent),
					null,
					typeof(BombSkulPassiveComponent),
					typeof(ArchlichSoulLootingPassiveComponent),
					typeof(AwakenDarkPaladinPassiveComponent),
					typeof(Berserker2PassiveComponent),
					typeof(GhoulPassiveComponent),
					typeof(LivingArmorPassiveComponent),
					typeof(PrisonerPassiveComponent),
					typeof(RecruitPassiveComponent),
					typeof(RockstarPassiveComponent),
					typeof(SamuraiPassive2Component),
					null,
					typeof(BoneOfBraveComponent),
					typeof(BoneOfManaComponent),
					typeof(BoneOfSpeedComponent),
					typeof(CriticalChanceByDistanceComponent),
					typeof(MagesManaBraceletComponent),
					typeof(MedalOfCarleonComponent),
					typeof(NonConsumptionComponent),
					null,
					typeof(Skeleton_ShieldExplosionPassiveComponent),
					typeof(Skeleton_Shield4GuardComponent),
					null,
					typeof(ElderEntsGratitudeComponent),
					typeof(OffensiveWheelComponent),
					typeof(GoldenManeRapierComponent),
					typeof(ForbiddenSwordComponent),
					typeof(ChimerasFangComponent),
					typeof(UnknownSeedComponent),
					typeof(DoomsdayComponent),
					typeof(LeoniasGraceComponent),
					typeof(CretanBullComponent)
				};
				int length = typeof(AbilityComponent).Namespace.Length;
				names = new string[types.Length];
				for (int i = 0; i < names.Length; i++)
				{
					Type type = types[i];
					if (type == null)
					{
						string text = names[i - 1];
						int num = text.LastIndexOf('/');
						if (num == -1)
						{
							names[i] = string.Empty;
						}
						else
						{
							names[i] = text.Substring(0, num + 1);
						}
					}
					else
					{
						names[i] = type.FullName.Substring(length + 1, type.FullName.Length - length - 1).Replace('.', '/');
					}
				}
			}

			public SubcomponentAttribute()
				: base(true, types, names)
			{
			}
		}

		[Serializable]
		public class Subcomponents : SubcomponentArray<AbilityComponent>
		{
			public void Initialize()
			{
				for (int i = 0; i < base.components.Length; i++)
				{
					base.components[i].Initialize();
				}
			}
		}

		public abstract IAbility ability { get; }

		public abstract void Initialize();

		public abstract IAbilityInstance CreateInstance(Character owner);
	}
	public abstract class AbilityComponent<T> : AbilityComponent where T : Ability
	{
		[SerializeField]
		protected T _ability;

		public override IAbility ability => _ability;

		public T baseAbility => _ability;

		public override void Initialize()
		{
			_ability.Initialize();
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return _ability.CreateInstance(owner);
		}
	}
}
