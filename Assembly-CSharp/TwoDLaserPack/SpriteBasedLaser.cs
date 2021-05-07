using System;
using System.Collections;
using UnityEngine;

namespace TwoDLaserPack
{
	public class SpriteBasedLaser : MonoBehaviour
	{
		public delegate void LaserHitTriggerHandler(RaycastHit2D hitInfo);

		public GameObject laserStartPiece;

		public GameObject laserMiddlePiece;

		public GameObject laserEndPiece;

		public LineRenderer laserLineRendererArc;

		public int laserArcSegments = 20;

		public RandomPositionMover laserOscillationPositionerScript;

		public bool oscillateLaser;

		public float maxLaserLength = 20f;

		public float oscillationSpeed = 1f;

		public bool laserActive;

		public bool ignoreCollisions;

		public GameObject targetGo;

		public ParticleSystem hitSparkParticleSystem;

		public float laserArcMaxYDown;

		public float laserArcMaxYUp;

		public float maxLaserRaycastDistance;

		public bool laserRotationEnabled;

		public bool lerpLaserRotation;

		public float turningRate = 3f;

		public float collisionTriggerInterval = 0.25f;

		public LayerMask mask;

		public bool useArc;

		public float oscillationThreshold = 0.2f;

		private GameObject gameObjectCached;

		private float laserAngle;

		private float lerpYValue;

		private float startLaserLength;

		private GameObject startGoPiece;

		private GameObject middleGoPiece;

		private GameObject endGoPiece;

		private float startSpriteWidth;

		private bool waitingForTriggerTime;

		private ParticleSystem.EmissionModule hitSparkEmission;

		public event LaserHitTriggerHandler OnLaserHitTriggered;

		private void Awake()
		{
			hitSparkEmission = hitSparkParticleSystem.emission;
		}

		private void OnEnable()
		{
			gameObjectCached = base.gameObject;
			if (laserLineRendererArc != null)
			{
				laserLineRendererArc.SetVertexCount(laserArcSegments);
			}
		}

		private void Start()
		{
			startLaserLength = maxLaserLength;
			if (laserOscillationPositionerScript != null)
			{
				laserOscillationPositionerScript.radius = oscillationThreshold;
			}
		}

		private void OscillateLaserParts(float currentLaserDistance)
		{
			if (!(laserOscillationPositionerScript == null))
			{
				lerpYValue = Mathf.Lerp(middleGoPiece.transform.localPosition.y, laserOscillationPositionerScript.randomPointInCircle.y, Time.deltaTime * oscillationSpeed);
				if (startGoPiece != null && middleGoPiece != null)
				{
					Vector2 vector = Vector2.Lerp(b: new Vector2(startGoPiece.transform.localPosition.x, laserOscillationPositionerScript.randomPointInCircle.y), a: startGoPiece.transform.localPosition, t: Time.deltaTime * oscillationSpeed);
					startGoPiece.transform.localPosition = vector;
					Vector2 vector2 = new Vector2(currentLaserDistance / 2f + startSpriteWidth / 4f, lerpYValue);
					middleGoPiece.transform.localPosition = vector2;
				}
				if (endGoPiece != null)
				{
					Vector2 vector3 = new Vector2(currentLaserDistance + startSpriteWidth / 2f, lerpYValue);
					endGoPiece.transform.localPosition = vector3;
				}
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

		private void Update()
		{
			if (!(gameObjectCached != null) || !laserActive)
			{
				return;
			}
			if (startGoPiece == null)
			{
				InstantiateLaserPart(ref startGoPiece, laserStartPiece);
				startGoPiece.transform.parent = base.transform;
				startGoPiece.transform.localPosition = Vector2.zero;
				startSpriteWidth = laserStartPiece.GetComponent<Renderer>().bounds.size.x;
			}
			if (middleGoPiece == null)
			{
				InstantiateLaserPart(ref middleGoPiece, laserMiddlePiece);
				middleGoPiece.transform.parent = base.transform;
				middleGoPiece.transform.localPosition = Vector2.zero;
			}
			middleGoPiece.transform.localScale = new Vector3(maxLaserLength - startSpriteWidth + 0.2f, middleGoPiece.transform.localScale.y, middleGoPiece.transform.localScale.z);
			if (oscillateLaser)
			{
				OscillateLaserParts(maxLaserLength);
			}
			else
			{
				if (middleGoPiece != null)
				{
					middleGoPiece.transform.localPosition = new Vector2(maxLaserLength / 2f + startSpriteWidth / 4f, lerpYValue);
				}
				if (endGoPiece != null)
				{
					endGoPiece.transform.localPosition = new Vector2(maxLaserLength + startSpriteWidth / 2f, 0f);
				}
			}
			RaycastHit2D hit;
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
					base.transform.rotation = Quaternion.Slerp(base.transform.rotation, Quaternion.AngleAxis(angle, base.transform.forward), Time.deltaTime * turningRate);
					Vector3 vector2 = base.transform.rotation * Vector3.right;
					hit = Physics2D.Raycast(base.transform.position, vector2, maxLaserRaycastDistance, mask);
				}
				else
				{
					base.transform.rotation = Quaternion.AngleAxis(angle, base.transform.forward);
					hit = Physics2D.Raycast(base.transform.position, vector, maxLaserRaycastDistance, mask);
				}
			}
			else
			{
				hit = Physics2D.Raycast(base.transform.position, base.transform.right, maxLaserRaycastDistance, mask);
			}
			if (!ignoreCollisions)
			{
				if (hit.collider != null)
				{
					maxLaserLength = Vector2.Distance(hit.point, base.transform.position) + startSpriteWidth / 4f;
					InstantiateLaserPart(ref endGoPiece, laserEndPiece);
					if (hitSparkParticleSystem != null)
					{
						hitSparkParticleSystem.transform.position = hit.point;
						hitSparkEmission.enabled = true;
					}
					if (useArc)
					{
						if (!laserLineRendererArc.enabled)
						{
							laserLineRendererArc.enabled = true;
						}
						SetLaserArcVertices(maxLaserLength, true);
						SetLaserArcSegmentLength();
					}
					else if (laserLineRendererArc.enabled)
					{
						laserLineRendererArc.enabled = false;
					}
					if (!waitingForTriggerTime)
					{
						StartCoroutine(HitTrigger(collisionTriggerInterval, hit));
					}
					return;
				}
				SetLaserBackToDefaults();
				if (useArc)
				{
					if (!laserLineRendererArc.enabled)
					{
						laserLineRendererArc.enabled = true;
					}
					SetLaserArcSegmentLength();
					SetLaserArcVertices(0f, false);
				}
				else if (laserLineRendererArc.enabled)
				{
					laserLineRendererArc.enabled = false;
				}
			}
			else
			{
				SetLaserBackToDefaults();
				SetLaserArcVertices(0f, false);
				SetLaserArcSegmentLength();
			}
		}

