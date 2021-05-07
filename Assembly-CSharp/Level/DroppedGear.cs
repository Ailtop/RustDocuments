using System;
using System.Linq;
using Characters;
using Characters.Gear;
using Characters.Gear.Items;
using Data;
using Scenes;
using Singletons;
using UnityEngine;

namespace Level
{
	public class DroppedGear : InteractiveObject
	{
		protected Vector2 _popupUIOffset = new Vector2(5f, 2f);

		[SerializeField]
		private DropMovement _dropMovement;

		[SerializeField]
		private DroppedEffect _droppedEffect;

		[SerializeField]
		private SpriteRenderer _spriteRenderer;

		[NonSerialized]
		public GameData.Currency.Type priceCurrency;

		[NonSerialized]
		public int price;

		private Vector3 _initialPosition;

		public override CharacterInteraction.InteractionType interactionType
		{
			get
			{
				if (!(gear == null) && gear.destructible)
				{
					return CharacterInteraction.InteractionType.Pressing;
				}
				return CharacterInteraction.InteractionType.Normal;
			}
		}

		public Gear gear { get; private set; }

		public SpriteRenderer spriteRenderer => _spriteRenderer;

		public DropMovement dropMovement => _dropMovement;

		public event Action<Character> onLoot;

		public event Action<Character> onDestroy;

		protected override void Awake()
		{
			base.Awake();
			if (_dropMovement == null)
			{
				Activate();
			}
			else
			{
				_dropMovement.onGround += OnGround;
			}
			_initialPosition = base.transform.localPosition;
		}

		public void Initialize(Gear gear)
		{
			this.gear = gear;
		}

		private void OnGround()
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Invalid comparison between Unknown and I4
			Activate();
			if (gear != null && (int)gear.rarity == 3 && _droppedEffect != null)
			{
				_droppedEffect.Spawn();
			}
		}

		public override void OpenPopupBy(Character character)
		{
			base.OpenPopupBy(character);
			Vector3 position = base.transform.position;
			Vector3 position2 = character.transform.position;
			position.x = position2.x + ((position.x > position2.x) ? _popupUIOffset.x : (0f - _popupUIOffset.x));
			position.y += _popupUIOffset.y;
			Scene<GameBase>.instance.uiManager.gearPopupCanvas.gearPopup.Set(gear);
			Scene<GameBase>.instance.uiManager.gearPopupCanvas.Open(position);
		}

		public override void ClosePopup()
		{
			base.ClosePopup();
			Scene<GameBase>.instance.uiManager.gearPopupCanvas.Close();
		}

		public override void InteractWith(Character character)
		{
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			//IL_0137: Invalid comparison between Unknown and I4
			if (gear != null && !gear.lootable)
			{
				return;
			}
			GameData.Currency currency = GameData.Currency.currencies[priceCurrency];
			if (!currency.Has(price))
			{
				PersistentSingleton<SoundManager>.Instance.PlaySound(_interactFailSound, base.transform.position);
				return;
			}
			Item fieldItem;
			if ((object)(fieldItem = gear as Item) != null && !character.playerComponents.inventory.item.items.Any((Item i) => i == null))
			{
				Scene<GameBase>.instance.uiManager.itemSelect.Open(fieldItem);
				return;
			}
			if (!currency.Consume(price))
			{
				PersistentSingleton<SoundManager>.Instance.PlaySound(_interactFailSound, base.transform.position);
				return;
			}
			ClosePopup();
			PersistentSingleton<SoundManager>.Instance.PlaySound(_interactSound, base.transform.position);
			price = 0;
			base.transform.localPosition = _initialPosition;
			if (gear != null && (int)gear.rarity == 3 && _droppedEffect != null)
			{
				_droppedEffect.Despawn();
			}
			this.onLoot?.Invoke(character);
		}

		public override void InteractWithByPressing(Character character)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Invalid comparison between Unknown and I4
			ClosePopup();
			if (gear != null && (int)gear.rarity == 3 && _droppedEffect != null)
			{
				_droppedEffect.Despawn();
			}
			this.onDestroy?.Invoke(character);
			UnityEngine.Object.Destroy(gear.gameObject);
		}
	}
}
