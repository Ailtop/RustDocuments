using UnityEngine;

namespace Instancing;

public static class ComputeBufferEx
{
	public static void SetBuffer<T>(this ComputeShader shader, int kernel, int name, GPUBuffer<T> buffer) where T : unmanaged
	{
		shader.SetBuffer(kernel, name, buffer.Buffer);
	}
}
