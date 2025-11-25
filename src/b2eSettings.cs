using System;
using Elements.Data;
using FrooxEngine;
// ReSharper disable UnassignedReadonlyField
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace BsB2eDriver;

[SettingCategory("Devices")]
[DataModelType]
public class b2eSettings : SettingComponent<b2eSettings>
{
	public enum SharedMemoryName
	{
		VRCFTMemmapData,
		EyeTrackingMemmapData
	}
	
	internal b2eTrackingDriver? CurrentDriver;
	
	public override bool UserspaceOnly => true;
	
	[SettingProperty("Bigscreen Beyond eye tracking enabled")]
	public readonly Sync<bool> B2eEyeTrackingEnabled;
	
	[SettingProperty("Shared memory source stream")]
	public readonly Sync<SharedMemoryName> B2eSharedMemoryName;
	
	[SettingProperty("Force shared-mem (re)initialization")]
	[SyncMethod(typeof(Action))]
	public void ForceInitialize()
	{
		CurrentDriver?.InitializeSharedMem();
	}

	#if DEBUG
	[SettingProperty("Debug: Use stale tracking data")]
	public readonly Sync<bool> DebugAllowStale;
	
	[SettingIndicatorProperty(nameOverride: "Debug: Last timestamp")]
	public readonly Sync<string> DebugDataTimestamp;
	
	[SettingIndicatorProperty(nameOverride: "Debug: Is shared-mem initialized")]
	public readonly Sync<bool> DebugIsInitialized;
	
	[SettingIndicatorProperty(nameOverride: "Debug: Is tracking active")]
	public readonly Sync<bool> DebugIsActive;
	#endif

	protected override void OnAwake()
	{
		base.OnAwake();
		B2eEyeTrackingEnabled.Value = CurrentDriver?.IsInitialized ?? false;
		B2eSharedMemoryName.Value = SharedMemoryName.VRCFTMemmapData;
	}
}