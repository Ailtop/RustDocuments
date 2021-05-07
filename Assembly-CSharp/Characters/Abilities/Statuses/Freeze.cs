using System.Linq;
using FX;
using FX.SpriteEffects;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.Abilities.Statuses
{
	public class Freeze : IAbility, IAbilityInstance
	{
		public const string damageKey = "status_freeze";

		private const string _floatingTextKey = "floating/status/freeze";

		private const string _floatingTextColor = "#04FFE6";

		private static readonly EnumArray<Character.SizeForEffect, ParticleEffectInfo> _particles;

		private static readonly ColorBlend _colorOverlay;

		private EffectInfo _effect;

		public double damageOnEnd;

		public Character attacker;

		public Character owner { get; private set; }

		public IAbility ability => this;

		public float remainTime { get; set; }

		public bool attached => true;

		public Sprite icon => null;

		public float iconFillAmount => remainTime / duration;

		public bool iconFillInversed => false;

		public bool iconFillFlipped => false;

		public int iconStacks => 0;

		public bool expired => remainTime <= 0f;

		public float duration { get; set; }

		public int iconPriority => 0;

		public bool removeOnSwapWeapon => false;

		static Freeze()
		{
			_particles = new EnumArray<Character.SizeForEffect, ParticleEffectInfo>();
			_colorOverlay = new ColorBlend(100, new Color(0f, 37f / 255f, 11f / 15f, 1f), 0f);
			_particles[Character.SizeForEffect.Small] = Resource.instance.freezeSmallParticle;
			_particles[Character.SizeForEffect.Medium] = Resource.instance.freezeMediumParticle;
			_particles[Character.SizeForEffect.Large] = Resource.instance.freezeLargeParticle;
			_particles[Character.SizeForEffect.ExtraLarge] = Resource.instance.freezeLargeParticle;
		}

		public IAbilityInstance CreateInstance(Character owner)
		{
			return this;
		}

		public Freeze(Character owner)
		{
			this.owner = owner;
		}

		public void UpdateTime(float deltaTime)
		{
			remainTime -= deltaTime;
		}

		public void Refresh()
		{
			remainTime = duration;
		}

		public void Attach()
		{
			remainTime = duration;
			owner.chronometer.animation.AttachTimeScale(this, 0f);
			owner.spriteEffectStack.Add(_colorOverlay);
			owner.blockLook.Attach(this);
			owner.movement?.blocked.Attach(this);
			_effect.DespawnChildren();
			ReusableChronoSpriteEffect reusableChronoSpriteEffect = _effect.Spawn(owner.transform.position, owner);
			reusableChronoSpriteEffect.renderer.sharedMaterial = Materials.effect_linearDodge;
			reusableChronoSpriteEffect.renderer.sortingLayerID = SortingLayer.layers.Last().id;
			_effect.Spawn(owner.transform.position, owner).renderer.sortingLayerID = SortingLayer.layers[0].id;
			SpawnFloatingText();
		}

		public void Detach()
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(GlobalSoundSettings.instance.endFreeze, owner.transform.position);
			GiveDamage();
			remainTime = 0f;
			owner.chronometer.animation.DetachTimeScale(this);
			owner.spriteEffectStack.Remove(_colorOverlay);
			owner.movement.push.Expire();
			owner.blockLook.Detach(this);
			owner.movement?.blocked.Detach(this);
			_effect.DespawnChildren();
			_particles[owner.sizeForEffect].Emit(owner.transform.position, owner.collider.bounds, Vector2.zero);
		}

		private void GiveDamage()
		{
			if (damageOnEnd != 0.0 && !owner.health.dead)
			{
				Damage damage = new Damage(attacker, damageOnEnd, MMMaths.RandomPointWithinBounds(owner.collider.bounds), Damage.Attribute.Fixed, Damage.AttackType.Additional, Damage.MotionType.Status, "status_freeze");
				attacker.Attack(owner, ref damage);
			}
		}

		public void Initialize()
		{
			RuntimeAnimatorController freezeAnimator = Resource.instance.GetFreezeAnimator(owner.collider.size * 32f);
			_effect = new EffectInfo(freezeAnimator)
			{
				attachInfo = new EffectInfo.AttachInfo(true, false, 1, EffectInfo.AttachInfo.Pivot.Center),
				loop = true,
				trackChildren = true,
				sortingLayerId = SortingLayer.layers.Last().id
			};
		}

		private void SpawnFloatingText()
		{
			Vector2 vector = MMMaths.RandomPointWithinBounds(owner.collider.bounds);
			Singleton<Service>.Instance.floatingTextSpawner.SpawnStatus(Lingua.GetLocalizedString("floating/status/freeze"), vector, "#04FFE6");
		}
	}
}
