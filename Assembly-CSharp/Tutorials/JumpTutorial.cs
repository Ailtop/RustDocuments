using System.Collections;
using Scenes;
using UnityEngine;

namespace Tutorials
{
	public class JumpTutorial : Tutorial
	{
		[SerializeField]
		private Transform _skeletone;

		[SerializeField]
		private Transform _trackPoint;

		public override void Activate()
		{
			base.Activate();
			Scene<GameBase>.instance.cameraController.StartTrack(_trackPoint);
		}

		public override void Deactivate()
		{
			StartCoroutine(_003CDeactivate_003Eg__CDeactivate_007C3_0());
		}

		protected override IEnumerator Process()
		{
			yield return Converse();
		}
	}
}
