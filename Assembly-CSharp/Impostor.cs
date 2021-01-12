using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public class Impostor : MonoBehaviour, IClientComponent
{
	public ImpostorAsset asset;

	[Header("Baking")]
	public GameObject reference;

	public float angle;

	public int resolution = 1024;

	public int padding = 32;

	public bool spriteOutlineAsMesh;

	private void OnEnable()
	{
	}
}
