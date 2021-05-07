using System;
using System.Collections.Generic;
using Characters.Abilities.Spirits;
using Characters.Gear;
using Characters.Gear.Items;
using UnityEngine;

namespace Characters.Player
{
	public class ItemInventory : MonoBehaviour
	{
		[SerializeField]
		[GetComponent]
		private Character _character;

		[SerializeField]
		private Transform[] _slots;

		private List<Spirit> _spirits;

		public List<Item> items { get; } = new List<Item> { null, null, null, null, null, null, null, null, null };


		public event Action onChanged;

		private void Awake()
		{
			_spirits = new List<Spirit>(_slots.Length);
		}

		public int IndexOf(Item item)
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i] == item)
				{
					return i;
				}
			}
			return -1;
		}

		public void Trim()
		{
			int num = 0;
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i] == null)
				{
					num++;
				}
				else
				{
					items.Swap(i, i - num);
				}
			}
		}

		public bool TryEquip(Item item)
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i] == null)
				{
					EquipAt(item, i);
					return true;
				}
			}
			return false;
		}

		public void RemoveAll()
		{
			for (int i = 0; i < items.Count; i++)
			{
				Remove(i);
			}
		}

		public void Remove(Item item)
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (!(items[i] == null) && !(items[i] != item))
				{
					Remove(i);
				}
			}
		}

		public bool Drop(int index)
		{
			Item item = items[index];
			if (item == null)
			{
				return false;
			}
			_character.stat.DetachValues(item.stat);
			item.state = Characters.Gear.Gear.State.Dropped;
			items[index] = null;
			this.onChanged?.Invoke();
			return true;
		}

		public bool Remove(int index)
		{
			Item item = items[index];
			if (!Drop(index))
			{
				return false;
			}
			item.destructible = false;
			item.transform.parent = _character.@base;
			item.gameObject.SetActive(false);
			base.transform.localScale = Vector3.one;
			return true;
		}

		public bool Discard(Item item)
		{
			int num = IndexOf(item);
			if (num == -1)
			{
				return false;
			}
			return Discard(num);
		}

		public bool Discard(int index)
		{
			Item item = items[index];
			if (!Drop(index))
			{
				return false;
			}
			UnityEngine.Object.Destroy(item.gameObject);
			return true;
		}

		public void EquipAt(Item item, int index)
		{
			Drop(index);
			item.transform.parent = _character.@base;
			item.transform.localPosition = Vector3.zero;
			_character.stat.AttachValues(item.stat);
			items[index] = item;
			this.onChanged?.Invoke();
		}

		public void Change(Item old, Item @new)
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (!(items[i] == null) && items[i].name.Equals(old.name, StringComparison.OrdinalIgnoreCase))
				{
					ChangeAt(@new, i);
				}
			}
		}

		public void ChangeAt(Item @new, int index)
		{
			Remove(index);
			EquipAt(@new, index);
		}

		public void AttachSpirit(Spirit spirit)
		{
			_spirits.Add(spirit);
			SortSpiritPositions();
		}

		public void DetachSpirit(Spirit spirit)
		{
			_spirits.Remove(spirit);
			SortSpiritPositions();
		}

		private void SortSpiritPositions()
		{
			for (int i = 0; i < _spirits.Count; i++)
			{
				_spirits[i].targetPosition = _slots[i];
			}
		}

		public int GetItemCountByRarity(Rarity rarity)
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			int num = 0;
			for (int i = 0; i < items.Count; i++)
			{
				if (!(items[i] == null) && items[i].rarity == rarity)
				{
					num++;
				}
			}
			return num;
		}

		public int GetItemCountByTag(Characters.Gear.Gear.Tag tag)
		{
			int num = 0;
			for (int i = 0; i < items.Count; i++)
			{
				if (!(items[i] == null) && items[i].gearTag.HasFlag(tag))
				{
					num++;
				}
			}
			return num;
		}

		public bool Has(Item item)
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (!(items[i] == null) && items[i].name.Equals(item.name, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}
	}
}
