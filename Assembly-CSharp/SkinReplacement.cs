using System;

[Serializable]
public class SkinReplacement
{
	public enum SkinType
	{
		NONE = 0,
		Hands = 1,
		Head = 2,
		Feet = 3,
		Torso = 4,
		Legs = 5
	}

	public SkinType skinReplacementType;

	public GameObjectRef targetReplacement;
}
