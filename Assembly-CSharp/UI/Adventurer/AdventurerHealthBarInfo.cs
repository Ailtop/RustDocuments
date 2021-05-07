using UnityEngine;

namespace UI.Adventurer
{
	[CreateAssetMenu(menuName = "AdventurerUI")]
	public class AdventurerHealthBarInfo : ScriptableObject
	{
		[SerializeField]
		private Sprite _portrait;

		[SerializeField]
		private string _nameKey;

		[SerializeField]
		private string _level;
	}
}
