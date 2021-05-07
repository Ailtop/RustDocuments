using System;
using UnityEditor;
using UnityEngine;

namespace Characters.Actions.Cooldowns
{
	public abstract class Cooldown : MonoBehaviour
	{
		public class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public SubcomponentAttribute()
				: base(typeof(Time), Cooldown.types)
			{
			}
		}

		public static readonly Type[] types = new Type[4]
		{
			typeof(Infinity),
			typeof(Time),
			typeof(Gauge),
			typeof(Damage)
		};

		protected int _stacks;

		protected Character _character;

		private System.Action _onReady;

		public int stacks
		{
			get
			{
				return _stacks;
			}
			protected set
			{
				if (_stacks == 0 && value > 0 && _onReady != null)
				{
					_onReady();
				}
				_stacks = value;
			}
		}

		public abstract bool canUse { get; }

		public abstract float remainPercent { get; }

		public event System.Action onReady
		{
			add
			{
				_onReady = (System.Action)Delegate.Combine(_onReady, value);
			}
			remove
			{
				_onReady = (System.Action)Delegate.Remove(_onReady, value);
			}
		}

		internal abstract bool Consume();

		protected virtual void Awake()
		{
		}

		internal virtual void Initialize(Character character)
		{
			_character = character;
		}

		public override string ToString()
		{
			return this.GetAutoName();
		}
	}
}
