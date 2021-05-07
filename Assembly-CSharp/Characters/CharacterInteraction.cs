using System.Collections;
using System.Collections.Generic;
using Characters.Controllers;
using UnityEngine;

namespace Characters
{
	public class CharacterInteraction : MonoBehaviour
	{
		public enum InteractionType
		{
			Normal,
			Pressing
		}

		public const float pressingTimeForPressing = 1f;

		private const float _maxPressingTimeForRelease = 0.5f;

		private const float _interactInterval = 0.2f;

		[SerializeField]
		[GetComponent]
		private Character _character;

		[SerializeField]
		[GetComponent]
		private PlayerInput _input;

		private readonly List<InteractiveObject> _interactiveObjects = new List<InteractiveObject>();

		private float _lastInteractedTime;

		private float _pressingTime;

		private InteractiveObject _objectToInteract;

		private void OnTriggerEnter2D(Collider2D collision)
		{
			InteractiveObject component = collision.GetComponent<InteractiveObject>();
			if (component != null)
			{
				_interactiveObjects.Add(component);
			}
			else
			{
				collision.GetComponent<IPickupable>()?.PickedUpBy(_character);
			}
		}

		private void OnTriggerExit2D(Collider2D collision)
		{
			InteractiveObject component = collision.GetComponent<InteractiveObject>();
			if (component != null)
			{
				_interactiveObjects.Remove(component);
			}
		}

		private void Update()
		{
			for (int num = _interactiveObjects.Count - 1; num >= 0; num--)
			{
				InteractiveObject interactiveObject = _interactiveObjects[num];
				if (interactiveObject == null || !interactiveObject.isActiveAndEnabled)
				{
					_interactiveObjects.RemoveAt(num);
				}
			}
			if (_interactiveObjects.Count == 0 || PlayerInput.blocked.value)
			{
				if (_objectToInteract != null)
				{
					_objectToInteract.ClosePopup();
				}
				_pressingTime = 0f;
				StopCoroutine("CPressing");
				_objectToInteract = null;
				return;
			}
			_interactiveObjects.Sort((InteractiveObject i1, InteractiveObject i2) => Mathf.Abs(base.transform.position.x - i1.transform.position.x).CompareTo(Mathf.Abs(base.transform.position.x - i2.transform.position.x)));
			for (int j = 0; j < _interactiveObjects.Count; j++)
			{
				InteractiveObject interactiveObject2 = _interactiveObjects[j];
				if (!interactiveObject2.activated)
				{
					continue;
				}
				if (interactiveObject2.autoInteract)
				{
					interactiveObject2.InteractWith(_character);
				}
				if (_objectToInteract != interactiveObject2)
				{
					if (_objectToInteract != null)
					{
						_pressingTime = 0f;
						StopCoroutine("CPressing");
						_objectToInteract.ClosePopup();
					}
					interactiveObject2.OpenPopupBy(_character);
					_objectToInteract = interactiveObject2;
				}
				break;
			}
		}

		public void Interact(InteractionType interactionType)
		{
			if (!(_objectToInteract == null) && _objectToInteract.activated && _objectToInteract.interactionType == interactionType && !(_lastInteractedTime + 0.2f > Time.realtimeSinceStartup))
			{
				_objectToInteract.InteractWith(_character);
				_lastInteractedTime = Time.realtimeSinceStartup;
			}
		}

		public void InteractionKeyWasPressed()
		{
			if (!(_objectToInteract == null) && _objectToInteract.activated && !(_lastInteractedTime + 0.2f > Time.realtimeSinceStartup))
			{
				if (_objectToInteract.interactionType == InteractionType.Normal)
				{
					_objectToInteract.InteractWith(_character);
					_lastInteractedTime = Time.realtimeSinceStartup;
				}
				else if (_objectToInteract.interactionType == InteractionType.Pressing)
				{
					StartCoroutine("CPressing");
				}
			}
		}

		private IEnumerator CPressing()
		{
			_pressingTime = Chronometer.global.deltaTime;
			while (_pressingTime < 1f)
			{
				yield return null;
				_pressingTime += Chronometer.global.deltaTime;
				_objectToInteract.pressingPercent = _pressingTime / 1f;
			}
			_objectToInteract.InteractWithByPressing(_character);
		}

		public void InteractionKeyWasReleased()
		{
			if (_pressingTime == 0f)
			{
				return;
			}
			StopCoroutine("CPressing");
			if (!(_objectToInteract == null) && _objectToInteract.activated && !(_lastInteractedTime + 0.2f > Time.realtimeSinceStartup))
			{
				_objectToInteract.pressingPercent = 0f;
				if (!(_pressingTime > 0.5f))
				{
					_objectToInteract.InteractWith(_character);
				}
			}
		}
	}
}
