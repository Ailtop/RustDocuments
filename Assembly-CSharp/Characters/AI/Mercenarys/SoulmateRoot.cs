using Level;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.AI.Mercenarys
{
	public class SoulmateRoot : MonoBehaviour
	{
		[SerializeField]
		private Soulmate _soulmate;

		private void Start()
		{
			Singleton<Service>.Instance.levelManager.onMapChangedAndFadedIn += OnMapChanged;
			Singleton<Service>.Instance.levelManager.onMapLoaded += _soulmate.Hide;
			Object.DontDestroyOnLoad(_soulmate.gameObject);
		}

		private void OnMapChanged(Map old, Map @new)
		{
			if (WitchBonus.instance.soul.fatalMind.level != 0)
			{
				if (Singleton<Service>.Instance.levelManager.currentChapter.type == Chapter.Type.Castle)
				{
					_soulmate.Hide();
				}
				else
				{
					StartCoroutine(_soulmate.CAppearance());
				}
			}
		}
	}
}
