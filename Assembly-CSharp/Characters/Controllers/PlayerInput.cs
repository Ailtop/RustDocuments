using Characters.Player;
using InControl;
using UnityEngine;
using UserInput;

namespace Characters.Controllers
{
	public sealed class PlayerInput : MonoBehaviour
	{
		public static readonly TrueOnlyLogicalSumList blocked = new TrueOnlyLogicalSumList();

		public readonly PlayerAction[] _map = new PlayerAction[Button.count];

		[SerializeField]
		[GetComponent]
		private Character _character;

		private WeaponInventory _weaponInventory;

		private QuintessenceInventory _quintessenceInventory;

		private CharacterInteraction _characterInteraction;

		public Vector2 direction;

		public PlayerAction attack
		{
			get
			{
				return _map[Button.Attack.index];
			}
			private set
			{
				_map[Button.Attack.index] = value;
			}
		}

		public PlayerAction dash
		{
			get
			{
				return _map[Button.Dash.index];
			}
			private set
			{
				_map[Button.Dash.index] = value;
			}
		}

		public PlayerAction jump
		{
			get
			{
				return _map[Button.Jump.index];
			}
			private set
			{
				_map[Button.Jump.index] = value;
			}
		}

		public PlayerAction skill
		{
			get
			{
				return _map[Button.Skill.index];
			}
			private set
			{
				_map[Button.Skill.index] = value;
			}
		}

		public PlayerAction skill2
		{
			get
			{
				return _map[Button.Skill2.index];
			}
			private set
			{
				_map[Button.Skill2.index] = value;
			}
		}

		public PlayerAction useItem
		{
			get
			{
				return _map[Button.UseItem.index];
			}
			private set
			{
				_map[Button.UseItem.index] = value;
			}
		}

		public PlayerAction notUsed
		{
			get
			{
				return _map[Button.None.index];
			}
			private set
			{
				_map[Button.None.index] = value;
			}
		}

		public PlayerAction interaction { get; private set; }

		public PlayerAction swap { get; private set; }

		public PlayerAction left { get; private set; }

		public PlayerAction right { get; private set; }

		public PlayerAction this[int index] => _map[index];

		public PlayerAction this[Button button] => _map[button.index];

		private void Awake()
		{
			attack = KeyMapper.Map.Attack;
			dash = KeyMapper.Map.Dash;
			jump = KeyMapper.Map.Jump;
			skill = KeyMapper.Map.Skill1;
			skill2 = KeyMapper.Map.Skill2;
			interaction = KeyMapper.Map.Interaction;
			swap = KeyMapper.Map.Swap;
			useItem = KeyMapper.Map.Quintessence;
			left = KeyMapper.Map.Left;
			right = KeyMapper.Map.Right;
			_weaponInventory = GetComponent<WeaponInventory>();
			_quintessenceInventory = GetComponent<QuintessenceInventory>();
			_characterInteraction = GetComponent<CharacterInteraction>();
		}

		private void Update()
		{
			if (blocked.value)
			{
				return;
			}
			direction = KeyMapper.Map.Move.Vector;
			if (direction.x > 0.33f)
			{
				_character.movement.Move(Vector2.right);
			}
			if (direction.x < -0.33f)
			{
				_character.movement.Move(Vector2.left);
			}
			for (int i = 0; i < _character.actions.Count; i++)
			{
				if (_character.actions[i].Process())
				{
					return;
				}
			}
			if (swap.WasPressed)
			{
				_weaponInventory.NextWeapon();
				return;
			}
			if (useItem.WasPressed)
			{
				_quintessenceInventory.UseAt(0);
				return;
			}
			if (interaction.WasPressed)
			{
				_characterInteraction.InteractionKeyWasPressed();
			}
			if (interaction.WasReleased)
			{
				_characterInteraction.InteractionKeyWasReleased();
			}
		}
	}
}
