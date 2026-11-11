using System.Runtime.InteropServices;
using Elements.Core;

namespace BsB2eDriver;

// https://github.com/BigscreenVR/VRCFT-Beyond/blob/65c78999f49dbc72f166196d12c8c7c9e38c26eb/BeyondExtTrackingModule.cs#L10
[StructLayout(LayoutKind.Sequential)]
public struct B2eSharedGazeData
{
	public float LeftEyeX;
	public float LeftEyeY;
	public float LeftEyeZ;
	public float RightEyeX;
	public float RightEyeY;
	public float RightEyeZ;
	public float CombinedX;
	public float CombinedY;
	public float CombinedZ;
	public float Confidence;
	public long Timestamp;

	public int IsValid;

	// Beyond-ET lacks these fields at the time of writing
	public float LeftEyeClosedAmount;
	public float RightEyeClosedAmount;
}

public static class SharedGazeDataExtensions
{
	public static float3 LeftEyeToEngine(this B2eSharedGazeData data)
	{
		return new float3(data.LeftEyeX, data.LeftEyeY, -data.LeftEyeZ);
	}

	public static float3 RightEyeToEngine(this B2eSharedGazeData data)
	{
		return new float3(data.RightEyeX, data.RightEyeY, -data.RightEyeZ);
	}

	public static float3 CombinedEyeToEngine(this B2eSharedGazeData data)
	{
		return new float3(data.CombinedX, data.CombinedY, -data.CombinedZ);
	}
}