using System.Collections.Generic;
using Level;
using UnityEngine;
using UnityEngine.UI;

namespace UI.TestingTool
{
	public class MapList : MonoBehaviour
	{
		[SerializeField]
		private MapListElement _mapListElementPrefab;

		[SerializeField]
		private Button _back;

		[SerializeField]
		private Button _chapter1;

		[SerializeField]
		private Button _chapter2;

		[SerializeField]
		private Button _chapter3;

		[SerializeField]
		private Button _chapter4;

		[SerializeField]
		private Button _chapter5;

		[SerializeField]
		private Transform _gridContainer;

		private readonly List<MapListElement> _mapListElements = new List<MapListElement>();

		private void Awake()
		{
			Chapter[] chapters = Resource.instance.chapters;
			foreach (Chapter chapter in chapters)
			{
				IStageInfo[] stages = chapter.stages;
				foreach (IStageInfo stageInfo in stages)
				{
					Resource.MapReference[] maps = stageInfo.maps;
					foreach (Resource.MapReference mapReference in maps)
					{
						MapListElement mapListElement = Object.Instantiate(_mapListElementPrefab, _gridContainer);
						mapListElement.Set(chapter, stageInfo, mapReference);
						_mapListElements.Add(mapListElement);
					}
				}
			}
			_chapter1.onClick.AddListener(delegate
			{
				FilterMapList(Chapter.Type.Chapter1);
			});
			_chapter2.onClick.AddListener(delegate
			{
				FilterMapList(Chapter.Type.Chapter2);
			});
			_chapter3.onClick.AddListener(delegate
			{
				FilterMapList(Chapter.Type.Chapter3);
			});
			_chapter4.onClick.AddListener(delegate
			{
				FilterMapList(Chapter.Type.Chapter4);
			});
			_chapter5.onClick.AddListener(delegate
			{
				FilterMapList(Chapter.Type.Chapter5);
			});
		}

		private void FilterMapList(Chapter.Type chapter)
		{
			foreach (MapListElement mapListElement in _mapListElements)
			{
				mapListElement.gameObject.SetActive(mapListElement.chapter == chapter);
			}
		}
	}
}
