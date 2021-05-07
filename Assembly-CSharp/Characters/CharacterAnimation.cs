using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Characters
{
	[RequireComponent(typeof(Animator))]
	public class CharacterAnimation : MonoBehaviour
	{
		public class Parameter
		{
			private readonly Animator animator;

			public readonly AnimatorBool walk;

			public readonly AnimatorBool grounded;

			public readonly AnimatorFloat movementSpeed;

			public readonly AnimatorFloat ySpeed;

			public readonly AnimatorFloat actionSpeed;

			internal Parameter(Animator animator)
			{
				this.animator = animator;
				walk = new AnimatorBool(animator, "Walk");
				movementSpeed = new AnimatorFloat(animator, "MovementSpeed");
				ySpeed = new AnimatorFloat(animator, "YSpeed");
				actionSpeed = new AnimatorFloat(animator, "ActionSpeed");
				grounded = new AnimatorBool(animator, "Grounded");
			}
		}

		public const string idleClipName = "EmptyIdle";

		public const string walkClipName = "EmptyWalk";

		public const string jumpUpClipName = "EmptyJumpUp";

		public const string fallClipName = "EmptyJumpDown";

		public const string fallRepeatClipName = "EmptyJumpDownLoop";

		public const string actionClipName = "EmptyAction";

		public static readonly AnimatorParameter action = new AnimatorParameter("Action");

		public static readonly AnimatorParameter idle = new AnimatorParameter("Idle");

		public static readonly AnimatorParameter ground = new AnimatorParameter("Ground");

		public static readonly AnimatorParameter air = new AnimatorParameter("Air");

		[SerializeField]
		[GetComponent]
		protected Animator _animator;

		protected AnimationClipOverrider _defaultOverrider;

		[SerializeField]
		[GetComponent]
		protected SpriteRenderer _spriteRenderer;

		[SerializeField]
		[CharacterAnimationController.Key]
		private string _key;

		[SerializeField]
		private AnimationClip _idleClip;

		[SerializeField]
		private AnimationClip _walkClip;

		[SerializeField]
		private AnimationClip _jumpClip;

		[SerializeField]
		private AnimationClip _fallClip;

		[SerializeField]
		private AnimationClip _fallRepeatClip;

		private AnimationClip _actionClip;

		private float _cycleOffset;

		private readonly List<AnimationClipOverrider> _overriders = new List<AnimationClipOverrider>();

		public Parameter parameter { get; protected set; }

		public float speed
		{
			get
			{
				return _animator.speed;
			}
			set
			{
				_animator.speed = value;
			}
		}

		public string key => _key;

		public SpriteRenderer spriteRenderer => _spriteRenderer;

		public AnimatorParameter state
		{
			get
			{
				int tagHash = _animator.GetCurrentAnimatorStateInfo(0).tagHash;
				if (tagHash == action.hash)
				{
					return action;
				}
				if (tagHash == ground.hash)
				{
					return ground;
				}
				if (tagHash == air.hash)
				{
					return air;
				}
				return null;
			}
		}

		public void Initialize()
		{
			if (_defaultOverrider == null)
			{
				_defaultOverrider = new AnimationClipOverrider(_animator.runtimeAnimatorController);
				AttachOverrider(_defaultOverrider);
			}
			_defaultOverrider.Override("EmptyIdle", _idleClip);
			_defaultOverrider.Override("EmptyWalk", _walkClip);
			_defaultOverrider.Override("EmptyJumpUp", _jumpClip);
			_defaultOverrider.Override("EmptyJumpDown", _fallClip);
			_defaultOverrider.Override("EmptyJumpDownLoop", _fallRepeatClip);
			parameter = new Parameter(_animator);
		}

		public void Play(AnimationClip clip, float speed)
		{
			_actionClip = clip;
			_overriders.Last().Override("EmptyAction", clip);
			AnimationEvent animationEvent = clip.events.SingleOrDefault((AnimationEvent @event) => @event.functionName.Equals("CycleOffset"));
			AnimationEvent animationEvent2 = clip.events.SingleOrDefault((AnimationEvent @event) => @event.functionName.Equals("Repeat"));
			if (animationEvent == null)
			{
				_cycleOffset = 0f;
			}
			else
			{
				if (animationEvent2 == null)
				{
					AnimationEvent animationEvent3 = new AnimationEvent();
					animationEvent3.functionName = "Repeat";
					animationEvent3.time = clip.length;
					clip.AddEvent(animationEvent3);
				}
				_cycleOffset = animationEvent.time / clip.length;
			}
			Play(speed);
		}

		public void Play(float speed)
		{
			parameter.actionSpeed.Value = speed;
			Play();
		}

		public void Play()
		{
			_animator.Play(action.hash, 0, 0f);
		}

		public void Stun()
		{
			Play(_idleClip, 1.5f);
		}

		public void Stop()
		{
			if (_animator.isActiveAndEnabled && _animator.GetCurrentAnimatorStateInfo(0).shortNameHash == action.hash)
			{
				_animator.Play(idle.hash, 0, 0f);
			}
		}

		public void Repeat()
		{
			_animator.Play(action.hash, 0, _cycleOffset);
		}

		public void CycleOffset()
		{
		}

		public void SetIdle(AnimationClip clip)
		{
			if (_defaultOverrider == null)
			{
				_defaultOverrider = new AnimationClipOverrider(_animator.runtimeAnimatorController);
				AttachOverrider(_defaultOverrider);
			}
			_defaultOverrider.Override("EmptyIdle", clip);
		}

		public void SetWalk(AnimationClip clip)
		{
			if (_defaultOverrider == null)
			{
				_defaultOverrider = new AnimationClipOverrider(_animator.runtimeAnimatorController);
				AttachOverrider(_defaultOverrider);
			}
			_defaultOverrider.Override("EmptyWalk", clip);
		}

		public void AttachOverrider(AnimationClipOverrider overrider)
		{
			if (!_overriders.Contains(overrider))
			{
				_overriders.Add(overrider);
				_animator.runtimeAnimatorController = _overriders.Last().animatorController;
			}
		}

		public void DetachOverrider(AnimationClipOverrider overrider)
		{
			_overriders.Remove(overrider);
			_animator.runtimeAnimatorController = _overriders.Last().animatorController;
		}
	}
}
