using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public sealed class UnlockNotice : MonoBehaviour
	{
		[SerializeField]
		private Image _icon;

		[SerializeField]
		private TextMeshProUGUI _name;

		[SerializeField]
		private Animator _animator;

		public void Show(Sprite icon, string name)
		{
			_icon.sprite = icon;
			_icon.SetNativeSize();
			_name.text = name;
			base.gameObject.SetActive(true);
			if (base.gameObject.activeInHierarchy)
			{
				StopAllCoroutines();
				StartCoroutine(CFadeInOut());
			}
		}

		private IEnumerator CFadeInOut()
		{
			if (_animator.runtimeAnimatorController != null)
			{
				if (!_animator.enabled)
				{
					_animator.enabled = true;
				}
				_animator.Play(0, 0, 0f);
			}
			float remain = _animator.GetCurrentAnimatorStateInfo(0).length;
			_animator.enabled = false;
			while (remain > float.Epsilon)
			{
				yield return null;
				float unscaledDeltaTime = Time.unscaledDeltaTime;
				_animator.Update(unscaledDeltaTime);
				remain -= unscaledDeltaTime;
			}
			base.gameObject.SetActive(false);
		}
	}
}
