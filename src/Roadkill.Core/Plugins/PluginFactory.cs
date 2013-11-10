﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.DI;
using Roadkill.Core.Logging;
using Roadkill.Core.Plugins.Text.BuiltIn;
using Roadkill.Core.Plugins.SpecialPages;
using StructureMap;

namespace Roadkill.Core.Plugins
{
	public class PluginFactory : IPluginFactory
	{
		/// <summary>
		/// Allows additional text plugins to be registered at runtime.
		/// </summary>
		public void RegisterTextPlugin(TextPlugin plugin)
		{
			ServiceLocator.RegisterType<TextPlugin>(plugin);
		}

		/// <summary>
		/// Retrieves all text plugins from the IoC container.
		/// </summary>
		public IEnumerable<TextPlugin> GetTextPlugins()
		{
			return ServiceLocator.GetAllInstances<TextPlugin>();
		}

		/// <summary>
		/// Retrieves all text plugins from the IoC container.
		/// </summary>
		public IEnumerable<TextPlugin> GetEnabledTextPlugins()
		{
			return ServiceLocator.GetAllInstances<TextPlugin>().Where(x => x.Settings.IsEnabled);
		}

		public TextPlugin GetTextPlugin(string id)
		{
			return ServiceLocator.GetAllInstances<TextPlugin>().FirstOrDefault(x => x.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase));
		}

		public IEnumerable<SpecialPagePlugin> GetSpecialPagePlugins()
		{
			return ServiceLocator.GetAllInstances<SpecialPagePlugin>();
		}

		public SpecialPagePlugin GetSpecialPagePlugin(string name)
		{
			return ServiceLocator.GetAllInstances<SpecialPagePlugin>().FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
		}

		/// <summary>
		/// Copies the user service plugin DLLs from their storage location in /Plugins/UserService/ to the /bin folder.
		/// </summary>
		public void CopyUserServicePlugins(ApplicationSettings applicationSettings)
		{
			CopyAssemblies(applicationSettings.UserServicePluginsPath, applicationSettings.UserServicePluginsBinPath, false);
		}

		/// <summary>
		/// Copies the text plugin DLLs from their storage location in /Plugins/TextPlugins/ to the /bin folder.
		/// </summary>
		public void CopyTextPlugins(ApplicationSettings applicationSettings)
		{
			CopyAssemblies(applicationSettings.TextPluginsPath, applicationSettings.TextPluginsBinPath);
		}

		/// <summary>
		/// Copies the special page (/Special:) plugins from their storage location in /Plugins/SpecialPages/to the /bin folder.
		/// </summary>
		public void CopySpecialPagePlugins(ApplicationSettings applicationSettings)
		{
			CopyAssemblies(applicationSettings.SpecialPagePluginsPath, applicationSettings.SpecialPagePluginsBinPath);
		}

		/// <summary>
		/// Copies plugins from their storage location to the bin folder.
		/// </summary>
		internal void CopyAssemblies(string pluginsourcePath, string pluginDestinationPath, bool hasSubDirectories = true)
		{
			try
			{
				if (Directory.Exists(pluginsourcePath))
				{
					if (!Directory.Exists(pluginDestinationPath))
						Directory.CreateDirectory(pluginDestinationPath);

					if (hasSubDirectories)
					{
						foreach (string subdirectory in Directory.GetDirectories(pluginsourcePath))
						{
							CopyDirectoryContents(subdirectory, pluginDestinationPath);
						}
					}
					else
					{
						CopyDirectoryContents(pluginsourcePath, pluginDestinationPath);
					}
				}
				else
				{
					Directory.CreateDirectory(pluginsourcePath);
				}
			}
			catch (IOException e)
			{
				Log.Error(e, "Unable to copy plugins to the bin folder");
			}
		}

		private void CopyDirectoryContents(string subdirectory, string pluginDestinationPath)
		{
			// Create the directory in the /bin/Plugins/CustomVariables folder,
			// e.g. /bin/Plugins/TextPlugins/MyPlugin
			DirectoryInfo dirInfo = new DirectoryInfo(subdirectory);
			string destination = Path.Combine(pluginDestinationPath, dirInfo.Name);
			if (!Directory.Exists(destination))
			{
				Directory.CreateDirectory(destination);
				Log.Information("Created directory {0} for plugin", destination);
			}

			foreach (string sourceFile in Directory.EnumerateFiles(subdirectory, "*.dll"))
			{
				// Copy the plugin's dlls only - but only if the file write time is more recent.
				// If this check is removed, a looping app restart occurs (as the bin folder
				// changes, so an app start occurs, and this method is called on app start, which then triggers another restart).
				FileInfo sourceInfo = new FileInfo(sourceFile);
				string destPath = Path.Combine(destination, sourceInfo.Name);
				FileInfo destInfo = new FileInfo(destPath);

				if (sourceInfo.LastWriteTimeUtc > destInfo.LastWriteTimeUtc)
				{
					File.Copy(sourceFile, destPath, true);
					Log.Information("Copied plugin file '{0}' to '{1}' as it's newer ({2} > {3})", sourceInfo.FullName, destInfo.FullName,
																								 sourceInfo.LastWriteTimeUtc, destInfo.LastWriteTimeUtc);
				}
			}
		}
	}
}