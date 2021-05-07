using Characters.Abilities;
using Characters.Gear.Synergy.Keywords;
using Characters.Player;
using Data;
using FX;
using Level;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.Gear.Items
{
	public class Item : Gear
	{
		public Keyword.Key keyword1;

		public Keyword.Key keyword2;

		[SerializeField]
		[AbilityAttacher.Subcomponent]
		private AbilityAttacher.Subcomponents _abilityAttacher;

		public override Type type => Type.Item;

		public override GameData.Currency.Type currencyTypeByDiscard => GameData.Currency.Type.Gold;

		public override int currencyByDiscard
		{
			get
			{
				if (base.dropped.price <= 0 && destructible)
				{
					return WitchBonus.instance.soul.ancientAlchemy.GetGoldByDiscard(this);
				}
				return 0;
			}
		}

		protected override string _prefix => "item";

		protected override void Awake()
		{
			base.Awake();
			Singleton<Service>.Instance.gearManager.RegisterInstance(this);
		}

		private void OnDestroy()
		{
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Expected I4, but got Unknown
			if (Service.quitting)
			{
				return;
			}
			Singleton<Service>.Instance.gearManager.UnregisterInstance(this);
			_abilityAttacher.StopAttach();
			_onDiscard?.Invoke();
			LevelManager levelManager = Singleton<Service>.Instance.levelManager;
			if (levelManager.player == null || !levelManager.player.liveAndActive)
			{
				return;
			}
			if (destructible)
			{
				PersistentSingleton<SoundManager>.Instance.PlaySound(GlobalSoundSettings.instance.gearDestroying, base.transform.position);
			}
			if (currencyByDiscard == 0)
			{
				return;
			}
			int count = 1;
			if (currencyByDiscard > 0)
			{
				Rarity val = base.rarity;
				switch ((int)val)
				{
				case 0:
					count = 5;
					break;
				case 1:
					count = 8;
					break;
				case 2:
					count = 15;
					break;
				case 3:
					count = 25;
					break;
				}
			}
			levelManager.DropGold(currencyByDiscard, count);
		}

		protected override void OnLoot(Character character)
		{
			if (character.playerComponents.inventory.item.TryEquip(this))
			{
				base.owner = character;
				base.state = State.Equipped;
			}
		}

		protected override void OnEquipped()
		{
			base.OnEquipped();
			_abilityAttacher.Initialize(base.owner);
			_abilityAttacher.StartAttach();
		}

		protected override void OnDropped()
		{
			base.OnDropped();
			_abilityAttacher.StopAttach();
		}

		public void DiscardOnInventory()
		{
			if (base.state == State.Dropped)
			{
				Object.Destroy(base.gameObject);
				return;
			}
			ItemInventory item = base.owner.playerComponents.inventory.item;
			item.Discard(this);
			item.Trim();
		}

		public void RemoveOnInventory()
		{
			if (base.state == State.Dropped)
			{
				destructible = false;
				Object.Destroy(base.gameObject);
			}
			else
			{
				ItemInventory item = base.owner.playerComponents.inventory.item;
				item.Remove(this);
				item.Trim();
			}
		}

		public void ChangeOnInventory(Item item)
		{
			if (base.state == State.Dropped)
			{
				Debug.LogError("Tried change item " + base.name + " but it's not on inventory");
				return;
			}
			base.owner.playerComponents.inventory.item.Change(this, item);
			item.owner = base.owner;
			item.state = State.Equipped;
		}

		public Item Instantiate()
		{
			Item item = Object.Instantiate(this);
			item.name = base.name;
			item.Initialize();
			return item;
		}
	}
}
