using System;
using UnityEngine;

namespace Level.Specials
{
	public class TimeCostEvent : CostEvent
	{
		[SerializeField]
		private TimeCostEventDisplay _display;

		[NonSerialized]
		public float updateInterval = 0.2f;

		[SerializeField]
		private int _maxCost;

		[SerializeField]
		private int _initialCost;

		[SerializeField]
		private int _baseIncreasePerSeconds;

		private double _currentIncrease;

		private double _currentCost;

		private void Awake()
		{
			_currentCost = _initialCost;
			_currentIncrease += (float)_baseIncreasePerSeconds * updateInterval;
		}

		public void UpdateCost()
		{
			if (_currentCost + _currentIncrease >= (double)_maxCost)
			{
				_currentCost = _maxCost;
				return;
			}
			_currentCost += _currentIncrease;
			_display.UpdateDisplay();
		}

		public void AddIncrease(double extraIncrease)
		{
			_currentIncrease += extraIncrease;
		}

		public void Multiply(double perecnt)
		{
			_currentIncrease *= perecnt;
		}

		public override double GetValue()
		{
			return _currentCost;
		}
	}
}
