using FX;
using UnityEngine;

public class GridLayoutGenerator : MonoBehaviour
{
	[SerializeField]
	private int _width;

	[SerializeField]
	private int _height;

	[SerializeField]
	private float _distanceX;

	[SerializeField]
	private float _distanceY;

	[SerializeField]
	private GameObject _prefab;

	[SerializeField]
	private PositionNoise _positionNoise;

	private void Generate()
	{
		RemoveAll();
		float num = _distanceX * (float)(_width - 1) / 2f;
		for (int i = 0; i < _height; i++)
		{
			for (int j = 0; j < _width; j++)
			{
				Object.Instantiate(_prefab, base.transform).transform.position = new Vector2(base.transform.position.x + _distanceX * (float)j - num, base.transform.position.y + _distanceY * (float)i);
			}
		}
	}

	private void RemoveAll()
	{
		for (int num = base.transform.childCount - 1; num >= 0; num--)
		{
			Object.DestroyImmediate(base.transform.GetChild(num).gameObject);
		}
	}

	public void Shuffle()
	{
		foreach (Transform item in base.transform)
		{
			item.SetSiblingIndex(Random.Range(0, base.transform.childCount - 1));
		}
	}

	public void Noise()
	{
		foreach (Transform item in base.transform)
		{
			item.transform.position += _positionNoise.Evaluate();
		}
	}
}
