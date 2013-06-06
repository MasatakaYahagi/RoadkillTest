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
using Roadkill.Core.Attachments;
using System.Collections.Specialized;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class FileManagerControllerTests
	{
		private ApplicationSettings _settings;
		private UserManagerBase _userManager;
		private IUserContext _context;
		private RepositoryMock _repository;
		private SettingsManager _settingsManager;
		private AttachmentFileHandler _attachmentFileHandler;
		private FileManagerController _filesController;

		[SetUp]
		public void Setup()
		{
			// File-specific settings
			_context = new Mock<IUserContext>().Object;
			_settings = new ApplicationSettings();
			_settings.AttachmentsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Attachments");
			_repository = new RepositoryMock();
			_attachmentFileHandler = new AttachmentFileHandler(_settings);
			_settingsManager = new SettingsManager(_settings, _repository);
			_filesController = new FileManagerController(_settings, _userManager, _context, _settingsManager, _attachmentFileHandler);

			try
			{
				// Delete any existing attachments folder
				DirectoryInfo directoryInfo = new DirectoryInfo(_settings.AttachmentsFolder);
				if (directoryInfo.Exists)
				{
					directoryInfo.Attributes = FileAttributes.Normal;
					directoryInfo.Delete(true);
				}

				Directory.CreateDirectory(_settings.AttachmentsFolder);
			}
			catch (IOException e)
			{
				Assert.Fail("Unable to delete the attachments folder "+_settings.AttachmentsFolder+", does it have a lock/explorer window open?" + e.ToString());
			}

			_userManager = new Mock<UserManagerBase>(_settings, null).Object;
		}

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

		[Test]
		public void Index_Should_Return_View()
		{
			// Arrange
			_filesController.SetFakeControllerContext();

			// Act
			ActionResult result = _filesController.Index();
			
			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>(), "ViewResult");
		}

		[Test]
		public void DeleteFile_Should_Return_Ok_Json_Status_And_Delete_File()
		{
			// Arrange
			string testFile1Path = CreateTestFileInAttachments("test.txt");
			string dirPath = CreateTestDirectoryInAttachments("test");
			string testFile2Path = Path.Combine(dirPath, "test.txt");
			File.WriteAllText(testFile2Path, "test");
			_filesController.SetFakeControllerContext();

			// Act
			JsonResult result = _filesController.DeleteFile("/test/", "test.txt") as JsonResult;

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("ok"));
			Assert.That(jsonObject.message, Is.EqualTo(""));

			Assert.That(File.Exists(testFile2Path), Is.False);
			Assert.That(File.Exists(testFile1Path), Is.True);
		}

		[Test]
		public void DeleteFile_In_Subfolder_Should_Return_Ok_Json_Status_And_Delete_File()
		{
			// Arrange
			string fullPath = CreateTestFileInAttachments("test.txt");
			_filesController.SetFakeControllerContext();

			// Act
			JsonResult result = _filesController.DeleteFile("/", "test.txt") as JsonResult;

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("ok"));
			Assert.That(jsonObject.message, Is.EqualTo(""));

			Assert.That(File.Exists(fullPath), Is.False);
		}

		[Test]
		public void DeleteFile_Missing_File_Should_Return_Json_Status_Ok()
		{
			// Arrange
			_filesController.SetFakeControllerContext();

			// Act
			JsonResult result = _filesController.DeleteFile("/", "doesntexist.txt") as JsonResult;

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("ok"));
			Assert.That(jsonObject.message, Is.EqualTo(""));
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void DeleteFile_With_Bad_Paths_Throws_Exception()
		{
			// Arrange
			_filesController.SetFakeControllerContext();

			// Act
			JsonResult result = _filesController.DeleteFile("/.././", "hacker.txt") as JsonResult;

			// Assert
		}

		[Test]
		public void DeleteFolder_Should_Return_Ok_Json_Status_And_Delete_Folder()
		{
			// Arrange
			_filesController.SetFakeControllerContext();
			string fullPath = CreateTestDirectoryInAttachments("folder1");

			// Act
			JsonResult result = _filesController.DeleteFolder("folder1");

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("ok"));
			Assert.That(jsonObject.message, Is.EqualTo(""));

			Assert.That(Directory.Exists(fullPath), Is.False);
		}

		[Test]
		public void DeleteFolder_With_SubDirectory_Should_Return_Ok_Json_Status_And_Delete_Folder()
		{
			// Arrange
			_filesController.SetFakeControllerContext();
			string fullPath = CreateTestDirectoryInAttachments("folder1");
			string subPath = Path.Combine(fullPath, "subfolder1");
			string subsubPath = Path.Combine(subPath, "subsubfolder");
			Directory.CreateDirectory(subPath);
			Directory.CreateDirectory(subsubPath);

			// Act
			JsonResult result = _filesController.DeleteFolder("folder1/subfolder1/subsubfolder");

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("ok"));
			Assert.That(jsonObject.message, Is.EqualTo(""));

			Assert.That(Directory.Exists(subsubPath), Is.False);
			Assert.That(Directory.Exists(subPath), Is.True);
			Assert.That(Directory.Exists(fullPath), Is.True);
		}

		[Test]
		public void DeleteFolder_Empty_Folder_Argument_Returns_Error()
		{
			// Arrange
			_filesController.SetFakeControllerContext();

			// Act
			JsonResult result = _filesController.DeleteFolder("");

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("error"));
			Assert.That(jsonObject.message, Is.Not.Null.Or.Empty);
		}

		[Test]
		public void DeleteFolder_Containing_Files_Should_Return_Error()
		{
			// Arrange
			_filesController.SetFakeControllerContext();
			CreateTestDirectoryInAttachments("folder1");
			string fullPath = Path.Combine(_settings.AttachmentsDirectoryPath, "folder1", "test.txt");
			File.WriteAllText(fullPath, "test");

			// Act
			JsonResult result = _filesController.DeleteFolder("folder1");

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("error"));
			Assert.That(jsonObject.message, Is.Not.Null.Or.Empty);
		}

		[Test]
		public void DeleteFolder_Has_Subdirectories_Should_Return_Error()
		{
			// Arrange
			_filesController.SetFakeControllerContext();
			string fullpath = CreateTestDirectoryInAttachments("folder1");
			Directory.CreateDirectory(Path.Combine(fullpath, "subfolder1"));

			// Act
			JsonResult result = _filesController.DeleteFolder("folder1");

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("error"));
			Assert.That(jsonObject.message, Is.Not.Null.Or.Empty);
		}

		[Test]
		public void DeleteFolder_With_Missing_Directory_Should_Return_Error()
		{
			// Arrange
			_filesController.SetFakeControllerContext();

			// Act
			JsonResult result = _filesController.DeleteFolder("folder1/folder2");

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("error"));
			Assert.That(jsonObject.message, Is.Not.Null.Or.Empty);
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void DeleteFolder_With_Hacky_Path_Should_Throw_Exception()
		{
			// Arrange
			_filesController.SetFakeControllerContext();

			// Act
			JsonResult result = _filesController.DeleteFolder("/../../folder1") as JsonResult;

			// Assert
		}

		[Test]
		public void FolderInfo_With_Empty_Path_Should_Contain_Model_With_Root()
		{
			// Arrange
			_filesController.SetFakeControllerContext();
			CreateTestDirectoryInAttachments("blah");
			CreateTestFileInAttachments("blah.png");

			// Act
			JsonResult result = _filesController.FolderInfo("") as JsonResult;

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult was not returned");

			DirectorySummary summary = result.Data as DirectorySummary;
			Assert.That(summary, Is.Not.Null, "DirectorySummary is null");
			Assert.That(summary.ChildFolders.Count, Is.EqualTo(1));
			Assert.That(summary.Files.Count, Is.EqualTo(1));
			Assert.That(summary.Name, Is.EqualTo(""));
			Assert.That(summary.UrlPath, Is.EqualTo(""));
		}

		[Test]
		public void FolderInfo_With_Root_Should_Contain_Model()
		{
			// Arrange
			_filesController.SetFakeControllerContext();
			CreateTestDirectoryInAttachments("blah");
			CreateTestFileInAttachments("blah.png");

			// Act
			JsonResult result = _filesController.FolderInfo("") as JsonResult;

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult was not returned");

			DirectorySummary summary = result.Data as DirectorySummary;
			Assert.That(summary, Is.Not.Null, "DirectorySummary is null");
			Assert.That(summary.ChildFolders.Count, Is.EqualTo(1));
			Assert.That(summary.Files.Count, Is.EqualTo(1));
			Assert.That(summary.Name, Is.EqualTo(""));
			Assert.That(summary.UrlPath, Is.EqualTo(""));
		}

		[Test]
		public void FolderInfo_With_SubFolder_Should_Contain_Model()
		{
			// Arrange
			_filesController.SetFakeControllerContext();
			CreateTestDirectoryInAttachments(@"blah\blah2\blah3");
			CreateTestDirectoryInAttachments(@"blah\blah2\blah3\blah4");
			CreateTestFileInAttachments(@"blah\blah2\blah3\something.png");
			CreateTestFileInAttachments(@"blah\blah2\blah3\something2.png");
			CreateTestFileInAttachments(@"blah\blah2\blah3\something3.png");

			// Act
			JsonResult result = _filesController.FolderInfo("/blah/blah2/blah3") as JsonResult;

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult was not returned");

			DirectorySummary summary = result.Data as DirectorySummary;
			Assert.That(summary, Is.Not.Null, "DirectorySummary is null");
			Assert.That(summary.ChildFolders.Count, Is.EqualTo(1));
			Assert.That(summary.Files.Count, Is.EqualTo(3));
			Assert.That(summary.Name, Is.EqualTo("blah3"));
			Assert.That(summary.UrlPath, Is.EqualTo("/blah/blah2/blah3"));
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void FolderInfo_With_Missing_Directory_Should_Throw_Exception()
		{
			// Arrange
			_filesController.SetFakeControllerContext();

			// Act
			JsonResult result = _filesController.FolderInfo("/missingfolder") as JsonResult;

			// Assert
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void FolderInfo_With_Hacky_Url_Should_Throw_Exception()
		{
			// Arrange
			_filesController.SetFakeControllerContext();

			// Act
			JsonResult result = _filesController.FolderInfo(".././") as JsonResult;

			// Assert
		}

		[Test]
		public void Select_Should_Return_View()
		{
			// Arrange
			_filesController.SetFakeControllerContext();

			// Act
			ActionResult result = _filesController.Select();

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>(), "ViewResult");
		}

		[Test]
		public void NewFolder_In_Root_Folder_Should_Create_Folder_And_Return_Ok_Json_Status()
		{
			// Arrange
			_filesController.SetFakeControllerContext();
			string folderName = "newfolder with spaces in it";
			string fullPath = Path.Combine(_settings.AttachmentsDirectoryPath, folderName);

			// Act
			JsonResult result = _filesController.NewFolder("/", folderName);

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("ok"));
			Assert.That(jsonObject.FolderName, Is.EqualTo(folderName));
			Assert.That(Directory.Exists(fullPath), Is.True);
		}

		[Test]
		public void NewFolder_With_SubDirectory_Should_Create_Folder_And_Return_Ok_Json_Status()
		{
			// Arrange
			_filesController.SetFakeControllerContext();
			string fullPath = CreateTestDirectoryInAttachments("folder1");
			string subPath = Path.Combine(fullPath, "subfolder1");
			string subsubPath = Path.Combine(subPath, "subsubfolder");
			Directory.CreateDirectory(subPath);


			// Act
			JsonResult result = _filesController.NewFolder("/folder1/subfolder1/", "subsubfolder");

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("ok"));
			Assert.That(jsonObject.FolderName, Is.EqualTo("subsubfolder"));
			Assert.That(Directory.Exists(subsubPath), Is.True);
		}

		[Test]
		public void NewFolder_With_Empty_FolderName_Argument_Should_Return_Error()
		{
			// Arrange
			_filesController.SetFakeControllerContext();

			// Act
			JsonResult result = _filesController.NewFolder("/","");

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("error"));
			Assert.That(jsonObject.message, Is.Not.Null.Or.Empty);
		}

		[Test]
		public void NewFolder_With_Missing_Directory_Should_Return_Error()
		{
			// Arrange
			_filesController.SetFakeControllerContext();

			// Act
			JsonResult result = _filesController.NewFolder("folder1/folder2", "newfolder");

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("error"));
			Assert.That(jsonObject.message, Is.Not.Null.Or.Empty);
		}

		[Test]
		public void NewFolder_With_Hacky_Path_Should_Return_Exception()
		{
			// Arrange
			_filesController.SetFakeControllerContext();

			// Act
			JsonResult result = _filesController.NewFolder("/../../folder1","../cheeky/path") as JsonResult;

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("error"));
			Assert.That(jsonObject.message, Is.Not.Null.Or.Empty);
		}

		// [X] DeleteFile
		// [X] DeleteFolder
		// [X] FolderInfo
		// [X] NewFolder
		// FileUpload

		[Test]
		public void FileUpload_with_Multiple_Files_To_Root_Should_Upload_And_Return_Ok_Json_Status()
		{
			// Arrange
			MvcMockContainer container = _filesController.SetFakeControllerContext();
			SetupMockPostedFiles(container, "/", "file1.png", "file2.png");
			string file1FullPath = Path.Combine(_settings.AttachmentsDirectoryPath, "file1.png");
			string file2FullPath = Path.Combine(_settings.AttachmentsDirectoryPath, "file2.png");

			// Act
			JsonResult result = _filesController.FileUpload();

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("ok"));
			Assert.That(jsonObject.filename, Is.EqualTo("file2.png"));

			Assert.That(File.Exists(file1FullPath), Is.True);
			Assert.That(File.Exists(file2FullPath), Is.True);
		}

		private string CreateTestFileInAttachments(string filename)
		{
			string fullPath = Path.Combine(_settings.AttachmentsDirectoryPath, filename);
			File.WriteAllText(fullPath, "test");

			return fullPath;
		}

		private string CreateTestDirectoryInAttachments(string directoryName)
		{
			string fullPath = Path.Combine(_settings.AttachmentsDirectoryPath, directoryName);
			Directory.CreateDirectory(fullPath);

			return fullPath;
		}

		private void SetupMockPostedFiles(MvcMockContainer container, string destinationFolder, params string[] fileNames)
		{
			container.Request.Setup(x => x.Form).Returns(delegate()
			{
				var values = new NameValueCollection();
				values.Add("destination_folder", destinationFolder);
				return values;
			});

			Mock<HttpFileCollectionBase> postedfilesKeyCollection = new Mock<HttpFileCollectionBase>();
			container.Request.Setup(req => req.Files).Returns(postedfilesKeyCollection.Object);

			List<HttpPostedFileBase> files = new List<HttpPostedFileBase>();
			container.Request.Setup(x => x.Files.Count).Returns(fileNames.Length);
			for (int i = 0; i < fileNames.Length; i++)
			{
				Mock<HttpPostedFileBase> postedfile = new Mock<HttpPostedFileBase>();
				postedfile.Setup(f => f.ContentLength).Returns(8192);
				postedfile.Setup(f => f.FileName).Returns(fileNames[i]);
				postedfile.Setup(f => f.SaveAs(It.IsAny<string>())).Callback<string>(filename => File.WriteAllText(Path.Combine(_settings.AttachmentsDirectoryPath, filename), "test contents"));
				container.Request.Setup(x => x.Files[i]).Returns(postedfile.Object);
			}
		}
	}
}
