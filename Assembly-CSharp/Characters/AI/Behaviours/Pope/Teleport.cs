using System.Collections;
using Characters.Actions;
using Characters.AI.Pope;
using FX;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Pope
{
	public sealed class Teleport : Move
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(Characters.AI.Behaviours.Teleport))]
		private Characters.AI.Behaviours.Teleport _teleport;

		[SerializeField]
		private Action _in;

		[SerializeField]
		private Action _out;

		[SerializeField]
		private Transform _teleportDestination;

		[SerializeField]
		private Navigation _navigation;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(LineEffect))]
		private LineEffect _lineEffect;

		public override IEnumerator CRun(AIController controller)
		{
			_teleportDestination.position = _navigation.destination.position;
			_in.TryStart();
			while (_in.running)
			{
				yield return null;
			}
			_out.TryStart();
			while (_out.running)
			{
				yield return null;
			}
		}

		public override void SetDestination(Point.Tag tag)
		{
			_navigation.destinationTag = tag;
		}
	}
}
