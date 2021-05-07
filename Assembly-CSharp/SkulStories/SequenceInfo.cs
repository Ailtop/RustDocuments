using System;
using System.Collections;
using UnityEngine;

namespace SkulStories
{
	public class SequenceInfo : MonoBehaviour
	{
		[Serializable]
		internal class Subcomponents : SubcomponentArray<SequenceInfo>
		{
			internal IEnumerator CRun()
			{
				for (int operationIndex = 0; operationIndex < base.components.Length; operationIndex++)
				{
					yield return base.components[operationIndex].sequence.CCheckWait();
				}
			}
		}

		[SerializeField]
		private string _tag;

		[SerializeField]
		[Sequence.Subcomponent]
		private Sequence _sequence;

		public Sequence sequence => _sequence;

		public override string ToString()
		{
			if (!string.IsNullOrEmpty(_tag))
			{
				return _tag;
			}
			return this.GetAutoName();
		}
	}
}
