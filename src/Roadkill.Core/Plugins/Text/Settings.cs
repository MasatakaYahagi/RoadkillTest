﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Roadkill.Core.Logging;

namespace Roadkill.Core.Plugins
{
	public class Settings
	{
		private List<SettingValue> _values;

		public bool IsEnabled { get; set; }
		public IEnumerable<SettingValue> Values
		{
			get
			{
				return _values;
			}
		}

		public Settings()
		{
			_values = new List<SettingValue>();
		}

		public void SetValue(string name, string value)
		{
			SettingValue settingValue = _values.FirstOrDefault(x => !string.IsNullOrEmpty(x.Name) &&
																	x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

			if (settingValue == null)
			{
				settingValue = new SettingValue();
				settingValue.Name = name;
			}
			
			settingValue.Value = value;
			_values.Add(settingValue);
		}

		public string GetValue(string name)
		{
			SettingValue settingValue = _values.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

			if (settingValue != null)
				return settingValue.Value;

			return "";
		}

		public string GetJson()
		{
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		}

		public static Settings LoadFromJson(string json)
		{
			// This looks familiar...
			if (string.IsNullOrEmpty(json))
			{
				Log.Warn("PluginSettings.LoadFromJson - json string was empty (returning a default Settings object)");
				return new Settings();
			}

			try
			{
				return JsonConvert.DeserializeObject<Settings>(json);
			}
			catch (JsonReaderException ex)
			{
				Log.Error(ex, "Settings.LoadFromJson - an exception occurred deserializing the JSON");
				return new Settings();
			}
		}
	}
}
