﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Services;
using Roadkill.Core.Security;
using StructureMap;
using StructureMap.Attributes;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Attachments;

namespace Roadkill.Core.Mvc.Attributes
{
	/// <summary>
	/// Includes 304 modified header support on the client.
	/// </summary>
	public class BrowserCacheAttribute : ActionFilterAttribute, IControllerAttribute
	{
		[SetterProperty]
		public ApplicationSettings ApplicationSettings { get; set; }

		[SetterProperty]
		public IUserContext Context { get; set; }

		[SetterProperty]
		public UserServiceBase UserManager { get; set; }

		[SetterProperty]
		public PageService PageService { get; set; }

		public override void OnResultExecuted(ResultExecutedContext filterContext)
		{
			if (!ApplicationSettings.Installed || !ApplicationSettings.UseBrowserCache || Context.IsLoggedIn)
				return;

			WikiController wikiController = filterContext.Controller as WikiController;
			HomeController homeController = filterContext.Controller as HomeController;

			if (wikiController == null && homeController == null)
				return;

			PageViewModel summary = null;

			// Find the page for the action we're on
			if (wikiController != null)
			{
				int id = 0;
				if (int.TryParse(filterContext.RouteData.Values["id"].ToString(), out id))
				{
					summary = PageService.GetById(id);
				}
			}
			else
			{
				summary = PageService.FindHomePage();
			}

			if (summary != null && summary.IsCacheable)
			{
				filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.Public);
				filterContext.HttpContext.Response.Cache.SetExpires(DateTime.UtcNow.AddSeconds(2));
				filterContext.HttpContext.Response.Cache.SetMaxAge(TimeSpan.FromSeconds(0));
				filterContext.HttpContext.Response.Cache.SetLastModified(summary.ModifiedOn.ToUniversalTime());
				filterContext.HttpContext.Response.StatusCode = ResponseWrapper.GetStatusCodeForCache(summary.ModifiedOn.ToUniversalTime(), filterContext.HttpContext.Request.Headers["If-Modified-Since"]);

				if (filterContext.HttpContext.Response.StatusCode == 304)
				{
					filterContext.Result = new HttpStatusCodeResult(304, "Not Modified");
				}
			}
		}
	}
}
