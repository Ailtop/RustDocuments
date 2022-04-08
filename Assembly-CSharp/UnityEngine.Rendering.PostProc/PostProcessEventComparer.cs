using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace UnityEngine.Rendering.PostProcessing
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal struct PostProcessEventComparer : IEqualityComparer<PostProcessEvent>
	{
		public bool Equals(PostProcessEvent x, PostProcessEvent y)
		{
			return x == y;
		}

		public int GetHashCode(PostProcessEvent obj)
		{
			return (int)obj;
		}
	}
}
