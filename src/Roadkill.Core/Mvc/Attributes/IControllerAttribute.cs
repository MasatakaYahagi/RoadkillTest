﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Configuration;
using Roadkill.Core.Services;
using Roadkill.Core.Security;
using StructureMap.Attributes;

namespace Roadkill.Core.Mvc.Attributes
{
	/// <summary>
	/// Defines an Attribute that has its property values setter injected by Structuremap.
	/// </summary>
	public interface IControllerAttribute
	{
		ApplicationSettings ApplicationSettings { get; set; }
		IUserContext Context { get; set; }
		UserServiceBase UserManager { get; set; }
		PageService PageService { get; set; }
	}
}
