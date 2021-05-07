using UnityEngine;

public class DeactivateOnAwake : MonoBehaviour
{
	private void Awake()
	{
		base.gameObject.SetActive(false);
	}
}
