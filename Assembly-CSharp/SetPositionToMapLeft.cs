using Level;
using UnityEngine;

public class SetPositionToMapLeft : MonoBehaviour
{
	private void Start()
	{
		base.transform.position = new Vector2(Map.Instance.bounds.min.x, Map.Instance.bounds.max.y);
	}
}
