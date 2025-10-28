using System;
using System.IO.MemoryMappedFiles;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Elements.Core;
using FrooxEngine;

namespace BsB2eDriver;

public class b2eTrackingDriver : IInputDriver
{
	private InputInterface _input;
	private Eyes _eyes;
	private b2eSettings? _settings;
	private MemoryMappedFile? _sharedMem;
	private MemoryMappedViewAccessor? _accessor;
	private SharedGazeData _eyeData;
	private const string SharedMemoryName = "VRCFTMemmapData";
	internal bool IsInitialized;
	internal bool IsActive;
	
	public int UpdateOrder => 100;

	private void OnSettingsChanged(b2eSettings component)
	{
		if (component == null) return;
		_settings = component;
		component.CurrentDriver = this;
	}

	internal void InitializeSharedMem()
	{
		try
		{
			_sharedMem = MemoryMappedFile.OpenExisting(SharedMemoryName);
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
		_eyes = new(inputInterface, "BsB2e", false);
	}

	private void SetTracking(bool active)
	{
		_eyes.SetTracking(active);
		_eyes.IsEyeTrackingActive = active;
		_settings?.RunSynchronously(() => _settings.DebugIsActive.Value = active);
	}

	public void UpdateInputs(float deltaTime)
	{
		if (_settings == null)
		{
			IsActive = false;
			SetTracking(false);
		}
		else
		{
			IsActive = IsInitialized && _settings.b2eEyeTrackingEnabled.Value;
		}
		
		_settings?.RunSynchronously(() => _settings.DebugIsInitialized.Value = IsInitialized);
		SetTracking(IsActive);

		if (!IsActive) return;
		
		try
		{
			_accessor?.Read(0, out _eyeData);
		}
		catch (Exception e)
		{
			IsActive = false;
			UniLog.Error($"BsB2e exception reading shared memory: {e.Message}");
		}

		_settings?.RunSynchronously(() => _settings.DebugDataTimestamp.Value = _eyeData.Timestamp);
		// IsActive &= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _eyeData.Timestamp > 10 * 1000; // Stop tracking if last update > 10 sec ago
		// IsActive &= _eyeData.IsValid == 1;
		// SetTracking(IsActive);

		if (_eyeData.IsValid != 1) return;

		if (_settings!.b2eUseCombined.Value)
		{
			_eyes.CombinedEye.Direction = _eyeData.CombinedEyeToEngine();
		}
		else
		{
			_eyes.RightEye.Direction = _eyeData.RightEyeToEngine();
			_eyes.LeftEye.Direction = _eyeData.LeftEyeToEngine();
		}
		
		// Even though this isn't tracked yet, theoretically this code is future-proof for when it is tracked
		_eyes.RightEye.Openness = 1 - _eyeData.RightEyeClosedAmount;
		_eyes.LeftEye.Openness = 1 - _eyeData.LeftEyeClosedAmount;
	}

	public void CollectDeviceInfos(DataTreeList list)
	{
		if (_eyes != null)
		{
			DataTreeDictionary dict = new();
			dict.Add("Name", "Bigscreen Beyond");
			dict.Add("Type", "Eye Tracking");
			dict.Add("Model", "2e");
		}
	}
}
