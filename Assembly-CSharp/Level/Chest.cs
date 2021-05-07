using System;
using Characters;
using Characters.Gear.Items;
using Services;
using Singletons;
using UnityEngine;

namespace Level
{
	public class Chest : InteractiveObject, ILootable
	{
		private const float _delayToDrop = 0.5f;

		[SerializeField]
		[GetComponent]
		private Animator _animator;

		[SerializeField]
		private RuntimeAnimatorController _commonChest;

		[SerializeField]
		private RuntimeAnimatorController _rareChest;

		[SerializeField]
		private RuntimeAnimatorController _uniqueChest;

		[SerializeField]
		private RuntimeAnimatorController _legendaryChest;

		private Rarity _rarity;

		private Rarity _gearRarity;

		private Resource.ItemInfo _itemToDrop;

		private Resource.Request<Item> _itemRequest;

		public bool looted { get; private set; }

		public event Action onLoot;

		protected override void Awake()
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Expected I4, but got Unknown
			base.Awake();
			_rarity = Singleton<Service>.Instance.levelManager.currentChapter.currentStage.gearPossibilities.Evaluate();
			EvaluateGearRarity();
			Rarity rarity = _rarity;
			switch ((int)rarity)
			{
			case 0:
				_animator.runtimeAnimatorController = _commonChest;
				break;
			case 1:
				_animator.runtimeAnimatorController = _rareChest;
				break;
			case 2:
				_animator.runtimeAnimatorController = _uniqueChest;
				break;
			case 3:
				_animator.runtimeAnimatorController = _legendaryChest;
				break;
			}
			Load();
		}

		private void Load()
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			do
			{
				EvaluateGearRarity();
				_itemToDrop = Singleton<Service>.Instance.gearManager.GetItemToTake(_gearRarity);
			}
			while (_itemToDrop == null);
			_itemRequest = _itemToDrop.LoadAsync();
		}

		private void EvaluateGearRarity()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			_gearRarity = Settings.instance.containerPossibilities[_rarity].Evaluate();
		}

		public override void OnActivate()
		{
			base.OnActivate();
			_animator.Play(InteractiveObject._activateHash);
		}

		public override void OnDeactivate()
		{
			base.OnDeactivate();
			_animator.Play(InteractiveObject._deactivateHash);
		}

		public override void InteractWith(Character character)
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_interactSound, base.transform.position);
			StartCoroutine(_003CInteractWith_003Eg__CDelayedDrop_007C22_0());
			Deactivate();
		}
	}
}
