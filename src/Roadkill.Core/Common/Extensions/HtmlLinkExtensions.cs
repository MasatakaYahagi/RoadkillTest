﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Roadkill.Core.Localization;
using System.Globalization;
using StructureMap;
using Roadkill.Core.Configuration;
using ControllerBase = Roadkill.Core.Mvc.Controllers.ControllerBase;
using Roadkill.Core.Services;
using Roadkill.Core.Mvc.ViewModels;
using System.Web.Optimization;
using System.Web;
using Roadkill.Core.Plugins.Text.BuiltIn;
using Roadkill.Core.Mvc;
using Roadkill.Core.DI;

namespace Roadkill.Core
{
	/// <summary>
	/// A set of extension methods for common links throughout the site.
	/// </summary>
	public static class HtmlLinkExtensions
	{
		/// <summary>
		/// Gets a string to indicate whether the current user is logged in. You should render the User action rather than this extension:
		/// @Html.Action("LoggedInAs", "User")
		/// </summary>
		/// <returns>"Logged in as {user}" if the user is logged in; "Not logged in" if the user is not logged in.</returns>
		public static MvcHtmlString LoginStatus(this HtmlHelper helper)
		{
			ControllerBase controller = helper.ViewContext.Controller as ControllerBase;
			if (controller != null && controller.Context.IsLoggedIn)
			{
				string text = string.Format("{0} {1}", SiteStrings.Shared_LoggedInAs, controller.Context.CurrentUsername);
				return helper.ActionLink(text, "Profile", "User");
			}
			else
			{
				return MvcHtmlString.Create(SiteStrings.Shared_NotLoggedIn);
			}
		}

		/// <summary>
		/// Provides a link to the settings page, with optional prefix and suffix tags or seperators.
		/// </summary>
		/// <returns>If the user is not logged in and not an admin, an empty string is returned.</returns>
		public static MvcHtmlString SettingsLink(this HtmlHelper helper, string prefix,string suffix)
		{
			ControllerBase controller = helper.ViewContext.Controller as ControllerBase;
			if (controller != null && controller.Context.IsAdmin)
			{
				string link = helper.ActionLink(SiteStrings.Navigation_SiteSettings, "Index", "Settings").ToString();
				return MvcHtmlString.Create(prefix + link + suffix);
			}
			else
			{
				return MvcHtmlString.Empty;
			}
		}

		/// <summary>
		/// Provides a link to the filemanager page, with optional prefix and suffix tags or seperators.
		/// </summary>
		/// <returns>If the user is not logged in, an empty string is returned.</returns>
		public static MvcHtmlString FileManagerLink(this HtmlHelper helper, string prefix, string suffix)
		{
			ControllerBase controller = helper.ViewContext.Controller as ControllerBase;
			if (controller != null)
			{
				string link = helper.ActionLink(SiteStrings.FileManager_Title, "Index", "FileManager").ToString();
				return MvcHtmlString.Create(prefix + link + suffix);
			}
			else
			{
				return MvcHtmlString.Empty;
			}
		}

		/// <summary>
		/// Provides a link to the login page, or if the user is logged in, the logout page.
		/// Optional prefix and suffix tags or seperators and also included.
		/// </summary>
		/// <returns>If windows authentication is being used, an empty string is returned.</returns>
		public static MvcHtmlString LoginLink(this HtmlHelper helper, string prefix, string suffix)
		{
			ControllerBase controller = helper.ViewContext.Controller as ControllerBase;

			if (controller == null || controller.ApplicationSettings.UseWindowsAuthentication)
				return MvcHtmlString.Empty;

			string link = "";

			if (controller.Context.IsLoggedIn)
			{
				link = helper.ActionLink(SiteStrings.Navigation_Logout, "Logout", "User").ToString();
			}
			else
			{
				string redirectPath = helper.ViewContext.HttpContext.Request.Path;
				link = helper.ActionLink(SiteStrings.Navigation_Login, "Login", "User", new { ReturnUrl = redirectPath }, null ).ToString();

				if (controller.SettingsService.GetSiteSettings().AllowUserSignup)
					link += "&nbsp;/&nbsp;" + helper.ActionLink(SiteStrings.Navigation_Register, "Signup", "User").ToString();
			}

			return MvcHtmlString.Create(prefix + link + suffix);
		}

		/// <summary>
		/// Provides a link to the "new page" page, with optional prefix and suffix tags or seperators.
		/// </summary>
		/// <returns>If the user is not logged in and not an admin, an empty string is returned.</returns>
		public static MvcHtmlString NewPageLink(this HtmlHelper helper, string prefix, string suffix)
		{
			ControllerBase controller = helper.ViewContext.Controller as ControllerBase;

			if (controller != null && (controller.Context.IsLoggedIn && (controller.Context.IsAdmin || controller.Context.IsEditor)))
			{
				string link = helper.ActionLink(SiteStrings.Navigation_NewPage, "New", "Pages").ToString();
				return MvcHtmlString.Create(prefix + link + suffix);
			}
			else
			{
				return MvcHtmlString.Empty;
			}
		}

		/// <summary>
		/// Provides a link to the index page of the site, with optional prefix and suffix tags or seperators.
		/// </summary>
		public static MvcHtmlString MainPageLink(this HtmlHelper helper, string linkText, string prefix,string suffix)
		{
			return helper.ActionLink(linkText, "Index", "Home");
		}

