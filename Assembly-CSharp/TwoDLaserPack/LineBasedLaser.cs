using System;
using System.Collections;
using Characters;
using UnityEngine;

namespace TwoDLaserPack
{
	public class LineBasedLaser : MonoBehaviour
	{
		public delegate void LaserHitTriggerHandler(RaycastHit2D hitInfo);

		[SerializeField]
		private Character _character;

		public LineRenderer laserLineRendererArc;

		public LineRenderer laserLineRenderer;

		public int laserArcSegments = 20;

		public bool laserActive;

		public bool ignoreCollisions;

		public GameObject targetGo;

		public float laserTexOffsetSpeed = 1f;

		public ParticleSystem hitSparkParticleSystem;

		[SerializeField]
		private PoolObject laserhitEffect;

		private PoolObject _hitEffect;

		public float laserArcMaxYDown;

		public float laserArcMaxYUp;

		public float maxLaserRaycastDistance = 20f;

		public bool laserRotationEnabled;

		public bool lerpLaserRotation;

		public float turningRate = 3f;

		public float collisionTriggerInterval = 0.25f;

		public LayerMask mask;

		public string sortLayer = "Default";

		public int sortOrder;

		public bool useArc;

		private GameObject gameObjectCached;

		private float laserAngle;

		private float laserTextureOffset;

		private float laserTextureXScale;

		private float startLaserTextureXScale;

		private int startLaserSegmentLength;

		private bool waitingForTriggerTime;

		private ParticleSystem.EmissionModule hitSparkEmission;

		public event LaserHitTriggerHandler OnLaserHitTriggered;

		private void Awake()
		{
			hitSparkEmission = hitSparkParticleSystem.emission;
		}

		private void Start()
		{
			startLaserTextureXScale = laserLineRenderer.material.mainTextureScale.x;
			startLaserSegmentLength = laserArcSegments;
			laserLineRenderer.sortingLayerName = sortLayer;
			laserLineRenderer.sortingOrder = sortOrder;
			laserLineRendererArc.sortingLayerName = sortLayer;
			laserLineRendererArc.sortingOrder = sortOrder;
		}

		private void OnEnable()
		{
			gameObjectCached = base.gameObject;
			if (laserLineRendererArc != null)
			{
				laserLineRendererArc.SetVertexCount(laserArcSegments);
			}
		}

		private void Update()
		{
			if (!(gameObjectCached != null) || !laserActive)
			{
				return;
			}
			laserLineRenderer.material.mainTextureOffset = new Vector2(laserTextureOffset, 0f);
			laserTextureOffset -= _character.chronometer.master.deltaTime * laserTexOffsetSpeed;
			RaycastHit2D raycastHit2D;
			if (laserRotationEnabled && targetGo != null)
			{
				Vector3 vector = targetGo.transform.position - gameObjectCached.transform.position;
				laserAngle = Mathf.Atan2(vector.y, vector.x);
				if (laserAngle < 0f)
				{
					laserAngle = (float)Math.PI * 2f + laserAngle;
				}
				float angle = laserAngle * 57.29578f;
				if (lerpLaserRotation)
				{
					base.transform.rotation = Quaternion.Slerp(base.transform.rotation, Quaternion.AngleAxis(angle, base.transform.forward), _character.chronometer.master.deltaTime * turningRate);
					Vector3 vector2 = base.transform.rotation * Vector3.right;
					raycastHit2D = Physics2D.Raycast(base.transform.position, vector2, maxLaserRaycastDistance, mask);
				}
				else
				{
					base.transform.rotation = Quaternion.AngleAxis(angle, base.transform.forward);
					raycastHit2D = Physics2D.Raycast(base.transform.position, vector, maxLaserRaycastDistance, mask);
				}
			}
			else
			{
				raycastHit2D = Physics2D.Raycast(base.transform.position, base.transform.right, maxLaserRaycastDistance, mask);
			}
			if (!ignoreCollisions)
			{
				if (raycastHit2D.collider != null)
				{
					SetLaserEndToTargetLocation(raycastHit2D);
					if (!waitingForTriggerTime)
					{
						StartCoroutine(HitTrigger(collisionTriggerInterval, raycastHit2D));
					}
				}
				else
				{
					SetLaserToDefaultLength();
				}
			}
			else
			{
				SetLaserToDefaultLength();
			}
		}

