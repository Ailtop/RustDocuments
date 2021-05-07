using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class FadeText : MonoBehaviour
{
	private Text textRef;

	public float alpha;

	private void Start()
	{
		textRef = GetComponent<Text>();
	}

	private void Update()
	{
		alpha = textRef.color.a;
		if (alpha > 0f)
		{
			alpha -= 0.01f;
			Color color = new Color(1f, 1f, 1f, alpha);
			textRef.color = color;
		}
	}
}
