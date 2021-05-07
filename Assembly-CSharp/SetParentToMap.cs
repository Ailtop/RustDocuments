using Level;
using UnityEngine;

public class SetParentToMap : MonoBehaviour
{
	private void Start()
	{
		base.transform.SetParent(Map.Instance.transform);
	}
}
