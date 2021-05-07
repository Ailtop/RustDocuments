using System.Collections;
using System.Text;
using Characters.Gear.Synergy.Keywords;
using Characters.Player;
using Services;
using Singletons;
using TMPro;
using UnityEngine;

public class SynergyDisplay : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _text;

	private Inventory _inventory;

	private readonly StringBuilder _stringBuilder = new StringBuilder();

	private IEnumerator Start()
	{
		while (Singleton<Service>.Instance.levelManager.player == null)
		{
			yield return null;
		}
		_inventory = Singleton<Service>.Instance.levelManager.player.playerComponents.inventory;
		_inventory.onUpdated += UpdateText;
		UpdateText();
	}

	private void UpdateText()
	{
		EnumArray<Keyword.Key, int> keywordCounts = _inventory.synergy.keywordCounts;
		_stringBuilder.Clear();
		Keyword.Key[] values = EnumValues<Keyword.Key>.Values;
		foreach (Keyword.Key key in values)
		{
			int num = keywordCounts[key];
			if (num != 0)
			{
				Keyword keyword = _inventory.synergy.keywordComponents[key];
				_stringBuilder.AppendFormat("{0} ({1})\n", key.GetName(), num);
				if (keyword == null)
				{
					Debug.Log(key);
				}
				if (keyword.level != 0)
				{
					_stringBuilder.AppendLine();
				}
			}
		}
		_text.text = _stringBuilder.ToString();
	}
}
