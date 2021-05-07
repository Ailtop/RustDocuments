using System;
using Characters;
using Characters.Gear.Weapons;
using Services;
using Singletons;
using UnityEngine;

namespace Level
{
	public class Grave : InteractiveObject, ILootable
	{
		private const float _delayToDrop = 0.4f;

		[SerializeField]
		[GetComponent]
		private Animator _animator;

		[SerializeField]
		private RuntimeAnimatorController _common;

		[SerializeField]
		private RuntimeAnimatorController _rare;

		[SerializeField]
		private RuntimeAnimatorController _unique;

		[SerializeField]
		private RuntimeAnimatorController _legendary;

		[SerializeField]
		private RewardEffect _effect;

		private Rarity _rarity;

		private Rarity _gearRarity;

		private Resource.WeaponReference _weaponToDrop;

		private Resource.Request<Weapon> _weaponRequest;

		public bool looted { get; private set; }

		public event Action onLoot;

		protected override void Awake()
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Expected I4, but got Unknown
			base.Awake();
			_rarity = Singleton<Service>.Instance.levelManager.currentChapter.currentStage.gearPossibilities.Evaluate();
			Rarity rarity = _rarity;
			switch ((int)rarity)
			{
			case 0:
				_animator.runtimeAnimatorController = _common;
				break;
			case 1:
				_animator.runtimeAnimatorController = _rare;
				break;
			case 2:
				_animator.runtimeAnimatorController = _unique;
				break;
			case 3:
				_animator.runtimeAnimatorController = _legendary;
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
				_weaponToDrop = Singleton<Service>.Instance.gearManager.GetWeaponToTake(_gearRarity);
			}
			while (_weaponToDrop == null);
			_weaponRequest = _weaponToDrop.LoadAsync();
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
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			base.OnActivate();
			_animator.Play(InteractiveObject._activateHash);
			_effect.Play(_rarity);
		}

		public override void OnDeactivate()
		{
			base.OnDeactivate();
			_animator.Play(InteractiveObject._deactivateHash);
		}

		public override void InteractWith(Character character)
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_interactSound, base.transform.position);
			StartCoroutine(_003CInteractWith_003Eg__CDrop_007C23_0());
			Deactivate();
		}
	}
}
