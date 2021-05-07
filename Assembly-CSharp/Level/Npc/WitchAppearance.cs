using Data;
using UnityEngine;

namespace Level.Npc
{
	public class WitchAppearance : MonoBehaviour
	{
		[SerializeField]
		private GameObject _humanForm;

		[SerializeField]
		private GameObject _catForm;

		public bool humanForm => _humanForm.activeSelf;

		private void Start()
		{
			if (!GameData.Generic.tutorial.isPlayed())
			{
				if (MMMaths.Chance(0.7))
				{
					_catForm.SetActive(false);
					_humanForm.SetActive(true);
				}
				else
				{
					_humanForm.SetActive(false);
					_catForm.SetActive(true);
				}
			}
		}
	}
}
