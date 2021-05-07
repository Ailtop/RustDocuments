using System;
using System.Collections;
using System.Collections.Generic;
using Characters.Actions;
using Characters.Gear.Weapons;
using Characters.Marks;
using Characters.Movements;
using Characters.Player;
using FX;
using Level;
using Services;
using Singletons;
using UnityEngine;
using UnityEngine.Rendering;

namespace Characters
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(CharacterAnimationController))]
	public class Character : MonoBehaviour
	{
		public enum Type
		{
			TrashMob,
			Named,
			Adventurer,
			Boss,
			Summoned,
			Trap,
			Player,
			Dummy
		}

		public enum LookingDirection
		{
			Right,
			Left
		}

		public enum SizeForEffect
		{
			Small,
			Medium,
			Large,
			ExtraLarge,
			None
		}

		public delegate void OnGaveStatusDelegate(Character target, CharacterStatus.ApplyInfo applyInfo, bool result);

		public delegate void OnKilledDelegate(ITarget target, ref Damage damage);

		private const float sizeLerpSpeed = 10f;

		public readonly GiveDamageEvent onGiveDamage = new GiveDamageEvent();

		public GaveDamageDelegate onGaveDamage;

		public OnGaveStatusDelegate onGaveStatus;

		public OnKilledDelegate onKilled;

		public Action<Characters.Actions.Action> onStartCharging;

		public Action<Characters.Actions.Action> onStopCharging;

		public Action<Characters.Actions.Action> onCancelCharging;

		public readonly TrueOnlyLogicalSumList invulnerable = new TrueOnlyLogicalSumList();

		public readonly TrueOnlyLogicalSumList blockLook = new TrueOnlyLogicalSumList();

		public readonly Stat stat;

		public readonly CharacterChronometer chronometer = new CharacterChronometer();

		[SerializeField]
		private Key _key;

		[SerializeField]
		[GetComponent]
		protected CharacterHealth _health;

		[SerializeField]
		[GetComponent]
		protected CharacterHit _hit;

		[SerializeField]
		protected BoxCollider2D _collider;

		[SerializeField]
		[GetComponent]
		protected Movement _movement;

		[SerializeField]
		[GetComponent]
		private CharacterAnimationController _animationController;

		[SerializeField]
		[GetComponent]
		private CharacterStatus _status;

		[SerializeField]
		private Type _type;

		[SerializeField]
		private SizeForEffect _sizeForEffect;

		[SerializeField]
		private SortingGroup _sortingGroup;

		[SerializeField]
		protected Stat.Values _baseStat = new Stat.Values(new Stat.Value(Stat.Category.Constant, Stat.Kind.Health, 0.0), new Stat.Value(Stat.Category.Constant, Stat.Kind.MovementSpeed, 0.0));

		[SerializeField]
		protected Transform _base;

		private LookingDirection _lookingDirection;

		[SerializeField]
		protected Weapon _weapon;

		[SerializeField]
		private GameObject _attach;

		private CoroutineReference _cWaitForEndOfAction;

		public Key key => _key;

		public CharacterHealth health => _health;

		public CharacterHit hit => _hit;

		public BoxCollider2D collider => _collider;

		public Movement movement => _movement;

		public CharacterAnimationController animationController => _animationController;

		public CharacterStatus status => _status;

		public bool stunedOrFreezed
		{
			get
			{
				if (_status != null)
				{
					if (!_status.stuned)
					{
						return _status.freezed;
					}
					return true;
				}
				return false;
			}
		}

		public Type type => _type;

		public SizeForEffect sizeForEffect => _sizeForEffect;

		public SortingGroup sortingGroup => _sortingGroup;

		public Transform @base => _base;

		public LookingDirection lookingDirection
		{
			get
			{
				return _lookingDirection;
			}
			set
			{
				desiringLookingDirection = value;
				if (!blockLook.value)
				{
					_lookingDirection = value;
					if (_lookingDirection == LookingDirection.Right)
					{
						_animationController.parameter.flipX = false;
						attachWithFlip.transform.localScale = Vector3.one;
					}
					else
					{
						_animationController.parameter.flipX = true;
						attachWithFlip.transform.localScale = new Vector3(-1f, 1f, 1f);
					}
				}
			}
		}

		public PlayerComponents playerComponents { get; private set; }

		public LookingDirection desiringLookingDirection { get; private set; }

		public List<Characters.Actions.Action> actions { get; private set; } = new List<Characters.Actions.Action>();


		public ISpriteEffectStack spriteEffectStack { get; private set; }

		public Mark mark { get; private set; }

		public CharacterAbilityManager ability { get; private set; }

		public Characters.Actions.Motion motion { get; private set; }

		public Characters.Actions.Motion runningMotion
		{
			get
			{
				if (motion == null || !motion.running)
				{
					return null;
				}
				return motion;
			}
		}

		public GameObject attach
		{
			get
			{
				return _attach;
			}
			set
			{
				_attach = value;
			}
		}

		public GameObject attachWithFlip { get; private set; }

		public bool liveAndActive
		{
			get
			{
				if (health != null)
				{
					if (!health.dead)
					{
						return base.gameObject.activeSelf;
					}
					return false;
				}
				return base.gameObject.activeSelf;
			}
		}

		public event System.Action onDie;

		public event EvadeDamageDelegate onEvade;

		public event Action<Characters.Actions.Action> onStartAction;

		public event Action<Characters.Actions.Action> onEndAction;

		public event Action<Characters.Actions.Action> onCancelAction;

		private Character()
		{
			stat = new Stat(this);
		}

		protected virtual void Awake()
		{
			if (hit == null)
			{
				invulnerable.Attach(this);
			}
			if (_attach == null)
			{
				_attach = new GameObject("_attach");
				_attach.transform.SetParent(base.transform, false);
			}
			if (attachWithFlip == null)
			{
				attachWithFlip = new GameObject("attachWithFlip");
				attachWithFlip.transform.SetParent(attach.transform, false);
			}
			spriteEffectStack = GetComponent<ISpriteEffectStack>();
			if (health != null)
			{
				mark = Mark.AddComponent(this);
			}
			ability = CharacterAbilityManager.AddComponent(this);
			stat.AttachValues(_baseStat);
			stat.Update();
			InitializeActions();
			_animationController.Initialize();
			_animationController.onExpire += OnAnimationExpire;
			if (_health != null)
			{
				_health.owner = this;
				_health.SetMaximumHealth(stat.Get(Stat.Category.Final, Stat.Kind.Health));
				_health.ResetToMaximumHealth();
				_health.onDie += OnDie;
				_health.onTakeDamage.Add(0, delegate(ref Damage damage)
				{
					return stat.ApplyDefense(ref damage);
				});
				_health.onTakeDamage.Add(int.MaxValue, _003CAwake_003Eg__CancelDamage_007C115_1);
			}
			if (type == Type.Player)
			{
				playerComponents = new PlayerComponents(this);
				playerComponents.Initialize();
			}
		}

		private void OnDestroy()
		{
			if (type == Type.Player)
			{
				playerComponents.Dispose();
			}
		}

		protected virtual void Update()
		{
			float deltaTime = chronometer.master.deltaTime;
			stat.TakeTime(deltaTime);
			playerComponents?.Update(deltaTime);
			if (_health == null)
			{
				stat.UpdateIfNecessary();
			}
			else if (stat.UpdateIfNecessary())
			{
				double num = stat.Get(Stat.Category.Final, Stat.Kind.Health);
				double current = _health.percent * num;
				_health.SetHealth(current, num);
			}
			double final = stat.GetFinal(Stat.Kind.CharacterSize);
			Vector3 localScale = base.transform.localScale;
			base.transform.localScale = Vector3.one * Mathf.Lerp(localScale.x, (float)final, Time.deltaTime * 10f);
		}

		protected void OnDie()
		{
			this.onDie?.Invoke();
		}

		public void Attack(ITarget target, ref Damage damage)
		{
			if (target.character != null)
			{
				AttackCharacter(target, ref damage);
			}
			else if (target.damageable != null)
			{
				AttackDamageable(target, ref damage);
			}
		}

		public void Attack(Character character, ref Damage damage)
		{
			AttackCharacter(new TargetStruct(character), ref damage);
		}

		public void Attack(DestructibleObject damageable, ref Damage damage)
		{
			AttackDamageable(new TargetStruct(damageable), ref damage);
		}

		public void AttackCharacter(ITarget target, ref Damage damage)
		{
			Character character = target.character;
			if (character.health.dead)
			{
				return;
			}
			Damage originalDamage = damage;
			if (onGiveDamage.Invoke(target, ref damage))
			{
				return;
			}
			character.hit.Stop(damage.stoppingPower);
			double dealtDamage;
			if (!character.health.TakeDamage(ref damage, out dealtDamage))
			{
				if (character.type == Type.Player)
				{
					Singleton<Service>.Instance.floatingTextSpawner.SpawnPlayerTakingDamage(ref damage);
				}
				else
				{
					Singleton<Service>.Instance.floatingTextSpawner.SpawnTakingDamage(ref damage);
				}
				onGaveDamage?.Invoke(target, ref originalDamage, ref damage, dealtDamage);
				if (target.character.health.dead)
				{
					onKilled?.Invoke(target, ref damage);
				}
			}
		}

		public void AttackDamageable(ITarget target, ref Damage damage)
		{
			DestructibleObject damageable = target.damageable;
			Damage originalDamage = damage;
			GiveDamageEvent giveDamageEvent = onGiveDamage;
			if (giveDamageEvent == null || giveDamageEvent.Invoke(target, ref damage))
			{
				damageable.Hit(this, ref damage);
				if (damage.amount > 0.0)
				{
					Singleton<Service>.Instance.floatingTextSpawner.SpawnTakingDamage(ref damage);
				}
				onGaveDamage?.Invoke(target, ref originalDamage, ref damage, 0.0);
				if (target.damageable.destroyed)
				{
					onKilled?.Invoke(target, ref damage);
				}
			}
		}

		public bool GiveStatus(Character target, CharacterStatus.ApplyInfo status)
		{
			if (target.status == null)
			{
				onGaveStatus?.Invoke(target, status, false);
				return false;
			}
			bool result = target.status.Apply(this, status);
			onGaveStatus?.Invoke(target, status, result);
			return result;
		}

		private IEnumerator CWaitForEndOfAction(Characters.Actions.Action action)
		{
			yield return motion.action.CWaitForEndOfRunning();
			this.onEndAction?.Invoke(action);
		}

		public void DoAction(Characters.Actions.Motion motion, float speedMultiplier)
		{
			_003C_003Ec__DisplayClass126_0 _003C_003Ec__DisplayClass126_ = new _003C_003Ec__DisplayClass126_0();
			_003C_003Ec__DisplayClass126_.motion = motion;
			_003C_003Ec__DisplayClass126_._003C_003E4__this = this;
			Characters.Actions.Motion motion2 = this.motion;
			if (motion2 != null && motion2.running)
			{
				CancelAction();
			}
			DoMotion(_003C_003Ec__DisplayClass126_.motion, speedMultiplier);
			if (_003C_003Ec__DisplayClass126_.motion.action != null)
			{
				_cWaitForEndOfAction.Stop();
				if (base.isActiveAndEnabled)
				{
					_cWaitForEndOfAction = this.StartCoroutineWithReference(CWaitForEndOfAction(_003C_003Ec__DisplayClass126_.motion.action));
				}
				else
				{
					Debug.LogWarning("Coroutine couldn't be started because the character is not active or disabled, so use onEnd event");
					_003C_003Ec__DisplayClass126_.motion.action.onEnd += _003C_003Ec__DisplayClass126_._003CDoAction_003Eg__onActionEnd_007C0;
				}
				this.onStartAction?.Invoke(_003C_003Ec__DisplayClass126_.motion.action);
			}
		}

		public void DoActionNonBlock(Characters.Actions.Motion motion)
		{
			this.onStartAction?.Invoke(motion.action);
			motion.StartBehaviour(1f);
			motion.EndBehaviour();
			this.onEndAction?.Invoke(motion.action);
		}

		public void DoMotion(Characters.Actions.Motion motion, float speedMultiplier = 1f)
		{
			Characters.Actions.Motion motion2 = this.motion;
			if (motion2 != null && motion2.running)
			{
				if (motion2.action != motion.action)
				{
					CancelAction();
				}
				else
				{
					_animationController.StopAll();
					movement?.blocked.Detach(this);
					blockLook.Detach(this);
					motion2.CancelBehaviour();
				}
			}
			this.motion = motion;
			float num = motion.speed * speedMultiplier;
			if (motion.stay)
			{
				_animationController.Play(motion.animationInfo, num);
			}
			else
			{
				_animationController.Play(motion.animationInfo, motion.length / num, num);
			}
			blockLook.Detach(this);
			lookingDirection = desiringLookingDirection;
			motion.StartBehaviour(num);
			if (movement != null)
			{
				movement.blocked.Detach(this);
				if (motion.blockMovement && motion.length > 0f)
				{
					movement.blocked.Attach(this);
				}
			}
			if (motion.blockLook)
			{
				blockLook.Attach(this);
			}
		}

		public void InitializeActions()
		{
			GetComponentsInChildren(true, actions);
			actions.Sort((Characters.Actions.Action a, Characters.Actions.Action b) => a.priority.CompareTo(b.priority));
			foreach (Characters.Actions.Action action in actions)
			{
				action.Initialize(this);
			}
		}

		public void CancelAction()
		{
			if (!(motion == null))
			{
				_animationController.StopAll();
				_cWaitForEndOfAction.Stop();
				if (this.onCancelAction != null && motion.action != null)
				{
					this.onCancelAction(motion.action);
				}
				movement?.blocked.Detach(this);
				blockLook.Detach(this);
				motion.CancelBehaviour();
			}
		}

		public LookingDirection DesireToLookAt(float targetX)
		{
			if (!(base.transform.position.x > targetX))
			{
				return desiringLookingDirection = LookingDirection.Right;
			}
			return desiringLookingDirection = LookingDirection.Left;
		}

		public LookingDirection ForceToLookAt(float targetX)
		{
			if (!(base.transform.position.x > targetX))
			{
				return this.lookingDirection = LookingDirection.Right;
			}
			return this.lookingDirection = LookingDirection.Left;
		}

		public void ForceToLookAt(LookingDirection lookingDirection)
		{
			desiringLookingDirection = lookingDirection;
			_lookingDirection = lookingDirection;
			_animationController.parameter.flipX = _lookingDirection != LookingDirection.Right;
		}

		private void OnAnimationExpire()
		{
			movement?.blocked.Detach(this);
			blockLook.Detach(this);
			motion?.EndBehaviour();
		}
	}
}
