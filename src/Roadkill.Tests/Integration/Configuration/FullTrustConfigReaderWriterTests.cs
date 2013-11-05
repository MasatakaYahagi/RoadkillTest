using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.DI;
using Roadkill.Core.Logging;
using Roadkill.Core.Services;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Security;
using Roadkill.Core.Security.Windows;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Description("Tests for both database and .config file settings.")]
	[Category("Integration")]
	public class FullTrustConfigReaderWriterTests
	{
		private ApplicationSettings _settings;

		[SetUp]
		public void Setup()
		{
			_settings = new ApplicationSettings();
		}

		[Test]
		public void RoadkillSection_Properties_Have_Correct_Key_Mappings_And_Values()
		{
			// Arrange
			string configFilePath = GetConfigPath("test.config");

			// Act
			FullTrustConfigReaderWriter configManager = new FullTrustConfigReaderWriter(configFilePath);
			ApplicationSettings appSettings = configManager.GetApplicationSettings();

			// Assert
			Assert.That(appSettings.AdminRoleName, Is.EqualTo("Admin-test"), "AdminRoleName");
			Assert.That(appSettings.AttachmentsRoutePath, Is.EqualTo("AttachmentsRoutePathTest"), "AttachmentsRoutePath"); 
			Assert.That(appSettings.AttachmentsFolder, Is.EqualTo("/Attachments-test"), "AttachmentsFolder");
			Assert.That(appSettings.UseObjectCache, Is.True, "UseObjectCache");
			Assert.That(appSettings.UseBrowserCache, Is.True, "UseBrowserCache");
			Assert.That(appSettings.ConnectionStringName, Is.EqualTo("Roadkill-test"), "ConnectionStringName");
			Assert.That(appSettings.DataStoreType, Is.EqualTo(DataStoreType.Sqlite), "DatabaseType");
			Assert.That(appSettings.EditorRoleName, Is.EqualTo("Editor-test"), "EditorRoleName");
			Assert.That(appSettings.IgnoreSearchIndexErrors, Is.True, "IgnoreSearchIndexErrors");
			Assert.That(appSettings.Installed, Is.True, "Installed");
			Assert.That(appSettings.IsPublicSite, Is.False, "IsPublicSite");
			Assert.That(appSettings.LdapConnectionString, Is.EqualTo("ldapstring-test"), "LdapConnectionString");
			Assert.That(appSettings.LdapPassword, Is.EqualTo("ldappassword-test"), "LdapPassword");
			Assert.That(appSettings.LdapUsername, Is.EqualTo("ldapusername-test"), "LdapUsername");
			Assert.That(appSettings.LoggingTypes, Is.EqualTo("All"), "LoggingType");
			Assert.That(appSettings.LogErrorsOnly, Is.False, "LogErrorsOnly");
			Assert.That(appSettings.UseHtmlWhiteList, Is.EqualTo(false), "UseHtmlWhiteList");
			Assert.That(appSettings.UserManagerType, Is.EqualTo("DefaultUserManager-test"), "DefaultUserManager");
			Assert.That(appSettings.UseWindowsAuthentication, Is.False, "UseWindowsAuthentication");
		}

		[Test]
		public void RoadkillSection_Optional_Settings_With_Missing_Values_Have_Default_Values()
		{
			// Arrange
			string configFilePath = GetConfigPath("test-optional-values.config");

			// Act
			FullTrustConfigReaderWriter configManager = new FullTrustConfigReaderWriter(configFilePath);
			ApplicationSettings appSettings = configManager.GetApplicationSettings();

			// Assert
			Assert.That(appSettings.AttachmentsRoutePath, Is.EqualTo("Attachments"), "AttachmentsRoutePath");
			Assert.That(appSettings.DataStoreType, Is.EqualTo(DataStoreType.SqlServer2005), "DatabaseType");
			Assert.That(appSettings.IgnoreSearchIndexErrors, Is.False, "IgnoreSearchIndexErrors");
			Assert.That(appSettings.IsPublicSite, Is.True, "IsPublicSite");
			Assert.That(appSettings.LdapConnectionString, Is.EqualTo(""), "LdapConnectionString");
			Assert.That(appSettings.LdapPassword, Is.EqualTo(""), "LdapPassword");
			Assert.That(appSettings.LdapUsername, Is.EqualTo(""), "LdapUsername");
			Assert.That(appSettings.LoggingTypes, Is.EqualTo("None"), "LoggingType");
			Assert.That(appSettings.LogErrorsOnly, Is.True, "LoggingType");
			Assert.That(appSettings.UseHtmlWhiteList, Is.EqualTo(true), "UseHtmlWhiteList");
			Assert.That(appSettings.UserManagerType, Is.EqualTo(""), "DefaultUserManager");
		}

		[Test]
		public void Connection_Setting_Should_Find_Connection_Value()
		{
			// Arrange
			string configFilePath = GetConfigPath("test.config");

			// Act
			FullTrustConfigReaderWriter configManager = new FullTrustConfigReaderWriter(configFilePath);
			ApplicationSettings appSettings = configManager.GetApplicationSettings();

			// Assert
			Assert.That(appSettings.ConnectionString, Is.EqualTo("connectionstring-test"), "ConnectionStringName");
		}

		[Test]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void RoadkillSection_Missing_Values_Throw_Exception()
		{
			// Arrange
			string configFilePath = GetConfigPath("test-missing-values.config");

			// Act
			FullTrustConfigReaderWriter configManager = new FullTrustConfigReaderWriter(configFilePath);
			
			// Assert
		}

		[Test]
		public void RoadkillSection_Legacy_CacheValues_Are_Ignored()
		{
			// Arrange
			string configFilePath = GetConfigPath("test-legacy-values.config");

			// Act
			FullTrustConfigReaderWriter configManager = new FullTrustConfigReaderWriter(configFilePath);
			ApplicationSettings appSettings = configManager.GetApplicationSettings();

			// Assert
			Assert.That(appSettings.UseObjectCache, Is.True, "UseObjectCache [legacy test for cacheEnabled]");
			Assert.That(appSettings.UseBrowserCache, Is.False, "UseBrowserCache [legacy test for cacheText]");
		}

		[Test]
		public void RoadkillSection_Legacy_DatabaseType_Is_Used()
		{
			// Arrange
			string configFilePath = GetConfigPath("test-legacy-values.config");

			// Act
			FullTrustConfigReaderWriter configManager = new FullTrustConfigReaderWriter(configFilePath);
			ApplicationSettings appSettings = configManager.GetApplicationSettings();

			// Assert
			Assert.That(appSettings.DataStoreType, Is.EqualTo(DataStoreType.Sqlite), "DataStoreType [legacy test for databaseType]");
		}

		[Test]
		public void SettingsService_Should_Save_Settings()
		{
			// Arrange
			SiteSettings siteSettings = new SiteSettings()
			{
				AllowedFileTypes = "jpg, png, gif",
				AllowUserSignup = true,
				IsRecaptchaEnabled = true,
				MarkupType = "markuptype",
				RecaptchaPrivateKey = "privatekey",
				RecaptchaPublicKey = "publickey",
				SiteName = "sitename",
				SiteUrl = "siteurl",
				Theme = "theme",
			};
			SettingsViewModel validConfigSettings = new SettingsViewModel()
			{
				AllowedFileTypes = "jpg, png, gif",
				AllowUserSignup = true,
				IsRecaptchaEnabled = true,
				MarkupType = "markuptype",
				RecaptchaPrivateKey = "privatekey",
				RecaptchaPublicKey = "publickey",
				SiteName = "sitename",
				SiteUrl = "siteurl",
				Theme = "theme",
			};

			RepositoryMock repository = new RepositoryMock();

			DependencyManager iocSetup = new DependencyManager(_settings, repository, new UserContext(null)); // context isn't used
			iocSetup.Configure();
			SettingsService settingsService = new SettingsService(_settings, repository);

			// Act
			settingsService.SaveSiteSettings(validConfigSettings);

			// Assert
			SiteSettings actualSettings = settingsService.GetSiteSettings();

			Assert.That(actualSettings.AllowedFileTypes.Contains("jpg"), "AllowedFileTypes jpg");
			Assert.That(actualSettings.AllowedFileTypes.Contains("gif"), "AllowedFileTypes gif");
			Assert.That(actualSettings.AllowedFileTypes.Contains("png"), "AllowedFileTypes png");
			Assert.That(actualSettings.AllowUserSignup, Is.True, "AllowUserSignup");
			Assert.That(actualSettings.IsRecaptchaEnabled, Is.True, "IsRecaptchaEnabled");
			Assert.That(actualSettings.MarkupType, Is.EqualTo("markuptype"), "MarkupType");
			Assert.That(actualSettings.RecaptchaPrivateKey, Is.EqualTo("privatekey"), "RecaptchaPrivateKey");
			Assert.That(actualSettings.RecaptchaPublicKey, Is.EqualTo("publickey"), "RecaptchaPublicKey");
			Assert.That(actualSettings.SiteName, Is.EqualTo("sitename"), "SiteName");
			Assert.That(actualSettings.SiteUrl, Is.EqualTo("siteurl"), "SiteUrl");
			Assert.That(actualSettings.Theme, Is.EqualTo("theme"), "Theme");
		}
		
		[Test]
		public void UseWindowsAuth_Should_Load_ActiveDirectory_UserManager()
		{
			// Arrange
			Mock<IRepository> mockRepository = new Mock<IRepository>();
			Mock<IUserContext> mockContext = new Mock<IUserContext>();

			ApplicationSettings settings = new ApplicationSettings();
			settings.UseWindowsAuthentication = true;
			settings.LdapConnectionString = "LDAP://dc=roadkill.org";
			settings.AdminRoleName = "editors";
			settings.EditorRoleName = "editors";

			// Act
			DependencyManager iocSetup = new DependencyManager(settings, mockRepository.Object, mockContext.Object);
			iocSetup.Configure();

			// Assert
			Assert.That(ServiceLocator.GetInstance<UserServiceBase>(), Is.TypeOf(typeof(ActiveDirectoryUserService)));
		}
		
		[Test]
		public void Should_Use_DefaultUserManager_By_Default()
		{
			// Arrange
			Mock<IRepository> mockRepository = new Mock<IRepository>();
			Mock<IUserContext> mockContext = new Mock<IUserContext>();
			ApplicationSettings settings = new ApplicationSettings();

			// Act
			DependencyManager iocSetup = new DependencyManager(settings, mockRepository.Object, mockContext.Object);
			iocSetup.Configure();

			// Assert
			Assert.That(ServiceLocator.GetInstance<UserServiceBase>(), Is.TypeOf(typeof(FormsAuthUserService)));
		}

		[Test]
		[Description("Test for the Save when it's called from the settings page, and installation")]
		public void Should_Save_All_ApplicationSettings()
		{
			// Arrange
			string configFilePath = GetConfigPath("test-empty.config");
			SettingsViewModel viewModel = new SettingsViewModel()
			{
				AdminRoleName = "admin role name",
				AttachmentsFolder = @"c:\AttachmentsFolder",
				UseObjectCache = true,
				UseBrowserCache = true,
				ConnectionString = "connection string",
				DataStoreTypeName = "MongoDB",
				EditorRoleName = "editor role name",
				LdapConnectionString = "ldap connection string",
				LdapUsername = "ldap username",
				LdapPassword = "ldap password",
				UseWindowsAuth = true,
				IsPublicSite = false,
				IgnoreSearchIndexErrors = false
			};

			// Act
			FullTrustConfigReaderWriter configManager = new FullTrustConfigReaderWriter(configFilePath);
			configManager.Save(viewModel);

			ApplicationSettings appSettings = configManager.GetApplicationSettings();

			// Assert
			Assert.That(appSettings.AdminRoleName, Is.EqualTo(viewModel.AdminRoleName), "AdminRoleName");
			Assert.That(appSettings.AttachmentsFolder, Is.EqualTo(viewModel.AttachmentsFolder), "AttachmentsFolder");
			Assert.That(appSettings.UseObjectCache, Is.EqualTo(viewModel.UseObjectCache), "UseObjectCache");
			Assert.That(appSettings.UseBrowserCache, Is.EqualTo(viewModel.UseBrowserCache), "UseBrowserCache");
			Assert.That(appSettings.ConnectionString, Is.EqualTo(viewModel.ConnectionString), "ConnectionStringName");
			Assert.That(appSettings.DataStoreType, Is.EqualTo(DataStoreType.MongoDB), "DatabaseType");
			Assert.That(appSettings.EditorRoleName, Is.EqualTo(viewModel.EditorRoleName), "EditorRoleName");
			Assert.That(appSettings.IgnoreSearchIndexErrors, Is.EqualTo(viewModel.IgnoreSearchIndexErrors), "IgnoreSearchIndexErrors");
			Assert.That(appSettings.IsPublicSite, Is.EqualTo(viewModel.IsPublicSite), "IsPublicSite");
			Assert.That(appSettings.LdapConnectionString, Is.EqualTo(viewModel.LdapConnectionString), "LdapConnectionString");
			Assert.That(appSettings.LdapPassword, Is.EqualTo(viewModel.LdapPassword), "LdapPassword");
			Assert.That(appSettings.LdapUsername, Is.EqualTo(viewModel.LdapUsername), "LdapUsername");
			Assert.That(appSettings.UseWindowsAuthentication, Is.EqualTo(viewModel.UseWindowsAuth), "UseWindowsAuthentication");
			Assert.That(appSettings.Installed, Is.True, "Installed");
		}

		[Test]
		[Explicit]
		public void MongoDB_databaseType_Should_Load_Repository()
		{
			// Arrange
			Mock<IRepository> mockRepository = new Mock<IRepository>();
			Mock<IUserContext> mockContext = new Mock<IUserContext>();
			ApplicationSettings settings = new ApplicationSettings();
			settings.DataStoreType = DataStoreType.MongoDB;

			// Act
			DependencyManager iocContainer = new DependencyManager(settings, mockRepository.Object, mockContext.Object);
			iocContainer.Configure();

			// Assert
			Assert.That(ServiceLocator.GetInstance<UserServiceBase>(), Is.TypeOf(typeof(FormsAuthUserService)));
		}

		private string GetConfigPath(string filename)
		{
			return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Integration", "Configuration", "TestConfigs", filename);
		}
	}
}