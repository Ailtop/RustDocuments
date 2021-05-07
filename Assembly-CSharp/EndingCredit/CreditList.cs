using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EndingCredit
{
	public class CreditList : MonoBehaviour
	{
		[SerializeField]
		private GameObject[] _maker;

		[SerializeField]
		private Transform _destination;

		[SerializeField]
		private ContentSizeFitter _contentSizeFitter;

		private List<GameObject> _creditList = new List<GameObject>();

		public void Add(GameObject[] supporter)
		{
			AddList(_maker);
			AddList(supporter);
			StartCoroutine(CRun());
		}

		private void AddList(GameObject[] list)
		{
			foreach (GameObject item in list)
			{
				_creditList.Add(item);
			}
		}

		private IEnumerator CRun()
		{
			Refresh();
			int currentCreditListIndex = 1;
			int listCount = _creditList.Count - 1;
			while (currentCreditListIndex < listCount)
			{
				if ((_destination.transform.position - _creditList[currentCreditListIndex].transform.position).normalized.y < 0f)
				{
					Activate(currentCreditListIndex + 1);
					Deactivate(currentCreditListIndex - 1);
					currentCreditListIndex++;
				}
				yield return null;
			}
		}

		private void Activate(int index)
		{
			_creditList[index].SetActive(true);
		}

		private void Deactivate(int index)
		{
			_creditList[index].SetActive(false);
		}

		private void Refresh()
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_contentSizeFitter.transform);
		}
	}
}
