using UnityEngine;

namespace ConVar;

[Factory("fps")]
public class FPS : ConsoleSystem
{
	private static int m_graph;

	[ClientVar(Saved = true)]
	[ServerVar(Saved = true)]
	public static int limit
	{
		get
		{
			return Application.targetFrameRate;
		}
		set
		{
			Application.targetFrameRate = value;
		}
	}

	[ClientVar]
	public static int graph
	{
		get
		{
			return m_graph;
		}
		set
		{
			m_graph = value;
			if ((bool)MainCamera.mainCamera)
			{
				FPSGraph component = MainCamera.mainCamera.GetComponent<FPSGraph>();
				if ((bool)component)
				{
					component.Refresh();
				}
			}
		}
	}
}
