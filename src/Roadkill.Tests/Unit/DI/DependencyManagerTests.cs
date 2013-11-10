﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Converters;
using Roadkill.Core.Database;
using Roadkill.Core.Database.LightSpeed;
using Roadkill.Core.Database.MongoDB;
using Roadkill.Core.Attachments;
using Roadkill.Core.Services;
using Roadkill.Core.Security;
using StructureMap;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Security.Windows;
using Roadkill.Core.Plugins;
using Roadkill.Core.Import;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class DependencyManagerTests
	{
		[SetUp]
		public void Setup()
		{
			RoadkillSection section = ConfigurationManager.GetSection("roadkill") as RoadkillSection;
			section.DataStoreType = "SQLite";
		}

		[Test]
		[ExpectedException(typeof(DatabaseException))]
		public void Three_Constructor_Argument_Should_Throw_If_Installed_With_No_Connection_String()
		{
			// Arrange, act, assert
			ApplicationSettings settings = new ApplicationSettings();
			settings.Installed = true;

			DependencyManager container = new DependencyManager(settings, new RepositoryMock(), new UserContext(null));
		}

		[Test]
		public void Single_Constructor_Argument_Should_Register_Default_Instances()
		{
			// Arrange
			DependencyManager container = new DependencyManager(new ApplicationSettings());

			// Act
			container.Configure();
			ApplicationSettings settings = ObjectFactory.TryGetInstance<ApplicationSettings>();
			IRepository repository = ObjectFactory.GetInstance<IRepository>();
			IUserContext context = ObjectFactory.GetInstance<IUserContext>();
			IPageService pageService = ObjectFactory.GetInstance<IPageService>();			
			MarkupConverter markupConverter = ObjectFactory.GetInstance<MarkupConverter>();
			CustomTokenParser tokenParser = ObjectFactory.GetInstance<CustomTokenParser>();
			UserViewModel userModel = ObjectFactory.GetInstance<UserViewModel>();
			SettingsViewModel settingsModel = ObjectFactory.GetInstance<SettingsViewModel>();
			AttachmentRouteHandler routerHandler = ObjectFactory.GetInstance<AttachmentRouteHandler>();
			UserServiceBase userManager = ObjectFactory.GetInstance<UserServiceBase>();
			IPluginFactory pluginFactory = ObjectFactory.GetInstance<IPluginFactory>();
			IWikiImporter wikiImporter = ObjectFactory.GetInstance<IWikiImporter>();

			// Assert
			Assert.That(settings, Is.Not.Null);
			Assert.That(repository, Is.TypeOf<LightSpeedRepository>());
			Assert.That(context, Is.TypeOf<UserContext>());
			Assert.That(pageService, Is.TypeOf<PageService>());			
			Assert.That(markupConverter, Is.TypeOf<MarkupConverter>());
			Assert.That(tokenParser, Is.TypeOf<CustomTokenParser>());
			Assert.That(userModel, Is.TypeOf<UserViewModel>());
			Assert.That(settingsModel, Is.TypeOf<SettingsViewModel>());
			Assert.That(userManager, Is.TypeOf<FormsAuthUserService>());
			Assert.That(pluginFactory, Is.TypeOf<PluginFactory>());
			Assert.That(wikiImporter, Is.TypeOf<ScrewTurnImporter>());
		}

		[Test]
		public void Should_Register_Controller_Instances()
		{
			// Arrange
			DependencyManager container = new DependencyManager(new ApplicationSettings());

			// Act
			container.Configure();
			IList<Roadkill.Core.Mvc.Controllers.ControllerBase> controllers = ObjectFactory.GetAllInstances<Roadkill.Core.Mvc.Controllers.ControllerBase>();

			// Assert
			Assert.That(controllers.Count, Is.GreaterThanOrEqualTo(9));
		}

		[Test]
		public void Should_Register_Service_Instances_When_Windows_Auth_Enabled()
		{
			// Arrange
			ApplicationSettings settings = new ApplicationSettings();
			settings.UseWindowsAuthentication = true;
			settings.LdapConnectionString = "LDAP://123.com";
			settings.EditorRoleName = "editor;";
			settings.AdminRoleName = "admins";

			DependencyManager container = new DependencyManager(new ApplicationSettings());

			// Act
			container.Configure();

			// fake some AD settings for the AD service
			ObjectFactory.Inject<ApplicationSettings>(settings);
	
			IList<ServiceBase> services = ObjectFactory.GetAllInstances<ServiceBase>();

			// Assert
			Assert.That(services.Count, Is.GreaterThanOrEqualTo(7));
		}

		[Test]
		public void Custom_Configuration_Repository_And_Context_Types_Should_Be_Registered()
		{
			// Arrange
			ApplicationSettings settings = new ApplicationSettings();
			DependencyManager container = new DependencyManager(settings, new RepositoryMock(), new RoadkillContextStub());

			// Act
			container.Configure();
			IRepository repository = ObjectFactory.GetInstance<IRepository>();
			IUserContext context = ObjectFactory.GetInstance<IUserContext>();

			// Assert
			Assert.That(repository, Is.TypeOf<RepositoryMock>());
			Assert.That(context, Is.TypeOf<RoadkillContextStub>());
		}

		[Test]
		public void WindowsAuth_Should_Register_ActiveDirectoryUserManager()
		{
			// Arrange
			ApplicationSettings settings = new ApplicationSettings();
			settings.UseWindowsAuthentication = true;
			settings.LdapConnectionString = "LDAP://123.com";
			settings.EditorRoleName = "editor;";
			settings.AdminRoleName = "admins";

			DependencyManager container = new DependencyManager(settings, new RepositoryMock(), new RoadkillContextStub());

			// Act
			container.Configure();
			UserServiceBase usermanager = ObjectFactory.GetInstance<UserServiceBase>();

			// Assert
			Assert.That(usermanager, Is.TypeOf<ActiveDirectoryUserService>());
		}

		[Test]
		public void RegisterMvcFactoriesAndRouteHandlers_Should_Register_Instances()
		{
			// Arrange
			DependencyManager iocSetup = new DependencyManager(new ApplicationSettings());

			// Act
			iocSetup.Configure();
			iocSetup.ConfigureMvc();

			// Assert
			Assert.That(RouteTable.Routes.Count, Is.EqualTo(1));
			Assert.That(((Route)RouteTable.Routes[0]).RouteHandler, Is.TypeOf<AttachmentRouteHandler>());
			Assert.True(ModelBinders.Binders.ContainsKey(typeof(SettingsViewModel)));
			Assert.True(ModelBinders.Binders.ContainsKey(typeof(UserViewModel)));
		}

		[Test]
		[ExpectedException(typeof(IoCException))]
		public void RegisterMvcFactoriesAndRouteHandlers_Requires_Run_First()
		{
			// Arrange
			DependencyManager container = new DependencyManager(new ApplicationSettings());

			// Act
			container.ConfigureMvc();

			// Assert
		}

		[Test]
		public void Should_Load_Custom_Repository_From_DatabaseType()
		{
			// Arrange
			ApplicationSettings applicationSettings = new ApplicationSettings();
			applicationSettings.DataStoreType = DataStoreType.MongoDB;
			DependencyManager container = new DependencyManager(applicationSettings);

			// Act
			container.Configure();

			// Assert
			IRepository respository = ObjectFactory.GetInstance<IRepository>();
			Assert.That(respository, Is.TypeOf<MongoDBRepository>());
		}

		[Test]
		public void Should_Copy_Plugins()
		{
			
		}

		[Test]
		public void Should_Load_Custom_UserService()
		{
			// Arrange
			ApplicationSettings applicationSettings = new ApplicationSettings();
			applicationSettings.UserServiceType = "Roadkill.Tests.UserServiceStub";
			DependencyManager iocSetup = new DependencyManager(applicationSettings);

			// Put the UserServiceStub in a new assembly so we can test it's loaded
			string tempFilename = Path.GetFileName(Path.GetTempFileName()) + ".dll";
			string thisAssembly = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Roadkill.Tests.dll");
			string pluginSourceDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", "UserService");
			string destPlugin = Path.Combine(pluginSourceDir, tempFilename);

			if (!Directory.Exists(pluginSourceDir))
				Directory.CreateDirectory(pluginSourceDir);

			File.Copy(thisAssembly, destPlugin, true);

			// Act
			iocSetup.Configure();

			// Assert
			UserServiceBase userManager = ObjectFactory.GetInstance<UserServiceBase>();
			Assert.That(userManager.GetType().FullName, Is.EqualTo("Roadkill.Tests.UserServiceStub"));
		}
	}
}
