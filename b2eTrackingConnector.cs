using System;
using System.Threading;
using System.Threading.Tasks;
using Elements.Core;
using FrooxEngine;

namespace BsB2eDriver;

class b2eTrackingConnector : IPlatformConnector
{
	internal static PlatformInterface? platform;
	
	static b2eTrackingConnector()
	{
		UniLog.Log("BsB2e Connector ctor");
		Engine.Current.OnReady += () =>
		{
			Engine.Current.InputInterface.RegisterInputDriver(new b2eTrackingDriver());
			UniLog.Log("BsB2e Driver registered");
		};
	}

	public void Dispose()
	{ }

	public async Task<bool> Initialize(PlatformInterface platformInterface)
	{
		platform = platformInterface;
		return true;
	}

	public void Update()
	{ }

	public void SetCurrentStatus(World world, bool isPrivate, int totalWorldCount)
	{ }

	public void ClearCurrentStatus()
	{ }

	public void NotifyOfLocalUser(User user)
	{ }

	public void NotifyOfFile(string file, string name)
	{ }

	public void NotifyOfScreenshot(World world, string file, ScreenshotType type, DateTime time)
	{ }

	public int Priority => 0;
	public string PlatformName => "BsB2eDriver";
	public string Username => null;
	public string PlatformUserId => null;
	public bool IsPlatformNameUnique => false;
}