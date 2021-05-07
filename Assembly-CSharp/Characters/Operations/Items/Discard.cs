using Characters.Gear.Items;
using UnityEngine;

namespace Characters.Operations.Items
{
	public class Discard : Operation
	{
		[SerializeField]
		private Item _item;

		public override void Run()
		{
			_item.DiscardOnInventory();
		}
	}
}
