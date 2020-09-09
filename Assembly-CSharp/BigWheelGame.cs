using Oxide.Core;
using System.Collections.Generic;
using UnityEngine;

public class BigWheelGame : SpinnerWheel
{
	public HitNumber[] hitNumbers;

	public GameObject indicator;

	public GameObjectRef winEffect;

	[ServerVar]
	public static float spinFrequencySeconds = 45f;

	public int spinNumber;

	public int lastPaidSpinNumber = -1;

	public List<BigWheelBettingTerminal> terminals = new List<BigWheelBettingTerminal>();

	public override bool AllowPlayerSpins()
	{
		return false;
	}

	public override bool CanUpdateSign(BasePlayer player)
	{
		return false;
	}

	public override float GetMaxSpinSpeed()
	{
		return 180f;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		Invoke(InitBettingTerminals, 3f);
		Invoke(DoSpin, 10f);
	}

	public void DoSpin()
	{
		if (!(velocity > 0f))
		{
			velocity += UnityEngine.Random.Range(7f, 10f);
			spinNumber++;
			SetTerminalsLocked(true);
		}
	}

	public void SetTerminalsLocked(bool isLocked)
	{
		foreach (BigWheelBettingTerminal terminal in terminals)
		{
			terminal.inventory.SetLocked(isLocked);
		}
	}

	public void InitBettingTerminals()
	{
		terminals.Clear();
		Vis.Entities(base.transform.position, 30f, terminals, 256);
	}

	public override void Update_Server()
	{
		float velocity = base.velocity;
		base.Update_Server();
		float velocity2 = base.velocity;
		if (velocity > 0f && velocity2 == 0f && spinNumber > lastPaidSpinNumber)
		{
			Payout();
			lastPaidSpinNumber = spinNumber;
			QueueSpin();
		}
	}

	public float SpinSpacing()
	{
		return spinFrequencySeconds;
	}

	public void QueueSpin()
	{
		foreach (BigWheelBettingTerminal terminal in terminals)
		{
			terminal.ClientRPC(null, "SetTimeUntilNextSpin", SpinSpacing());
		}
		Invoke(DoSpin, SpinSpacing());
	}

	public void Payout()
	{
		HitNumber currentHitType = GetCurrentHitType();
		foreach (BigWheelBettingTerminal terminal in terminals)
		{
			if (!terminal.isClient)
			{
				bool flag = false;
				bool flag2 = false;
				Item slot = terminal.inventory.GetSlot((int)currentHitType.hitType);
				if (slot != null)
				{
					int num = currentHitType.ColorToMultiplier(currentHitType.hitType);
					if (Interface.CallHook("OnBigWheelWin", this, slot, terminal, num) != null)
					{
						return;
					}
					slot.amount += slot.amount * num;
					slot.RemoveFromContainer();
					slot.MoveToContainer(terminal.inventory, 5);
					flag = true;
				}
				for (int i = 0; i < 5; i++)
				{
					Item slot2 = terminal.inventory.GetSlot(i);
					if (Interface.CallHook("OnBigWheelLoss", this, slot2, terminal) != null)
					{
						return;
					}
					if (slot2 != null)
					{
						slot2.Remove();
						flag2 = true;
					}
				}
				if (flag | flag2)
				{
					terminal.ClientRPC(null, "WinOrLoseSound", flag);
				}
			}
		}
		ItemManager.DoRemoves();
		SetTerminalsLocked(false);
	}

	public HitNumber GetCurrentHitType()
	{
		HitNumber result = null;
		float num = float.PositiveInfinity;
		HitNumber[] array = hitNumbers;
		foreach (HitNumber hitNumber in array)
		{
			float num2 = Vector3.Distance(indicator.transform.position, hitNumber.transform.position);
			if (num2 < num)
			{
				result = hitNumber;
				num = num2;
			}
		}
		return result;
	}

	[ContextMenu("LoadHitNumbers")]
	public void LoadHitNumbers()
	{
		HitNumber[] array = hitNumbers = GetComponentsInChildren<HitNumber>();
	}
}
