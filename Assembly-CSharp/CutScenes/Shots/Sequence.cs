using System;
using System.Collections;
using System.Linq;
using CutScenes.Shots.Sequences;
using Runnables;
using UnityEditor;

namespace CutScenes.Shots
{
	public abstract class Sequence : CRunnable
	{
		public class SubcomponentAttribute : UnityEditor.SubcomponentAttribute
		{
			public SubcomponentAttribute()
				: base(true, CRunnable.types.Concat(Sequence.types).ToArray())
			{
			}
		}

		[Serializable]
		public class Subcomponents : SubcomponentArray<CRunnable>
		{
			public IEnumerator CRun()
			{
				for (int i = 0; i < base.components.Length; i++)
				{
					yield return base.components[i].CRun();
				}
			}
		}

		public new static readonly Type[] types = new Type[14]
		{
			typeof(CharacterMoveTo),
			typeof(ShowDialog),
			typeof(ShowLine),
			typeof(ShowRandomDialog),
			typeof(ShowEndingGameResult),
			typeof(RunAction),
			typeof(OpenChatSelector),
			typeof(OpenContentSelector),
			typeof(Talk),
			typeof(TalkRaw),
			typeof(TalkRandomly),
			typeof(TalkCacheText),
			typeof(NextTalk),
			typeof(WaitForTransfom)
		};
	}
}