		private IEnumerator HitTrigger(float triggerInterval, RaycastHit2D hit)
		{
			waitingForTriggerTime = true;
			if (this.OnLaserHitTriggered != null)
			{
				this.OnLaserHitTriggered(hit);
			}
			yield return new WaitForSeconds(triggerInterval);
			waitingForTriggerTime = false;
		}

		public void SetLaserState(bool enabledStatus)
		{
			laserActive = enabledStatus;
			if (startGoPiece != null)
			{
				startGoPiece.SetActive(enabledStatus);
			}
			if (middleGoPiece != null)
			{
				middleGoPiece.SetActive(enabledStatus);
			}
			if (endGoPiece != null)
			{
				endGoPiece.SetActive(enabledStatus);
			}
			if (laserLineRendererArc != null)
			{
				laserLineRendererArc.enabled = enabledStatus;
			}
			if (hitSparkParticleSystem != null)
			{
				hitSparkEmission.enabled = enabledStatus;
			}
		}

		private void SetLaserArcSegmentLength()
		{
			int vertexCount = Mathf.Abs((int)maxLaserLength);
			laserLineRendererArc.SetVertexCount(vertexCount);
			laserArcSegments = vertexCount;
		}

		private void SetLaserBackToDefaults()
		{
			UnityEngine.Object.Destroy(endGoPiece);
			maxLaserLength = startLaserLength;
			if (hitSparkParticleSystem != null)
			{
				hitSparkEmission.enabled = false;
				hitSparkParticleSystem.transform.position = new Vector2(maxLaserLength, base.transform.position.y);
			}
		}

		private void InstantiateLaserPart(ref GameObject laserComponent, GameObject laserPart)
		{
			if (laserComponent == null)
			{
				laserComponent = UnityEngine.Object.Instantiate(laserPart);
				laserComponent.transform.parent = base.gameObject.transform;
				laserComponent.transform.localPosition = Vector2.zero;
				laserComponent.transform.localEulerAngles = Vector2.zero;
			}
		}

		public void DisableLaserGameObjectComponents()
		{
			UnityEngine.Object.Destroy(startGoPiece);
			UnityEngine.Object.Destroy(middleGoPiece);
			UnityEngine.Object.Destroy(endGoPiece);
		}
	}
}
