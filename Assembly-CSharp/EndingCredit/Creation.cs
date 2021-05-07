using System.Collections.Generic;
using UnityEngine;

namespace EndingCredit
{
	public class Creation : MonoBehaviour
	{
		[SerializeField]
		private CreditList _creditList;

		[SerializeField]
		private CreditText _text;

		[SerializeField]
		private Transform _transform;

		[SerializeField]
		private GameObject _supporter;

		[SerializeField]
		private int _number;

		private List<GameObject> _suppoterGroup;

		private void Awake()
		{
			_suppoterGroup = new List<GameObject>();
			for (int i = 0; i < _number; i++)
			{
				GameObject item = Object.Instantiate(_supporter, _transform).transform.GetChild(0).gameObject;
				_suppoterGroup.Add(item);
			}
			_text.Initialize();
			_creditList.Add(_suppoterGroup.ToArray());
		}
	}
}
