﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Managers;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Tests.Unit;

namespace Roadkill.Tests.Integration
{
	[TestFixture]
	[Category("Integration")]
	public class SearchManagerTests
	{
		private IRepository _repository;
		private ApplicationSettings _config;

		[SetUp]
		public void Initialize()
		{
			string indexPath = AppDomain.CurrentDomain.BaseDirectory + @"\App_Data\SearchTests";
			if (Directory.Exists(indexPath))
				Directory.Delete(indexPath, true);

			_repository = new RepositoryMock();
			_config = new ApplicationSettings();
			_config.Installed = true;
		}

		[Test]
		public void Search_With_No_Field_Returns_Results()
		{
			// Arrange
			SearchManager searchManager = new SearchManager(_config, _repository);
			searchManager.CreateIndex();

			PageSummary page1 = CreatePage(1, "admin", "title content", "tag1", "title content1");
			PageSummary page2 = CreatePage(2, "admin", "title content", "tag1", "title content2");

			searchManager.Add(page1);
			searchManager.Add(page2);

			// Act
			List<SearchResult> results = searchManager.Search("title content").ToList();

			// Assert
			Assert.That(results.Count, Is.EqualTo(2)); // Lucene will ignore the 1 and 2 in the content
		}

		[Test]
		public void Search_By_Title()
		{
			// Arrange
			SearchManager searchManager = new SearchManager(_config, _repository);
			searchManager.CreateIndex();

			PageSummary page1 = CreatePage(1, "admin", "the title", "tag1", "title content");
			PageSummary page2 = CreatePage(2, "admin", "random name1", "tag1", "title content");
			PageSummary page3 = CreatePage(3, "admin", "random name2", "tag1", "title content");
			PageSummary page4 = CreatePage(4, "admin", "random name3", "tag1", "title content");

			searchManager.Add(page1);
			searchManager.Add(page2);
			searchManager.Add(page3);
			searchManager.Add(page4);

			// Act
			List<SearchResult> results = searchManager.Search("title:\"the title\"").ToList();

			// Assert
			Assert.That(results.Count, Is.EqualTo(1));
		}

		[Test]
		public void Search_By_TagsField_Returns_Multiple_Results()
		{
			// Arrange
			SearchManager searchManager = new SearchManager(_config, _repository);
			searchManager.CreateIndex();

			PageSummary page1 = CreatePage(1, "admin", "random name1", "homepage1, tag1", "title content");
			PageSummary page2 = CreatePage(2, "admin", "random name2", "tag1, tag", "title content");
			PageSummary page3 = CreatePage(3, "admin", "random name3", "tag3, tag", "title content");
			PageSummary page4 = CreatePage(4, "admin", "random name4", "tag4, tag", "title content");

			searchManager.Add(page1);
			searchManager.Add(page2);
			searchManager.Add(page3);
			searchManager.Add(page4);

			// Act
			List<SearchResult> results = searchManager.Search("tags:\"tag1\"").ToList();

			// Assert
			Assert.That(results.Count, Is.EqualTo(2));
		}

		[Test]
		public void Search_By_IdField_Returns_Single_Results()
		{
			// Arrange
			SearchManager searchManager = new SearchManager(_config, _repository);
			searchManager.CreateIndex();

			PageSummary page1 = CreatePage(1, "admin", "random name2", "tag1", "title content");
			PageSummary page2 = CreatePage(2, "admin", "random name2", "1tag1", "title content");
			PageSummary page3 = CreatePage(3, "admin", "random name3", "1tag1", "title content");
			PageSummary page4 = CreatePage(4, "admin", "random name4", "1tag1", "title content");

			searchManager.Add(page1);
			searchManager.Add(page2);
			searchManager.Add(page3);
			searchManager.Add(page4);

			// Act
			List<SearchResult> results = searchManager.Search("id:1").ToList();

			// Assert
			Assert.That(results.Count, Is.EqualTo(1));
		}

		[Test]
		public void CreatedBy_Only_Searchable_Using_Field_Syntax()
		{
			// Arrange
			SearchManager searchManager = new SearchManager(_config, _repository);
			searchManager.CreateIndex();

			PageSummary page1 = CreatePage(1, "admin", "random name2", "homepage, tag1", "title content 11");
			PageSummary page2 = CreatePage(2, "admin", "random name2", "tag1, tag", "title content 2");

			searchManager.Add(page1);
			searchManager.Add(page2);

			// Act
			List<SearchResult> results = searchManager.Search("admin").ToList();
			List<SearchResult> createdByResults = searchManager.Search("createdby: admin").ToList();

			// Assert
			Assert.That(results.Count, Is.EqualTo(0), "admin title count");
			Assert.That(createdByResults.Count, Is.EqualTo(2), "createdby count");
		}

		[Test]
		public void Search_By_CreatedOnField_Returns_Results()
		{
			// Arrange
			string todaysDate = DateTime.Today.ToShortDateString(); // (SearchManager stores dates, not times)
			SearchManager searchManager = new SearchManager(_config, _repository);
			searchManager.CreateIndex();

			PageSummary page1 = CreatePage(1, "admin", "random name2", "homepage, tag1", "title content", DateTime.Today);
			PageSummary page2 = CreatePage(2, "admin", "random name2", "tag1, tag", "title content", DateTime.Today);
			PageSummary page3 = CreatePage(3, "admin", "random name3", "tag1, tag", "title content", DateTime.Today.AddDays(1));
			PageSummary page4 = CreatePage(4, "admin", "random name4", "tag1, tag", "title content", DateTime.Today.AddDays(2));

			searchManager.Add(page1);
			searchManager.Add(page2);
			searchManager.Add(page3);
			searchManager.Add(page4);

			// Act
			List<SearchResult> createdOnResults = searchManager.Search("createdon:" +todaysDate).ToList();

			// Assert
			Assert.That(createdOnResults.Count, Is.EqualTo(2), "createdon count");
		}

