using UnityEngine;

public class SwapKeycard : MonoBehaviour
{
	public GameObject[] accessLevels;

	public void UpdateAccessLevel(int level)
	{
		GameObject[] array = accessLevels;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(false);
		}
		accessLevels[level - 1].SetActive(true);
	}
}
