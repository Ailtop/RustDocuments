using System;
using System.Collections.Generic;
using Characters.Controllers;
using Characters.Gear;
using Characters.Gear.Quintessences;
using UnityEngine;

namespace Characters.Player
{
	public class QuintessenceInventory : MonoBehaviour
	{
		[SerializeField]
		[GetComponent]
		private Character _character;

		[SerializeField]
		[GetComponent]
		private PlayerInput _input;

		public List<Quintessence> items { get; } = new List<Quintessence> { null };


		public event Action onChanged;

		private void Trim()
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

		public void UseAt(int index)
		{
			Quintessence quintessence = items[index];
			if (quintessence != null)
			{
				quintessence.Use();
			}
		}

		public bool TryEquip(Quintessence item)
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

		public void EquipAt(Quintessence item, int index)
		{
			Quintessence quintessence = items[index];
			if (quintessence != null)
			{
				_character.stat.DetachValues(quintessence.stat);
				quintessence.state = Characters.Gear.Gear.State.Dropped;
			}
			quintessence = item;
			quintessence.transform.parent = _character.@base;
			quintessence.transform.localPosition = Vector3.zero;
			_character.stat.AttachValues(quintessence.stat);
			items[index] = quintessence;
			this.onChanged?.Invoke();
		}

		public bool RemoveAt(int index)
		{
			Quintessence quintessence = items[index];
			if (quintessence == null)
			{
				return false;
			}
			_character.stat.DetachValues(quintessence.stat);
			quintessence.state = Characters.Gear.Gear.State.Dropped;
			items[index] = null;
			this.onChanged?.Invoke();
			return true;
		}

		public bool Discard(int index)
		{
			Quintessence quintessence = items[index];
			if (RemoveAt(index))
			{
				UnityEngine.Object.Destroy(quintessence.gameObject);
				Trim();
				return true;
			}
			return false;
		}

		public int GetCountByRarity(Rarity rarity)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			int num = 0;
			foreach (Quintessence item in items)
			{
				if (!(item == null) && item.rarity == rarity)
				{
					num++;
				}
			}
			return num;
		}
	}
}
