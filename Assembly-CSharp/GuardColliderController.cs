using Characters;
using Characters.AI;
using UnityEngine;

public class GuardColliderController : MonoBehaviour
{
	[SerializeField]
	private Collider2D _collider;

	[SerializeField]
	private AIController _ai;

	private Character _owner;

	private void Awake()
	{
		_owner = _ai.character;
	}

	private void Update()
	{
		if (_ai.target == null)
		{
			return;
		}
		if (_ai.stuned)
		{
			_collider.gameObject.SetActive(false);
			base.gameObject.SetActive(false);
		}
		if (_owner.lookingDirection == Character.LookingDirection.Right)
		{
			if (_ai.target.transform.position.x < base.transform.position.x)
			{
				_collider.enabled = false;
			}
			else
			{
				_collider.enabled = true;
			}
		}
		else if (_ai.target.transform.position.x > base.transform.position.x)
		{
			_collider.enabled = false;
		}
		else
		{
			_collider.enabled = true;
		}
	}
}
