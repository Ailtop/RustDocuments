using System;
using UnityEngine;

namespace CutScenes.Shots
{
	public class ShotInfo : MonoBehaviour
	{
		[Serializable]
		internal class Subcomponents : SubcomponentArray<ShotInfo>
		{
			public void Run(Shot onStart, Shot onEnd)
			{
				if (base.components.Length != 0)
				{
					if (onStart != null)
					{
						onStart.SetNext(base.components[0].shot);
					}
					for (int i = 0; i < base.components.Length - 1; i++)
					{
						base.components[i].shot.SetNext(base.components[i + 1].shot);
					}
					if (onEnd != null)
					{
						base.components[base.components.Length - 1].shot.SetNext(onEnd);
					}
					if (onStart != null)
					{
						onStart.Run();
					}
					else
					{
						base.components[0].shot.Run();
					}
				}
			}
		}

		[SerializeField]
		private string _tag;

		[SerializeField]
		[Shot.Subcomponent]
		private Shot _shot;

		public Shot shot => _shot;

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
