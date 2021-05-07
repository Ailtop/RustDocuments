using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public abstract class Behaviour : MonoBehaviour
	{
		[AttributeUsage(AttributeTargets.Field)]
		public class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public new static readonly Type[] types = new Type[14]
			{
				typeof(Selector),
				typeof(Sequence),
				typeof(Conditional),
				typeof(Count),
				typeof(Chance),
				typeof(CoolTime),
				typeof(UniformSelector),
				typeof(WeightedSelector),
				typeof(Repeat),
				typeof(RandomBehaviour),
				typeof(InfiniteLoop),
				typeof(Idle),
				typeof(SkipableIdle),
				typeof(TimeLoop)
			};

			public SubcomponentAttribute(bool allowCustom = true)
				: base(allowCustom, types)
			{
			}
		}

		[Serializable]
		public class Subcomponents : SubcomponentArray<Behaviour>
		{
		}

		public enum Result
		{
			Fail,
			Doing,
			Success,
			Done
		}

		public Action onStart;

		public Action onEnd;

		protected List<Behaviour> _childs;

		public Result result { get; set; }

		private void Stop()
		{
			if (result.Equals(Result.Doing))
			{
				result = Result.Done;
			}
		}

		protected IEnumerator CExpire(AIController controller, Vector2 durationMinMax)
		{
			float seconds = UnityEngine.Random.Range(durationMinMax.x, durationMinMax.y);
			yield return controller.character.chronometer.master.WaitForSeconds(seconds);
			Stop();
		}

		protected IEnumerator CExpire(AIController controller, float duration)
		{
			yield return controller.character.chronometer.master.WaitForSeconds(duration);
			Stop();
		}

		public abstract IEnumerator CRun(AIController controller);

		public void StopPropagation()
		{
			result = Result.Done;
			if (_childs == null)
			{
				return;
			}
			foreach (Behaviour child in _childs)
			{
				child?.StopPropagation();
			}
		}

		public override string ToString()
		{
			return this.GetAutoName();
		}
	}
}
