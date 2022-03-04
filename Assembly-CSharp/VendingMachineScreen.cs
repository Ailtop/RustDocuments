using UnityEngine;
using UnityEngine.UI;

public class VendingMachineScreen : MonoBehaviour
{
	public enum vmScreenState
	{
		ItemScroll = 0,
		Vending = 1,
		Message = 2,
		ShopName = 3,
		OutOfStock = 4
	}

	public RawImage largeIcon;

	public RawImage blueprintIcon;

	public Text mainText;

	public Text lowerText;

	public Text centerText;

	public RawImage smallIcon;

	public VendingMachine vendingMachine;

	public Sprite outOfStockSprite;

	public Renderer fadeoutMesh;

	public CanvasGroup screenCanvas;

	public Renderer light1;

	public Renderer light2;

	public float nextImageTime;

	public int currentImageIndex;
}
