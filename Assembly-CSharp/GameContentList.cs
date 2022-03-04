using System.Collections.Generic;
using UnityEngine;

public class GameContentList : MonoBehaviour
{
	public enum ResourceType
	{
		Audio = 0,
		Textures = 1,
		Models = 2
	}

	public ResourceType resourceType;

	public List<Object> foundObjects;
}
