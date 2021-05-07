using System;
using Characters.Operations.Attack;
using Characters.Operations.Customs;
using Characters.Operations.Customs.AquaSkull;
using Characters.Operations.Customs.BombSkul;
using Characters.Operations.Customs.EntSkul;
using Characters.Operations.Decorator;
using Characters.Operations.Decorator.Deprecated;
using Characters.Operations.Fx;
using Characters.Operations.Gauge;
using Characters.Operations.Health;
using Characters.Operations.Items;
using Characters.Operations.Movement;
using Characters.Operations.ObjectTransform;
using Characters.Operations.Summon;
using UnityEditor;

namespace Characters.Operations
{
	public abstract class CharacterOperation : TargetedCharacterOperation
	{
		public new class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public SubcomponentAttribute()
				: base(true, CharacterOperation.types, CharacterOperation.names)
			{
			}
		}

		[Serializable]
		public class Subcomponents : SubcomponentArray<CharacterOperation>
		{
			public void Initialize()
			{
				for (int i = 0; i < base.components.Length; i++)
				{
					base.components[i].Initialize();
				}
			}

			public void Run(Character owner)
			{
				for (int i = 0; i < base.components.Length; i++)
				{
					base.components[i].Run(owner);
				}
			}

			public void Run(Character owner, Character target)
			{
				Run(target);
			}

			public void Stop()
			{
				for (int i = 0; i < base.components.Length; i++)
				{
					base.components[i].Stop();
				}
			}
		}

		public new static readonly Type[] types;

		public new static readonly string[] names;

		static CharacterOperation()
		{
			types = new Type[140]
			{
				typeof(InstantAttack),
				typeof(InstantAttack2),
				null,
				typeof(SweepAttack),
				typeof(SweepAttack2),
				null,
				typeof(CastAttack),
				null,
				typeof(FireProjectile),
				typeof(FireProjectileInBounds),
				typeof(MultipleFireProjectile),
				typeof(CameraShake),
				typeof(CameraShakeCurve),
				typeof(CameraZoom),
				null,
				typeof(PlayMusic),
				typeof(PlayChapterMusic),
				typeof(PauseBackgroundMusic),
				typeof(StopMusic),
				typeof(PlaySound),
				typeof(SetInternalMusicVolume),
				null,
				typeof(MotionTrail),
				typeof(ScreenFlash),
				typeof(ShaderEffect),
				typeof(SpawnEffect),
				typeof(SpawnEffectOnScreen),
				null,
				typeof(Vignette),
				typeof(Vibration),
				typeof(SpawnLineText),
				typeof(DropParts),
				typeof(ByAbility),
				typeof(Repeater),
				typeof(Repeater2),
				typeof(Repeater3),
				typeof(Chance),
				typeof(Characters.Operations.Decorator.Random),
				typeof(WeightedRandom),
				typeof(ByLookingDirection),
				typeof(OneByOne),
				typeof(RandomlyRunningOperation),
				typeof(SummonCharacter),
				typeof(SummonMinion),
				typeof(SummonMonster),
				typeof(SummonOperationRunner),
				typeof(SummonMultipleOperationRunners),
				typeof(SummonOperationRunnersOnGround),
				typeof(SummonOperationRunnersAtTargetWithinRange),
				typeof(Move),
				typeof(DualMove),
				null,
				typeof(ChangeGravity),
				typeof(ModifyVerticalVelocity),
				typeof(OverrideMovementConfig),
				typeof(Jump),
				typeof(JumpDown),
				typeof(Teleport),
				typeof(TeleportToCharacter),
				typeof(TeleportOverTime),
				typeof(FlipObject),
				typeof(SetPositionTo),
				typeof(SetRotationTo),
				typeof(MoveTransform),
				typeof(MoveTransformFromPosition),
				typeof(ResetGlobalTransformToLocal),
				typeof(RotateAngle),
				typeof(RotateTransform),
				typeof(RotateToTarget),
				typeof(Heal),
				typeof(LoseHealth),
				typeof(Invulnerable),
				typeof(Suicide),
				typeof(AddGaugeValue),
				typeof(SetGaugeValue),
				typeof(ClearGaugeValue),
				typeof(Change),
				typeof(Remove),
				typeof(Discard),
				typeof(ModifyTimeScale),
				typeof(ApplyStatus),
				typeof(AddMarkStack),
				null,
				typeof(AttachAbility),
				typeof(AttachAbilityWithinCollider),
				null,
				typeof(Polymorph),
				typeof(StartWeaponPolymorph),
				typeof(EndWeaponPolymorph),
				null,
				typeof(SwapWeapon),
				typeof(ReduceCooldownTime),
				typeof(SetRemainCooldownTime),
				typeof(TriggerActionStart),
				typeof(DoAction),
				null,
				typeof(ActivateGameObjectOperation),
				typeof(DeactivateGameObject),
				null,
				typeof(SpawnCharacter),
				typeof(SpawnRandomCharacter),
				typeof(DestoryCharacter),
				typeof(SetCharacterVisible),
				null,
				typeof(LookAt),
				typeof(LookTarget),
				typeof(LookTargetOpposition),
				null,
				typeof(TakeAim),
				typeof(TakeAimTowardsTheFront),
				typeof(GiveBuff),
				null,
				typeof(SpawnGold),
				typeof(ConsumeGold),
				null,
				typeof(InvokeUnityEvent),
				typeof(LerpCollider),
				typeof(StopAnotherOperation),
				typeof(PrintDebugLog),
				typeof(TeleportToSkulHead),
				typeof(SpawnThiefGoldAtTarget),
				typeof(ArchlichPassive),
				typeof(AddYakshaStompStack),
				typeof(PrisonerPhaser),
				typeof(Samurai2IlseomInstantAttack),
				null,
				typeof(AddRockstarPassiveStack),
				typeof(SummonRockstarAmp),
				typeof(FireHighTideProjectile),
				typeof(FireLowTideProjectile),
				typeof(FireWaterspoutProjectile),
				typeof(SetFloodSweepAttackDamageMultiplier),
				typeof(Explode),
				typeof(AddDamageStack),
				typeof(RiskyUpgrade),
				typeof(SummonSmallBomb),
				typeof(EntSkulPassive),
				typeof(EntSkulThornyVine),
				typeof(SummonEntMinionAtEntSapling),
				typeof(SummonEntSapling)
			};
			int length = typeof(CharacterOperation).Namespace.Length;
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

		public abstract void Run(Character owner);

		public override void Run(Character owner, Character target)
		{
			Run(target);
		}

		public virtual void Stop()
		{
		}

		protected virtual void OnDestroy()
		{
			Stop();
		}
	}
}
