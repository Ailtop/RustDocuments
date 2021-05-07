using System;
using Characters;
using Characters.Utils;
using PhysicsUtils;
using UnityEngine;

[Serializable]
public class CollisionDetector
{
	public delegate void onTerrainHitDelegate(Collider2D collider, Vector2 origin, Vector2 direction, float distance, RaycastHit2D raycastHit);

	public delegate void onTargetHitDelegate(Collider2D collider, Vector2 origin, Vector2 direction, float distance, RaycastHit2D raycastHit, Target target);

	private const int maxHits = 99;

	private GameObject _owner;

	[SerializeField]
	private TargetLayer _layer = new TargetLayer(2048, false, true, false, false);

	[SerializeField]
	private LayerMask _terrainLayer = Layers.groundMask;

	[SerializeField]
	private Collider2D _collider;

	[SerializeField]
	[Range(1f, 99f)]
	private int _maxHits = 15;

	[SerializeField]
	private int _maxHitsPerUnit = 1;

	[SerializeField]
	private float _hitIntervalPerUnit = 0.5f;

	private HitHistoryManager _hits = new HitHistoryManager(99);

	private int _propPenetratingHits;

	private ContactFilter2D _filter;

	private static readonly NonAllocCaster _caster;

	public LayerMask layerMask
	{
		get
		{
			return _filter.layerMask;
		}
		set
		{
			_filter.layerMask = value;
		}
	}

	public event onTerrainHitDelegate onTerrainHit;

	public event onTargetHitDelegate onHit;

	public event Action onStop;

	static CollisionDetector()
	{
		_caster = new NonAllocCaster(99);
	}

	internal void Initialize()
	{
		_hits.Clear();
		_propPenetratingHits = 0;
		if (_collider != null)
		{
			_collider.enabled = false;
		}
		if (_maxHitsPerUnit == 0)
		{
			_maxHitsPerUnit = int.MaxValue;
		}
	}

	internal void Detect(Vector2 origin, Vector2 distance)
	{
		Detect(origin, distance.normalized, distance.magnitude);
	}

	internal void Detect(Vector2 origin, Vector2 direction, float distance)
	{
		_caster.contactFilter.SetLayerMask(_filter.layerMask);
		_caster.RayCast(origin, direction, distance);
		if ((bool)_collider)
		{
			_collider.enabled = true;
			_caster.ColliderCast(_collider, direction, distance);
			_collider.enabled = false;
		}
		else
		{
			_caster.RayCast(origin, direction, distance);
		}
		for (int i = 0; i < _caster.results.Count; i++)
		{
			if (_terrainLayer.Contains(_caster.results[i].collider.gameObject.layer))
			{
				this.onTerrainHit(_collider, origin, direction, distance, _caster.results[i]);
			}
			else
			{
				Target component = _caster.results[i].collider.GetComponent<Target>();
				if (!_hits.CanAttack(component, _maxHits, _maxHitsPerUnit, _hitIntervalPerUnit))
				{
					continue;
				}
				if (component.character != null)
				{
					if (!component.character.liveAndActive)
					{
						continue;
					}
					this.onHit(_collider, origin, direction, distance, _caster.results[i], component);
					_hits.AddOrUpdate(component);
				}
				else if (component.damageable != null)
				{
					this.onHit(_collider, origin, direction, distance, _caster.results[i], component);
					if (!component.damageable.blockCast)
					{
						_propPenetratingHits++;
					}
					_hits.AddOrUpdate(component);
				}
			}
			if (_hits.Count - _propPenetratingHits >= _maxHits)
			{
				this.onStop?.Invoke();
			}
		}
	}
}
