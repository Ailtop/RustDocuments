using UnityEngine;

namespace Characters.Gear.Weapons.Gauges
{
	public class ValueGauge : Gauge
	{
		public delegate void onChangedDelegate(float oldValue, float newValue);

		[SerializeField]
		protected Color _defaultBarColor = Color.black;

		[Space]
		[SerializeField]
		private bool _secondBar;

		[SerializeField]
		private Color _secondBarColor;

		protected float _currentValue;

		[Space]
		[SerializeField]
		protected float _maxValue;

		[SerializeField]
		protected bool _displayText = true;

		public override float gaugePercent => _currentValue / _maxValue;

		public override string displayText
		{
			get
			{
				if (!_displayText)
				{
					return string.Empty;
				}
				return $"{_currentValue:0} / {_maxValue:0}";
			}
		}

		public float currentValue => _currentValue;

		public float maxValue
		{
			get
			{
				return _maxValue;
			}
			set
			{
				_maxValue = value;
			}
		}

		public Color defaultBarColor
		{
			get
			{
				return _defaultBarColor;
			}
			set
			{
				_defaultBarColor = value;
			}
		}

		public override Color barColor => _defaultBarColor;

		public override bool secondBar => _secondBar;

		public override Color secondBarColor => _secondBarColor;

		public event onChangedDelegate onChanged;

		public bool Has(float amount)
		{
			return _currentValue >= amount;
		}

		public void Clear()
		{
			Set(0f);
		}

		public bool Consume(float amount)
		{
			if (!Has(amount))
			{
				return false;
			}
			Set(_currentValue - amount);
			return true;
		}

		public void Add(float amount)
		{
			Set(_currentValue + amount);
		}

		public void FillUp()
		{
			Set(_maxValue);
		}

		public void Set(float value)
		{
			value = Mathf.Clamp(value, 0f, _maxValue);
			float oldValue = _currentValue;
			_currentValue = value;
			this.onChanged?.Invoke(oldValue, _currentValue);
		}
	}
}
