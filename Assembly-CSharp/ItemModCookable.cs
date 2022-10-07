using UnityEngine;

public class ItemModCookable : ItemMod
{
	[ItemSelector(ItemCategory.All)]
	public ItemDefinition becomeOnCooked;

	public float cookTime = 30f;

	public int amountOfBecome = 1;

	public int lowTemp;

	public int highTemp;

	public bool setCookingFlag;

	public void OnValidate()
	{
		if (amountOfBecome < 1)
		{
			amountOfBecome = 1;
		}
		if (becomeOnCooked == null)
		{
			Debug.LogWarning("[ItemModCookable] becomeOnCooked is unset! [" + base.name + "]", base.gameObject);
		}
	}

	public bool CanBeCookedByAtTemperature(float temperature)
	{
		if (temperature > (float)lowTemp)
		{
			return temperature < (float)highTemp;
		}
		return false;
	}

	private void CycleCooking(Item item, float delta)
	{
		if (!CanBeCookedByAtTemperature(item.temperature) || item.cookTimeLeft < 0f)
		{
			if (setCookingFlag && item.HasFlag(Item.Flag.Cooking))
			{
				item.SetFlag(Item.Flag.Cooking, b: false);
				item.MarkDirty();
			}
			return;
		}
		if (setCookingFlag && !item.HasFlag(Item.Flag.Cooking))
		{
			item.SetFlag(Item.Flag.Cooking, b: true);
			item.MarkDirty();
		}
		item.cookTimeLeft -= delta;
		if (item.cookTimeLeft > 0f)
		{
			item.MarkDirty();
			return;
		}
		float num = item.cookTimeLeft * -1f;
		int a = 1 + Mathf.FloorToInt(num / cookTime);
		item.cookTimeLeft = cookTime - num % cookTime;
		BaseOven baseOven = item.GetEntityOwner() as BaseOven;
		a = Mathf.Min(a, item.amount);
		if (item.amount > a)
		{
			item.amount -= a;
			item.MarkDirty();
		}
		else
		{
			item.Remove();
		}
		if (!(becomeOnCooked != null))
		{
			return;
		}
		Item item2 = ItemManager.Create(becomeOnCooked, amountOfBecome * a, 0uL);
		if (item2 != null && !item2.MoveToContainer(item.parent) && !item2.MoveToContainer(item.parent))
		{
			item2.Drop(item.parent.dropPosition, item.parent.dropVelocity);
			if ((bool)item.parent.entityOwner && baseOven != null)
			{
				baseOven.OvenFull();
			}
		}
	}

	public override void OnItemCreated(Item itemcreated)
	{
		itemcreated.cookTimeLeft = cookTime;
		itemcreated.onCycle += CycleCooking;
	}
}
