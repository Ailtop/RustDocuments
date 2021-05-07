using Characters;
using UnityEngine;

namespace UI.Witch
{
	public class Tree : MonoBehaviour
	{
		private Panel _panel;

		[SerializeField]
		private TreeElement[] _elements;

		private WitchBonus.Tree _tree;

		public TreeElement[] elements => _elements;

		public void SetInteractable(bool value)
		{
			TreeElement[] array = _elements;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].interactable = value;
			}
		}

		public void Initialize(Panel panel)
		{
			_panel = panel;
			for (int i = 0; i < _elements.Length; i++)
			{
				_elements[i].Initialize(panel);
			}
		}

		public void Set(WitchBonus.Tree tree)
		{
			_tree = tree;
			for (int i = 0; i < _elements.Length; i++)
			{
				_elements[i].Set(tree.list[i]);
			}
		}
	}
}