		/// <summary>
		/// Provides a link to the page with the provided title, querying it in the database first.
		/// </summary>
		/// <returns>If the page is not found, the link text is returned.</returns>
		public static MvcHtmlString PageLink(this HtmlHelper helper, string linkText, string pageTitle)
		{
			return helper.PageLink(linkText, pageTitle, null,"","");
		}

		/// <summary>
		/// Provides a link to the page with the provided title, querying it in the database first,
		/// with optional prefix and suffix tags or seperators.
		/// </summary>
		/// <returns>If the page is not found, the link text is returned.</returns>
		public static MvcHtmlString PageLink(this HtmlHelper helper, string linkText, string pageTitle, string prefix, string suffix)
		{
			return helper.PageLink(linkText, pageTitle, null, prefix, suffix);
		}

		/// <summary>
		/// Provides a link to the page with the provided title, querying it in the database first,
		/// with optional prefix and suffix tags or seperators and html attributes.
		/// </summary>
		/// <param name="htmlAttributes">Any additional html attributes to add to the link</param>
		/// <returns>If the page is not found, the link text is returned.</returns>
		public static MvcHtmlString PageLink(this HtmlHelper helper, string linkText, string pageTitle, object htmlAttributes,string prefix,string suffix)
		{
			PageService manager = ServiceLocator.GetInstance<PageService>();
			PageViewModel model = manager.FindByTitle(pageTitle);
			if (model != null)
			{
				string link = helper.ActionLink(linkText, "Index", "Wiki", new { id = model.Id, title = pageTitle }, htmlAttributes).ToString();
				return MvcHtmlString.Create(prefix + link + suffix);
			}
			else
			{
				return MvcHtmlString.Create(linkText);
			}
		}

		/// <summary>
		/// Provides a CSS link tag for the CSS file provided. If the relative path does not begin with ~ then
		/// the Assets/Css folder is assumed.
		/// </summary>
		public static MvcHtmlString CssLink(this UrlHelper helper, string relativePath)
		{
			if (!relativePath.StartsWith("~"))
				relativePath = "~/Assets/CSS/" + relativePath;

			return MvcHtmlString.Create("<link href=\"" + helper.Content(relativePath) + "\" rel=\"stylesheet\" type=\"text/css\" />");
		}

		/// <summary>
		/// Provides a Javascript script tag for the Javascript file provided. If the relative path does not begin with ~ then
		/// the Assets/Scripts folder is assumed.
		/// </summary>
		public static MvcHtmlString ScriptLink(this UrlHelper helper, string relativePath)
		{
			if (!relativePath.StartsWith("~"))
				relativePath = "~/Assets/Scripts/" + relativePath;

			return MvcHtmlString.Create("<script type=\"text/javascript\" language=\"javascript\" src=\"" + helper.Content(relativePath) + "\"></script>");
		}

		/// <summary>
		/// Gets the localization strings for the jQuery validationEngine.
		/// </summary>
		public static MvcHtmlString ScriptLinkForValidationLocalization(this UrlHelper helper)
		{
			string path = "~/Assets/Scripts/languages/jquery.validationEngine-" + CultureInfo.CurrentUICulture.TwoLetterISOLanguageName + ".js";

			return MvcHtmlString.Create("<script type=\"text/javascript\" language=\"javascript\" src=\"" + helper.Content(path) + "\"></script>");
		}

		/// <summary>
		/// Provides a Javascript and CSS tags for the Bootstrap framework.
		/// </summary>
		public static MvcHtmlString BootStrap(this UrlHelper helper)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine("<link href=\"" + helper.Content("~/Assets/bootstrap/css/bootstrap.min.css") + "\" rel=\"stylesheet\" media=\"screen\" />");
			builder.Append("<script type=\"text/javascript\" language=\"javascript\" src=\"" + helper.Content("~/Assets/bootstrap/js/bootstrap.min.js") + "\"></script>", 2);
			
			return MvcHtmlString.Create(builder.ToString());
		}

		/// <summary>
		/// Returns the script link for the JS bundle
		/// </summary>
		public static MvcHtmlString JsBundle(this UrlHelper helper)
		{
			StringBuilder builder = new StringBuilder();
			string mainJs = Scripts.Render("~/Assets/Scripts/" + Bundles.JsFilename).ToHtmlString();
			mainJs = mainJs.Replace("\r\n", ""); // remove them, the lines are done in the view
			builder.AppendLine(mainJs);

			string jsVars = "";
			jsVars = ScriptLink(helper, "~/home/globaljsvars?version=" + ApplicationSettings.ProductVersion).ToHtmlString();
			jsVars = jsVars.Replace("\r\n", "");
			builder.Append(jsVars, 2); 
			return MvcHtmlString.Create(builder.ToString());
		}

		/// <summary>
		/// Returns the script link for the CSS bundle.
		/// </summary>
		public static MvcHtmlString CssBundle(this UrlHelper helper)
		{
			string html = Styles.Render("~/Assets/CSS/" + Bundles.CssFilename).ToHtmlString();
			html = html.Replace("\r\n", ""); // done in the view
			return MvcHtmlString.Create(html);
		}

		/// <summary>
		/// Sometime in the future, the JS bundle will be split out into seperate files, this is for that.
		/// </summary>
		public static MvcHtmlString EditorScriptLink(this HtmlHelper helper)
		{
			ControllerBase controller = helper.ViewContext.Controller as ControllerBase;

			if (controller != null && controller.Context.IsLoggedIn)
			{
				return MvcHtmlString.Create(Scripts.Render("~/Assets/Scripts/roadkilleditor.js").ToHtmlString());
			}
			else
			{
				return MvcHtmlString.Empty;
			}
		}
	}
}