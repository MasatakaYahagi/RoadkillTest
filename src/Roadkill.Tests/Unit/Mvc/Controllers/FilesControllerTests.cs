﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Managers;
using Roadkill.Core.Security;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class FilesControllerTests
	{
		private ApplicationSettings _settings;
		private UserManagerBase _userManager;
		private IUserContext _context;
		private RepositoryMock _repository;
		private SettingsManager _settingsManger;

		[SetUp]
		public void Setup()
		{
			// File-specific settings
			_context = new Mock<IUserContext>().Object;
			_settings = new ApplicationSettings();
			_settings.AttachmentsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "attachments");
			_repository = new RepositoryMock();
			_settingsManger = new SettingsManager(_settings, _repository);

			try
			{
				// Delete any existing attachments folder
				DirectoryInfo directoryInfo = new DirectoryInfo(_settings.AttachmentsFolder);
				if (directoryInfo.Exists)
				{
					directoryInfo.Attributes = FileAttributes.Normal;
					directoryInfo.Delete(true);
				}
			}
			catch (IOException e)
			{
				Assert.Fail("Unable to delete the attachments folder "+_settings.AttachmentsFolder+", does it have a lock?" + e.ToString());
			}

			_userManager = new Mock<UserManagerBase>(_settings, null).Object;
		}

		[Test]
		public void UploadFile_Should_Save_To_Filesystem_With_No_Errors_And_Redirect()
		{
			// Arrange
			FilesController filesController = new FilesController(_settings, _userManager, _context, _settingsManger);
			MvcMockContainer mocksContainer =  filesController.SetFakeControllerContext();
			SetupMockPostedFile(mocksContainer);

			// Act
			string pathAsBase64 = @"\".ToBase64();
			ActionResult result = filesController.UploadFile(pathAsBase64) as ActionResult;
			
			// Assert
			string expectedFilePath = Path.Combine(_settings.AttachmentsFolder, "test.png");
			Assert.That(File.Exists(expectedFilePath), "Filepath: " + expectedFilePath);
			Assert.That(filesController.TempData["Error"], Is.Null, "Errors");
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>(), "RedirectToRouteResult");
		}

		[Test]
		public void DeleteFile_Should_Remove_From_Filesystem_With_No_Errors_And_Redirect()
		{
			// Arrange
			FilesController filesController = new FilesController(_settings, _userManager, _context, _settingsManger);
			MvcMockContainer mocksContainer = filesController.SetFakeControllerContext();
			SetupMockPostedFile(mocksContainer);

			string dir = Path.Combine(_settings.AttachmentsFolder, "folder1");
			Directory.CreateDirectory(dir);
			File.WriteAllText(Path.Combine(dir, "test.png"), "fake png");

			// Act
			string pathAsBase64 = @"\folder1\test.png".ToBase64(); // - All incoming paths are based 64 encoded
			ActionResult result = filesController.DeleteFile(pathAsBase64) as ActionResult;

			// Assert
			string expectedFilePath = Path.Combine(_settings.AttachmentsFolder, "test.png");
			Assert.That(File.Exists(expectedFilePath), Is.Not.True, "Filepath: " + expectedFilePath);
			Assert.That(filesController.TempData["Error"], Is.Null, "Errors");
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>(), "RedirectToRouteResult");
		}

		[Test]
		public void NewFolder_Should_Create_Folder_On_Filesystem_With_No_Errors_And_Redirect()
		{
			// Arrange
			FilesController filesController = new FilesController(_settings, _userManager, _context, _settingsManger);
			MvcMockContainer mocksContainer = filesController.SetFakeControllerContext();
			SetupMockPostedFile(mocksContainer);

			// Act
			string currentDirAsBase64 = @"\".ToBase64(); // - All incoming paths are based 64 encoded
			string newDir = @"folder1";
			ActionResult result = filesController.NewFolder(currentDirAsBase64, newDir) as ActionResult;

			// Assert
			string expectedPath = Path.Combine(_settings.AttachmentsFolder, newDir);
			Assert.That(Directory.Exists(expectedPath), "Filepath: " + expectedPath);
			Assert.That(filesController.TempData["Error"], Is.Null, "Errors");
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>(), "RedirectToRouteResult");
		}

		[Test]
		public void Folder_Action_Should_Contain_Files_And_Folders()
		{
			// Arrange
			FilesController filesController = new FilesController(_settings, _userManager, _context, _settingsManger);
			MvcMockContainer mocksContainer = filesController.SetFakeControllerContext();
			SetupMockPostedFile(mocksContainer);
			string dir = @"folder1";
			string fullPath = Path.Combine(_settings.AttachmentsFolder, "folder1");

			Directory.CreateDirectory(fullPath);
			Directory.CreateDirectory(Path.Combine(fullPath, "anotherfolder1"));
			Directory.CreateDirectory(Path.Combine(fullPath, "anotherfolder2"));
			File.WriteAllText(Path.Combine(fullPath, "test1.png"), "fake png 1");
			File.WriteAllText(Path.Combine(fullPath, "test2.png"), "fake png 2");
			File.WriteAllText(Path.Combine(fullPath, "test3.png"), "fake png 3");

			// Act
			string folderAsBase64 = ("/" + dir).ToBase64();
			ActionResult result = filesController.Folder(folderAsBase64) as ActionResult;

			// Assert
			Assert.That(filesController.TempData["Error"], Is.Null, "Errors");
			Assert.That(result, Is.TypeOf<PartialViewResult>(), "PartialViewResult");

			DirectorySummary summary = result.ModelFromActionResult<DirectorySummary>();
			Assert.IsNotNull(summary, "Null DirectorySummary as the controller model");
			Assert.That(summary.ChildFolders.Count, Is.EqualTo(2));
			Assert.That(summary.Files.Count, Is.EqualTo(3));
			Assert.IsNotNull(summary.ChildFolders.SingleOrDefault(d => d.Name == "anotherfolder1"));
			Assert.IsNotNull(summary.Files.SingleOrDefault(f => f.Name == "test1.png"));
		}

		[Test]
		public void GetPath_Should_Return_PlainText_Path()
		{
			// Arrange
			FilesController filesController = new FilesController(_settings, _userManager, _context, _settingsManger);
			MvcMockContainer mocksContainer = filesController.SetFakeControllerContext();
			SetupMockPostedFile(mocksContainer);

			string dir = @"folder1";
			string fullPath = Path.Combine(_settings.AttachmentsFolder, "folder1");

			Directory.CreateDirectory(fullPath);

			// Act
			string currentDirAsBase64 = (@"\" +dir).ToBase64(); // - All incoming paths are based 64 encoded
			ActionResult result = filesController.GetPath(currentDirAsBase64) as ActionResult;

			// Assert
			Assert.That(result, Is.TypeOf<ContentResult>(), "ContentResult");
			ContentResult content = result as ContentResult;
			Assert.That(content.Content, Is.EqualTo("/folder1"), "Plain text content");
		}

		// GetPath

		private void SetupMockPostedFile(MvcMockContainer container)
		{
			Mock<HttpFileCollectionBase> postedfilesKeyCollection = new Mock<HttpFileCollectionBase>();
			List<string> fakeFileKeys = new List<string>() { "uploadFile" };
			Mock<HttpPostedFileBase> postedfile = new Mock<HttpPostedFileBase>();

			container.Request.Setup(req => req.Files).Returns(postedfilesKeyCollection.Object);
			postedfilesKeyCollection.Setup(keys => keys.GetEnumerator()).Returns(fakeFileKeys.GetEnumerator());
			postedfilesKeyCollection.Setup(keys => keys["uploadFile"]).Returns(postedfile.Object);

			postedfile.Setup(f => f.ContentLength).Returns(8192);
			postedfile.Setup(f => f.FileName).Returns("test.png");
			postedfile.Setup(f => f.SaveAs(It.IsAny<string>())).Callback<string>(filename => File.WriteAllText(filename, "test contents"));
		}
	}
}
