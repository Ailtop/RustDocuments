using UnityEngine;

public class ObjectFlasher : BaseMonoBehaviour
{
	public GameObject enabledObj;

	public GameObject disabledObj;

	public float toggleLength = 1f;

	public float timeOffset;

	public float randomOffset;

	public void Awake()
	{
		InvokeRepeating(Toggle, Random.Range(0f, randomOffset) + timeOffset, toggleLength);
		disabledObj.SetActive(value: false);
		enabledObj.SetActive(value: true);
	}

	public void Toggle()
	{
		enabledObj.SetActive(!enabledObj.activeSelf);
		disabledObj.SetActive(!disabledObj.activeSelf);
	}
}
