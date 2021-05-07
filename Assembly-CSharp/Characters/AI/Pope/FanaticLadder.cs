using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Characters.AI.Pope
{
	public sealed class FanaticLadder : MonoBehaviour
	{
		[Serializable]
		private class LadderAnimationInfo
		{
			[SerializeField]
			private FanaticFactory.SummonType _tag;

			[SerializeField]
			private GameObject _object;

			[SerializeField]
			private float _duration;

			internal FanaticFactory.SummonType tag => _tag;

			internal void Active()
			{
				_object.SetActive(true);
			}

			internal void Deactive()
			{
				_object.SetActive(false);
			}

			internal IEnumerator CRun()
			{
				Active();
				yield return Chronometer.global.WaitForSeconds(_duration);
				Deactive();
			}

			internal IEnumerator CFall(Transform transform)
			{
				_object.GetComponent<Animator>().speed = 0f;
				float elapsed2 = 0f;
				float speed = 6f;
				while (elapsed2 < 0.4f)
				{
					transform.Translate(Vector2.up * speed * elapsed2 * Chronometer.global.deltaTime);
					elapsed2 += Chronometer.global.deltaTime;
					speed -= Chronometer.global.deltaTime;
					yield return null;
				}
				elapsed2 = 0f;
				while (elapsed2 < 3f)
				{
					transform.Translate(Vector2.down * 5f * (1f + elapsed2) * Chronometer.global.deltaTime);
					elapsed2 += Chronometer.global.deltaTime;
					yield return null;
				}
			}
		}

		[SerializeField]
		private Transform _spawnPoint;

		[SerializeField]
		private LadderAnimationInfo[] _infos;

		[SerializeField]
		private Animator _ladderAnimationController;

		[SerializeField]
		private AnimationClip _introClip;

		[SerializeField]
		private AnimationClip _outroClip;

		[SerializeField]
		private AnimationClip _fallClip;

		private Dictionary<FanaticFactory.SummonType, LadderAnimationInfo> _animations;

		public Vector3 spawnPoint => _spawnPoint.position;

		public FanaticFactory.SummonType fanatic { get; set; }

		private void Awake()
		{
			_animations = new Dictionary<FanaticFactory.SummonType, LadderAnimationInfo>(_infos.Length);
			LadderAnimationInfo[] infos = _infos;
			foreach (LadderAnimationInfo ladderAnimationInfo in infos)
			{
				if (_animations.ContainsKey(ladderAnimationInfo.tag))
				{
					Debug.LogError($"An item with the same key has alread been added. Key:{ladderAnimationInfo.tag}");
					break;
				}
				_animations.Add(ladderAnimationInfo.tag, ladderAnimationInfo);
			}
		}

		public void SetFanatic(FanaticFactory.SummonType who)
		{
			fanatic = who;
		}

		public IEnumerator CClimb()
		{
			_ladderAnimationController.gameObject.SetActive(true);
			_ladderAnimationController.Play("Intro", 0, 0f);
			yield return _animations[fanatic].CRun();
			StartCoroutine(CHide());
		}

		private IEnumerator CHide()
		{
			_ladderAnimationController.Play("Outtro", 0, 0f);
			yield return Chronometer.global.WaitForSeconds(_outroClip.length);
			_ladderAnimationController.gameObject.SetActive(false);
		}

		public IEnumerator CFall()
		{
			yield return _animations[fanatic].CFall(base.transform);
			_ladderAnimationController.gameObject.SetActive(false);
		}
	}
}
