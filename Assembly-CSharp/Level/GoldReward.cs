using System;
using Characters;
using Services;
using Singletons;
using UnityEngine;

namespace Level
{
	public class GoldReward : InteractiveObject, ILootable
	{
		[SerializeField]
		[GetComponent]
		private Animator _animator;

		public bool looted { get; private set; }

		public event Action onLoot;

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
			this.onLoot?.Invoke();
			looted = true;
			PersistentSingleton<SoundManager>.Instance.PlaySound(_interactSound, base.transform.position);
			Vector2Int goldrewardAmount = Singleton<Service>.Instance.levelManager.currentChapter.currentStage.goldrewardAmount;
			int amount = UnityEngine.Random.Range(goldrewardAmount.x, goldrewardAmount.y);
			Singleton<Service>.Instance.levelManager.DropGold(amount, 40, base.transform.position);
			Deactivate();
		}
	}
}
