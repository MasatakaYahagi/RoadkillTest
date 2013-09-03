﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Configuration;
using System.Xml.Linq;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Core.Configuration
{
	public abstract class ConfigReader
	{
		protected string ConfigFilePath;

		public ConfigReader(string configFilePath)
		{
			ConfigFilePath = configFilePath;
		}

		public abstract void UpdateCurrentVersion(string currentVersion);
		public abstract void Save(SettingsSummary settings);
		public abstract RoadkillSection Load();
		public abstract ApplicationSettings GetApplicationSettings();
		public abstract void ResetInstalledState();
		public abstract string TestSaveWebConfig();
	}
}
