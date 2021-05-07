using Characters.Gear.Items;
using UnityEngine;

namespace Characters.Operations.Items
{
	public class Change : Operation
	{
		[SerializeField]
		private Item _item;

		[SerializeField]
		private Item _itemToChange;

		public override void Run()
		{
			_item.ChangeOnInventory(_itemToChange.Instantiate());
		}
	}
}
