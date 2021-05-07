using TMPro;
using UnityEngine;

namespace UI.Inventory
{
	public class MainSkill : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text _name;

		[SerializeField]
		private TMP_Text _description;

		public void Set(string name, string description)
		{
			_name.text = name;
			_description.text = description;
		}
	}
}
