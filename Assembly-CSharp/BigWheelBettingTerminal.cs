using Network;
using UnityEngine;

public class BigWheelBettingTerminal : StorageContainer
{
	public BigWheelGame bigWheel;

	public Vector3 seatedPlayerOffset = Vector3.forward;

	public float offsetCheckRadius = 0.4f;

	public SoundDefinition winSound;

	public SoundDefinition loseSound;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BigWheelBettingTerminal.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public new void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(base.transform.TransformPoint(seatedPlayerOffset), offsetCheckRadius);
		base.OnDrawGizmos();
	}

	public bool IsPlayerValid(BasePlayer player)
	{
		if (!player.isMounted || !(player.GetMounted() is BaseChair))
		{
			return false;
		}
		Vector3 b = base.transform.TransformPoint(seatedPlayerOffset);
		if (Vector3Ex.Distance2D(player.transform.position, b) > offsetCheckRadius)
		{
			return false;
		}
		return true;
	}

	public override bool PlayerOpenLoot(BasePlayer player, string panelToOpen = "", bool doPositionChecks = true)
	{
		if (!IsPlayerValid(player))
		{
			return false;
		}
		return base.PlayerOpenLoot(player, panelToOpen);
	}

	public bool TrySetBigWheel(BigWheelGame newWheel)
	{
		if (base.isClient)
		{
			return false;
		}
		if (bigWheel != null && bigWheel != newWheel)
		{
			float num = Vector3.SqrMagnitude(bigWheel.transform.position - base.transform.position);
			if (Vector3.SqrMagnitude(newWheel.transform.position - base.transform.position) >= num)
			{
				return false;
			}
			bigWheel.RemoveTerminal(this);
		}
		bigWheel = newWheel;
		return true;
	}
}
