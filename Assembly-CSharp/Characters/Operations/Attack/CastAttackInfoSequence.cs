using System;
using UnityEditor;
using UnityEngine;

namespace Characters.Operations.Attack
{
	public class CastAttackInfoSequence : MonoBehaviour
	{
		[Serializable]
		public sealed class Subcomponents : SubcomponentArray<CastAttackInfoSequence>
		{
			internal bool noDelay { get; private set; }

			internal void Initialize()
			{
				Array.Sort(base.components, (CastAttackInfoSequence x, CastAttackInfoSequence y) => x.timeToTrigger.CompareTo(y.timeToTrigger));
				noDelay = true;
				CastAttackInfoSequence[] array = base.components;
				foreach (CastAttackInfoSequence obj in array)
				{
					if (obj._timeToTrigger > 0f)
					{
						noDelay = false;
					}
					obj.attackInfo.Initialize();
				}
			}

			internal void StopAllOperationsToOwner()
			{
				for (int i = 0; i < _components.Length; i++)
				{
					_components[i].attackInfo.operationsToOwner.StopAll();
				}
			}
		}

		[SerializeField]
		[FrameTime]
		private float _timeToTrigger;

		[SerializeField]
		[Subcomponent(typeof(CastAttackInfo))]
		private CastAttackInfo _attackInfo;

		public float timeToTrigger => _timeToTrigger;

		public CastAttackInfo attackInfo => _attackInfo;

		public override string ToString()
		{
			return $"{_timeToTrigger:0.##}s({_timeToTrigger * 60f:0.##}f), {_attackInfo.GetAutoName()}";
		}
	}
}
