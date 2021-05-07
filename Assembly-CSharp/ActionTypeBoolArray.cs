using System;
using Characters.Actions;

[Serializable]
public class ActionTypeBoolArray : EnumArray<Characters.Actions.Action.Type, bool>
{
	public ActionTypeBoolArray()
	{
	}

	public ActionTypeBoolArray(EnumArray<Characters.Actions.Action.Type, bool> defaultValue)
		: base(defaultValue)
	{
	}

	public void GetOrDefault()
	{
	}
}
