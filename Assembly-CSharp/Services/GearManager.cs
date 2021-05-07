using System.Collections.Generic;
using System.Linq;
using Characters.Gear;
using Characters.Gear.Items;
using Characters.Gear.Quintessences;
using Characters.Gear.Weapons;
using Characters.Player;
using FX;
using Singletons;
using UnityEngine;

namespace Services
{
	public class GearManager : MonoBehaviour
	{
		private readonly EnumArray<Rarity, Resource.ItemInfo[]> _items = new EnumArray<Rarity, Resource.ItemInfo[]>();

		private readonly EnumArray<Rarity, Resource.QuintessenceInfo[]> _quintessences = new EnumArray<Rarity, Resource.QuintessenceInfo[]>();

		private readonly EnumArray<Rarity, Resource.WeaponReference[]> _weapons = new EnumArray<Rarity, Resource.WeaponReference[]>();

		private readonly EnumArray<Rarity, List<Resource.GearReference>> _lockedGears = new EnumArray<Rarity, List<Resource.GearReference>>();

		private readonly List<Item> _itemInstances = new List<Item>();

		private readonly List<Quintessence> _essenceInstances = new List<Quintessence>();

		private readonly List<Weapon> _weaponInstances = new List<Weapon>();

		[Header("Drop effects")]
		[SerializeField]
		private EffectInfo _commonDropEffect;

		[SerializeField]
		private EffectInfo _rareDropEffect;

		[SerializeField]
		private EffectInfo _uniqueDropEffect;

		[SerializeField]
		private EffectInfo _legendaryDropEffect;

		[Header("Drop sounds")]
		[SerializeField]
		private SoundInfo _commonDropSound;

		[SerializeField]
		private SoundInfo _rareDropSound;

		[SerializeField]
		private SoundInfo _uniqueDropSound;

		[SerializeField]
		private SoundInfo _legendaryDropSound;

		private void Awake()
		{
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			Resource instance = Resource.instance;
			Resource.ItemInfo[] items = instance.items;
			Resource.QuintessenceInfo[] quintessences = instance.quintessences;
			Resource.WeaponReference[] weapons = instance.weapons;
			foreach (IGrouping<Rarity, Resource.ItemInfo> item in from item in items
				group item by item.rarity)
			{
				_items[item.Key] = item.ToArray();
			}
			foreach (IGrouping<Rarity, Resource.QuintessenceInfo> item2 in from quintessence in quintessences
				group quintessence by quintessence.rarity)
			{
				_quintessences[item2.Key] = item2.ToArray();
			}
			foreach (IGrouping<Rarity, Resource.WeaponReference> item3 in from weapon in weapons
				group weapon by weapon.rarity)
			{
				_weapons[item3.Key] = item3.ToArray();
			}
		}

		public void RegisterInstance(Item item)
		{
			_itemInstances.Add(item);
		}

		public void UnregisterInstance(Item item)
		{
			_itemInstances.Remove(item);
		}

		public void RegisterInstance(Quintessence essence)
		{
			_essenceInstances.Add(essence);
		}

		public void UnregisterInstance(Quintessence essence)
		{
			_essenceInstances.Remove(essence);
		}

		public void RegisterInstance(Weapon weapon)
		{
			_weaponInstances.Add(weapon);
		}

		public void UnregisterInstance(Weapon weapon)
		{
			_weaponInstances.Remove(weapon);
		}

		public void SpawnFx(Gear gear)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Expected I4, but got Unknown
			Vector3 position = gear.transform.position;
			Rarity rarity = gear.rarity;
			switch ((int)rarity)
			{
			case 0:
				_commonDropEffect.Spawn(position);
				PersistentSingleton<SoundManager>.Instance.PlaySound(_commonDropSound, position);
				break;
			case 1:
				_rareDropEffect.Spawn(position);
				PersistentSingleton<SoundManager>.Instance.PlaySound(_rareDropSound, position);
				break;
			case 2:
				_uniqueDropEffect.Spawn(position);
				PersistentSingleton<SoundManager>.Instance.PlaySound(_uniqueDropSound, position);
				break;
			case 3:
				_legendaryDropEffect.Spawn(position);
				PersistentSingleton<SoundManager>.Instance.PlaySound(_legendaryDropSound, position);
				break;
			}
		}

		private void UpdateLockedGearList()
		{
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			Resource instance = Resource.instance;
			Resource.ItemInfo[] items = instance.items;
			Resource.QuintessenceInfo[] quintessences = instance.quintessences;
			Resource.WeaponReference[] weapons = instance.weapons;
			List<Resource.GearReference> list = new List<Resource.GearReference>(items.Length + quintessences.Length + weapons.Length);
			list.AddRange(items);
			list.AddRange(quintessences);
			list.AddRange(weapons);
			foreach (IGrouping<Rarity, Resource.GearReference> item in from item in list
				group item by item.rarity)
			{
				_lockedGears[item.Key] = item.Where((Resource.GearReference gear) => gear.obtainable && !gear.unlocked).ToList();
			}
		}

