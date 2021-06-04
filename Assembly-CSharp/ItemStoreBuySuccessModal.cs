using System;
using Rust.UI;
using UnityEngine;

public class ItemStoreBuySuccessModal : MonoBehaviour
{
	public void Show(ulong orderId)
	{
		base.gameObject.SetActive(true);
		GetComponent<CanvasGroup>().alpha = 0f;
		LeanTween.alphaCanvas(GetComponent<CanvasGroup>(), 1f, 0.1f);
		bool flag = SingletonComponent<SteamInventoryManager>.Instance != null;
	}

	public void Hide()
	{
		LeanTween.alphaCanvas(GetComponent<CanvasGroup>(), 0f, 0.2f).setOnComplete((Action)delegate
		{
			base.gameObject.SetActive(false);
		});
	}
}
