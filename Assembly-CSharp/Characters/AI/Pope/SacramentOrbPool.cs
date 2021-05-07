using UnityEngine;

namespace Characters.AI.Pope
{
	public class SacramentOrbPool : MonoBehaviour
	{
		[SerializeField]
		private Transform _leftTop;

		[SerializeField]
		private SacramentOrb _orbPrefab;

		[SerializeField]
		[Information("홀수", InformationAttribute.InformationType.Info, false)]
		private int _width = 5;

		[SerializeField]
		private int _height = 5;

		[SerializeField]
		private float _distance = 5f;

		[SerializeField]
		private float _noise = 2f;

		private Vector3[] _originPositions;

		public void Initialize(Character character)
		{
			_originPositions = new Vector3[_width * _height];
			for (int i = 0; i < _height; i++)
			{
				for (int j = 0; j < _width; j++)
				{
					SacramentOrb sacramentOrb = Object.Instantiate(_orbPrefab, base.transform);
					sacramentOrb.transform.position = new Vector2(_leftTop.position.x + _distance * (float)j, _leftTop.position.y + _distance * (float)i);
					sacramentOrb.Initialize(character);
					sacramentOrb.gameObject.SetActive(false);
					_originPositions[_width * i + j] = sacramentOrb.transform.position;
				}
			}
		}

		public void Run()
		{
			int num = 0;
			foreach (Transform item in base.transform)
			{
				item.position = _originPositions[num++];
				item.Translate(Random.insideUnitSphere * _noise);
				item.gameObject.SetActive(true);
			}
		}

		public void Hide()
		{
			foreach (Transform item in base.transform)
			{
				item.gameObject.SetActive(false);
			}
		}
	}
}
