using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using Elements.Core;
using FrooxEngine;

namespace BsB2eDriver;

public class B2eTrackingDriver(B2eSettings? settings = null) : IInputDriver
{
	private InputInterface? _input;
	private Eyes? _eyes;
	private B2eSettings? _settings = settings;
	private MemoryMappedFile? _sharedMem;
	private MemoryMappedViewAccessor? _accessor;
	private B2eSharedGazeData _eyeData;
	internal bool IsInitialized;
	internal bool IsActive;
	internal const string EyeDriverName = "Beyond Eyetracking";
	private const string SharedMemoryName = "VRCFTMemmapData";
#if DEBUG
	private const string BuildConfig = "DEBUG";
#else
	private const string BuildConfig = "Release";
#endif

	public int UpdateOrder => 100;

	private void OnSettingsChanged(B2eSettings component)
	{
		_settings = component;
		B2eSettings.CurrentDriver = this;
	}

	internal void InitializeSharedMem()
	{
		if (_sharedMem != null)
		{
			_accessor!.Dispose();
			_sharedMem.Dispose();
			IsInitialized = false;
		}

		// UniLog.Log($"[BsB2e] using shared memory map \"{SharedMemoryName}\"");
		try
		{
			_sharedMem = MemoryMappedFile.OpenExisting(SharedMemoryName);
			_accessor = _sharedMem.CreateViewAccessor(0, Marshal.SizeOf<B2eSharedGazeData>());
			IsInitialized = true;
			UniLog.Log("[BsB2e] shared memory initialization successful");
			_settings?.TrackingStatus.Value = "Awaiting tracking data";
			return;
		}
		catch (FileNotFoundException e)
		{
			UniLog.Error($"[BsB2e] shared memory initialization failed: {e.Message}");
			_settings?.TrackingStatus.Value = $"Shared memory initialization failed (Is the Beyond Eyetracking app running?)";
		}
		catch (Exception e)
		{
			UniLog.Error($"[BsB2e] shared memory initialization failed: {e.Message}");
			_settings?.TrackingStatus.Value = $"Shared memory initialization failed: {e.Message}";
		}

		IsInitialized = false;
	}

	public void RegisterInputs(InputInterface inputInterface)
	{
		if (inputInterface.Engine.Platform != Platform.Windows)
			throw new PlatformNotSupportedException("BsB2e eye tracking only supports Windows!");

		_input = inputInterface;
		UniLog.Log($"[BsB2e] Driver initializing (Build config: {BuildConfig})");
		if (_settings == null)
			Settings.RegisterComponentChanges<B2eSettings>(OnSettingsChanged);
		InitializeSharedMem();
		_eyes = new Eyes(inputInterface, EyeDriverName, false);
	}

	private void SetTracking(bool active)
	{
		_eyes!.SetTracking(active);
		_eyes!.IsEyeTrackingActive = active;
#if DEBUG
		_settings?.RunSynchronously(() => _settings.DebugIsActive.Value = active);
#endif
	}

	public void UpdateInputs(float deltaTime)
	{
		if (_settings == null || _eyes == null || _input == null)
			return;

#if DEBUG
		_settings?.RunSynchronously(() => _settings.DebugIsInitialized.Value = IsInitialized);
#endif

		IsActive = IsInitialized && _settings!.EyeTrackingEnabled.Value;
		SetTracking(IsActive);

		if (!IsActive)
		{
			_settings?.TrackingStatus.Value = $"Tracking disabled";
			return;
		}

		try
		{
			_accessor?.Read(0, out _eyeData);
		}
		catch (Exception e)
		{
			IsActive = false;
			UniLog.Error($"[BsB2e] exception reading shared memory: {e.Message}");
			_settings?.TrackingStatus.Value = $"Tracking data exception: {e.Message}";
		}

		var nowms = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		var ettms = _eyeData.Timestamp;
		var tdiff = nowms - ettms;
		var isstale = tdiff > 5 * 1000; // Stop tracking if last update > 5 sec ago
#if DEBUG
		if (!_settings!.DebugAllowStale.Value)
			IsActive &= !isstale;
		_settings?.RunSynchronously(() =>
			_settings.DebugDataTimestamp.Value = $"{nowms} - {ettms} = {tdiff} ({isstale})");
#else
		IsActive &= !isstale;
#endif
		IsActive &= _eyeData.IsValid == 1;
		IsActive &= _input.VR_Active;

		if (!IsActive)
		{
			SetTracking(false);
			_settings?.TrackingStatus.Value = "Tracking paused";
			return;
		}

		_eyes.RightEye.UpdateWithDirection(_eyeData.RightEyeToEngine());
		_eyes.LeftEye.UpdateWithDirection(_eyeData.LeftEyeToEngine());
		_eyes.CombinedEye.UpdateWithDirection(_eyeData.CombinedEyeToEngine());
		_eyes.RightEye.Openness = 1 - _eyeData.RightEyeClosedAmount;
		_eyes.LeftEye.Openness = 1 - _eyeData.LeftEyeClosedAmount;
		_eyes.Timestamp = ettms;
		_eyes.ComputeCombinedEyeParameters();
		_eyes.FinishUpdate();
		
		_settings?.TrackingStatus.Value = "Tracking active";
	}

	public void CollectDeviceInfos(DataTreeList list)
	{
		if (_eyes != null)
		{
			DataTreeDictionary dict = new();
			dict.Add("Name", "Bigscreen Beyond");
			dict.Add("Type", "Eye Tracking");
			dict.Add("Model", "2e");
			list.Add(dict);
		}
	}
}