		public Resource.GearReference GetGearToUnlock(Rarity rarity)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			UpdateLockedGearList();
			List<Resource.GearReference> item = _lockedGears[rarity];
			if (_lockedGears.Select((List<Resource.GearReference> list) => list.Count).Sum() == 0)
			{
				return null;
			}
			Resource.GearReference gearReference;
			if (TryGetGearToUnlock(rarity, out gearReference))
			{
				return gearReference;
			}
			List<Rarity> list2 = EnumValues<Rarity>.Values.ToList();
			int num = list2.IndexOf(rarity);
			if ((int)rarity == 0)
			{
				for (int i = 1; i < list2.Count; i++)
				{
					int index = (num + i) % list2.Count;
					if (TryGetGearToUnlock(list2[index], out gearReference))
					{
						return gearReference;
					}
				}
			}
			else
			{
				for (int j = 1; j < list2.Count; j++)
				{
					int num2 = num - j;
					if (num2 < 0)
					{
						num2 += list2.Count;
					}
					if (TryGetGearToUnlock(list2[num2], out gearReference))
					{
						return gearReference;
					}
				}
			}
			return null;
		}

		private bool TryGetGearToUnlock(Rarity rarity, out Resource.GearReference gearReference)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			gearReference = null;
			List<Resource.GearReference> list2 = _lockedGears[rarity];
			if (_lockedGears.Select((List<Resource.GearReference> list) => list.Count).Sum() == 0)
			{
				return false;
			}
			if (list2.Count == 0)
			{
				return false;
			}
			gearReference = list2.Random();
			return true;
		}

		public Resource.GearReference GetGearToTake(Rarity rarity)
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			switch (EnumValues<Gear.Type>.Values.Random())
			{
			case Gear.Type.Item:
				return GetItemToTake(rarity);
			case Gear.Type.Quintessence:
				return GetQuintessenceToTake(rarity);
			case Gear.Type.Weapon:
				return GetWeaponToTake(rarity);
			default:
				return null;
			}
		}

		public Resource.ItemInfo GetItemToTake(Rarity rarity)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			if (Singleton<Service>.Instance.levelManager.player == null)
			{
				return _items[rarity].Where((Resource.ItemInfo item) => item.obtainable && item.unlocked).Random();
			}
			ItemInventory item = Singleton<Service>.Instance.levelManager.player.playerComponents.inventory.item;
			IEnumerable<Resource.ItemInfo> enumerable = _items[rarity].Where(delegate(Resource.ItemInfo item)
			{
				if (!item.obtainable)
				{
					return false;
				}
				if (!item.unlocked)
				{
					return false;
				}
				for (int i = 0; i < _itemInstances.Count; i++)
				{
					if (item.name.Equals(_itemInstances[i].name))
					{
						return false;
					}
				}
				return true;
			});
			if (enumerable.Count() == 0)
			{
				return GetItemToTake((Rarity)(((int)rarity == 0) ? (rarity + 1) : (rarity - 1)));
			}
			return enumerable.Random();
		}

		public Resource.QuintessenceInfo GetQuintessenceToTake(Rarity rarity)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			if (Singleton<Service>.Instance.levelManager.player == null)
			{
				return _quintessences[rarity].Where((Resource.QuintessenceInfo essence) => essence.obtainable && essence.unlocked).Random();
			}
			QuintessenceInventory quintessence = Singleton<Service>.Instance.levelManager.player.playerComponents.inventory.quintessence;
			IEnumerable<Resource.QuintessenceInfo> enumerable = _quintessences[rarity].Where(delegate(Resource.QuintessenceInfo essence)
			{
				if (!essence.obtainable)
				{
					return false;
				}
				if (!essence.unlocked)
				{
					return false;
				}
				for (int i = 0; i < _essenceInstances.Count; i++)
				{
					if (essence.name.Equals(_essenceInstances[i].name))
					{
						return false;
					}
				}
				return true;
			});
			if (enumerable.Count() == 0)
			{
				return GetQuintessenceToTake((Rarity)(rarity - 1));
			}
			return enumerable.Random();
		}

		private string StripAwakeNumber(string name)
		{
			int num = name.IndexOf('_');
			if (num == -1)
			{
				return name;
			}
			return name.Substring(0, num);
		}

		public Resource.WeaponReference GetWeaponToTake(Rarity rarity)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			if (Singleton<Service>.Instance.levelManager.player == null)
			{
				return _weapons[rarity].Where((Resource.WeaponReference weapon) => weapon.obtainable && weapon.unlocked).Random();
			}
			WeaponInventory weapon = Singleton<Service>.Instance.levelManager.player.playerComponents.inventory.weapon;
			IEnumerable<Resource.WeaponReference> enumerable = _weapons[rarity].Where(delegate(Resource.WeaponReference weapon)
			{
				if (!weapon.obtainable)
				{
					return false;
				}
				if (!weapon.unlocked)
				{
					return false;
				}
				for (int i = 0; i < _weaponInstances.Count; i++)
				{
					if (StripAwakeNumber(weapon.name).Equals(StripAwakeNumber(_weaponInstances[i].name)))
					{
						return false;
					}
				}
				return true;
			});
			if (enumerable.Count() == 0)
			{
				return GetWeaponToTake((Rarity)(rarity - 1));
			}
			return enumerable.Random();
		}
	}
}
