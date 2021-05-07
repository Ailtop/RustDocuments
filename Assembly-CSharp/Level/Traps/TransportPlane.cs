using System.Collections;
using System.Collections.Generic;
using Characters;
using Characters.Actions;
using PhysicsUtils;
using UnityEngine;

namespace Level.Traps
{
	[ExecuteAlways]
	public class TransportPlane : ControlableTrap
	{
		[SerializeField]
		[GetComponent]
		private SpriteRenderer _spriteRenderer;

		[SerializeField]
		private BoxCollider2D _collider;

		[SerializeField]
		private Character _character;

		[SerializeField]
		private Action _action;

		[SerializeField]
		private int _size = 2;

		[SerializeField]
		private float _speed;

		[SerializeField]
		private float _castDistance;

		[SerializeField]
		private LayerMask _layer;

		private List<Character> targets = new List<Character>();

		private IEnumerator _coroutine;

		private static readonly NonAllocCaster _caster;

		static TransportPlane()
		{
			_caster = new NonAllocCaster(15);
		}

		private void SetSize()
		{
			Vector2 size = _spriteRenderer.size;
			size.x = _size + 1;
			_spriteRenderer.size = size;
			Vector2 size2 = _collider.size;
			size2.x = (float)(_size + 1) - 1.5f;
			_collider.size = size2;
		}

		private void SetSpeed()
		{
		}

		private void Awake()
		{
			_coroutine = CRun();
			SetSize();
			SetSpeed();
		}

		private void Update()
		{
		}

		public override void Activate()
		{
			_action.TryStart();
			StartCoroutine(_coroutine);
		}

		public override void Deactivate()
		{
			if (_action.running)
			{
				_character.CancelAction();
			}
			StopCoroutine(_coroutine);
		}

		private IEnumerator CRun()
		{
			while (true)
			{
				foreach (Character target in targets)
				{
					if (target == null || !target.liveAndActive)
					{
						targets.Remove(target);
					}
					else
					{
						target.movement.force.x += (float)((_character.lookingDirection == Character.LookingDirection.Right) ? 1 : (-1)) * _speed * _character.chronometer.master.deltaTime;
					}
				}
				yield return null;
			}
		}

		private void OnTriggerEnter2D(Collider2D other)
		{
			if (_layer.Contains(other.gameObject.layer))
			{
				AddTarget(other.gameObject);
			}
		}

		private void OnTriggerExit2D(Collider2D other)
		{
			if (_layer.Contains(other.gameObject.layer))
			{
				RemoveTarget(other.gameObject);
			}
		}

		private void AddTarget(GameObject target)
		{
			Character character;
			if (target.TryFindCharacterComponent(out character))
			{
				targets.Add(character);
			}
		}

		private void RemoveTarget(GameObject target)
		{
			Character character;
			if (target.TryFindCharacterComponent(out character) && targets.Contains(character))
			{
				targets.Remove(character);
			}
		}
	}
}
