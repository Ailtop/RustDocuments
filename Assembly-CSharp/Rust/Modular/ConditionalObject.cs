using System;
using UnityEngine;

namespace Rust.Modular
{
	[Serializable]
	public class ConditionalObject
	{
		public enum AdjacentCondition
		{
			SameInFront,
			SameBehind,
			DifferentInFront,
			DifferentBehind,
			BothDifferent,
			BothSame
		}

		public enum AdjacentMatchType
		{
			GroupOrExact,
			ExactOnly,
			GroupNotExact
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