		private IEnumerator HitTrigger(float triggerInterval, RaycastHit2D hit)
		{
			waitingForTriggerTime = true;
			this.OnLaserHitTriggered?.Invoke(hit);
			yield return _character.chronometer.master.WaitForSeconds(triggerInterval);
			waitingForTriggerTime = false;
		}

		public void SetLaserState(bool enabledStatus)
		{
			laserActive = enabledStatus;
			laserLineRenderer.enabled = enabledStatus;
			if (laserLineRendererArc != null)
			{
				laserLineRendererArc.enabled = enabledStatus;
			}
			if (hitSparkParticleSystem != null)
			{
				hitSparkEmission.enabled = enabledStatus;
			}
		}

		private void SetLaserEndToTargetLocation(RaycastHit2D hit)
		{
			float num = Vector2.Distance(hit.point, laserLineRenderer.transform.position);
			laserLineRenderer.SetPosition(1, new Vector2(num, 0f));
			laserTextureXScale = startLaserTextureXScale * num;
			laserLineRenderer.material.mainTextureScale = new Vector2(laserTextureXScale, 1f);
			if (useArc)
			{
				if (!laserLineRendererArc.enabled)
				{
					laserLineRendererArc.enabled = true;
				}
				int vertexCount = Mathf.Abs((int)num);
				laserLineRendererArc.SetVertexCount(vertexCount);
				laserArcSegments = vertexCount;
				SetLaserArcVertices(num, true);
			}
			else if (laserLineRendererArc.enabled)
			{
				laserLineRendererArc.enabled = false;
			}
			if (hitSparkParticleSystem != null)
			{
				hitSparkParticleSystem.transform.position = hit.point;
				if (_hitEffect == null)
				{
					laserhitEffect.Spawn(hit.point);
				}
				_hitEffect.transform.position = hit.point;
				hitSparkEmission.enabled = true;
			}
		}

		private void SetLaserToDefaultLength()
		{
			laserLineRenderer.SetPosition(1, new Vector2(laserArcSegments, 0f));
			laserTextureXScale = startLaserTextureXScale * (float)laserArcSegments;
			laserLineRenderer.material.mainTextureScale = new Vector2(laserTextureXScale, 1f);
			if (useArc)
			{
				if (!laserLineRendererArc.enabled)
				{
					laserLineRendererArc.enabled = true;
				}
				laserLineRendererArc.SetVertexCount(startLaserSegmentLength);
				laserArcSegments = startLaserSegmentLength;
				SetLaserArcVertices(0f, false);
			}
			else
			{
				if (laserLineRendererArc.enabled)
				{
					laserLineRendererArc.enabled = false;
				}
				laserLineRendererArc.SetVertexCount(startLaserSegmentLength);
				laserArcSegments = startLaserSegmentLength;
			}
			if (hitSparkParticleSystem != null)
			{
				hitSparkEmission.enabled = false;
				hitSparkParticleSystem.transform.position = new Vector2(laserArcSegments, base.transform.position.y);
			}
		}

		private void SetLaserArcVertices(float distancePoint, bool useHitPoint)
		{
			for (int i = 1; i < laserArcSegments; i++)
			{
				float y = Mathf.Clamp(Mathf.Sin((float)i + Time.time * UnityEngine.Random.Range(0.5f, 1.3f)), laserArcMaxYDown, laserArcMaxYUp);
				Vector2 vector = new Vector2((float)i * 1.2f, y);
				if (useHitPoint && i == laserArcSegments - 1)
				{
					laserLineRendererArc.SetPosition(i, new Vector2(distancePoint, 0f));
				}
				else
				{
					laserLineRendererArc.SetPosition(i, vector);
				}
			}
		}
	}
}
