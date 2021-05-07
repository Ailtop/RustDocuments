using System;
using Characters;
using Characters.Gear;
using Services;
using Singletons;
using UnityEngine;

namespace Level
{
	public class BossChest : InteractiveObject
	{
		[Serializable]
		private class BossGears : ReorderableArray<BossGears.Property>
		{
			[Serializable]
			internal class Property
			{
				[SerializeField]
				private float _weight;

				[SerializeField]
				private Gear _gear;

				public float weight => _weight;

				public Gear gear => _gear;
			}
		}

		private const float _delayToDrop = 0.5f;

		[Header("Boss Gears")]
		[SerializeField]
		[Range(0f, 100f)]
		private int _bossItemDropChance = 10;

		[SerializeField]
		private BossGears _bossGears;

		[SerializeField]
		[GetComponent]
		private Animator _animator;

		[Header("Gold")]
		[SerializeField]
		private int _goldAmount;

		[SerializeField]
		private int _goldCount;

		[Header("Potion")]
		[SerializeField]
		private PotionPossibilities _potionPossibilities;

		private Rarity _rarity;

		private Rarity _gearRarity;

		private const float _droppedGearHorizontalInterval = 5f;

		private const float _droppedGearHorizontalSpeed = 2f;

		public event Action OnOpen;

		private void EvaluateGearRarity()
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			_rarity = Singleton<Service>.Instance.levelManager.currentChapter.currentStage.gearPossibilities.Evaluate();
			_gearRarity = Settings.instance.containerPossibilities[_rarity].Evaluate();
		}

		public override void InteractWith(Character character)
		{
			PersistentSingleton<SoundManager>.Instance.PlaySound(_interactSound, base.transform.position);
			StartCoroutine(_003CInteractWith_003Eg__CDelayedDrop_007C16_0());
			Deactivate();
			this.OnOpen?.Invoke();
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
	}
}
