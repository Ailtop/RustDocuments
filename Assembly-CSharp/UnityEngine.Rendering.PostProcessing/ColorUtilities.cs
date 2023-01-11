namespace UnityEngine.Rendering.PostProcessing;

public static class ColorUtilities
{
	private const float logC_cut = 0.011361f;

	private const float logC_a = 5.555556f;

	private const float logC_b = 0.047996f;

	private const float logC_c = 0.244161f;

	private const float logC_d = 0.386036f;

	private const float logC_e = 5.301883f;

	private const float logC_f = 0.092819f;

	public static float StandardIlluminantY(float x)
	{
		return 2.87f * x - 3f * x * x - 0.27509508f;
	}

	public static Vector3 CIExyToLMS(float x, float y)
	{
		float num = 1f;
		float num2 = num * x / y;
		float num3 = num * (1f - x - y) / y;
		float x2 = 0.7328f * num2 + 0.4296f * num - 0.1624f * num3;
		float y2 = -0.7036f * num2 + 1.6975f * num + 0.0061f * num3;
		float z = 0.003f * num2 + 0.0136f * num + 0.9834f * num3;
		return new Vector3(x2, y2, z);
	}

	public static Vector3 ComputeColorBalance(float temperature, float tint)
	{
		float num = temperature / 60f;
		float num2 = tint / 60f;
		float x = 0.31271f - num * ((num < 0f) ? 0.1f : 0.05f);
		float y = StandardIlluminantY(x) + num2 * 0.05f;
		Vector3 vector = new Vector3(0.949237f, 1.03542f, 1.08728f);
		Vector3 vector2 = CIExyToLMS(x, y);
		return new Vector3(vector.x / vector2.x, vector.y / vector2.y, vector.z / vector2.z);
	}

	public static Vector3 ColorToLift(Vector4 color)
	{
		Vector3 vector = new Vector3(color.x, color.y, color.z);
		float num = vector.x * 0.2126f + vector.y * 0.7152f + vector.z * 0.0722f;
		vector = new Vector3(vector.x - num, vector.y - num, vector.z - num);
		float w = color.w;
		return new Vector3(vector.x + w, vector.y + w, vector.z + w);
	}

	public static Vector3 ColorToInverseGamma(Vector4 color)
	{
		Vector3 vector = new Vector3(color.x, color.y, color.z);
		float num = vector.x * 0.2126f + vector.y * 0.7152f + vector.z * 0.0722f;
		vector = new Vector3(vector.x - num, vector.y - num, vector.z - num);
		float num2 = color.w + 1f;
		return new Vector3(1f / Mathf.Max(vector.x + num2, 0.001f), 1f / Mathf.Max(vector.y + num2, 0.001f), 1f / Mathf.Max(vector.z + num2, 0.001f));
	}

	public static Vector3 ColorToGain(Vector4 color)
	{
		Vector3 vector = new Vector3(color.x, color.y, color.z);
		float num = vector.x * 0.2126f + vector.y * 0.7152f + vector.z * 0.0722f;
		vector = new Vector3(vector.x - num, vector.y - num, vector.z - num);
		float num2 = color.w + 1f;
		return new Vector3(vector.x + num2, vector.y + num2, vector.z + num2);
	}

	public static float LogCToLinear(float x)
	{
		if (!(x > 0.1530537f))
		{
			return (x - 0.092819f) / 5.301883f;
		}
		return (Mathf.Pow(10f, (x - 0.386036f) / 0.244161f) - 0.047996f) / 5.555556f;
	}

	public static float LinearToLogC(float x)
	{
		if (!(x > 0.011361f))
		{
			return 5.301883f * x + 0.092819f;
		}
		return 0.244161f * Mathf.Log10(5.555556f * x + 0.047996f) + 0.386036f;
	}

	public static uint ToHex(Color c)
	{
		return ((uint)(c.a * 255f) << 24) | ((uint)(c.r * 255f) << 16) | ((uint)(c.g * 255f) << 8) | (uint)(c.b * 255f);
	}

	public static Color ToRGBA(uint hex)
	{
		return new Color((float)((hex >> 16) & 0xFFu) / 255f, (float)((hex >> 8) & 0xFFu) / 255f, (float)(hex & 0xFFu) / 255f, (float)((hex >> 24) & 0xFFu) / 255f);
	}
}
