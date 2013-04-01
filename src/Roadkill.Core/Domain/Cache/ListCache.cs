﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using Mindscape.LightSpeed;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Logging;

namespace Roadkill.Core.Cache
{
	public class ListCache
	{
		internal static MemoryCache _cache = new MemoryCache("ListCache");
		private ApplicationSettings _applicationSettings;

		public ListCache(ApplicationSettings settings)
		{
			_applicationSettings = settings;
		}

		public void Add<T>(string key, IEnumerable<T> items)
		{
			if (!_applicationSettings.UseObjectCache)
				return;

			Log.Information("ListCache: Added {0} to cache", key);
			_cache.Add(key, items.ToList(), new CacheItemPolicy());
		}

		public List<T> Get<T>(string key)
		{
			Log.Information("ListCache: Retrieved {0} from cache", key);
			return _cache.Get(key) as List<T>;
		}

		public void Remove(string key)
		{
			if (!_applicationSettings.UseObjectCache)
				return;

			Log.Information("ListCache: Removed {0} from cache", key);
			_cache.Remove(key);
		}

		public void RemoveAll()
		{
			if (!_applicationSettings.UseObjectCache)
				return;

			Log.Information("ListCache: RemoveAll from cache");

			foreach (var item in _cache)
			{
				_cache.Remove(item.Key);
			}
		}

		public IEnumerable<string> GetAllKeys()
		{
			return _cache.Select(x => x.Key);
		}
	}
}
