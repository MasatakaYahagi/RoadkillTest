using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Database;
using Roadkill.Core.Managers;
using Roadkill.Core.Security;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class UserControllerTests
	{
		public static string AdminEmail = "admin@localhost";
		public static string AdminUsername = "admin";
		public static string AdminPassword = "password";

		private ApplicationSettings _settings;
		private RepositoryMock _repository;
		private Mock<UserManagerBase> _userManager;
		private IUserContext _context;
		private List<User> _users = new List<User>();
		private SettingsManager _settingsManager;

		[TestFixtureSetUp]
		public void Setup()
		{
			_settings = new ApplicationSettings();
			_repository = new RepositoryMock();
			User dummyUser = new User() 
			{ 
				Id = Guid.NewGuid(),
				Email = AdminEmail, 
				Username = AdminUsername,
				PasswordResetKey = "resetkey", 
				ActivationKey = "activatekey",
				Firstname = "Firstname",
				Lastname = "LastnameNotSurname"
			};
			_users.Add(dummyUser);

			_userManager = new Mock<UserManagerBase>(_settings, _repository);
			_userManager.Setup(u => u.Authenticate(AdminEmail, AdminPassword)).Returns(true).Callback(() =>
			{
				_context.CurrentUser = dummyUser.Id.ToString();
			});
			_userManager.Setup(s => s.ActivateUser("activatekey")).Returns(true);
			_userManager.Setup(s => s.GetUserByResetKey("resetkey")).Returns(_users[0]);
			_userManager.Setup(s => s.GetUserById(dummyUser.Id, It.IsAny<bool>())).Returns(dummyUser);

			_context = new UserContext(_userManager.Object);
			_settingsManager = new SettingsManager(_settings, _repository);
		}

		[Test]
		public void Activate_With_Valid_Key_Returns_View()
		{
			// Arrange
			UserController userController = new UserController(_settings, _userManager.Object, _context, _settingsManager);
			userController.SetFakeControllerContext();

			// Act	
			ActionResult result = userController.Activate("activatekey");

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>());
		}

		[Test]
		public void Activate_With_Invalid_Key_Should_Have_Model_Error()
		{
			// Arrange
			UserController userController = new UserController(_settings, _userManager.Object, _context, _settingsManager);
			userController.SetFakeControllerContext();

			// Act	
			ActionResult result = userController.Activate("badkey");

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>());
			Assert.That(userController.ModelState.Count, Is.EqualTo(1));
		}

		[Test]
		public void Activate_With_Empty_Key_Returns_RedirectResult()
		{
			// Arrange
			UserController userController = new UserController(_settings, _userManager.Object, _context, _settingsManager);
			userController.SetFakeControllerContext();

			// Act	
			ActionResult result = userController.Activate("");

			// Assert
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>());

			RedirectToRouteResult redirectResult = result as RedirectToRouteResult;
			Assert.That(redirectResult.RouteValues["action"], Is.EqualTo("Index"));
			Assert.That(redirectResult.RouteValues["controller"], Is.EqualTo("Home"));
		}

		[Test]
		public void CompleteResetPassword_Has_Correct_Model_And_ActionResult()
		{
			// Arrange
			UserController userController = new UserController(_settings, _userManager.Object, _context, _settingsManager);
			userController.SetFakeControllerContext();

			// Act	
			ActionResult result = userController.CompleteResetPassword("resetkey");

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>());

			UserSummary summary = result.ModelFromActionResult<UserSummary>();
			User expectedUser = _users[0];

			Assert.That(summary.Id, Is.EqualTo(expectedUser.Id));
			Assert.That(summary.NewEmail, Is.EqualTo(expectedUser.Email));
			Assert.That(summary.PasswordResetKey, Is.EqualTo(expectedUser.PasswordResetKey));
			Assert.That(summary.Firstname, Is.EqualTo(expectedUser.Firstname));
			Assert.That(summary.Lastname, Is.EqualTo(expectedUser.Lastname));
		}

		[Test]
		public void CompleteResetPassword_With_WindowsAuth_Enabled_Redirects()
		{
			// Arrange
			_settings.UseWindowsAuthentication = true;
			UserController userController = new UserController(_settings, _userManager.Object, _context, _settingsManager);
			userController.SetFakeControllerContext();

			// Act	
			ActionResult result = userController.CompleteResetPassword("resetkey");

			// Assert
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>());

			RedirectToRouteResult redirectResult = result as RedirectToRouteResult;
			Assert.That(redirectResult.RouteValues["action"], Is.EqualTo("Index"));
			Assert.That(redirectResult.RouteValues["controller"], Is.EqualTo("Home"));
		}

		[Test]
		public void Login_Should_Call_Authenticate_And_Redirect()
		{
			// Arrange
			UserController userController = new UserController(_settings, _userManager.Object, _context, _settingsManager);
			userController.SetFakeControllerContext();

			// Act	
			ActionResult result = userController.Login(AdminEmail, AdminPassword, "");

			// Assert
			_userManager.Verify(x => x.Authenticate(AdminEmail, AdminPassword));
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>());
			Assert.That(userController.ModelState.Count, Is.EqualTo(0));
		}

		[Test]
		public void Login_With_Wrong_Email_And_Password_Should_Have_Model_Error()
		{
			// Arrange
			UserController userController = new UserController(_settings, _userManager.Object, _context, _settingsManager);
			userController.SetFakeControllerContext();

			// Act	
			ActionResult result = userController.Login("wrongemail", "wrongpassword", "");

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>());
			Assert.That(userController.ModelState.Count, Is.EqualTo(1));
		}

		[Test]
		public void Logout_Should_Have_RedirectResult()
		{
			// Arrange
			UserController userController = new UserController(_settings, _userManager.Object, _context, _settingsManager);
			userController.SetFakeControllerContext();

			// Act	
			ActionResult result = userController.Logout();

			// Assert
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>());

			RedirectToRouteResult redirectResult = result as RedirectToRouteResult;
			Assert.That(redirectResult.RouteValues["action"], Is.EqualTo("Index"));
			Assert.That(redirectResult.RouteValues["controller"], Is.EqualTo("Home"));
		}

		[Test]
		public void Profile_Should_Return_Correct_ActionResult_And_Model()
		{
			// Arrange
			_settings.UseWindowsAuthentication = false;
			UserController userController = new UserController(_settings, _userManager.Object, _context, _settingsManager);
			userController.SetFakeControllerContext();

			// Act
			userController.Login(AdminEmail, AdminPassword, "");
			ActionResult result = userController.Profile();

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>());

			UserSummary summary = result.ModelFromActionResult<UserSummary>();
			User expectedUser = _users[0];

			Assert.That(summary.Id, Is.EqualTo(expectedUser.Id));
			Assert.That(summary.NewEmail, Is.EqualTo(expectedUser.Email));
			Assert.That(summary.PasswordResetKey, Is.EqualTo(expectedUser.PasswordResetKey));
			Assert.That(summary.Firstname, Is.EqualTo(expectedUser.Firstname));
			Assert.That(summary.Lastname, Is.EqualTo(expectedUser.Lastname));
		}

		[Test]
		[Ignore]
		public void Profile_Post_Should_Update_User()
		{

		}

		[Test]
		[Ignore]
		public void ResetPassword_Should_Return_ViewResult()
		{

		}

		[Test]
		public void ResetPassword_WithWindows_Auth_Enabled_Should_Return_RedirectResult()
		{
			// Arrange
			_settings.UseWindowsAuthentication = true;
			UserController userController = new UserController(_settings, _userManager.Object, _context, _settingsManager);
			userController.SetFakeControllerContext();

			// Act	
			ActionResult result = userController.Logout();

			// Assert
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>());

			RedirectToRouteResult redirectResult = result as RedirectToRouteResult;
			Assert.That(redirectResult.RouteValues["action"], Is.EqualTo("Index"));
			Assert.That(redirectResult.RouteValues["controller"], Is.EqualTo("Home"));
		}

		[Test]
		[Ignore]
		public void ResetPassword_Post_Should_()
		{

		}

		[Test]
		[Ignore]
		public void ResendConfirmation_Post()
		{

		}

		[Test]
		public void Signup_When_LoggedIn_Should_Return_RedirectResult()
		{
			// Arrange
			UserController userController = new UserController(_settings, _userManager.Object, _context, _settingsManager);
			userController.SetFakeControllerContext();

			// Act	
			userController.Login(AdminEmail, AdminPassword, "");
			ActionResult result = userController.Signup(null, null);

			// Assert
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>());

			RedirectToRouteResult redirectResult = result as RedirectToRouteResult;
			Assert.That(redirectResult.RouteValues["action"], Is.EqualTo("Index"));
			Assert.That(redirectResult.RouteValues["controller"], Is.EqualTo("Home"));
		}

		[Test]
		public void Signup_With_WindowsAuth_Enabled_Should_Return_RedirectResult()
		{
			// Arrange
			_repository.SiteSettings.AllowUserSignup = true;
			_settings.UseWindowsAuthentication = true;
			UserController userController = new UserController(_settings, _userManager.Object, _context, _settingsManager);
			userController.SetFakeControllerContext();

			// Act	
			ActionResult result = userController.Signup(null, null);

			// Assert
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>());

			RedirectToRouteResult redirectResult = result as RedirectToRouteResult;
			Assert.That(redirectResult.RouteValues["action"], Is.EqualTo("Index"));
			Assert.That(redirectResult.RouteValues["controller"], Is.EqualTo("Home"));
		}

		[Test]
		public void Signup_With_Signups_Disabled_Should_Return_RedirectResult()
		{
			// Arrange
			_repository.SiteSettings.AllowUserSignup = false;
			UserController userController = new UserController(_settings, _userManager.Object, _context, _settingsManager);
			userController.SetFakeControllerContext();

			// Act	
			ActionResult result = userController.Signup(null, null);

			// Assert
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>());

			RedirectToRouteResult redirectResult = result as RedirectToRouteResult;
			Assert.That(redirectResult.RouteValues["action"], Is.EqualTo("Index"));
			Assert.That(redirectResult.RouteValues["controller"], Is.EqualTo("Home"));
		}

		[Test]
		[Ignore]
		public void Signup_Should_Send_Email()
		{
			// Needs a refactor of the Email class, so it's turtles all the way down
		}
	}
}
