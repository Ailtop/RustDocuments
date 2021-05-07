using TMPro;
using UnityEngine;

public class UISleepingScreen : SingletonComponent<UISleepingScreen>, IUIScreen
{
	protected CanvasGroup canvasGroup;

	private bool visible;

	protected override void Awake()
	{
		base.Awake();
		canvasGroup = GetComponent<CanvasGroup>();
		visible = true;
	}

	public void SetVisible(bool b)
	{
		if (visible != b)
		{
			visible = b;
			canvasGroup.alpha = (visible ? 1f : 0f);
			SingletonComponent<UISleepingScreen>.Instance.gameObject.SetChildComponentsEnabled<TMP_Text>(visible);
		}
	}
}
