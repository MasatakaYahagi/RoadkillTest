﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Security;
using StructureMap;

namespace Roadkill.Tests
{
	[Pluggable("x")]
	public class UserManagerStub : UserServiceBase
	{
		public UserManagerStub()
			: base(null, null)
		{

		}

		public UserManagerStub(ApplicationSettings settings, IRepository repository)
			: base(settings, repository)
		{

		}

		public override bool IsReadonly
		{
			get { throw new NotImplementedException(); }
		}

		public override bool ActivateUser(string activationKey)
		{
			throw new NotImplementedException();
		}

		public override bool AddUser(string email, string username, string password, bool isAdmin, bool isEditor)
		{
			throw new NotImplementedException();
		}

		public override bool Authenticate(string email, string password)
		{
			throw new NotImplementedException();
		}

		public override void ChangePassword(string email, string newPassword)
		{
			throw new NotImplementedException();
		}

		public override bool ChangePassword(string email, string oldPassword, string newPassword)
		{
			throw new NotImplementedException();
		}

		public override bool DeleteUser(string email)
		{
			throw new NotImplementedException();
		}

		public override User GetUserById(Guid id, bool isActivated = true)
		{
			throw new NotImplementedException();
		}

		public override User GetUser(string email, bool isActivated = true)
		{
			throw new NotImplementedException();
		}

		public override User GetUserByResetKey(string resetKey)
		{
			throw new NotImplementedException();
		}

		public override bool IsAdmin(string email)
		{
			throw new NotImplementedException();
		}

		public override bool IsEditor(string email)
		{
			throw new NotImplementedException();
		}

		public override IEnumerable<UserViewModel> ListAdmins()
		{
			throw new NotImplementedException();
		}

		public override IEnumerable<UserViewModel> ListEditors()
		{
			throw new NotImplementedException();
		}

		public override void Logout()
		{
			throw new NotImplementedException();
		}

		public override string ResetPassword(string email)
		{
			throw new NotImplementedException();
		}

		public override string Signup(UserViewModel summary, Action completed)
		{
			throw new NotImplementedException();
		}

		public override void ToggleAdmin(string email)
		{
			throw new NotImplementedException();
		}

		public override void ToggleEditor(string email)
		{
			throw new NotImplementedException();
		}

		public override bool UpdateUser(UserViewModel summary)
		{
			throw new NotImplementedException();
		}

		public override bool UserExists(string email)
		{
			throw new NotImplementedException();
		}

		public override bool UserNameExists(string username)
		{
			throw new NotImplementedException();
		}

		public override string GetLoggedInUserName(System.Web.HttpContextBase context)
		{
			throw new NotImplementedException();
		}
	}
}
