using Network;
using TMPro;
using UnityEngine;

public class NetworkInfoGeneralText : MonoBehaviour
{
	public TextMeshProUGUI text;

	private void Update()
	{
		UpdateText();
	}

	private void UpdateText()
	{
		string str = "";
		if (Net.sv != null)
		{
			str += "Server\n";
			str += Net.sv.GetDebug(null);
			str += "\n";
		}
		text.text = str;
	}

	private static string ChannelStat(int window, int left)
	{
		return $"{left}/{window}";
	}
}
