﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core.Configuration;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class SiteSettingsTests
	{
		[Test]
		public void Deserialize_Should_Have_Correct_Values_With_Valid_Json()
		{
			// Arrange
			string json = @"{
							  ""AllowedFileTypes"": ""pdf, swf, avi"",
							  ""AllowUserSignup"": true,
							  ""IsRecaptchaEnabled"": true,
							  ""MarkupType"": ""Markdown"",
							  ""RecaptchaPrivateKey"": ""captchaprivatekey"",
							  ""RecaptchaPublicKey"": ""captchapublickey"",
							  ""SiteUrl"": ""http://siteurl"",
							  ""SiteName"": ""my sitename"",
							  ""Theme"": ""Mytheme"",
							  ""OverwriteExistingFiles"": true,
							  ""HeadContent"": ""<script type=\""text/javascript\"">alert('foo');</script>"",
							  ""MenuMarkup"": ""* %allpages*""
							}";

			// Act
			SiteSettings settings = SiteSettings.LoadFromJson(json);

			// Assert
			Assert.That(settings.AllowedFileTypes, Is.EqualTo("pdf, swf, avi"));
			Assert.That(settings.AllowedFileTypesList, Contains.Item("pdf"));
			Assert.That(settings.AllowedFileTypesList, Contains.Item("swf"));
			Assert.That(settings.AllowedFileTypesList, Contains.Item("avi"));
			Assert.That(settings.AllowUserSignup, Is.EqualTo(true));
			Assert.That(settings.IsRecaptchaEnabled, Is.EqualTo(true));
			Assert.That(settings.MarkupType, Is.EqualTo("Markdown"));
			Assert.That(settings.RecaptchaPrivateKey, Is.EqualTo("captchaprivatekey"));
			Assert.That(settings.RecaptchaPublicKey, Is.EqualTo("captchapublickey"));
			Assert.That(settings.SiteUrl, Is.EqualTo("http://siteurl"));
			Assert.That(settings.SiteName, Is.EqualTo("my sitename"));
			Assert.That(settings.Theme, Is.EqualTo("Mytheme"));

			// 1.8
			Assert.That(settings.OverwriteExistingFiles, Is.EqualTo(true));
			Assert.That(settings.HeadContent, Is.EqualTo("<script type=\"text/javascript\">alert('foo');</script>"));
			Assert.That(settings.MenuMarkup, Is.EqualTo("* %allpages*"));
		}

		[Test]
		public void Deserialize_Should_Have_Correct_Values_When_Json_Has_Unknown_Properties()
		{
			// Arrange
			string json = @"{
							  ""SomeProperty"": ""blah"",
							  ""AllowedFileTypes"": ""pdf, swf, avi"",
							  ""AllowUserSignup"": true,
							  ""IsRecaptchaEnabled"": true,
							  ""MarkupType"": ""Markdown"",
							  ""RecaptchaPrivateKey"": ""captchaprivatekey"",
							  ""RecaptchaPublicKey"": ""captchapublickey"",
							  ""SiteUrl"": ""http://siteurl"",
							  ""SiteName"": ""my sitename"",
							  ""Theme"": ""Mytheme"",
							  ""Youswipe"": ""Youstay"",
							  ""YouGo"": ""Youstay"",
							  ""YouGo"": ""Youstay"",
							  ""HeadContent"": ""head content"",
							  ""MenuMarkup"": ""menu markup""
							}";

			// Act
			SiteSettings settings = SiteSettings.LoadFromJson(json);

			// Assert
			Assert.That(settings.AllowedFileTypes, Is.EqualTo("pdf, swf, avi"));
			Assert.That(settings.AllowedFileTypesList, Contains.Item("pdf"));
			Assert.That(settings.AllowedFileTypesList, Contains.Item("swf"));
			Assert.That(settings.AllowedFileTypesList, Contains.Item("avi"));
			Assert.That(settings.AllowUserSignup, Is.EqualTo(true));
			Assert.That(settings.IsRecaptchaEnabled, Is.EqualTo(true));
			Assert.That(settings.MarkupType, Is.EqualTo("Markdown"));
			Assert.That(settings.RecaptchaPrivateKey, Is.EqualTo("captchaprivatekey"));
			Assert.That(settings.RecaptchaPublicKey, Is.EqualTo("captchapublickey"));
			Assert.That(settings.SiteUrl, Is.EqualTo("http://siteurl"));
			Assert.That(settings.SiteName, Is.EqualTo("my sitename"));
			Assert.That(settings.Theme, Is.EqualTo("Mytheme"));
			Assert.That(settings.HeadContent, Is.EqualTo("head content"));
			Assert.That(settings.MenuMarkup, Is.EqualTo("menu markup"));
		}

		[Test]
		public void Deserialize_Should_Have_Correct_Values_With_Fragment_Of_Json()
		{
			// Arrange
			string json = @"{
							  ""MarkupType"": ""Markdown"",
							  ""RecaptchaPrivateKey"": ""captchaprivatekey"",
							  ""RecaptchaPublicKey"": ""captchapublickey"",
							  ""SiteUrl"": ""http://siteurl"",
							  ""SiteName"": ""my sitename"",
							}";

			// Act
			SiteSettings settings = SiteSettings.LoadFromJson(json);

			// Assert
			Assert.That(settings.MarkupType, Is.EqualTo("Markdown"));
			Assert.That(settings.RecaptchaPrivateKey, Is.EqualTo("captchaprivatekey"));
			Assert.That(settings.RecaptchaPublicKey, Is.EqualTo("captchapublickey"));
			Assert.That(settings.SiteUrl, Is.EqualTo("http://siteurl"));
			Assert.That(settings.SiteName, Is.EqualTo("my sitename"));
		}

		[Test]
		public void Deserialize_Should_Have_Default_Values_With_Empty_Json()
		{
			// Arrange
			string json = "";

			// Act
			SiteSettings settings = SiteSettings.LoadFromJson(json);

			// Assert
			Assert.That(settings.AllowedFileTypes, Is.EqualTo("jpg, png, gif"));
			Assert.That(settings.AllowedFileTypesList, Contains.Item("jpg"));
			Assert.That(settings.AllowedFileTypesList, Contains.Item("png"));
			Assert.That(settings.AllowedFileTypesList, Contains.Item("gif"));
			Assert.That(settings.AllowUserSignup, Is.EqualTo(false));
			Assert.That(settings.IsRecaptchaEnabled, Is.EqualTo(false));
			Assert.That(settings.MarkupType, Is.EqualTo("Creole"));
			Assert.That(settings.RecaptchaPrivateKey, Is.EqualTo(""));
			Assert.That(settings.RecaptchaPublicKey, Is.EqualTo(""));
			Assert.That(settings.SiteUrl, Is.EqualTo(""));
			Assert.That(settings.SiteName, Is.EqualTo("Your site"));
			Assert.That(settings.Theme, Is.EqualTo("Mediawiki"));

			// v1.8
			Assert.That(settings.OverwriteExistingFiles, Is.EqualTo(false));
			Assert.That(settings.HeadContent, Is.EqualTo(""));
			Assert.That(settings.MenuMarkup, Is.EqualTo(settings.GetDefaultMenuMarkup()));
		}

		[Test]
		public void Deserialize_Should_Have_Default_Values_With_Invalid_Json()
		{
			// Arrange
			string json = "asdf";

			// Act
			SiteSettings settings = SiteSettings.LoadFromJson(json);

			// Assert
			Assert.That(settings.AllowedFileTypes, Is.EqualTo("jpg, png, gif"));
			Assert.That(settings.AllowedFileTypesList, Contains.Item("jpg"));
			Assert.That(settings.AllowedFileTypesList, Contains.Item("png"));
			Assert.That(settings.AllowedFileTypesList, Contains.Item("gif"));
			Assert.That(settings.AllowUserSignup, Is.EqualTo(false));
			Assert.That(settings.IsRecaptchaEnabled, Is.EqualTo(false));
			Assert.That(settings.MarkupType, Is.EqualTo("Creole"));
			Assert.That(settings.RecaptchaPrivateKey, Is.EqualTo(""));
			Assert.That(settings.RecaptchaPublicKey, Is.EqualTo(""));
			Assert.That(settings.SiteUrl, Is.EqualTo(""));
			Assert.That(settings.SiteName, Is.EqualTo("Your site"));
			Assert.That(settings.Theme, Is.EqualTo("Mediawiki"));

			// 1.8
			Assert.That(settings.OverwriteExistingFiles, Is.EqualTo(false));
			Assert.That(settings.HeadContent, Is.EqualTo(""));
			Assert.That(settings.MenuMarkup, Is.EqualTo(settings.GetDefaultMenuMarkup()));
		}

		[Test]
		public void Deserialize_Should_Have_Default_MenuMarkup_When_Json_Value_Is_Null()
		{
			// Arrange
			string json = @"{
							  ""AllowedFileTypes"": ""pdf, swf, avi"",
							  ""AllowUserSignup"": true,
							  ""IsRecaptchaEnabled"": true,
							  ""MarkupType"": ""Markdown"",
							  ""RecaptchaPrivateKey"": ""captchaprivatekey"",
							  ""RecaptchaPublicKey"": ""captchapublickey"",
							  ""SiteUrl"": ""http://siteurl"",
							  ""SiteName"": ""my sitename"",
							  ""Theme"": ""Mytheme"",
							}";

			// Act
			SiteSettings settings = SiteSettings.LoadFromJson(json);

			// Assert
			Assert.That(settings.MenuMarkup, Is.EqualTo(settings.GetDefaultMenuMarkup()));
		}

		[Test]
		public void GetJson_Should_Return_Known_Json()
		{
			// Arrange
			string expectedJson = @"{
  ""AllowedFileTypes"": ""pdf, swf, avi"",
  ""AllowUserSignup"": true,
  ""IsRecaptchaEnabled"": true,
  ""MarkupType"": ""Markdown"",
  ""RecaptchaPrivateKey"": ""captchaprivatekey"",
  ""RecaptchaPublicKey"": ""captchapublickey"",
  ""SiteUrl"": ""http://siteurl"",
  ""SiteName"": ""my sitename"",
  ""Theme"": ""Mytheme"",
  ""OverwriteExistingFiles"": false,
  ""HeadContent"": """",
  ""MenuMarkup"": ""* %mainpage%\r\n* %categories%\r\n* %allpages%\r\n* %newpage%\r\n* %managefiles%\r\n* %sitesettings%\r\n\r\n""
}";

			SiteSettings settings = new SiteSettings();
			settings.AllowedFileTypes = "pdf, swf, avi";
			settings.AllowUserSignup = true;
			settings.IsRecaptchaEnabled = true;
			settings.MarkupType = "Markdown";
			settings.RecaptchaPrivateKey = "captchaprivatekey";
			settings.RecaptchaPublicKey = "captchapublickey";
			settings.SiteUrl = "http://siteurl";
			settings.SiteName = "my sitename";
			settings.Theme = "Mytheme";

			// Act
			string actualJson = settings.GetJson();

			// Assert
			Assert.That(actualJson, Is.EqualTo(expectedJson), actualJson);
		}

		// The two previous default value tests might make this test redundant
		[Test]
		public void Deserialize_Should_Have_Default_Values_For_New_v1_8_Settings()
		{
			// Arrange
			string json = @"{
							  ""AllowedFileTypes"": ""pdf, swf, avi"",
							  ""AllowUserSignup"": true,
							  ""IsRecaptchaEnabled"": true,
							  ""MarkupType"": ""Markdown"",
							  ""RecaptchaPrivateKey"": ""captchaprivatekey"",
							  ""RecaptchaPublicKey"": ""captchapublickey"",
							  ""SiteUrl"": ""http://siteurl"",
							  ""SiteName"": ""my sitename"",
							  ""Theme"": ""Mytheme""
							}";

			// Act
			SiteSettings settings = SiteSettings.LoadFromJson(json);

			// Assert
			Assert.That(settings.OverwriteExistingFiles, Is.EqualTo(false));
			Assert.That(settings.HeadContent, Is.Empty);
			Assert.That(settings.MenuMarkup, Is.EqualTo(settings.GetDefaultMenuMarkup()));
		}
	}
}
