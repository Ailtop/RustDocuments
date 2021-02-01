using System;
using UnityEngine;
using UnityEngine.UI;

public class TabToggle : MonoBehaviour
{
	public Transform TabHolder;

	public Transform ContentHolder;

	public bool FadeIn;

	public bool FadeOut;

	public void Awake()
	{
		if (!TabHolder)
		{
			return;
		}
		for (int i = 0; i < TabHolder.childCount; i++)
		{
			Button c = TabHolder.GetChild(i).GetComponent<Button>();
			if ((bool)c)
			{
				c.onClick.AddListener(delegate
				{
					SwitchTo(c);
				});
			}
		}
	}

	public void SwitchTo(Button sourceTab)
	{
		string name = sourceTab.transform.name;
		if ((bool)TabHolder)
		{
			for (int i = 0; i < TabHolder.childCount; i++)
			{
				Button component = TabHolder.GetChild(i).GetComponent<Button>();
				if ((bool)component)
				{
					component.interactable = component.name != name;
				}
			}
		}
		if (!ContentHolder)
		{
			return;
		}
		for (int j = 0; j < ContentHolder.childCount; j++)
		{
			Transform child = ContentHolder.GetChild(j);
			if (child.name == name)
			{
				Show(child.gameObject);
			}
			else
			{
				Hide(child.gameObject);
			}
		}
	}

	private void Hide(GameObject go)
	{
		if (!go.activeSelf)
		{
			return;
		}
		CanvasGroup component = go.GetComponent<CanvasGroup>();
		if (FadeOut && (bool)component)
		{
			LeanTween.alphaCanvas(component, 0f, 0.1f).setOnComplete((Action)delegate
			{
				go.SetActive(false);
			});
		}
		else
		{
			go.SetActive(false);
		}
	}

	private void Show(GameObject go)
	{
		if (!go.activeSelf)
		{
			CanvasGroup component = go.GetComponent<CanvasGroup>();
			if (FadeIn && (bool)component)
			{
				component.alpha = 0f;
				LeanTween.alphaCanvas(component, 1f, 0.1f);
			}
			go.SetActive(true);
		}
	}
}
