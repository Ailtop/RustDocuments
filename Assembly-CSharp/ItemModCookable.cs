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

	public override void OnItemCreated(Item itemcreated)
	{
		float cooktimeLeft = cookTime;
		itemcreated.onCycle += delegate(Item item, float delta)
		{
			if (!CanBeCookedByAtTemperature(item.temperature) || cooktimeLeft < 0f)
			{
				if (setCookingFlag && item.HasFlag(Item.Flag.Cooking))
				{
					item.SetFlag(Item.Flag.Cooking, b: false);
					item.MarkDirty();
				}
			}
			else
			{
				if (setCookingFlag && !item.HasFlag(Item.Flag.Cooking))
				{
					item.SetFlag(Item.Flag.Cooking, b: true);
					item.MarkDirty();
				}
				cooktimeLeft -= delta;
				if (!(cooktimeLeft > 0f))
				{
					BaseOven baseOven = item.GetEntityOwner() as BaseOven;
					int a = ((baseOven == null) ? 1 : baseOven.GetSmeltingSpeed());
					a = Mathf.Min(a, item.amount);
					int iTargetPos = item.position + 1;
					if (item.amount > a)
					{
						cooktimeLeft = cookTime;
						item.amount -= a;
						item.MarkDirty();
					}
					else
					{
						item.Remove();
					}
					if (becomeOnCooked != null)
					{
						Item item2 = ItemManager.Create(becomeOnCooked, amountOfBecome * a, 0uL);
						if (item2 != null && !item2.MoveToContainer(item.parent, iTargetPos) && !item2.MoveToContainer(item.parent))
						{
							item2.Drop(item.parent.dropPosition, item.parent.dropVelocity);
							if ((bool)item.parent.entityOwner && baseOven != null)
							{
								baseOven.OvenFull();
							}
						}
					}
				}
			}
		};
	}
}
