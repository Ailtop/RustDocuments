using System.Text;
using Characters;
using Characters.Controllers;
using Services;
using Singletons;
using TMPro;
using UI.TestingTool;
using UnityEngine;

namespace UI
{
	public class DebugPanel : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text _status;

		[SerializeField]
		private Log _logger;

		public void StartLog()
		{
			_logger.StartLog();
		}

		private void OnEnable()
		{
			PlayerInput.blocked.Attach(this);
			Chronometer.global.AttachTimeScale(this, 0f);
			DisplayPlayerStat();
		}

		private void OnDisable()
		{
			PlayerInput.blocked.Detach(this);
			Chronometer.global.DetachTimeScale(this);
		}

		private void DisplayPlayerStat()
		{
			StringBuilder stringBuilder = new StringBuilder();
			Character player = Singleton<Service>.Instance.levelManager.player;
			if (player == null)
			{
				_status.text = string.Empty;
				return;
			}
			double final = player.stat.GetFinal(Stat.Kind.AttackDamage);
			stringBuilder.AppendFormat("Attack Damage : {0:0%}\r\n", final);
			stringBuilder.AppendFormat("Physical Attack Damage : {0:0%}\r\n", final * player.stat.GetFinal(Stat.Kind.PhysicalAttackDamage));
			stringBuilder.AppendFormat("Magic Attack Damage : {0:0%}\r\n", final * player.stat.GetFinal(Stat.Kind.MagicAttackDamage));
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("Critical Chance : {0:0%}\r\n", player.stat.GetFinal(Stat.Kind.CriticalChance));
			stringBuilder.AppendFormat("Critical Damage Multiplier : {0:0%}\r\n", player.stat.GetFinal(Stat.Kind.CriticalDamage));
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("Attack Speed : {0:0%}\r\n", player.stat.GetFinal(Stat.Kind.AttackSpeed));
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("Movement Speed : {0:0%}\r\n", player.stat.GetFinalPercent(Stat.Kind.MovementSpeed));
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("Health : {0}\r\n", player.stat.GetFinal(Stat.Kind.Health));
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("Taking Damage (smaller is better) : {0:0%}\r\n", player.stat.GetFinal(Stat.Kind.TakingDamage));
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("Cooldown Speed : {0:0%}\r\n", player.stat.GetFinal(Stat.Kind.CooldownSpeed));
			stringBuilder.AppendFormat("Skill Cooldown Speed : {0:0%}\r\n", player.stat.GetFinal(Stat.Kind.SkillCooldownSpeed));
			stringBuilder.AppendFormat("Dash Cooldown Speed : {0:0%}\r\n", player.stat.GetFinal(Stat.Kind.DashCooldownSpeed));
			stringBuilder.AppendFormat("Swap Cooldown Speed : {0:0%}\r\n", player.stat.GetFinal(Stat.Kind.SwapCooldownSpeed));
			stringBuilder.AppendFormat("Essence Cooldown Speed : {0:0%}\r\n", player.stat.GetFinal(Stat.Kind.EssenceCooldownSpeed));
			_status.text = stringBuilder.ToString();
		}
	}
}
