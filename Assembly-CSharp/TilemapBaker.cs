using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapBaker : MonoBehaviour
{
	private static readonly List<Vector2> _pathesCache = new List<Vector2>(32);

	[SerializeField]
	[GetComponent]
	private Tilemap _tilemap;

	[SerializeField]
	[GetComponent]
	private TilemapCollider2D _tilemapCollider;

	[SerializeField]
	[GetComponent]
	private CompositeCollider2D _compositeCollider;

	[SerializeField]
	[GetComponent]
	private Rigidbody2D _rigidbody;

	private Bounds _bounds;

	public Bounds bounds => _bounds;

	private void Bake(CustomColliderTile.ColliderFilter filter, int layer)
	{
		CustomColliderTile.colliderFilter = filter;
		_tilemapCollider.usedByComposite = false;
		_tilemap.RefreshAllTiles();
		if (_tilemapCollider.bounds.size != Vector3.zero)
		{
			if (_bounds.size == Vector3.zero)
			{
				_bounds = _tilemapCollider.bounds;
			}
			else
			{
				_bounds.max = Vector3.Max(_bounds.max, _tilemapCollider.bounds.max);
				_bounds.min = Vector3.Min(_bounds.min, _tilemapCollider.bounds.min);
			}
		}
		_tilemapCollider.usedByComposite = true;
		_compositeCollider.GenerateGeometry();
		GameObject gameObject = new GameObject(filter.ToString());
		gameObject.transform.parent = base.transform;
		gameObject.transform.position = Vector3.zero;
		gameObject.layer = layer;
		gameObject.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
		for (int i = 0; i < _compositeCollider.shapeCount; i++)
		{
			int path = _compositeCollider.GetPath(i, _pathesCache);
			GameObject obj = new GameObject(filter.ToString());
			obj.transform.parent = gameObject.transform;
			obj.transform.position = Vector3.zero;
			obj.layer = layer;
			obj.AddComponent<PolygonCollider2D>().points = _pathesCache.Take(path).ToArray();
		}
	}

	private void FillPadding(int amount)
	{
		_003C_003Ec__DisplayClass9_0 _003C_003Ec__DisplayClass9_ = default(_003C_003Ec__DisplayClass9_0);
		_003C_003Ec__DisplayClass9_._003C_003E4__this = this;
		_003C_003Ec__DisplayClass9_.amount = amount;
		_tilemap.CompressBounds();
		Bounds localBounds = _tilemap.localBounds;
		Vector2Int vector2Int = new Vector2Int((int)localBounds.min.x, (int)localBounds.min.y);
		Vector2Int vector2Int2 = new Vector2Int((int)localBounds.max.x - 1, (int)localBounds.max.y - 1);
		for (int i = vector2Int.x; i <= vector2Int2.x; i++)
		{
			_003CFillPadding_003Eg__Fill_007C9_0(new Vector3Int(i, vector2Int.y, 0), Vector3Int.down, (CustomColliderTile t) => t.verticallyOpened, ref _003C_003Ec__DisplayClass9_);
			_003CFillPadding_003Eg__Fill_007C9_0(new Vector3Int(i, vector2Int2.y, 0), Vector3Int.up, (CustomColliderTile t) => t.verticallyOpened, ref _003C_003Ec__DisplayClass9_);
		}
		for (int j = vector2Int.y; j <= vector2Int2.y; j++)
		{
			_003CFillPadding_003Eg__Fill_007C9_0(new Vector3Int(vector2Int.x, j, 0), Vector3Int.left, (CustomColliderTile t) => t.horizontallyOpened, ref _003C_003Ec__DisplayClass9_);
			_003CFillPadding_003Eg__Fill_007C9_0(new Vector3Int(vector2Int2.x, j, 0), Vector3Int.right, (CustomColliderTile t) => t.horizontallyOpened, ref _003C_003Ec__DisplayClass9_);
		}
		_003CFillPadding_003Eg__FillCorners_007C9_1(new Vector3Int(vector2Int.x, vector2Int.y, 0), -1, -1, ref _003C_003Ec__DisplayClass9_);
		_003CFillPadding_003Eg__FillCorners_007C9_1(new Vector3Int(vector2Int2.x, vector2Int.y, 0), 1, -1, ref _003C_003Ec__DisplayClass9_);
		_003CFillPadding_003Eg__FillCorners_007C9_1(new Vector3Int(vector2Int.x, vector2Int2.y, 0), -1, 1, ref _003C_003Ec__DisplayClass9_);
		_003CFillPadding_003Eg__FillCorners_007C9_1(new Vector3Int(vector2Int2.x, vector2Int2.y, 0), 1, 1, ref _003C_003Ec__DisplayClass9_);
	}

	public void Bake()
	{
		_rigidbody.bodyType = RigidbodyType2D.Static;
		_compositeCollider.generationType = CompositeCollider2D.GenerationType.Manual;
		Bake(CustomColliderTile.ColliderFilter.Terrain, 8);
		Bake(CustomColliderTile.ColliderFilter.TerrainFoothold, 18);
		Bake(CustomColliderTile.ColliderFilter.PlatformProjectileBlock, 19);
		Bake(CustomColliderTile.ColliderFilter.PlatformFoothold, 17);
		FillPadding(3);
		Object.Destroy(_compositeCollider);
		Object.Destroy(_tilemapCollider);
		Object.Destroy(_rigidbody);
		Object.Destroy(this);
	}
}
