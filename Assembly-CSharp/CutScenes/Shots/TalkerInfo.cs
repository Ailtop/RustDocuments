using UnityEngine;

namespace CutScenes.Shots
{
	public class TalkerInfo : MonoBehaviour
	{
		[SerializeField]
		protected Sprite _portrait;

		[SerializeField]
		protected string _nameKey;

		[Tooltip("한 번의 컷씬에서 출력되는 대사들의 키 중 공통되는 부분까지")]
		[SerializeField]
		protected string _textKey;

		protected int _currentIndex;

		public Sprite portrait => _portrait;

		public new string name => Lingua.GetLocalizedString(_nameKey);

		public virtual string[] GetNextText()
		{
			return Lingua.GetLocalizedStringArray($"{_textKey}/{_currentIndex++}");
		}
	}
}
