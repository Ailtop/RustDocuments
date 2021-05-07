using PhysicsUtils;
using UnityEngine;

public class SpriteLaser : MonoBehaviour
{
	[SerializeField]
	private float _minWidth = 2f;

	[SerializeField]
	private float _maxWidth = 40f;

	[SerializeField]
	private float _minHeight = 25f / 32f;

	[SerializeField]
	private LayerMask _terrainMask;

	[SerializeField]
	private Transform _firePosition;

	[SerializeField]
	private float _maxDistance = 30f;

	[SerializeField]
	private Transform _body;

	[SerializeField]
	private Transform _hitEffect;

	private static NonAllocCaster _laycaster;

	static SpriteLaser()
	{
		_laycaster = new NonAllocCaster(15);
	}

	private void Update()
	{
		_laycaster.contactFilter.SetLayerMask(_terrainMask);
		_laycaster.RayCast(_firePosition.position, Vector2.down, _maxDistance);
		ReadonlyBoundedList<RaycastHit2D> results = _laycaster.results;
		if (results.Count < 0)
		{
			_hitEffect.gameObject.SetActive(false);
			return;
		}
		int index = 0;
		float num = results[0].distance;
		for (int i = 1; i < results.Count; i++)
		{
			float distance = results[i].distance;
			if (distance < num)
			{
				num = distance;
				index = i;
			}
		}
		RaycastHit2D raycastHit2D = results[index];
		_body.localScale = new Vector2(1f, _firePosition.transform.position.y - raycastHit2D.point.y);
		_hitEffect.transform.position = raycastHit2D.point;
		_hitEffect.gameObject.SetActive(true);
	}
}
