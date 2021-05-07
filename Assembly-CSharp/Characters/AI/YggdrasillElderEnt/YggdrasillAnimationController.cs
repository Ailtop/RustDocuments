using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Characters.AI.YggdrasillElderEnt
{
	public class YggdrasillAnimationController : MonoBehaviour
	{
		[SerializeField]
		private Character _owner;

		[SerializeField]
		private Transform _phase1Container;

		[SerializeField]
		private Transform _phase2Container;

		private Dictionary<YggdrasillAnimation.Tag, CharacterAnimationController.AnimationInfo> _phase1Mapper = new Dictionary<YggdrasillAnimation.Tag, CharacterAnimationController.AnimationInfo>();

		private Dictionary<YggdrasillAnimation.Tag, CharacterAnimationController.AnimationInfo> _phase2Mapper = new Dictionary<YggdrasillAnimation.Tag, CharacterAnimationController.AnimationInfo>();

		private float _speed = 1f;

		private const string _referenceAnimationTag = "Behind";

		private void Awake()
		{
			CreateMapper(_phase1Container, _phase1Mapper);
			CreateMapper(_phase2Container, _phase2Mapper);
		}

		private void CreateMapper(Transform transform, Dictionary<YggdrasillAnimation.Tag, CharacterAnimationController.AnimationInfo> mapper)
		{
			YggdrasillAnimation[] componentsInChildren = transform.GetComponentsInChildren<YggdrasillAnimation>();
			foreach (YggdrasillAnimation yggdrasillAnimation in componentsInChildren)
			{
				mapper.Add(yggdrasillAnimation.tag, yggdrasillAnimation.info);
			}
		}

		public IEnumerator CPlayAndWaitAnimation(YggdrasillAnimation.Tag tag)
		{
			CharacterAnimationController.AnimationInfo value;
			if (_phase1Mapper.TryGetValue(tag, out value) || _phase2Mapper.TryGetValue(tag, out value))
			{
				_owner.animationController.Play(value, _speed);
				yield return _owner.chronometer.animation.WaitForSeconds(value.dictionary["Behind"].length);
			}
		}

		public void PlayCutSceneAnimation()
		{
			StartCoroutine(_003CPlayCutSceneAnimation_003Eg__CLoop_007C10_0());
		}
	}
}
