using CutScenes.Shots;
using UnityEditor;
using UnityEngine;

namespace Runnables
{
	public class ACutScene : Runnable
	{
		[SerializeField]
		private OnStartEnd _onStartEnd;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(ShotInfo))]
		private ShotInfo.Subcomponents _shotInfos;

		public override void Run()
		{
			_shotInfos.Run(_onStartEnd?.onStart, _onStartEnd?.onEnd);
		}
	}
}
