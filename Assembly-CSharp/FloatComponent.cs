using UnityEngine;

public class FloatComponent : MonoBehaviour
{
	[SerializeField]
	private string _label;

	[SerializeField]
	private float _value;

	public float value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
		}
	}

	public void Increase(float amount)
	{
		_value += amount;
	}

	public void Increase(FloatComponent amount)
	{
		_value += amount.value;
	}

	public void Decrease(float amount)
	{
		_value -= amount;
	}

	public void Decrease(FloatComponent amount)
	{
		_value += amount.value;
	}
}
