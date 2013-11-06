﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using PluginSettings = Roadkill.Core.Plugins.Settings;

namespace Roadkill.Core.Plugins.BuiltIn
{
	public class SyntaxHighlighter : TextPlugin
	{
		internal static readonly string _regexString = @"\[\[\[code lang=(?'lang'.*?)\|(?'code'.*?)\]\]\]";
		internal static readonly Regex _variableRegex = new Regex(_regexString, RegexOptions.Singleline | RegexOptions.Compiled);
		internal static string _replacePattern = "<pre class=\"brush: ${lang}\">${code}</pre>";

		public override bool IsEnabled
		{
			get
			{
				return false;
			}
		}

		public override string Id
		{
			get 
			{ 
				return "SyntaxHighlighter";	
			}
		}

		public override string Name
		{
			get
			{
				return "Syntax Highlighter";
			}
		}

		public override string Description
		{
			get
			{
				return "Syntax highlights a code block, using the language you specify. Example:\n\n" +
						"[[[code lang=sql|ENTER YOUR CODE HERE]]]";
			}
		}

		public override string Version
		{

			get
			{
				return "1.0";
			}
		}

		static SyntaxHighlighter()
		{
			_replacePattern = ParserSafeToken(_replacePattern);
		}

		public override void OnInitializeSettings(Settings settings)
		{
			settings.SetValue("name", "value");
		}

		public override string BeforeParse(string text)
		{
			if (_variableRegex.IsMatch(text))
			{
				// Replaces the {{{roadkillinternal[[[code lang=sql|xxx]]]roadkillinternal}}}
				// with the HTML pre tags. As the code is HTML encoded, it doesn't get butchered by the HTML cleaner.
				MatchCollection matches = _variableRegex.Matches(text);
				foreach (Match match in matches)
				{
					string language = match.Groups["lang"].Value;
					string code = HttpUtility.HtmlEncode(match.Groups["code"].Value);
					text = text.Replace(match.Groups["code"].Value, code);

					text = Regex.Replace(text, _regexString, _replacePattern, _variableRegex.Options);
				}
			}

			return text;
		}

		public override string AfterParse(string html)
		{
			html = RemoveParserIgnoreTokens(html);

			// Undo the HTML sanitizer's attribute cleaning on the pre's.
			html = html.Replace("<pre class=\"brush&#x3A;&#x20;c&#x23;", "<pre class=\"brush: c#");
			html = html.Replace("<pre class=\"brush&#x3A;&#x20;", "<pre class=\"brush: ");

			return html;
		}

		public override string GetHeadContent()
		{
			string html = "";

			foreach (string file in HeadContent.CssFiles)
			{
				html += GetCssLink("css/" +file);
			}

			foreach (string file in HeadContent.JsFiles)
			{
				AddScript("javascript/" + file);
			}

			SetHeadJsOnLoadedFunction("SyntaxHighlighter.all()");
			html += GetJavascriptHtml();

			return html;
		}

		private class HeadContent
		{
			public static string[] CssFiles = 
			{
				"shCore.css",
				"shThemeDefault.css"
			};

			public static string[] JsFiles = 
			{
				"shCore.js", // needs to be 1st
				"shBrushAppleScript.js",
				"shBrushAS3.js",
				"shBrushBash.js",
				"shBrushColdFusion.js",
				"shBrushCpp.js",
				"shBrushCSharp.js",
				"shBrushCss.js",
				"shBrushDelphi.js",
				"shBrushDiff.js",
				"shBrushErlang.js",
				"shBrushGroovy.js",
				"shBrushJava.js",
				"shBrushJavaFX.js",
				"shBrushJScript.js",
				"shBrushPerl.js",
				"shBrushPhp.js",
				"shBrushPlain.js",
				"shBrushPowerShell.js",
				"shBrushPython.js",
				"shBrushRuby.js",
				"shBrushSass.js",
				"shBrushScala.js",
				"shBrushSql.js",
				"shBrushVb.js",
				"shBrushXml.js",
			};
		}
	}
}
