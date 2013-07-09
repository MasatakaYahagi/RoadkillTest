﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using System.Globalization;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Core
{
	/// <summary>
	/// The template for password reset emails.
	/// </summary>
	public class ResetPasswordEmail : Email
	{
		private static string _htmlContent;
		private static string _plainTextContent;

		public ResetPasswordEmail(ApplicationSettings applicationSettings, SiteSettings siteSettings)
			: base(applicationSettings, siteSettings)
		{
		}

		public override void Send(UserSummary summary)
		{
			// Thread safety should not be an issue here
			if (string.IsNullOrEmpty(_plainTextContent))
				_plainTextContent = ReadTemplateFile("ResetPassword.txt");

			if (string.IsNullOrEmpty(_htmlContent))
				_htmlContent = ReadTemplateFile("ResetPassword.html");

			PlainTextView = _plainTextContent;
			HtmlView = _htmlContent;

			base.Send(summary);
		}
	}
}
