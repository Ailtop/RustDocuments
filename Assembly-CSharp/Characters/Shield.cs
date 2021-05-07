using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Characters
{
	public class Shield
	{
		public class Instance
		{
			public readonly object key;

			private Action _onBroke;

			public double originalAmount { get; set; }

			public double amount { get; set; }

			public Instance(object key, float amount, Action onBroke)
			{
				this.key = key;
				originalAmount = amount;
				this.amount = amount;
				_onBroke = onBroke;
			}

			public void Break()
			{
				amount = 0.0;
				_onBroke?.Invoke();
			}

			public double Consume(double damage)
			{
				if (damage < amount)
				{
					amount -= damage;
					return 0.0;
				}
				damage -= amount;
				amount = 0.0;
				_onBroke?.Invoke();
				return damage;
			}
		}

		private readonly List<Instance> _shields = new List<Instance>();

		public double amount => _shields.Sum((Instance shield) => shield.amount);

		public bool hasAny => _shields.Count > 0;

		public event Action<Instance> onAdd;

		public event Action<Instance> onRemove;

		public event Action<Instance> onUpdate;

		public Instance Add(object key, float amount, Action onBroke = null)
		{
			Instance instance = new Instance(key, amount, onBroke);
			_shields.Add(instance);
			this.onAdd?.Invoke(instance);
			return instance;
		}

		public void AddOrUpdate(object key, float amount, Action onBroke = null)
		{
			for (int i = 0; i < _shields.Count; i++)
			{
				Instance instance = _shields[i];
				if (instance.key.Equals(key))
				{
					instance.amount = amount;
					this.onUpdate?.Invoke(instance);
					return;
				}
			}
			_shields.Add(new Instance(key, amount, onBroke));
		}

		public bool Remove(object key)
		{
			for (int i = 0; i < _shields.Count; i++)
			{
				Instance instance = _shields[i];
				if (instance.key.Equals(key))
				{
					_shields.RemoveAt(i);
					this.onRemove?.Invoke(instance);
					return true;
				}
			}
			return false;
		}

		public void Clear()
		{
			for (int i = 0; i < _shields.Count; i++)
			{
				Instance obj = _shields[i];
				this.onRemove?.Invoke(obj);
			}
			_shields.Clear();
		}

		public double Consume(double damage)
		{
			for (int i = 0; i < _shields.Count; i++)
			{
				damage = _shields[i].Consume(damage);
			}
			for (int num = _shields.Count - 1; num >= 0; num--)
			{
				Instance instance = _shields[num];
				if (instance.amount > 0.0)
				{
					break;
				}
				Debug.Log("Shield was not destroyed even though the shield amount is 0. So it's manually broken.");
				instance.Break();
			}
			return damage;
		}
	}
}
