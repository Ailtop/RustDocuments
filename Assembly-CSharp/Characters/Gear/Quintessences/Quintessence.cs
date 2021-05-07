using System;
using Characters.Cooldowns;
using Characters.Player;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.Gear.Quintessences
{
	public class Quintessence : Gear
	{
		[SerializeField]
		private CooldownSerializer _cooldown;

		public override Type type => Type.Quintessence;

		public CooldownSerializer cooldown => _cooldown;

		protected override string _prefix => "quintessence";

		public string activeName => Lingua.GetLocalizedString(base._keyBase + "/active/name");

		public string activeDescription => Lingua.GetLocalizedString(base._keyBase + "/active/desc");

		public Sprite silhouette => Resource.instance.GetQuintessenceSilhouette(base.name);

		public Sprite hudIcon => Resource.instance.GetQuintessenceHudIcon(base.name) ?? base.icon;

		public override int currencyByDiscard => 0;

		public event Action onUse;

		protected override void OnLoot(Character character)
		{
			QuintessenceInventory quintessence = character.playerComponents.inventory.quintessence;
			if (!quintessence.TryEquip(this))
			{
				quintessence.EquipAt(this, 0);
			}
			base.owner = character;
			base.state = State.Equipped;
		}

		protected override void Awake()
		{
			base.Awake();
			Singleton<Service>.Instance.gearManager.RegisterInstance(this);
			_cooldown.Serialize();
		}

		private void OnDestroy()
		{
			if (!Service.quitting)
			{
				Singleton<Service>.Instance.gearManager.UnregisterInstance(this);
				_onDiscard?.Invoke();
			}
		}

		protected override void OnEquipped()
		{
			base.OnEquipped();
			if (_cooldown.type == CooldownSerializer.Type.Time)
			{
				_cooldown.time.GetCooldownSpeed = base.owner.stat.GetQuintessenceCooldownSpeed;
			}
		}

		public void Use()
		{
			if (_cooldown.Consume())
			{
				this.onUse?.Invoke();
			}
		}
	}
}
