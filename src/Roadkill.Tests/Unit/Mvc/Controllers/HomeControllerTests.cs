﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Converters;
using Roadkill.Core.Database;
using Roadkill.Core.Localization;
using Roadkill.Core.Services;
using Roadkill.Core.Security;
using Roadkill.Core.Mvc.ViewModels;
using System.Runtime.Caching;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class HomeControllerTests
	{
		private ApplicationSettings _applicationSettings;
		private IUserContext _context;
		private RepositoryMock _repository;

		private UserServiceBase _userManager;
		private PageService _pageService;
		private SearchServiceMock _searchService;
		private PageHistoryService _historyService;
		private SettingsService _settingsService;
		private PluginFactoryMock _pluginFactory;

		[SetUp]
		public void Setup()
		{
			_context = new Mock<IUserContext>().Object;
			_applicationSettings = new ApplicationSettings();
			_applicationSettings.Installed = true;
			_applicationSettings.AttachmentsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "attachments");

			// Cache
			ListCache listCache = new ListCache(_applicationSettings, CacheMock.RoadkillCache);
			SiteCache siteCache = new SiteCache(_applicationSettings, CacheMock.RoadkillCache);
			PageViewModelCache pageViewModelCache = new PageViewModelCache(_applicationSettings, CacheMock.RoadkillCache);

			// Dependencies for PageService
			Mock<SearchService> searchMock = new Mock<SearchService>();
			_pluginFactory = new PluginFactoryMock();

			_repository = new RepositoryMock();
			_settingsService = new SettingsService(_applicationSettings, _repository);
			_userManager = new Mock<UserServiceBase>(_applicationSettings, null).Object;
			_searchService = new SearchServiceMock(_applicationSettings, _repository, _pluginFactory);
			_searchService.PageContents = _repository.PageContents;
			_searchService.Pages = _repository.Pages;
			_historyService = new PageHistoryService(_applicationSettings, _repository, _context, pageViewModelCache, _pluginFactory);
			_pageService = new PageService(_applicationSettings, _repository, _searchService, _historyService, _context, listCache, pageViewModelCache, siteCache, _pluginFactory);
		}

		[Test]
		public void Index_Should_Return_Default_Message_When_No_Homepage_Tag_Exists()
		{
			// Arrange
			HomeController homeController = new HomeController(_applicationSettings, _userManager, new MarkupConverter(_applicationSettings, _repository, _pluginFactory), _pageService, _searchService, _context, _settingsService);
			homeController.SetFakeControllerContext();

			// Act
			ActionResult result = homeController.Index();

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>(), "ViewResult");

			PageViewModel summary = result.ModelFromActionResult<PageViewModel>();
			Assert.NotNull(summary, "Null model");
			Assert.That(summary.Title, Is.EqualTo(SiteStrings.NoMainPage_Title));
			Assert.That(summary.Content, Is.EqualTo(SiteStrings.NoMainPage_Label));
		}

		[Test]
		public void Index_Should_Return_Homepage_When_Tag_Exists()
		{
			// Arrange
			HomeController homeController = new HomeController(_applicationSettings, _userManager, new MarkupConverter(_applicationSettings, _repository, _pluginFactory), _pageService, _searchService, _context, _settingsService);
			homeController.SetFakeControllerContext();
			Page page1 = new Page() 
			{ 
				Id = 1, 
				Tags = "homepage, tag2", 
				Title = "Welcome to the site" 
			};
			PageContent page1Content = new PageContent() 
			{ 
				Id = Guid.NewGuid(), 
				Page = page1, 
				Text = "Hello world" 
			};
			_repository.Pages.Add(page1);
			_repository.PageContents.Add(page1Content);

			// Act
			ActionResult result = homeController.Index();
			
			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>(), "ViewResult");

			PageViewModel summary = result.ModelFromActionResult<PageViewModel>();
			Assert.NotNull(summary, "Null model");
			Assert.That(summary.Title, Is.EqualTo(page1.Title));
			Assert.That(summary.Content, Is.EqualTo(page1Content.Text));
		}

		[Test]
		public void Search_Should_Return_Some_Results_With_Unicode_Content()
		{
			// Arrange
			HomeController homeController = new HomeController(_applicationSettings, _userManager, new MarkupConverter(_applicationSettings, _repository, _pluginFactory), _pageService, _searchService, _context, _settingsService);
			homeController.SetFakeControllerContext();
			Page page1 = new Page()
			{
				Id = 1,
				Tags = "homepage, tag2",
				Title = "ОШИБКА: неверная последовательность байт для кодировки"
			};
			PageContent page1Content = new PageContent()
			{
				Id = Guid.NewGuid(),
				Page = page1,
				Text = "БД сервера событий была перенесена из PostgreSQL 8.3 на PostgreSQL 9.1.4. Сервер, развернутый на Windows платформе"
			};
			_repository.Pages.Add(page1);
			_repository.PageContents.Add(page1Content);

			// Act
			ActionResult result = homeController.Search("ОШИБКА: неверная последовательность байт для кодировки");

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>(), "ViewResult");

			List<SearchResultViewModel> searchResults = result.ModelFromActionResult<IEnumerable<SearchResultViewModel>>().ToList();
			Assert.NotNull(searchResults, "Null model");
			Assert.That(searchResults.Count(), Is.EqualTo(1));
			Assert.That(searchResults[0].Title, Is.EqualTo(page1.Title));
			Assert.That(searchResults[0].ContentSummary, Contains.Substring(page1Content.Text));
		}
	}
}
