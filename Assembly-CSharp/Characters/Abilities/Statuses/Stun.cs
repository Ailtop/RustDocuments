using FX;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.Abilities.Statuses
{
	public class Stun : IAbility, IAbilityInstance
	{
		private const string _floatingTextKey = "floating/status/stun";

		private const string _floatingTextColor = "#ffffff";

		private EffectInfo _effect;

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

		public IAbilityInstance CreateInstance(Character owner)
		{
			return this;
		}

		public Stun(Character owner)
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
			owner.CancelAction();
			owner.animationController.Stun();
			owner.blockLook.Attach(this);
			owner.movement?.blocked.Attach(this);
			_effect.DespawnChildren();
			_effect.Spawn(owner.transform.position, owner);
			SpawnFloatingText();
		}

		public void Detach()
		{
			remainTime = 0f;
			owner.animationController.StopAll();
			owner.blockLook.Detach(this);
			owner.movement?.blocked.Detach(this);
			_effect.DespawnChildren();
		}

		public void Initialize()
		{
			_effect = new EffectInfo(Resource.instance.stunEffect)
			{
				attachInfo = new EffectInfo.AttachInfo(true, false, 1, EffectInfo.AttachInfo.Pivot.Top),
				loop = true,
				trackChildren = true
			};
		}

		private void SpawnFloatingText()
		{
			Vector2 vector = MMMaths.RandomPointWithinBounds(owner.collider.bounds);
			Singleton<Service>.Instance.floatingTextSpawner.SpawnStatus(Lingua.GetLocalizedString("floating/status/stun"), vector, "#ffffff");
		}
	}
}
