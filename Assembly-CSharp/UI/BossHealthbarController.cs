using System;
using Characters;
using UnityEngine;

namespace UI
{
	public class BossHealthbarController : MonoBehaviour
	{
		[Serializable]
		public class TypeHealthBarArray : EnumArray<Type, CharacterHealthBar>
		{
		}

		public enum Type
		{
			Tutorial,
			Chpater1_Phase1,
			Chapter1_Phase2,
			Chpater2_Phase1,
			Chpater2_Phase1_Right,
			Chapter2_Phase2,
			Chpater3_Phase1,
			Chapter3_Phase2,
			Chapter4_Phase1,
			Chapter4_Phase1_Right,
			Chapter4_Phase2,
			Chapter5_Phase1,
			Chapter5_Phase2,
			Chapter5_Phase3
		}

		[SerializeField]
		private TypeHealthBarArray _healthbars;

		[SerializeField]
		private VeteranHealthbarController _veteranHealthbarController;

		private EnumArray<Type, HangingPanelAnimator> _animators;

		private void Awake()
		{
			_animators = new EnumArray<Type, HangingPanelAnimator>();
			for (int i = 0; i < _healthbars.Count; i++)
			{
				_animators.Array[i] = _healthbars.Array[i].GetComponent<HangingPanelAnimator>();
			}
		}

		public void Open(Type type, Character character)
		{
			for (int i = 0; i < _healthbars.Count; i++)
			{
				_healthbars.Array[i].gameObject.SetActive(false);
			}
			_healthbars[type].Initialize(character);
			_animators[type].Appear();
		}

		public void OpenChapter2Phase1(Character left, Character right)
		{
			for (int i = 0; i < _healthbars.Count; i++)
			{
				_healthbars.Array[i].gameObject.SetActive(false);
			}
			_healthbars[Type.Chpater2_Phase1].Initialize(left);
			_healthbars[Type.Chpater2_Phase1_Right].Initialize(right);
			_animators[Type.Chpater2_Phase1].Appear();
		}

		public void OpenChapter4Phase1(Character left, Character right)
		{
			for (int i = 0; i < _healthbars.Count; i++)
			{
				_healthbars.Array[i].gameObject.SetActive(false);
			}
			_healthbars[Type.Chapter4_Phase1].Initialize(left);
			_healthbars[Type.Chapter4_Phase1_Right].Initialize(right);
			_animators[Type.Chapter4_Phase1].Appear();
		}

		public void OpenVeteranAdventurer(Character character, string nameKey, string titleKey)
		{
			for (int i = 0; i < _healthbars.Count; i++)
			{
				_healthbars.Array[i].gameObject.SetActive(false);
			}
			_veteranHealthbarController.Appear(character, nameKey, titleKey);
		}

		public void CloseAll()
		{
			for (int i = 0; i < _healthbars.Count; i++)
			{
				if (_healthbars.Array[i].gameObject.activeSelf)
				{
					_animators.Array[i]?.Disappear();
				}
			}
			_veteranHealthbarController.Disappear();
		}
	}
}
