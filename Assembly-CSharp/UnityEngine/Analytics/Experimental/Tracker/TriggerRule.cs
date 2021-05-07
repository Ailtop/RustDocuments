using System;

namespace UnityEngine.Analytics.Experimental.Tracker
{
	[Serializable]
	public class TriggerRule
	{
		[SerializeField]
		private TrackableField m_Target;

		[SerializeField]
		private TriggerOperator m_Operator;

		[SerializeField]
		private ValueProperty m_Value;

		[SerializeField]
		private ValueProperty m_Value2;

		public bool Test()
		{
			bool error;
			string message;
			return Test(out error, out message);
		}

		public bool Test(out bool error, out string message)
		{
			error = false;
			message = null;
			if (m_Target == null || m_Target.GetValue() == null)
			{
				error = true;
				message = "rule target is not set";
				return false;
			}
			if (m_Value == null || m_Value.target == null || m_Value.propertyValue == null)
			{
				error = true;
				message = "rule value is not set";
				return false;
			}
			object value = m_Target.GetValue();
			string valueType = m_Value.valueType;
			if (valueType == typeof(string).ToString())
			{
				return TestByString((string)value);
			}
			if (valueType == typeof(bool).ToString())
			{
				return TestByBool((bool)value);
			}
			if (valueType == typeof(float).ToString())
			{
				return TestByDouble((float)value);
			}
			if (valueType == typeof(double).ToString())
			{
				return TestByDouble((double)value);
			}
			if (valueType == typeof(decimal).ToString())
			{
				return TestByDouble((double)(decimal)value);
			}
			if (valueType == typeof(int).ToString())
			{
				return TestByDouble((int)value);
			}
			if (valueType == typeof(short).ToString())
			{
				return TestByDouble((short)value);
			}
			if (valueType == typeof(long).ToString())
			{
				return TestByDouble((long)value);
			}
			if (valueType == "enum")
			{
				return TestByEnum(((Enum)value).ToString().ToLower());
			}
			return TestByObject(value);
		}

		private bool TestByObject(object currentValue)
		{
			bool result = false;
			switch (m_Operator)
			{
			case TriggerOperator.Equals:
				result = currentValue.Equals(m_Value.propertyValue);
				break;
			case TriggerOperator.DoesNotEqual:
				result = !currentValue.Equals(m_Value.propertyValue);
				break;
			}
			return result;
		}

		private bool TestByEnum(string currentValue)
		{
			bool result = false;
			switch (m_Operator)
			{
			case TriggerOperator.Equals:
				result = currentValue.Equals(m_Value.propertyValue.ToString().ToLower());
				break;
			case TriggerOperator.DoesNotEqual:
				result = !currentValue.Equals(m_Value.propertyValue.ToString().ToLower());
				break;
			}
			return result;
		}

		private bool TestByString(string currentValue)
		{
			bool result = false;
			switch (m_Operator)
			{
			case TriggerOperator.Equals:
				result = currentValue.Equals(m_Value.propertyValue);
				break;
			case TriggerOperator.DoesNotEqual:
				result = !currentValue.Equals(m_Value.propertyValue);
				break;
			}
			return result;
		}

		private bool TestByBool(bool currentValue)
		{
			bool result = false;
			bool obj = bool.Parse(m_Value.propertyValue);
			switch (m_Operator)
			{
			case TriggerOperator.Equals:
				result = currentValue.Equals(obj);
				break;
			case TriggerOperator.DoesNotEqual:
				result = !currentValue.Equals(obj);
				break;
			}
			return result;
		}

		private bool TestByDouble(double currentValue)
		{
			bool result = false;
			double @double = GetDouble(m_Value.propertyValue);
			switch (m_Operator)
			{
			case TriggerOperator.Equals:
				result = SafeEquals(currentValue, @double);
				break;
			case TriggerOperator.DoesNotEqual:
				result = !SafeEquals(currentValue, @double);
				break;
			case TriggerOperator.IsGreaterThan:
				result = currentValue > @double;
				break;
			case TriggerOperator.IsGreaterThanOrEqualTo:
				result = currentValue > @double || SafeEquals(currentValue, @double);
				break;
			case TriggerOperator.IsLessThan:
				result = currentValue < @double;
				break;
			case TriggerOperator.IsLessThanOrEqualTo:
				result = currentValue < @double || SafeEquals(currentValue, @double);
				break;
			case TriggerOperator.IsBetween:
			{
				double double3 = GetDouble(m_Value2.propertyValue);
				result = currentValue > @double && currentValue < double3;
				break;
			}
			case TriggerOperator.IsBetweenOrEqualTo:
			{
				double double2 = GetDouble(m_Value2.propertyValue);
				result = SafeEquals(currentValue, @double) || SafeEquals(currentValue, double2) || (currentValue > @double && currentValue < double2);
				break;
			}
			}
			return result;
		}

		private bool SafeEquals(double double1, double double2)
		{
			return Math.Abs(double1 - double2) < 1E-07;
		}

		private double GetDouble(object value)
		{
			return double.Parse(value.ToString());
		}
	}
}
