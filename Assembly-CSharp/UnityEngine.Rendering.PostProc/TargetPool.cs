using System.Collections.Generic;

namespace UnityEngine.Rendering.PostProcessing;

internal class TargetPool
{
	private readonly List<int> m_Pool;

	private int m_Current;

	internal TargetPool()
	{
		m_Pool = new List<int>();
		Get();
	}

	internal int Get()
	{
		int result = Get(m_Current);
		m_Current++;
		return result;
	}

	private int Get(int i)
	{
		if (m_Pool.Count > i)
		{
			return m_Pool[i];
		}
		while (m_Pool.Count <= i)
		{
			m_Pool.Add(Shader.PropertyToID("_TargetPool" + i));
		}
		return m_Pool[i];
	}

	internal void Reset()
	{
		m_Current = 0;
	}
}
