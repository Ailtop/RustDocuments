using System.Collections;
using UnityEngine;

namespace FX
{
	public class SkillChangingEffect : MonoBehaviour
	{
		[SerializeField]
		private Animator _animator;

		[SerializeField]
		private SpriteRenderer _oldSkill1;

		[SerializeField]
		private SpriteRenderer _oldSkill2;

		[SerializeField]
		private SpriteRenderer _newSkill1;

		[SerializeField]
		private SpriteRenderer _newSkill2;

		private float _animationLength;

		private float _remainTime;

		private void Awake()
		{
			_animationLength = _animator.GetCurrentAnimatorStateInfo(0).length;
		}

		public void Play(Sprite[] oldSkills, Sprite[] newSkills)
		{
			if (oldSkills.Length != 0)
			{
				_oldSkill1.enabled = true;
				_oldSkill1.sprite = oldSkills[0];
			}
			else
			{
				_oldSkill1.enabled = false;
			}
			if (oldSkills.Length > 1)
			{
				_oldSkill2.enabled = true;
				_oldSkill2.sprite = oldSkills[1];
			}
			else
			{
				_oldSkill2.enabled = false;
			}
			if (newSkills.Length != 0)
			{
				_newSkill1.enabled = true;
				_newSkill1.sprite = newSkills[0];
			}
			else
			{
				_newSkill1.enabled = false;
			}
			if (newSkills.Length > 1)
			{
				_newSkill2.enabled = true;
				_newSkill2.sprite = newSkills[1];
			}
			else
			{
				_newSkill2.enabled = false;
			}
			PlayAnimation();
		}

		private void PlayAnimation()
		{
			base.gameObject.SetActive(true);
			_animator.enabled = true;
			_animator.Play(0, 0, 0f);
			_animator.enabled = false;
			_remainTime = _animationLength;
			StopAllCoroutines();
			StartCoroutine(CPlay());
		}

		private IEnumerator CPlay()
		{
			while (_remainTime > 0f)
			{
				yield return null;
				float deltaTime = Chronometer.global.deltaTime;
				_animator.Update(deltaTime);
				_remainTime -= deltaTime;
			}
			base.gameObject.SetActive(false);
		}
	}
}
