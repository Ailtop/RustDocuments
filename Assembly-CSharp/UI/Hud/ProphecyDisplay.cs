using System.Collections;
using Level.Npc.Prophecies;
using TMPro;
using UnityEngine;

namespace UI.Hud
{
	public class ProphecyDisplay : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text _name;

		[SerializeField]
		private TMP_Text _description;

		private void OnEnable()
		{
			StartCoroutine(CUpdate());
		}

		private IEnumerator CUpdate()
		{
			WaitForSeconds interval = new WaitForSeconds(0.5f);
			while (true)
			{
				if (Prophecy.prophecyFromDruid == null)
				{
					_name.gameObject.SetActive(false);
					_description.gameObject.SetActive(false);
				}
				else
				{
					_name.gameObject.SetActive(true);
					_description.gameObject.SetActive(true);
					_name.text = Prophecy.prophecyFromDruid.name;
					_description.text = Prophecy.prophecyFromDruid.description;
				}
				yield return interval;
			}
		}
	}
}
