using Characters;
using Characters.Operations.Fx;
using Data;
using FX.SpriteEffects;
using Singletons;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Level
{
	public class Potion : DroppedGear
	{
		public enum Size
		{
			Small,
			Medium,
			Large
		}

		[SerializeField]
		[FormerlySerializedAs("_healthHealingPercent")]
		private int _healAmount;

		[SerializeField]
		private int priority;

		[SerializeField]
		private Color _startColor;

		[SerializeField]
		private Color _endColor;

		[SerializeField]
		private Curve _curve;

		[SerializeField]
		[Subcomponent(typeof(SpawnEffect))]
		private SpawnEffect _spawn;

		private const string _prefix = "Potion";

		protected string _keyBase => "Potion/" + base.name;

		public string displayName => Lingua.GetLocalizedString(_keyBase + "/name");

		public string description => Lingua.GetLocalizedString(_keyBase + "/desc");

		public override void InteractWith(Character character)
		{
			if (GameData.Currency.gold.Has(price) && character.health.percent != 1.0)
			{
				PersistentSingleton<SoundManager>.Instance.PlaySound(_interactSound, base.transform.position);
				_spawn.Run(character);
				character.spriteEffectStack.Add(new EasedColorBlend(priority, _startColor, _endColor, _curve));
				character.health.Heal(_healAmount);
				Object.Destroy(base.gameObject);
			}
		}

		public override void OpenPopupBy(Character character)
		{
		}

		public override void ClosePopup()
		{
		}

		public void Initialize()
		{
		}
	}
}
