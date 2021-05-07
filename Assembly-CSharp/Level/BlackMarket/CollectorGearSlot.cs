using Data;
using TMPro;
using UnityEngine;

namespace Level.BlackMarket
{
	public class CollectorGearSlot : MonoBehaviour
	{
		[SerializeField]
		private Transform _itemPosition;

		[SerializeField]
		private TMP_Text _text;

		private DroppedGear _droppedGear;

		public Vector3 itemPosition => _itemPosition.position;

		public DroppedGear droppedGear
		{
			get
			{
				return _droppedGear;
			}
			set
			{
				_droppedGear = value;
				_text.text = _droppedGear.price.ToString();
			}
		}

		private void Update()
		{
			if (!(_droppedGear == null))
			{
				if (_droppedGear.price > 0)
				{
					_text.color = (GameData.Currency.gold.Has(_droppedGear.price) ? Color.white : Color.red);
					return;
				}
				_text.text = "---";
				_text.color = Color.white;
			}
		}
	}
}
