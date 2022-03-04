using System;
using UnityEngine;

namespace Rust.Modular
{
	[Serializable]
	public class ConditionalObject
	{
		public enum AdjacentCondition
		{
			SameInFront = 0,
			SameBehind = 1,
			DifferentInFront = 2,
			DifferentBehind = 3,
			BothDifferent = 4,
			BothSame = 5
		}

		public enum AdjacentMatchType
		{
			GroupOrExact = 0,
			ExactOnly = 1,
			GroupNotExact = 2
		}

		public GameObject gameObject;

		public GameObject ownerGameObject;

		public ConditionalSocketSettings[] socketSettings;

		public bool restrictOnHealth;

		public float healthRestrictionMin;

		public float healthRestrictionMax;

		public bool restrictOnAdjacent;

		public AdjacentCondition adjacentRestriction;

		public AdjacentMatchType adjacentMatch;

		public bool restrictOnLockable;

		public bool lockableRestriction;

		public bool? IsActive { get; private set; }

		public ConditionalObject(GameObject conditionalGO, GameObject ownerGO, int socketsTaken)
		{
			gameObject = conditionalGO;
			ownerGameObject = ownerGO;
			socketSettings = new ConditionalSocketSettings[socketsTaken];
		}

		public void SetActive(bool active)
		{
			if (!IsActive.HasValue || active != IsActive.Value)
			{
				gameObject.SetActive(active);
				IsActive = active;
			}
		}

		public void RefreshActive()
		{
			if (IsActive.HasValue)
			{
				gameObject.SetActive(IsActive.Value);
			}
		}
	}
}
