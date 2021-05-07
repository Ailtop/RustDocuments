using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Inventory
{
	public class GearElement : Button
	{
		public Action onSelected;

		[SerializeField]
		private Image _placeholder;

		[SerializeField]
		private Image _icon;

		[SerializeField]
		private Image _shadowIcon;

		[SerializeField]
		private Image _setImage;

		[SerializeField]
		private Animator _setAnimator;

		[SerializeField]
		private Shadow _shadow;

		public override void OnSelect(BaseEventData eventData)
		{
			base.OnSelect(eventData);
			onSelected?.Invoke();
		}

		public void SetIcon(Sprite sprite)
		{
			_placeholder.color = new Color(1f, 1f, 1f, 0f);
			_icon.enabled = true;
			_icon.sprite = sprite;
			_icon.SetNativeSize();
			_shadowIcon.enabled = true;
			_shadowIcon.sprite = sprite;
			_shadowIcon.SetNativeSize();
		}

		public void SetSetImage(Sprite image)
		{
			_setImage.enabled = true;
			_setImage.sprite = image;
			_setImage.SetNativeSize();
		}

		public void SetSetAnimator(RuntimeAnimatorController animatorController)
		{
			_setAnimator.enabled = true;
			_setAnimator.runtimeAnimatorController = animatorController;
			_setAnimator.Update(0f);
			_setImage.SetNativeSize();
		}

		public void DisableSetEffect()
		{
			_setImage.enabled = false;
			_setAnimator.enabled = false;
		}

		public void Deactivate()
		{
			_placeholder.color = Color.white;
			_icon.enabled = false;
			_shadowIcon.enabled = false;
			_setImage.enabled = false;
			_setAnimator.enabled = false;
		}
	}
}
