using System;
using Characters;
using FX;
using Singletons;
using UnityEngine;

public abstract class InteractiveObject : MonoBehaviour
{
	protected static readonly int _activateHash = Animator.StringToHash("Activate");

	protected static readonly int _deactivateHash = Animator.StringToHash("Deactivate");

	[SerializeField]
	private CharacterInteraction.InteractionType _interactionType;

	public bool autoInteract;

	[Space]
	[SerializeField]
	protected SoundInfo _activateSound;

	[SerializeField]
	protected SoundInfo _deactivateSound;

	[SerializeField]
	protected SoundInfo _interactSound;

	[SerializeField]
	[Tooltip("모든 오브젝트에서 작동하는 건 아니며 코드상에서 직접 설정해주어야 함")]
	protected SoundInfo _interactFailSound;

	[Space]
	[SerializeField]
	protected GameObject _uiObject;

	[SerializeField]
	protected GameObject[] _uiObjects;

	[Space]
	[SerializeField]
	protected bool _activated = true;

	protected Character _character;

	[NonSerialized]
	public float pressingPercent;

	public virtual CharacterInteraction.InteractionType interactionType => _interactionType;

	public bool popupVisible => _character != null;

	public bool activated
	{
		get
		{
			return _activated;
		}
		private set
		{
			_activated = value;
		}
	}

	protected virtual void Awake()
	{
		ClosePopup();
	}

	private void OnDisable()
	{
		_activated = false;
	}

	public void Activate()
	{
		_activated = true;
		OnActivate();
	}

	public void Deactivate()
	{
		ClosePopup();
		_activated = false;
		OnDeactivate();
	}

	public virtual void OnActivate()
	{
		PersistentSingleton<SoundManager>.Instance.PlaySound(_activateSound, base.transform.position);
	}

	public virtual void OnDeactivate()
	{
		PersistentSingleton<SoundManager>.Instance.PlaySound(_deactivateSound, base.transform.position);
	}

	public virtual void InteractWith(Character character)
	{
	}

	public virtual void InteractWithByPressing(Character character)
	{
	}

	public virtual void OpenPopupBy(Character character)
	{
		_character = character;
		pressingPercent = 0f;
		GameObject[] uiObjects = _uiObjects;
		foreach (GameObject gameObject in uiObjects)
		{
			if (!(gameObject == null) && !gameObject.activeSelf)
			{
				gameObject.SetActive(true);
			}
		}
		if (!(_uiObject == null) && !_uiObject.activeSelf && !autoInteract)
		{
			_uiObject.SetActive(true);
		}
	}

	public virtual void ClosePopup()
	{
		_character = null;
		GameObject[] uiObjects = _uiObjects;
		foreach (GameObject gameObject in uiObjects)
		{
			if (gameObject != null)
			{
				gameObject.SetActive(false);
			}
		}
		if (_uiObject != null)
		{
			_uiObject.SetActive(false);
		}
	}
}
