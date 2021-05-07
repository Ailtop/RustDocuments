using UnityEngine;

public class ActivateOnEnterZone : MonoBehaviour
{
	[SerializeField]
	private GameObject _target;

	[SerializeField]
	[GetComponent]
	private Collider2D _collider;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		_target.gameObject.SetActive(true);
		Object.Destroy(_collider);
		Object.Destroy(_target, 10f);
	}
}
