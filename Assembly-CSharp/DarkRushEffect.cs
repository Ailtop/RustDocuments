using UnityEngine;

public class DarkRushEffect : MonoBehaviour
{
	[SerializeField]
	private Transform _sign;

	[SerializeField]
	private SpriteRenderer _signSpriteRender;

	[SerializeField]
	private Transform _impact;

	[SerializeField]
	private SpriteRenderer _impactSpriteRender;

	public void ShowSign()
	{
		_sign.gameObject.SetActive(true);
	}

	public void HideSign()
	{
		_sign.gameObject.SetActive(false);
	}

	public void SetSignEffectOrder(int order)
	{
		_signSpriteRender.sortingOrder = order;
	}

	public void ShowImpact()
	{
		_impact.gameObject.SetActive(true);
	}

	public void HideImpact()
	{
		_impact.gameObject.SetActive(false);
	}

	public void SetImpactEffectOrder(int order)
	{
		_impactSpriteRender.sortingOrder = order;
	}
}
