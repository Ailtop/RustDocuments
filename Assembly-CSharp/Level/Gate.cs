using System;
using Characters;
using Services;
using Singletons;
using UnityEngine;

namespace Level
{
	public class Gate : InteractiveObject
	{
		[Serializable]
		public class GateGraphicSetting : ReorderableArray<GateGraphicSetting.GateProperty>
		{
			[Serializable]
			public class GateProperty
			{
				[SerializeField]
				private Type _type;

				[SerializeField]
				private RuntimeAnimatorController _animator;

				[SerializeField]
				private GameObject _gameObject;

				public Type type => _type;

				public RuntimeAnimatorController animator => _animator;

				public GameObject gameObject => _gameObject;

				public GateProperty(Type type, RuntimeAnimatorController animator, GameObject gameObject)
				{
					_type = type;
					_animator = animator;
					_gameObject = gameObject;
				}

				public void ActivateGameObject()
				{
					if (_gameObject != null)
					{
						_gameObject.SetActive(true);
					}
				}

				public void DeactivateGameObject()
				{
					if (_gameObject != null)
					{
						_gameObject.SetActive(false);
					}
				}
			}

			public GateGraphicSetting(params GateProperty[] gateProperties)
			{
				values = gateProperties;
			}

			public GateProperty GetPropertyOf(Type type)
			{
				GateProperty[] array = values;
				foreach (GateProperty gateProperty in array)
				{
					if (gateProperty.type.Equals(type))
					{
						return gateProperty;
					}
				}
				return null;
			}
		}

		public enum Type
		{
			None,
			Normal,
			Grave,
			Chest,
			Npc,
			Terminal,
			Adventurer,
			Boss
		}

		[SerializeField]
		[GetComponent]
		private Animator _animator;

		[SerializeField]
		[GetComponent]
		private Collider2D _collider;

		[SerializeField]
		private RuntimeAnimatorController _destoryed;

		[SerializeField]
		private RuntimeAnimatorController _destoryedForTerminal;

		[SerializeField]
		private GateGraphicSetting _gateGraphicSetting;

		private GateGraphicSetting.GateProperty _gateProperty;

		private PathNode _pathNode;

		private bool _used;

		public PathNode pathNode
		{
			get
			{
				return _pathNode;
			}
			internal set
			{
				_pathNode = value;
				if (_pathNode.gate == Type.None)
				{
					base.gameObject.SetActive(false);
					return;
				}
				_gateProperty = _gateGraphicSetting.GetPropertyOf(_pathNode.gate);
				_animator.runtimeAnimatorController = _gateProperty.animator;
				if (base.activated)
				{
					_animator.Play(InteractiveObject._activateHash);
					_gateProperty.ActivateGameObject();
				}
				else
				{
					_gateProperty.DeactivateGameObject();
				}
			}
		}

		public override void OnActivate()
		{
			base.OnActivate();
			_animator.Play(InteractiveObject._activateHash);
			if (_pathNode != null && _pathNode.gate != 0)
			{
				_gateProperty.ActivateGameObject();
			}
		}

		public override void OnDeactivate()
		{
			base.OnDeactivate();
			_animator.Play(InteractiveObject._deactivateHash);
			if (_pathNode != null && _pathNode.gate != 0)
			{
				_gateProperty.DeactivateGameObject();
			}
		}

		public override void InteractWith(Character character)
		{
			if (!_used)
			{
				PersistentSingleton<SoundManager>.Instance.PlaySound(_interactSound, base.transform.position);
				_used = true;
				Singleton<Service>.Instance.levelManager.LoadNextMap(_pathNode);
			}
		}

		public void ShowDestroyed(bool terminal)
		{
			_collider.enabled = false;
			_animator.runtimeAnimatorController = (terminal ? _destoryedForTerminal : _destoryed);
			Deactivate();
		}
	}
}
