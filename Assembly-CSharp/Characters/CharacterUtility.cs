using UnityEngine;

namespace Characters
{
	public static class CharacterUtility
	{
		public static bool TryFindCharacterComponent(this GameObject gameObject, out Character character)
		{
			if (gameObject.TryGetComponent<Character>(out character))
			{
				return true;
			}
			Target component;
			if (gameObject.TryGetComponent<Target>(out component))
			{
				character = component.character;
				return true;
			}
			character = null;
			return false;
		}

		public static bool TryFindCharacterComponent(this Component component, out Character character)
		{
			return component.gameObject.TryFindCharacterComponent(out character);
		}
	}
}
