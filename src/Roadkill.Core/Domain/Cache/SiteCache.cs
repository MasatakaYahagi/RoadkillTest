﻿using System.Linq;
using System.Runtime.Caching;
using System.Text;
using Mindscape.LightSpeed;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Logging;

namespace Roadkill.Core.Cache
{
	public class SiteCache
	{
		private ObjectCache _cache; 
		private ApplicationSettings _applicationSettings;

		public SiteCache(ApplicationSettings settings, ObjectCache cache)
		{
			_applicationSettings = settings;
			_cache = cache;
		}

		public void AddMenu(string html)
		{
			_cache.Add(CacheKeys.MENU, html, new CacheItemPolicy());
		}

		public void AddLoggedInMenu(string html)
		{
			_cache.Add(CacheKeys.LOGGEDINMENU, html, new CacheItemPolicy());
		}

		public void AddAdminMenu(string html)
		{
			_cache.Add(CacheKeys.ADMINMENU, html, new CacheItemPolicy());
		}

		public void RemoveMenuCacheItems()
		{
			_cache.Remove(CacheKeys.MENU);
			_cache.Remove(CacheKeys.LOGGEDINMENU);
			_cache.Remove(CacheKeys.ADMINMENU);
		}

		public string GetMenu()
		{
			return _cache.Get(CacheKeys.MENU) as string;
		}

		public string GetLoggedInMenu()
		{
			return _cache.Get(CacheKeys.LOGGEDINMENU) as string;
		}

		public string GetAdminMenu()
		{
			return _cache.Get(CacheKeys.ADMINMENU) as string;
		}
	}
}