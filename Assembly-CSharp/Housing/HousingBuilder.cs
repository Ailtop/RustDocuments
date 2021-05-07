using Data;
using UnityEngine;

namespace Housing
{
	public class HousingBuilder : MonoBehaviour
	{
		[SerializeField]
		private BuildLevel _entry;

		public BuildLevel Entry
		{
			get
			{
				return _entry;
			}
			set
			{
				_entry = value;
			}
		}

		private void Start()
		{
			Build();
		}

		private void Build()
		{
			int housingPoint = GameData.Progress.housingPoint;
			int housingSeen = GameData.Progress.housingSeen;
			_entry.Build(housingPoint, housingSeen);
			if (housingPoint != housingSeen)
			{
				UpdateHousingSeen(housingPoint);
			}
		}

		private void UpdateHousingSeen(int housingSeen)
		{
			GameData.Progress.housingSeen = housingSeen;
			GameData.Progress.SaveAll();
		}

		public BuildLevel GetLevelAfterPoint(int targetOrder)
		{
			return _entry.GetLevelAfterPoint(targetOrder);
		}
	}
}
