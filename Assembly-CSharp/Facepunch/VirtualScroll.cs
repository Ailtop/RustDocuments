using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Facepunch;

public class VirtualScroll : MonoBehaviour
{
	public interface IDataSource
	{
		int GetItemCount();

		void SetItemData(int i, GameObject obj);
	}

	public int ItemHeight = 40;

	public int ItemSpacing = 10;

	public RectOffset Padding;

	[Tooltip("Optional, we'll try to GetComponent IDataSource from this object on awake")]
	public GameObject DataSourceObject;

	public GameObject SourceObject;

	public ScrollRect ScrollRect;

	private IDataSource dataSource;

	private Dictionary<int, GameObject> ActivePool = new Dictionary<int, GameObject>();

	private Stack<GameObject> InactivePool = new Stack<GameObject>();

	private int BlockHeight => ItemHeight + ItemSpacing;

	public void Awake()
	{
		ScrollRect.onValueChanged.AddListener(OnScrollChanged);
		if (DataSourceObject != null)
		{
			SetDataSource(DataSourceObject.GetComponent<IDataSource>());
		}
	}

	public void OnDestroy()
	{
		ScrollRect.onValueChanged.RemoveListener(OnScrollChanged);
	}

	private void OnScrollChanged(Vector2 pos)
	{
		Rebuild();
	}

	public void SetDataSource(IDataSource source)
	{
		if (dataSource != source)
		{
			dataSource = source;
			FullRebuild();
		}
	}

	public void FullRebuild()
	{
		int[] array = ActivePool.Keys.ToArray();
		foreach (int key in array)
		{
			Recycle(key);
		}
		Rebuild();
	}

	public void DataChanged()
	{
		foreach (KeyValuePair<int, GameObject> item in ActivePool)
		{
			dataSource.SetItemData(item.Key, item.Value);
		}
		Rebuild();
	}

	public void Rebuild()
	{
		if (dataSource == null)
		{
			return;
		}
		int itemCount = dataSource.GetItemCount();
		RectTransform obj = ScrollRect.viewport.GetChild(0) as RectTransform;
		obj.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, BlockHeight * itemCount - ItemSpacing + Padding.top + Padding.bottom);
		int num = Mathf.Max(2, Mathf.CeilToInt(ScrollRect.viewport.rect.height / (float)BlockHeight));
		int num2 = Mathf.FloorToInt((obj.anchoredPosition.y - (float)Padding.top) / (float)BlockHeight);
		int num3 = num2 + num;
		RecycleOutOfRange(num2, num3);
		for (int i = num2; i <= num3; i++)
		{
			if (i >= 0 && i < itemCount)
			{
				BuildItem(i);
			}
		}
	}

	private void RecycleOutOfRange(int startVisible, float endVisible)
	{
		int[] array = (from x in ActivePool.Keys
			where x < startVisible || (float)x > endVisible
			select (x)).ToArray();
		foreach (int key in array)
		{
			Recycle(key);
		}
	}

	private void Recycle(int key)
	{
		GameObject gameObject = ActivePool[key];
		gameObject.SetActive(value: false);
		ActivePool.Remove(key);
		InactivePool.Push(gameObject);
	}

	private void BuildItem(int i)
	{
		if (i >= 0 && !ActivePool.ContainsKey(i))
		{
			GameObject item = GetItem();
			item.SetActive(value: true);
			dataSource.SetItemData(i, item);
			RectTransform obj = item.transform as RectTransform;
			obj.anchorMin = new Vector2(0f, 1f);
			obj.anchorMax = new Vector2(1f, 1f);
			obj.pivot = new Vector2(0.5f, 1f);
			obj.offsetMin = new Vector2(0f, 0f);
			obj.offsetMax = new Vector2(0f, ItemHeight);
			obj.sizeDelta = new Vector2((Padding.left + Padding.right) * -1, ItemHeight);
			obj.anchoredPosition = new Vector2((float)(Padding.left - Padding.right) * 0.5f, -1 * (i * BlockHeight + Padding.top));
			ActivePool[i] = item;
		}
	}

	private GameObject GetItem()
	{
		if (InactivePool.Count == 0)
		{
			GameObject gameObject = Object.Instantiate(SourceObject);
			gameObject.transform.SetParent(ScrollRect.viewport.GetChild(0), worldPositionStays: false);
			gameObject.transform.localScale = Vector3.one;
			gameObject.SetActive(value: false);
			InactivePool.Push(gameObject);
		}
		return InactivePool.Pop();
	}
}
