using UnityEngine;
using UnityEngine.UI;

public class Achievements : SingletonComponent<global::Achievements>
{
	public SoundDefinition listComplete;

	public SoundDefinition itemComplete;

	public SoundDefinition popup;

	public Canvas Canvas;

	public Text titleText;
}
