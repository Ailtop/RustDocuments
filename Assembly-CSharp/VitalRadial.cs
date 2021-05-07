using UnityEngine;

public class VitalRadial : MonoBehaviour
{
	private void Awake()
	{
		Debug.LogWarning("VitalRadial is obsolete " + base.transform.GetRecursiveName(), base.gameObject);
	}
}
