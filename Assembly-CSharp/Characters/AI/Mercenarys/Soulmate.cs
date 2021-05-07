using System.Collections;
using Characters.Actions;
using Characters.Operations;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.AI.Mercenarys
{
	public class Soulmate : MonoBehaviour
	{
		[SerializeField]
		private Character _character;

		[Header("Movement")]
		[SerializeField]
		private float _timeToChase = 2f;

		[SerializeField]
		private float _minimumDistance = 1.5f;

		[SerializeField]
		private Action _teleport;

		[SerializeField]
		private Transform _teleportDestination;

		[Header("Buff")]
		[SerializeField]
		private float _buffInterval = 30f;

		[SerializeField]
		private AttachAbility[] _buffs;

		private Character _owner;

		private float _timeAwayfromOwner;

		private bool _hidden;

		private float _buffElapsed;

		private void Start()
		{
			if (WitchBonus.instance.soul.fatalMind.level != 0)
			{
				Object.DontDestroyOnLoad(_teleportDestination);
				_teleportDestination.SetParent(null);
			}
		}

		private void OnEnable()
		{
			StartCoroutine(CProcess());
		}

		private IEnumerator CBuff()
		{
			while (true)
			{
				if (_hidden)
				{
					yield return null;
					continue;
				}
				_buffElapsed += _owner.chronometer.master.deltaTime;
				if (_buffElapsed >= _buffInterval)
				{
					_buffs.Random().Run(_owner);
					_buffElapsed = 0f;
				}
				yield return null;
			}
		}

		private IEnumerator CSetOwner()
		{
			while (_owner == null)
			{
				yield return null;
				_owner = Singleton<Service>.Instance.levelManager.player;
			}
		}

		private IEnumerator CProcess()
		{
			yield return CSetOwner();
			StartCoroutine(CMove());
			StartCoroutine(CBuff());
		}

		private IEnumerator CMove()
		{
			while (true)
			{
				if (_hidden)
				{
					yield return null;
					continue;
				}
				Collider2D lastStandingCollider = _owner.movement.controller.collisionState.lastStandingCollider;
				if (lastStandingCollider == null)
				{
					yield return null;
					continue;
				}
				Collider2D lastStandingCollider2 = _character.movement.controller.collisionState.lastStandingCollider;
				if (lastStandingCollider2 == null)
				{
					yield return null;
					continue;
				}
				if (lastStandingCollider != lastStandingCollider2)
				{
					_timeAwayfromOwner += _owner.chronometer.master.deltaTime;
					if (_timeAwayfromOwner > _timeToChase)
					{
						yield return CTeleport();
						_timeAwayfromOwner = 0f;
					}
				}
				else
				{
					_timeAwayfromOwner = 0f;
					float f = _owner.transform.position.x - _character.transform.position.x;
					if (Mathf.Abs(f) > _minimumDistance)
					{
						_character.movement.Move(new Vector2(Mathf.Sign(f), 0f));
					}
				}
				yield return null;
			}
		}

		private IEnumerator CTeleport()
		{
			_teleportDestination.position = _owner.transform.position;
			_teleport.TryStart();
			while (_teleport.running)
			{
				yield return null;
			}
		}

		public void Hide()
		{
			if (WitchBonus.instance.soul.fatalMind.level != 0)
			{
				_hidden = true;
				_character.gameObject.SetActive(false);
				_buffElapsed = 0f;
				_timeAwayfromOwner = 0f;
			}
		}

		public IEnumerator CAppearance()
		{
			yield return CSetOwner();
			Collider2D lastStandingCollider;
			while (true)
			{
				lastStandingCollider = _owner.movement.controller.collisionState.lastStandingCollider;
				if (!(lastStandingCollider == null))
				{
					break;
				}
				yield return null;
			}
			_teleportDestination.position = new Vector2(_owner.transform.position.x - 1f, lastStandingCollider.bounds.max.y);
			_character.gameObject.SetActive(true);
			_teleport.TryStart();
			while (_teleport.running)
			{
				yield return null;
			}
			_hidden = false;
		}
	}
}
