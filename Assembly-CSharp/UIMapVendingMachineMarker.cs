using System;
using ProtoBuf;
using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class UIMapVendingMachineMarker : MonoBehaviour
{
	public Color inStock;

	public Color outOfStock;

	public Image colorBackground;

	public string displayName;

	public Tooltip toolTip;

	public RustButton button;

	[NonSerialized]
	public bool isInStock;

	[NonSerialized]
	public EntityRef<VendingMachine> vendingMachine;

	[NonSerialized]
	public ProtoBuf.VendingMachine vendingMachineData;

	public static event Action<UIMapVendingMachineMarker> onClicked;

	public void SetOutOfStock(bool stock)
	{
		colorBackground.color = (stock ? inStock : outOfStock);
		isInStock = stock;
	}

	public void UpdateInfo(ProtoBuf.VendingMachine vendingMachineData)
	{
		vendingMachine = new EntityRef<VendingMachine>(vendingMachineData.networkID);
		this.vendingMachineData?.Dispose();
		this.vendingMachineData = vendingMachineData.Copy();
		displayName = vendingMachineData.shopName.EscapeRichText();
		toolTip.Text = displayName;
		if (isInStock && vendingMachineData?.sellOrderContainer?.sellOrders != null && vendingMachineData.sellOrderContainer.sellOrders.Count > 0)
		{
			toolTip.Text += "\n";
			foreach (ProtoBuf.VendingMachine.SellOrder sellOrder in vendingMachineData.sellOrderContainer.sellOrders)
			{
				if (sellOrder.inStock > 0)
				{
					string text = ItemManager.FindItemDefinition(sellOrder.itemToSellID).displayName.translated + (sellOrder.itemToSellIsBP ? " (BP)" : "");
					string text2 = ItemManager.FindItemDefinition(sellOrder.currencyID).displayName.translated + (sellOrder.currencyIsBP ? " (BP)" : "");
					Tooltip tooltip = toolTip;
					tooltip.Text = tooltip.Text + "\n" + sellOrder.itemToSellAmount + " " + text + " | " + sellOrder.currencyAmountPerItem + " " + text2;
					tooltip = toolTip;
					tooltip.Text = tooltip.Text + " (" + sellOrder.inStock + " Left)";
				}
			}
		}
		toolTip.enabled = toolTip.Text != "";
		if (button != null)
		{
			button.SetDisabled(UIMapVendingMachineMarker.onClicked == null);
		}
	}

	public void Clicked()
	{
		UIMapVendingMachineMarker.onClicked?.Invoke(this);
	}

	public static void RemoveAllHandlers()
	{
		UIMapVendingMachineMarker.onClicked = null;
	}
}
