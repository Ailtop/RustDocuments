using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Characters
{
	public class CharacterAnimationController : MonoBehaviour
	{
		public class KeyAttribute : PopupAttribute
		{
			public KeyAttribute()
				: base(true, "CharacterBody", "Polymorph")
			{
			}
		}

		[Serializable]
		public class AnimationInfo : ReorderableArray<AnimationInfo.KeyClip>
		{
			[Serializable]
			public class KeyClip
			{
				[SerializeField]
				[Key]
				private string _key = "CharacterBody";

				[SerializeField]
				private AnimationClip _clip;

				public string key => _key;

				public AnimationClip clip => _clip;

				public KeyClip(string key, AnimationClip clip)
				{
					_key = key;
					_clip = clip;
				}
			}

			private Dictionary<string, AnimationClip> _dictionary;

			public Dictionary<string, AnimationClip> dictionary => _dictionary ?? (_dictionary = values.ToDictionary((KeyClip v) => v.key, (KeyClip v) => v.clip));

			public AnimationClip defaultClip
			{
				get
				{
					if (values.Length == 0)
					{
						return null;
					}
					return values[0].clip;
				}
			}

			public AnimationInfo(params KeyClip[] keyClips)
			{
				values = keyClips;
			}
		}

		public class Parameter
		{
			public bool walk;

			public bool grounded;

			public float movementSpeed;

			public float ySpeed;

			public bool flipX;

			public void CopyFrom(Parameter parameter)
			{
				walk = parameter.walk;
				grounded = parameter.grounded;
				movementSpeed = parameter.movementSpeed;
				ySpeed = parameter.ySpeed;
				flipX = parameter.flipX;
			}
		}

		public const string characterBodyKey = "CharacterBody";

		public const string polymorphKey = "Polymorph";

		public readonly Parameter parameter = new Parameter();

		[SerializeField]
		[GetComponent]
		private Character _character;

		public List<CharacterAnimation> animations = new List<CharacterAnimation>();

		private Coroutine _coroutine;

		public event Action onExpire;

		private void Update()
		{
			for (int i = 0; i < animations.Count; i++)
			{
				CharacterAnimation characterAnimation = animations[i];
				characterAnimation.speed = _character.chronometer.animation.timeScale / Time.timeScale;
				characterAnimation.parameter.walk.Value = parameter.walk;
				characterAnimation.parameter.grounded.Value = parameter.grounded;
				characterAnimation.parameter.movementSpeed.Value = parameter.movementSpeed;
				characterAnimation.parameter.ySpeed.Value = parameter.ySpeed;
				characterAnimation.transform.localScale = (parameter.flipX ? new Vector3(-1f, 1f, 1f) : Vector3.one);
			}
		}

		public void Initialize()
		{
			GetComponentsInChildren(true, animations);
			animations.ForEach(delegate(CharacterAnimation animation)
			{
				animation.Initialize();
			});
		}

		public void ForceUpdate()
		{
			for (int i = 0; i < animations.Count; i++)
			{
				CharacterAnimation characterAnimation = animations[i];
				characterAnimation.speed = _character.chronometer.animation.timeScale / Time.timeScale;
				characterAnimation.parameter.walk.ForceSet(parameter.walk);
				characterAnimation.parameter.grounded.ForceSet(parameter.grounded);
				characterAnimation.parameter.movementSpeed.ForceSet(parameter.movementSpeed);
				characterAnimation.parameter.ySpeed.ForceSet(parameter.ySpeed);
				characterAnimation.transform.localScale = (parameter.flipX ? new Vector3(-1f, 1f, 1f) : Vector3.one);
			}
		}

		public void UpdateScale()
		{
			for (int i = 0; i < animations.Count; i++)
			{
				animations[i].transform.localScale = (parameter.flipX ? new Vector3(-1f, 1f, 1f) : Vector3.one);
			}
		}

		public void Play(AnimationInfo animationInfo, float speed)
		{
			if (_coroutine != null)
			{
				StopCoroutine(_coroutine);
				_coroutine = null;
			}
			foreach (CharacterAnimation animation in animations)
			{
				AnimationClip value;
				if (animation.isActiveAndEnabled && animationInfo.dictionary.TryGetValue(animation.key, out value))
				{
					animation.Play(value, speed);
				}
			}
		}

		public void Play(AnimationInfo animationInfo, float length, float speed)
		{
			Play(animationInfo, speed);
			_coroutine = StartCoroutine(ExpireInSeconds(length));
		}

		public void Stun()
		{
			if (_coroutine != null)
			{
				StopCoroutine(_coroutine);
				_coroutine = null;
			}
			foreach (CharacterAnimation animation in animations)
			{
				animation.Stun();
			}
		}

		private IEnumerator ExpireInSeconds(float seconds)
		{
			while (seconds >= 0f)
			{
				yield return null;
				seconds -= _character.chronometer.animation.deltaTime;
			}
			StopAll();
			this.onExpire?.Invoke();
		}

		public void Loop()
		{
			foreach (CharacterAnimation animation in animations)
			{
				animation.Play();
			}
		}

		public void StopAll()
		{
			foreach (CharacterAnimation animation in animations)
			{
				animation.Stop();
			}
		}
	}
}
