using System;

namespace UnityEngine.Rendering.PostProcessing;

public abstract class ParameterOverride
{
	public bool overrideState;

	internal abstract void Interp(ParameterOverride from, ParameterOverride to, float t);

	public abstract int GetHash();

	public T GetValue<T>()
	{
		return ((ParameterOverride<T>)this).value;
	}

	protected internal virtual void OnEnable()
	{
	}

	protected internal virtual void OnDisable()
	{
	}

	internal abstract void SetValue(ParameterOverride parameter);
}
[Serializable]
public class ParameterOverride<T> : ParameterOverride
{
	public T value;

	public ParameterOverride()
		: this(default(T), overrideState: false)
	{
	}

	public ParameterOverride(T value)
		: this(value, overrideState: false)
	{
	}

	public ParameterOverride(T value, bool overrideState)
	{
		this.value = value;
		base.overrideState = overrideState;
	}

	internal override void Interp(ParameterOverride from, ParameterOverride to, float t)
	{
		Interp(from.GetValue<T>(), to.GetValue<T>(), t);
	}

	public virtual void Interp(T from, T to, float t)
	{
		value = ((t > 0f) ? to : from);
	}

	public void Override(T x)
	{
		overrideState = true;
		value = x;
	}

	internal override void SetValue(ParameterOverride parameter)
	{
		value = parameter.GetValue<T>();
	}

	public override int GetHash()
	{
		return (17 * 23 + overrideState.GetHashCode()) * 23 + value.GetHashCode();
	}

	public static implicit operator T(ParameterOverride<T> prop)
	{
		return prop.value;
	}
}
