namespace UnityEngine.UI;

public class ScrollRectSettable : ScrollRect
{
	public void SetHorizNormalizedPosition(float value)
	{
		SetNormalizedPosition(value, 0);
	}

	public void SetVertNormalizedPosition(float value)
	{
		SetNormalizedPosition(value, 1);
	}
}
