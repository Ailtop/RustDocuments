using Level;
using Services;
using Singletons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.TestingTool
{
	public class MapListElement : MonoBehaviour
	{
		[SerializeField]
		private Button _button;

		[SerializeField]
		private TMP_Text _text;

		public Chapter.Type chapter { get; private set; }

		public void Set(Chapter chapter, IStageInfo stage, Resource.MapReference mapReference)
		{
			this.chapter = chapter.type;
			_text.text = stage.name + "/" + mapReference.path.Substring(mapReference.path.LastIndexOf('/') + 1);
			PathNode pathNode = new PathNode(mapReference, MapReward.Type.None, Gate.Type.None);
			_button.onClick.AddListener(delegate
			{
				Singleton<Service>.Instance.levelManager.currentChapter.ChangeMap(pathNode);
			});
		}
	}
}
