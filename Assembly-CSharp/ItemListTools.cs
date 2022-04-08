using System.Linq;
using Rust.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemListTools : MonoBehaviour
{
	public GameObject categoryButton;

	public GameObject itemButton;

	public RustInput searchInputText;

	internal Button lastCategory;

	private IOrderedEnumerable<ItemDefinition> currentItems;

	private IOrderedEnumerable<ItemDefinition> allItems;

	public void OnPanelOpened()
	{
		CacheAllItems();
		Refresh();
		searchInputText.InputField.ActivateInputField();
	}

	private void OnOpenDevTools()
	{
		searchInputText.InputField.ActivateInputField();
	}

	private void CacheAllItems()
	{
		if (allItems == null)
		{
			allItems = from x in ItemManager.GetItemDefinitions()
				orderby x.displayName.translated
				select x;
		}
	}

	public void Refresh()
	{
		RebuildCategories();
	}

	private void RebuildCategories()
	{
		for (int i = 0; i < categoryButton.transform.parent.childCount; i++)
		{
			Transform child = categoryButton.transform.parent.GetChild(i);
			if (!(child == categoryButton.transform))
			{
				GameManager.Destroy(child.gameObject);
			}
		}
		categoryButton.SetActive(value: true);
		foreach (IGrouping<ItemCategory, ItemDefinition> item in from x in ItemManager.GetItemDefinitions()
			group x by x.category into x
			orderby x.First().category
			select x)
		{
			GameObject gameObject = Object.Instantiate(categoryButton);
			gameObject.transform.SetParent(categoryButton.transform.parent, worldPositionStays: false);
			gameObject.GetComponentInChildren<TextMeshProUGUI>().text = item.First().category.ToString();
			Button btn = gameObject.GetComponentInChildren<Button>();
			ItemDefinition[] itemArray = item.ToArray();
			btn.onClick.AddListener(delegate
			{
				if ((bool)lastCategory)
				{
					lastCategory.interactable = true;
				}
				lastCategory = btn;
				lastCategory.interactable = false;
				SwitchItemCategory(itemArray);
			});
			if (lastCategory == null)
			{
				lastCategory = btn;
				lastCategory.interactable = false;
				SwitchItemCategory(itemArray);
			}
		}
		categoryButton.SetActive(value: false);
	}

	private void SwitchItemCategory(ItemDefinition[] defs)
	{
		currentItems = defs.OrderBy((ItemDefinition x) => x.displayName.translated);
		searchInputText.Text = "";
		FilterItems(null);
	}

	public void FilterItems(string searchText)
	{
		if (itemButton == null)
		{
			return;
		}
		for (int i = 0; i < itemButton.transform.parent.childCount; i++)
		{
			Transform child = itemButton.transform.parent.GetChild(i);
			if (!(child == itemButton.transform))
			{
				GameManager.Destroy(child.gameObject);
			}
		}
		itemButton.SetActive(value: true);
		bool flag = !string.IsNullOrEmpty(searchText);
		string value = (flag ? searchText.ToLower() : null);
		IOrderedEnumerable<ItemDefinition> obj = (flag ? allItems : currentItems);
		int num = 0;
		foreach (ItemDefinition item in obj)
		{
			if (!item.hidden && (!flag || item.displayName.translated.ToLower().Contains(value)))
			{
				GameObject obj2 = Object.Instantiate(itemButton);
				obj2.transform.SetParent(itemButton.transform.parent, worldPositionStays: false);
				obj2.GetComponentInChildren<TextMeshProUGUI>().text = item.displayName.translated;
				obj2.GetComponentInChildren<ItemButtonTools>().itemDef = item;
				obj2.GetComponentInChildren<ItemButtonTools>().image.sprite = item.iconSprite;
				num++;
				if (num >= 160)
				{
					break;
				}
			}
		}
		itemButton.SetActive(value: false);
	}
}
