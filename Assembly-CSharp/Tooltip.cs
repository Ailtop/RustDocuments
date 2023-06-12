using UnityEngine;

public class Tooltip : BaseMonoBehaviour, IClientComponent
{
	public static TooltipContainer Current;

	[TextArea]
	public string Text;

	public GameObject TooltipObject;

	public string token = "";

	public string english => Text;
}
