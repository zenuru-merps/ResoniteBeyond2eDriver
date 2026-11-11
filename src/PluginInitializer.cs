using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Elements.Core;
using Elements.Data;
using FrooxEngine;

#pragma warning disable CA2255

[assembly: DataModelAssembly(DataModelAssemblyType.UserspaceCore)]

namespace BsB2eDriver;

public static class PluginInitializer
{
	[ModuleInitializer]
	internal static void ModuleInitializer()
	{
		UniLog.Log("[BsB2e] ModuleInitializer called");
		if (Engine.Current.IsReady)
			EngineOnReady();
		else
			Engine.Current.OnReady += EngineOnReady;
	}

	private static void EngineOnReady()
	{
		UniLog.Log("[BsB2e] EngineOnReady called");
		if (Userspace.Current?.World != null)
			Userspace.Current.World.RunInSeconds(1, LoadLocale);
		else
			Engine.Current.WorldManager.WorldAdded += world =>
			{
				if (world.IsUserspace())
					world.RunInSeconds(1, LoadLocale);
			};
	}

	internal static void LoadLocale()
	{
		UniLog.Log("[BsB2e] LoadLocale called");
		var localeData = Userspace.Current.GetCoreLocale().Asset?.Data;
		if (localeData == null)
		{
			UniLog.Warning("[BsB2e] Locale asset didn't load in time");
			return;
		}

		var executingAssembly = Assembly.GetExecutingAssembly();
		foreach (var resourceName in executingAssembly.GetManifestResourceNames())
		{
			using var stream = executingAssembly.GetManifestResourceStream(resourceName);
			if (stream == null)
			{
				UniLog.Warning($"[BsB2e] Got null resource stream for {resourceName}");
				continue;
			}
			using var streamReader = new StreamReader(stream);
			localeData.LoadDataAdditively(streamReader.ReadToEnd());
		}

		UniLog.Log("[BsB2e] Locale strings loaded");
	}
}