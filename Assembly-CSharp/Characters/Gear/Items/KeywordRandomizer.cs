using System.Collections.Generic;
using System.Linq;
using Characters.Gear.Synergy.Keywords;
using UnityEngine;

namespace Characters.Gear.Items
{
	public class KeywordRandomizer : MonoBehaviour
	{
		private enum Type
		{
			Normal,
			Clone
		}

		[SerializeField]
		private Item _item;

		[SerializeField]
		private Type _type;

		private void Awake()
		{
			List<Keyword.Key> list = EnumValues<Keyword.Key>.Values.ToList();
			list.Remove(Keyword.Key.None);
			_item.keyword1 = list.Random();
			if (_type == Type.Normal)
			{
				list.Remove(_item.keyword1);
				_item.keyword2 = list.Random();
			}
			else if (_type == Type.Clone)
			{
				_item.keyword2 = _item.keyword1;
			}
		}
	}
}
