using System;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using Elements.Core;
using FrooxEngine;

namespace BsB2eDriver;

public class b2eTrackingDriver : IInputDriver
{
	private InputInterface? _input;
	private Eyes? _eyes;
	private b2eSettings? _settings;
	private MemoryMappedFile? _sharedMem;
	private MemoryMappedViewAccessor? _accessor;
	private SharedGazeData _eyeData;
	private string _sharedMemoryName = "VRCFTMemmapData";
	internal bool IsInitialized;
	internal bool IsActive;
	
	public int UpdateOrder => 100;

	private void OnSettingsChanged(b2eSettings component)
	{
		if (component == null) return;
		_settings = component;
		component.CurrentDriver = this;
		string newSharedMem = Enum.GetName(component.B2eSharedMemoryName.Value);
		if (_sharedMemoryName != newSharedMem)
		{
			_sharedMemoryName = newSharedMem;
			InitializeSharedMem();
		}
	}

	internal void InitializeSharedMem()
	{
		if (_sharedMem != null)
		{
			_accessor!.Dispose();
			_sharedMem.Dispose();
			IsInitialized = false;
		}
		try
		{
			_sharedMem = MemoryMappedFile.OpenExisting(_sharedMemoryName);
			_accessor = _sharedMem.CreateViewAccessor(0, Marshal.SizeOf<SharedGazeData>());
			IsInitialized = true;
			UniLog.Log("BsB2e shared memory initialization successful");
			return;
		}
		catch (Exception e)
		{
			UniLog.Error($"BsB2e shared memory initialization failed: {e.Message}");
		}
		IsInitialized = false;
	}
	
	public void RegisterInputs(InputInterface inputInterface)
	{
		if (inputInterface.Engine.Platform != Platform.Windows)
			throw new PlatformNotSupportedException("BsB2e eye tracking only supports Windows!");
		
		_input = inputInterface;
		UniLog.Log("BsB2e Driver initializing");
		InitializeSharedMem();
		Settings.RegisterComponentChanges<b2eSettings>(OnSettingsChanged);
		_eyes = new(inputInterface, "Beyond Eyetracking", false);
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
		
		IsActive = IsInitialized && _settings.B2eEyeTrackingEnabled.Value;
		SetTracking(IsActive);
	
		if (!IsActive)
			return;
		
		try
		{
			_accessor?.Read(0, out _eyeData);
		}
		catch (Exception e)
		{
			IsActive = false;
			UniLog.Error($"BsB2e exception reading shared memory: {e.Message}");
		}

		long nowms = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		long ettms = _eyeData.Timestamp;
		long tdiff = nowms - ettms;
		bool isstale = tdiff > 10 * 1000; // Stop tracking if last update > 10 sec ago
		#if DEBUG
		if (!_settings.DebugAllowStale.Value)
			IsActive &= !isstale;
		_settings?.RunSynchronously(() => _settings.DebugDataTimestamp.Value = $"{nowms} - {ettms} = {tdiff} ({isstale})");
		#else
		IsActive &= !isstale;
		#endif
		IsActive &= _eyeData.IsValid == 1;
		IsActive &= _input.VR_Active;

		if (!IsActive)
		{
			SetTracking(false);
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
