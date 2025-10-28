using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using Elements.Core;
using Elements.Data;
using FrooxEngine;
// ReSharper disable UnassignedReadonlyField
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace BsB2eDriver;

[SettingCategory("Devices")]
[DataModelType]
public class b2eSettings : SettingComponent<b2eSettings>
{
	internal b2eTrackingDriver? CurrentDriver;
#if DEBUG
	private const bool HideDebug = false;
#else
	private const bool HideDebug = true;
#endif
	
	public override bool UserspaceOnly => true;
	
	[SettingProperty("Bigscreen Beyond eye tracking enabled")]
	public readonly Sync<bool> b2eEyeTrackingEnabled;	
	
	[SettingProperty("Use combined values instead of individual eyes")]
	public readonly Sync<bool> b2eUseCombined;
	
	[SettingProperty("Force shared-mem (re)initialization")]
	[SyncMethod(typeof(Action))]
	public void ForceInitialize()
	{
		CurrentDriver?.InitializeSharedMem();
	}

	[SettingIndicatorProperty(nameOverride: "Debug: Last timestamp", hidden: HideDebug)]
	public readonly Sync<long> DebugDataTimestamp;
	
	[SettingIndicatorProperty(nameOverride: "Debug: Is shared-mem initialized", hidden: HideDebug)]
	public readonly Sync<bool> DebugIsInitialized;
	
	[SettingIndicatorProperty(nameOverride: "Debug: Is tracking active", hidden: HideDebug)]
	public readonly Sync<bool> DebugIsActive;

	protected override void OnAwake()
	{
		base.OnAwake();
		b2eEyeTrackingEnabled.Value = CurrentDriver?.IsInitialized ?? false;
		b2eUseCombined.Value = false;
		DebugDataTimestamp.Value = -1;
	}
}