using System;
using Elements.Core;
using Elements.Data;
using FrooxEngine;

// ReSharper disable UnassignedReadonlyField
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace BsB2eDriver;

[DataModelType]
[SettingCategory("Devices")]
[AutoRegisterSetting]
public class B2eSettings : SettingComponent<B2eSettings>
{
	internal static B2eTrackingDriver? CurrentDriver;
	public override bool UserspaceOnly => true;
	
	[SettingProperty]
	public readonly Sync<bool> EyeTrackingEnabled;

	[SettingIndicatorProperty]
	public readonly Sync<string> TrackingStatus;

	[SettingProperty] [SyncMethod(typeof(Action))]
	public void ForceInitialize() => CurrentDriver?.InitializeSharedMem();

#if DEBUG
	[SettingProperty] public readonly Sync<bool> DebugAllowStale;

	[SettingIndicatorProperty] public readonly Sync<string> DebugDataTimestamp;

	[SettingIndicatorProperty] public readonly Sync<bool> DebugIsInitialized;

	[SettingIndicatorProperty] public readonly Sync<bool> DebugIsActive;
#endif
	
	protected override void OnAwake()
	{
		base.OnAwake();
		Persistent = false;
		TrackingStatus.Value = "Driver initialization failed";
		if (CurrentDriver == null)
		{
			CurrentDriver = new B2eTrackingDriver(this);
			Engine.Current.InputInterface.RegisterInputDriver(CurrentDriver);
			UniLog.Log("[BsB2e] Driver registered");
		}
		EyeTrackingEnabled.Value = CurrentDriver.IsInitialized;
	}
}