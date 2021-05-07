using System;
using Characters;
using Characters.Gear.Items;
using Characters.Player;
using Singletons;
using UnityEngine;

namespace Level
{
	public class DroppedItem : InteractiveObject
	{
		[NonSerialized]
		public Item item;

		[SerializeField]
		private PoolObject _effect;

		[SerializeField]
		[GetComponent]
		private DropMovement _dropMovement;

		[SerializeField]
		private AudioClip _clip;

		protected override void Awake()
		{
			base.Awake();
			_dropMovement.onGround += Activate;
		}

		private void OnEnable()
		{
			Deactivate();
		}

		public override void InteractWith(Character character)
		{
			if (_clip != null)
			{
				PersistentSingleton<SoundManager>.Instance.PlaySound(_clip, base.transform.position);
			}
			ItemInventory itemInventory = character.playerComponents.inventory.item;
			if (!itemInventory.TryEquip(item))
			{
				itemInventory.EquipAt(item, 0);
			}
			_effect.Spawn(base.transform.position);
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