		[Test]
		public void Delete_Should_Remove_Page_From_Index()
		{
			// Arrange
			SearchManager searchManager = new SearchManager(_config, _repository);
			searchManager.CreateIndex();

			PageSummary page1 = CreatePage(1, "admin", "homepage title", "homepage1, tag1", "title content");
			PageSummary page2 = CreatePage(2, "admin", "random name2", "tag1", "random name 2");

			searchManager.Add(page1);
			searchManager.Add(page2);

			// Act
			searchManager.Delete(page1);
			List<SearchResult> results = searchManager.Search("homepage title").ToList();

			// Assert
			Assert.That(results.Count, Is.EqualTo(0), "homepage title still appears after deletion");
		}

		[Test]
		public void Update_Should_Show_In_Index_Search()
		{
			// Arrange
			SearchManager searchManager = new SearchManager(_config, _repository);
			searchManager.CreateIndex();

			PageSummary page1 = CreatePage(1, "admin", "homepage title", "homepage1, tag1", "title content");
			PageSummary page2 = CreatePage(2, "admin", "random name2", "tag1", "random name 2");

			searchManager.Add(page1);
			searchManager.Add(page2);

			// Act
			page1.Title = "A new hope";
			searchManager.Update(page1);

			Thread thread = new Thread(delegate()
			{
				// Perform the test in a new thread, so that the add + delete commit is picked up
				// which is periodically done by Lucene.
				List<SearchResult> oldResults = searchManager.Search("homepage title").ToList();
				List<SearchResult> newResults = searchManager.Search("A new hope").ToList();

				// Assert
				Assert.That(oldResults.Count, Is.EqualTo(0), "old results");
				Assert.That(newResults.Count, Is.EqualTo(1), "new results");
			});
			thread.Start();
		}

		[Test]
		[Description("See bug #93")]
		public void Cyrillic_Content_Should_Be_Stored_And_Retrieved_Correctly()
		{
			// Arrange
			SearchManager searchManager = new SearchManager(_config, _repository);
			searchManager.CreateIndex();

			PageSummary page1 = CreatePage(1, "admin", "ОШИБКА: неверная последовательность байт для кодировки", "tag1", 
				"БД сервера событий была перенесена из PostgreSQL 8.3 на PostgreSQL 9.1.4. Сервер, развернутый на Windows платформе, не мог с ней работать, т.к. установщик PostgreSQL 9.1.4 создает шаблон базы с использованием кодировки UTF8 и, сответственно, новая БД не могла быть создана с требуемой");

			searchManager.Add(page1);

			// Act
			List<SearchResult> results = searchManager.Search("ОШИБКА").ToList();

			// Assert
			Assert.That(results[0].ContentSummary, Contains.Substring("БД сервера событий была перенесена из"));
		}

		[Test]
		[Description("Not an integration test, but grouped here for convenience.")]
		public void GetContentSummary_Should_Only_Contain_First_150_Characters_For_Summary()
		{
			

			// Arrange
			SearchManager searchManager = new SearchManager(_config, _repository);
			searchManager.CreateIndex();

			PageSummary page1 = CreatePage(1, "admin", "A page title", "tag1", "Lorizzle ipsizzle dolor sit amizzle, (pre character 150 boundary) rizzle adipiscing tellivizzle. Nullizzle sapizzle velizzle, yo mamma volutpat, suscipizzle bow wow wow, gravida vizzle, (post 150 character boundary) shizznit. Pellentesque da bomb tortizzle. Hizzle erizzle. Its fo rizzle izzle sheezy dapibizzle mofo tempizzle tempizzle. Maurizzle away nibh izzle turpis. Phat izzle hizzle. Pellentesque eleifend rhoncus rizzle. Da bomb things dang platea dictumst. Fo shizzle my nizzle dapibizzle. Shiz tellus owned, pretizzle eu, mattizzle ac, bow wow wow its fo rizzle, nunc. Shiz suscipit. Integizzle own yo' we gonna chung sed go to hizzle.");
			searchManager.Add(page1);

			// Act
			List<SearchResult> results = searchManager.Search("rizzle").ToList();

			// Assert
			Assert.That(results[0].ContentSummary, Contains.Substring("(pre character 150 boundary)"));
			Assert.That(results[0].ContentSummary, Is.Not.StringContaining("(post character 150 boundary)"));

		}

		[Test]
		[Description("Not an integration test, but grouped here for convenience.")]
		public void GetContentSummary_Should_Remove_Html_From_Summary()
		{
			// Arrange
			SearchManager searchManager = new SearchManager(_config, _repository);
			searchManager.CreateIndex();

			PageSummary page1 = CreatePage(1, "admin", "A page title", "tag1", "**some bold** \n\n=my header=");
			searchManager.Add(page1);

			// Act
			List<SearchResult> results = searchManager.Search("my header").ToList();

			// Assert
			Assert.That(results[0].ContentSummary, Is.Not.StringContaining("<b>some bold</b>"));
			Assert.That(results[0].ContentSummary, Is.Not.StringContaining("<h1>my header</h1>"));

		}

		protected PageSummary CreatePage(int id, string createdBy, string title, string tags, string textContent = "", DateTime? createdOn = null)
		{
			if (createdOn == null)
				createdOn = DateTime.UtcNow;

			return new PageSummary()
			{
				Id = id,
				CreatedBy = createdBy,
				Title = title,
				RawTags = tags,
				Content = textContent,
				CreatedOn = createdOn.Value,
				ModifiedBy = createdBy,
				ModifiedOn = createdOn.Value,
				VersionNumber = 1
			};
		}
	}
}
