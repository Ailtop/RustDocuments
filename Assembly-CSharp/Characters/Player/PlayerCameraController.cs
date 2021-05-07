using System.Collections;
using Characters.Movements;
using PhysicsUtils;
using UnityEngine;

namespace Characters.Player
{
	public class PlayerCameraController : MonoBehaviour
	{
		[SerializeField]
		[GetComponent]
		private Character _character;

		[SerializeField]
		private Camera _deathCamera;

		private readonly LineSequenceNonAllocCaster _lineCaster = new LineSequenceNonAllocCaster(1, 2);

		private Collider2D _ground;

		private CharacterController2D.CollisionState _collisionState;

		private Vector3 _trackPosition;

		public Vector3 trackPosition => _trackPosition;

		public float trackSpeed => 7f;

		private void Awake()
		{
			_collisionState = _character.movement.controller.collisionState;
			RayCaster rayCaster = new RayCaster
			{
				direction = Vector2.down,
				distance = 2.5f
			};
			rayCaster.contactFilter.SetLayerMask(393216);
			_lineCaster.caster = rayCaster;
			_character.health.onDied += OnDie;
		}

		private void OnDie()
		{
			CoroutineProxy.instance.StartCoroutine(CRenderDeathCamera());
		}

		private IEnumerator CRenderDeathCamera()
		{
			yield return new WaitForSecondsRealtime(1.6f);
			RenderDeathCamera();
		}

		public void RenderDeathCamera()
		{
			_deathCamera.Render();
		}

		private void Update()
		{
			Bounds bounds = _character.collider.bounds;
			_lineCaster.start = new Vector2(bounds.min.x, bounds.min.y);
			_lineCaster.end = new Vector2(bounds.max.x, bounds.min.y);
			_lineCaster.Cast();
			_trackPosition = base.transform.position;
			_trackPosition.y += 1f;
			RaycastHit2D? raycastHit2D = null;
			ReadonlyBoundedList<RaycastHit2D> results = _lineCaster.nonAllocCasters[0].results;
			ReadonlyBoundedList<RaycastHit2D> results2 = _lineCaster.nonAllocCasters[1].results;
			RaycastHit2D raycastHit2D2 = results[0];
			RaycastHit2D raycastHit2D3 = results2[0];
			bool flag = results.Count > 0;
			bool flag2 = results2.Count > 0;
			if (flag && flag2)
			{
				raycastHit2D = ((raycastHit2D2.distance < raycastHit2D3.distance) ? raycastHit2D2 : raycastHit2D3);
			}
			else if (flag && !flag2)
			{
				raycastHit2D = raycastHit2D2;
			}
			else if (!flag && flag2)
			{
				raycastHit2D = raycastHit2D3;
			}
			if (raycastHit2D.HasValue)
			{
				_ground = raycastHit2D.Value.collider;
				if (_ground.bounds.size.x > 6f)
				{
					_trackPosition.y = _ground.bounds.max.y + 2.5f;
				}
			}
		}
	}
}
