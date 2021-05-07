using Characters;
using Characters.Gear.Items;
using Runnables;
using Runnables.Triggers;
using Services;
using Singletons;
using UnityEngine;

namespace Level.Specials
{
	public class TimeCostEventReward : InteractiveObject
	{
		private const float _delayToDrop = 0.5f;

		[SerializeField]
		private Transform _dropPoint;

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

		[SerializeField]
		private RarityPossibilities _rarityPossibilities;

		[SerializeField]
		[Trigger.Subcomponent]
		private Trigger _trigger;

		[SerializeField]
		private Runnable _runnable;

		private Rarity _rarity;

		private Rarity _gearRarity;

		private Resource.ItemInfo _itemToDrop;

		private Resource.Request<Item> _itemRequest;

		public Rarity rarity => _rarity;

		protected override void Awake()
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Expected I4, but got Unknown
			base.Awake();
			_rarity = _rarityPossibilities.Evaluate();
			EvaluateGearRarity();
			Rarity val = _rarity;
			switch ((int)val)
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
			if (_trigger.isSatisfied())
			{
				PersistentSingleton<SoundManager>.Instance.PlaySound(_interactSound, base.transform.position);
				StartCoroutine(_003CInteractWith_003Eg__CDelayedDrop_007C21_0());
				_runnable.Run();
				Deactivate();
			}
		}
	}
}
