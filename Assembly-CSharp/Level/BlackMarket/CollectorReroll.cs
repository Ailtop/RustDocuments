using System;
using Characters;
using Data;
using FX;
using Services;
using Singletons;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Level.BlackMarket
{
	public class CollectorReroll : InteractiveObject
	{
		private static readonly int _idleHash = Animator.StringToHash("Idle");

		private static readonly int _interactHash = Animator.StringToHash("Interact");

		private int[] _costs;

		private int _refreshCount;

		private const string _goldColor = "#FFDE37";

		private const string _notEnoughGoldColor = "#FF0000";

		[SerializeField]
		private SoundInfo _interactionFailedSound;

		[SerializeField]
		private Animator _animator;

		[SerializeField]
		private TMP_Text _text;

		[SerializeField]
		private UnityEvent _onReroll;

		private int cost => _costs[Math.Min(_refreshCount, _costs.Length - 1)];

		public event Action onInteracted;

		private void OnEnable()
		{
			_costs = Singleton<Service>.Instance.levelManager.currentChapter.collectorRefreshCosts;
			UpdateCostText();
			_animator.Play(_idleHash);
		}

		public override void InteractWith(Character character)
		{
			if (!GameData.Currency.gold.Consume(cost))
			{
				PersistentSingleton<SoundManager>.Instance.PlaySound(_interactionFailedSound, base.transform.position);
				return;
			}
			_animator.Play(_interactHash, 0, 0f);
			_refreshCount++;
			UpdateCostText();
			PersistentSingleton<SoundManager>.Instance.PlaySound(_interactSound, base.transform.position);
			this.onInteracted?.Invoke();
			_onReroll?.Invoke();
		}

		private void UpdateCostText()
		{
			_text.text = cost.ToString();
		}

		private void Update()
		{
			string arg = (GameData.Currency.gold.Has(cost) ? "#FFDE37" : "#FF0000");
			_text.text = string.Format("{0}(<color={1}>{2}</color>)", Lingua.GetLocalizedString("label/interaction/refresh"), arg, cost);
		}
	}
}
