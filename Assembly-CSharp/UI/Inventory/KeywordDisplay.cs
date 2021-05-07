using System;
using System.Collections.Generic;
using System.Linq;
using Characters.Gear.Synergy.Keywords;
using Services;
using Singletons;
using UnityEngine;
using UserInput;

namespace UI.Inventory
{
	public class KeywordDisplay : MonoBehaviour
	{
		[SerializeField]
		private KeywordElement[] _keywordElements;

		[SerializeField]
		private GameObject _detailFrame;

		[SerializeField]
		private GameObject _viewDetailKeyGuide;

		private bool _needDetail;

		private int _count;

		public void UpdateElements()
		{
			_detailFrame.SetActive(false);
			KeyValuePair<Keyword.Key, int>[] array = (from pair in Singleton<Service>.Instance.levelManager.player.playerComponents.inventory.synergy.keywordCounts.ToKeyValuePairs()
				where pair.Value > 0
				select pair into keywordCount
				orderby keywordCount.Value descending
				select keywordCount).ToArray();
			_count = Math.Min(array.Length, _keywordElements.Length);
			KeywordElement[] keywordElements = _keywordElements;
			for (int i = 0; i < keywordElements.Length; i++)
			{
				keywordElements[i].gameObject.SetActive(false);
			}
			for (int j = 0; j < _count; j++)
			{
				if (j < _keywordElements.Length / 2)
				{
					_keywordElements[j].gameObject.SetActive(true);
				}
				_keywordElements[j].Set(array[j].Key);
			}
			if (_count > _keywordElements.Length / 2)
			{
				_needDetail = true;
				_viewDetailKeyGuide.gameObject.SetActive(true);
			}
			else
			{
				_needDetail = false;
				_viewDetailKeyGuide.gameObject.SetActive(false);
			}
		}

		private void Update()
		{
			if (!_needDetail)
			{
				return;
			}
			if (!_detailFrame.activeSelf && KeyMapper.Map.Quintessence.IsPressed)
			{
				_detailFrame.SetActive(true);
				for (int i = _keywordElements.Length / 2; i < _count; i++)
				{
					_keywordElements[i].gameObject.SetActive(true);
				}
			}
			else if (_detailFrame.activeSelf && !KeyMapper.Map.Quintessence.IsPressed)
			{
				_detailFrame.SetActive(false);
				for (int j = _keywordElements.Length / 2; j < _count; j++)
				{
					_keywordElements[j].gameObject.SetActive(false);
				}
			}
		}
	}
}
