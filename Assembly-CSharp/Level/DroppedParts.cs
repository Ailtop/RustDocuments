using System.Collections;
using Characters;
using Characters.Movements;
using FX;
using UnityEngine;

namespace Level
{
	public class DroppedParts : DestructibleObject, IPoolObjectCopiable<DroppedParts>
	{
		public enum Priority
		{
			High,
			Middle,
			Low
		}

		private static int orderInLayerCount;

		[SerializeField]
		private Priority _priority;

		[SerializeField]
		[MinMaxSlider(0f, 30f)]
		private Vector2Int _count = new Vector2Int(1, 1);

		[SerializeField]
		private bool _randomize;

		[SerializeField]
		private bool _collideWithTerrain = true;

		[Information("0이면 영구지속", InformationAttribute.InformationType.Info, false)]
		[SerializeField]
		private float _duration;

		[SerializeField]
		private float _fadeOutDuration;

		[SerializeField]
		private AnimationCurve _fadeOut;

		[SerializeField]
		[GetComponent]
		private PoolObject _poolObject;

		[SerializeField]
		[GetComponent]
		private SpriteRenderer _spriteRenderer;

		[SerializeField]
		[GetComponent]
		private Rigidbody2D _rigidbody;

		[SerializeField]
		[GetComponent]
		private BoxCollider2D _boxCollider;

		[SerializeField]
		[GetComponent]
		private CircleCollider2D _circleCollider;

		[SerializeField]
		private Vector2 _additionalForce = new Vector2(0f, 10f);

		public Priority priority => _priority;

		public Vector2Int count => _count;

		public bool randomize => _randomize;

		public bool collideWithTerrain => _collideWithTerrain;

		public PoolObject poolObject => _poolObject;

		public SpriteRenderer spriteRenderer => _spriteRenderer;

		public override Collider2D collider => _circleCollider;

		public void Copy(DroppedParts to)
		{
			to._spriteRenderer.sprite = _spriteRenderer.sprite;
			to._spriteRenderer.color = _spriteRenderer.color;
			to._spriteRenderer.flipX = _spriteRenderer.flipX;
			to._spriteRenderer.flipY = _spriteRenderer.flipY;
			to._spriteRenderer.sharedMaterial = _spriteRenderer.sharedMaterial;
			to._spriteRenderer.drawMode = _spriteRenderer.drawMode;
			to._spriteRenderer.sortingLayerID = _spriteRenderer.sortingLayerID;
			to._spriteRenderer.sortingOrder = _spriteRenderer.sortingOrder;
			to._spriteRenderer.maskInteraction = _spriteRenderer.maskInteraction;
			to._spriteRenderer.spriteSortPoint = _spriteRenderer.spriteSortPoint;
			to._spriteRenderer.renderingLayerMask = _spriteRenderer.renderingLayerMask;
			to._rigidbody.bodyType = _rigidbody.bodyType;
			to._rigidbody.sharedMaterial = _rigidbody.sharedMaterial;
			to._rigidbody.useAutoMass = _rigidbody.useAutoMass;
			to._rigidbody.constraints = _rigidbody.constraints;
			to._rigidbody.mass = _rigidbody.mass;
			to._rigidbody.drag = _rigidbody.drag;
			to._rigidbody.angularDrag = _rigidbody.angularDrag;
			to._rigidbody.gravityScale = _rigidbody.gravityScale;
			to._boxCollider.enabled = _boxCollider.enabled;
			if (to._boxCollider.enabled)
			{
				to._boxCollider.offset = _boxCollider.offset;
				to._boxCollider.size = _boxCollider.size;
				to._boxCollider.edgeRadius = _boxCollider.edgeRadius;
			}
			to._circleCollider.enabled = _circleCollider.enabled;
			if (to._circleCollider.enabled)
			{
				to._circleCollider.offset = _circleCollider.offset;
				to._circleCollider.radius = _circleCollider.radius;
			}
			to._additionalForce = _additionalForce;
			to.gameObject.layer = base.gameObject.layer;
			to._priority = _priority;
			to._count = _count;
			to._randomize = _randomize;
			to._collideWithTerrain = _collideWithTerrain;
			to._duration = _duration;
			to._fadeOut = _fadeOut;
		}

		private void Awake()
		{
			_rigidbody = GetComponent<Rigidbody2D>();
		}

		public void Initialize(Push push, float multiplier = 1f, bool interpolate = true)
		{
			Vector2 force = Vector2.zero;
			if (push != null && !push.expired)
			{
				force = push.direction * push.totalForce;
			}
			force *= multiplier;
			Initialize(force, multiplier, interpolate);
		}

		public void Initialize(Vector2 force, float multiplier = 1f, bool interpolate = true)
		{
			if (interpolate)
			{
				if (Mathf.Abs(force.x) < 0.66f && Mathf.Abs(force.y) < 0.66f)
				{
					force = Random.insideUnitCircle;
				}
				force.y = Mathf.Abs(force.y);
				force *= multiplier;
				force = Quaternion.AngleAxis(Random.Range(-15f, 15f), Vector3.forward) * force * Random.Range(0.8f, 1.2f);
				force += _additionalForce;
				_rigidbody.AddForce(force * Random.Range(0.5f, 1f), ForceMode2D.Impulse);
				_rigidbody.AddTorque(Random.Range(-0.5f, 0.5f), ForceMode2D.Impulse);
			}
			else
			{
				_rigidbody.AddForce(force, ForceMode2D.Impulse);
			}
			_spriteRenderer.sortingOrder = orderInLayerCount++;
			if (_duration > 0f)
			{
				StartCoroutine(CFadeOut());
			}
		}

		private IEnumerator CFadeOut()
		{
			yield return Chronometer.global.WaitForSeconds(_duration);
			if (_fadeOut.length > 0)
			{
				yield return poolObject.CFadeOut(_spriteRenderer, Chronometer.global, _fadeOut, _duration);
			}
			poolObject.Despawn();
		}

		public override void Hit(Character from, ref Damage damage, Vector2 force)
		{
			if (Mathf.Abs(force.x) < 0.66f && Mathf.Abs(force.y) < 0.66f)
			{
				force = Random.insideUnitCircle;
			}
			force.y = Mathf.Abs(force.y);
			force *= 3f;
			force = Quaternion.AngleAxis(Random.Range(-15f, 15f), Vector3.forward) * force * Random.Range(0.8f, 1.2f);
			if (_rigidbody.IsTouchingLayers())
			{
				force += _additionalForce;
			}
			_rigidbody.AddForce(force * Random.Range(0.5f, 1f), ForceMode2D.Impulse);
			_rigidbody.AddTorque(Random.Range(-0.5f, 0.5f), ForceMode2D.Impulse);
		}
	}
}
