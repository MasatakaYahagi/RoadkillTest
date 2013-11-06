using System;
using System.Configuration;
using System.IO;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Roadkill.Core;
using Roadkill.Core.Database;

namespace Roadkill.Tests.Acceptance
{
	[TestFixture]
	[Category("Acceptance")]
	public class OtherDatabasesInstallerTests : InstallerTests
	{
		[Test]
		[Explicit("This works locally but not on Teamcity.")]
		public void All_Steps_With_Minimum_Required_SQLite_Should_Complete()
		{
			// Arrange
			try
			{
				// Delete SQLiteinterop.dll
				string sitePath = AcceptanceTestsSetup.GetSitePath();
				string sqlitePath = Path.Combine(sitePath, "bin", "SQLite.Interop.dll");
				if (File.Exists(sqlitePath))
					File.Delete(sqlitePath);
			}
			catch { }

			Driver.Navigate().GoToUrl(BaseUrl);
			ClickLanguageLink();

			//
			// ***Act***
			//

			// step 1
			Driver.FindElement(By.CssSelector("input[id=testwebconfig]")).Click();
			Driver.WaitForElementDisplayed(By.CssSelector(".continue > a")).Click();

			// step 2
			Driver.FindElement(By.Id("SiteName")).SendKeys("Acceptance tests");
			SelectElement select = new SelectElement(Driver.FindElement(By.Id("DataStoreTypeName")));
			select.SelectByValue(DataStoreType.Sqlite.Name);

			// install sqlite
			Driver.FindElement(By.CssSelector("#sqlite-details a")).Click();
			Driver.WaitForElementDisplayed(By.CssSelector("#sqlitesuccess"), 10);
			Assert.That(Driver.FindElement(By.CssSelector("#sqlitesuccess")).Displayed, Is.True);

			Driver.FindElement(By.Id("ConnectionString")).SendKeys(@"Data Source=|DataDirectory|\roadkill.sqlite;");
			Driver.FindElement(By.CssSelector("div.continue input")).Click();

			// step 3
			Driver.FindElement(By.CssSelector("div.continue input")).Click();

			// step 3b
			Driver.FindElement(By.Id("AdminEmail")).SendKeys("admin@localhost");
			Driver.FindElement(By.Id("AdminPassword")).SendKeys("password");
			Driver.FindElement(By.Id("password2")).SendKeys("password");
			Driver.FindElement(By.CssSelector("div.continue input")).Click();

			// step 4
			Driver.FindElement(By.CssSelector("input[id=UseObjectCache]")).Click();
			Driver.FindElement(By.CssSelector("div.continue input")).Click();

			// step5
			Assert.That(Driver.FindElement(By.CssSelector("div#installsuccess h1")).Text, Is.EqualTo("Installation successful"), Driver.PageSource);
			Driver.FindElement(By.CssSelector("div#installsuccess a")).Click();

			// login, create a page
			LoginAsAdmin();
			CreatePageWithTitleAndTags("Homepage", "homepage");

			//
			// ***Assert***
			//
			Driver.Navigate().GoToUrl(BaseUrl);
			Assert.That(Driver.FindElement(By.CssSelector(".pagetitle")).Text, Contains.Substring("Homepage"));
			Assert.That(Driver.FindElement(By.CssSelector("#pagecontent p")).Text, Contains.Substring("Some content goes here"));
		}

		[Test]
		[Explicit("Requires MySQL 5 installed on the machine the acceptance tests are running first.")]
		public void All_Steps_With_Minimum_Required_MySQL_Should_Complete()
		{
			// Arrange
			Driver.Navigate().GoToUrl(BaseUrl);
			ClickLanguageLink();

			//
			// ***Act***
			//

			// step 1
			Driver.FindElement(By.CssSelector("input[id=testwebconfig]")).Click();
			Driver.WaitForElementDisplayed(By.CssSelector(".continue > a")).Click();

			// step 2
			Driver.FindElement(By.Id("SiteName")).SendKeys("Acceptance tests");
			SelectElement select = new SelectElement(Driver.FindElement(By.Id("DataStoreTypeName")));
			select.SelectByValue(DataStoreType.MySQL.Name);

			Driver.FindElement(By.Id("ConnectionString")).SendKeys(@"server=localhost;database=roadkill;uid=root;pwd=Passw0rd;");
			Driver.FindElement(By.CssSelector("div.continue input")).Click();

			// step 3
			Driver.FindElement(By.CssSelector("div.continue input")).Click();

			// step 3b
			Driver.FindElement(By.Id("AdminEmail")).SendKeys("admin@localhost");
			Driver.FindElement(By.Id("AdminPassword")).SendKeys("password");
			Driver.FindElement(By.Id("password2")).SendKeys("password");
			Driver.FindElement(By.CssSelector("div.continue input")).Click();

			// step 4
			Driver.FindElement(By.CssSelector("input[id=UseObjectCache]")).Click();
			Driver.FindElement(By.CssSelector("div.continue input")).Click();

			// step5
			Assert.That(Driver.FindElement(By.CssSelector("div#installsuccess h1")).Text, Is.EqualTo("Installation successful"), Driver.PageSource);
			Driver.FindElement(By.CssSelector("div#installsuccess a")).Click();

			// login, create a page
			LoginAsAdmin();
			CreatePageWithTitleAndTags("Homepage", "homepage");

			//
			// ***Assert***
			//
			Driver.Navigate().GoToUrl(BaseUrl);
			Assert.That(Driver.FindElement(By.CssSelector(".pagetitle")).Text, Contains.Substring("Homepage"));
			Assert.That(Driver.FindElement(By.CssSelector("#pagecontent p")).Text, Contains.Substring("Some content goes here"));
		}

		[Test]
		[Explicit("Requires Postgres 9 server installed on the machine the acceptance tests are running first.")]
		public void All_Steps_With_Minimum_Required_Postgres_Should_Complete()
		{
			// Arrange
			Driver.Navigate().GoToUrl(BaseUrl);
			ClickLanguageLink();

			//
			// ***Act***
			//

			// step 1
			Driver.FindElement(By.CssSelector("input[id=testwebconfig]")).Click();
			Driver.WaitForElementDisplayed(By.CssSelector(".continue > a")).Click();

			// step 2
			Driver.FindElement(By.Id("SiteName")).SendKeys("Acceptance tests");
			SelectElement select = new SelectElement(Driver.FindElement(By.Id("DataStoreTypeName")));
			select.SelectByValue(DataStoreType.Postgres.Name);

			Driver.FindElement(By.Id("ConnectionString")).SendKeys(@"server=localhost;database=roadkill;uid=postgres;pwd=Passw0rd;");
			Driver.FindElement(By.CssSelector("div.continue input")).Click();

			// step 3
			Driver.FindElement(By.CssSelector("div.continue input")).Click();

			// step 3b
			Driver.FindElement(By.Id("AdminEmail")).SendKeys("admin@localhost");
			Driver.FindElement(By.Id("AdminPassword")).SendKeys("password");
			Driver.FindElement(By.Id("password2")).SendKeys("password");
			Driver.FindElement(By.CssSelector("div.continue input")).Click();

			// step 4
			Driver.FindElement(By.CssSelector("input[id=UseObjectCache]")).Click();
			Driver.FindElement(By.CssSelector("div.continue input")).Click();

			// step5
			Assert.That(Driver.FindElement(By.CssSelector("div#installsuccess h1")).Text, Is.EqualTo("Installation successful"), Driver.PageSource);
			Driver.FindElement(By.CssSelector("div#installsuccess a")).Click();

			// login, create a page
			LoginAsAdmin();
			CreatePageWithTitleAndTags("Homepage", "homepage");

			//
			// ***Assert***
			//
			Driver.Navigate().GoToUrl(BaseUrl);
			Assert.That(Driver.FindElement(By.CssSelector(".pagetitle")).Text, Contains.Substring("Homepage"));
			Assert.That(Driver.FindElement(By.CssSelector("#pagecontent p")).Text, Contains.Substring("Some content goes here"));
		}
		
		[Test]
		[Explicit("Requires SQL Server Express 2012 (but it uses the Lightspeed SQL Server 2005 driver) installed on the machine the acceptance tests are running first, using LocalDB.")]
		public void All_Steps_With_Minimum_Required_SQLServer2005_Should_Complete()
		{
			// Arrange
			Driver.Navigate().GoToUrl(BaseUrl);
			ClickLanguageLink();

			//
			// ***Act***
			//

			// step 1
			Driver.FindElement(By.CssSelector("input[id=testwebconfig]")).Click();
			Driver.WaitForElementDisplayed(By.CssSelector(".continue > a")).Click();

			// step 2
			Driver.FindElement(By.Id("SiteName")).SendKeys("Acceptance tests");
			SelectElement select = new SelectElement(Driver.FindElement(By.Id("DataStoreTypeName")));
			select.SelectByValue(DataStoreType.SqlServer2005.Name);

			Driver.FindElement(By.Id("ConnectionString")).SendKeys(@"Server=(LocalDB)\v11.0;Integrated Security=true;");
			Driver.FindElement(By.CssSelector("div.continue input")).Click();

			// step 3
			Driver.FindElement(By.CssSelector("div.continue input")).Click();

			// step 3b
			Driver.FindElement(By.Id("AdminEmail")).SendKeys("admin@localhost");
			Driver.FindElement(By.Id("AdminPassword")).SendKeys("password");
			Driver.FindElement(By.Id("password2")).SendKeys("password");
			Driver.FindElement(By.CssSelector("div.continue input")).Click();

			// step 4
			Driver.FindElement(By.CssSelector("input[id=UseObjectCache]")).Click();
			Driver.FindElement(By.CssSelector("div.continue input")).Click();

			// step5
			Assert.That(Driver.FindElement(By.CssSelector("div#installsuccess h1")).Text, Is.EqualTo("Installation successful"), Driver.PageSource);
			Driver.FindElement(By.CssSelector("div#installsuccess a")).Click();

			// login, create a page
			LoginAsAdmin();
			CreatePageWithTitleAndTags("Homepage", "homepage");

			//
			// ***Assert***
			//
			Driver.Navigate().GoToUrl(BaseUrl);
			Assert.That(Driver.FindElement(By.CssSelector(".pagetitle")).Text, Contains.Substring("Homepage"));
			Assert.That(Driver.FindElement(By.CssSelector("#pagecontent p")).Text, Contains.Substring("Some content goes here"));
		}

		[Test]
		[Explicit("Requires SQL Server Express 2012 (but it uses the Lightspeed SQL Server 2008 driver) installed on the machine the acceptance tests are running first, using LocalDB.")]
		public void All_Steps_With_Minimum_Required_SQLServer2008_Should_Complete()
		{
			// Arrange
			Driver.Navigate().GoToUrl(BaseUrl);
			ClickLanguageLink();

			//
			// ***Act***
			//

			// step 1
			Driver.FindElement(By.CssSelector("input[id=testwebconfig]")).Click();
			Driver.WaitForElementDisplayed(By.CssSelector(".continue > a")).Click();

			// step 2
			Driver.FindElement(By.Id("SiteName")).SendKeys("Acceptance tests");
			SelectElement select = new SelectElement(Driver.FindElement(By.Id("DataStoreTypeName")));
			select.SelectByValue(DataStoreType.SqlServer2008.Name);

			Driver.FindElement(By.Id("ConnectionString")).SendKeys(@"Server=(LocalDB)\v11.0;Integrated Security=true;");
			Driver.FindElement(By.CssSelector("div.continue input")).Click();

			// step 3
			Driver.FindElement(By.CssSelector("div.continue input")).Click();

			// step 3b
			Driver.FindElement(By.Id("AdminEmail")).SendKeys("admin@localhost");
			Driver.FindElement(By.Id("AdminPassword")).SendKeys("password");
			Driver.FindElement(By.Id("password2")).SendKeys("password");
			Driver.FindElement(By.CssSelector("div.continue input")).Click();

			// step 4
			Driver.FindElement(By.CssSelector("input[id=UseObjectCache]")).Click();
			Driver.FindElement(By.CssSelector("div.continue input")).Click();

			// step5
			Assert.That(Driver.FindElement(By.CssSelector("div#installsuccess h1")).Text, Is.EqualTo("Installation successful"), Driver.PageSource);
			Driver.FindElement(By.CssSelector("div#installsuccess a")).Click();

			// login, create a page
			LoginAsAdmin();
			CreatePageWithTitleAndTags("Homepage", "homepage");

			//
			// ***Assert***
			//
			Driver.Navigate().GoToUrl(BaseUrl);
			Assert.That(Driver.FindElement(By.CssSelector(".pagetitle")).Text, Contains.Substring("Homepage"));
			Assert.That(Driver.FindElement(By.CssSelector("#pagecontent p")).Text, Contains.Substring("Some content goes here"));
		}

		[Test]
		[Explicit("Requires SQL Server Express 2012 installed on the machine the acceptance tests are running first, using LocalDB.")]
		[Description("These tests go through the entire installer workflow to ensure no localization strings break the installer.")]
		[TestCase(Language.English)]
		[TestCase(Language.Czech)]
		[TestCase(Language.Dutch)]
		[TestCase(Language.German)]
		[TestCase(Language.Hindi)]
		[TestCase(Language.Italian)]
		[TestCase(Language.Polish)]
		[TestCase(Language.Portuguese)]
		[TestCase(Language.Russian)]
		[TestCase(Language.Spanish)]
		[TestCase(Language.Swedish)]
		public void All_Steps_With_Minimum_Required_SQLServer2012_Should_Complete(Language language)
		{
			// Arrange
			Driver.Navigate().GoToUrl(BaseUrl);
			ClickLanguageLink(language);

			//
			// ***Act***
			//

			// step 1
			Driver.FindElement(By.CssSelector("input[id=testwebconfig]")).Click();
			Driver.WaitForElementDisplayed(By.CssSelector(".continue > a")).Click();

			// step 2
			Driver.FindElement(By.Id("SiteName")).SendKeys("Acceptance tests");
			SelectElement select = new SelectElement(Driver.FindElement(By.Id("DataStoreTypeName")));
			select.SelectByValue(DataStoreType.SqlServer2012.Name);

			Driver.FindElement(By.Id("ConnectionString")).SendKeys(@"Server=(LocalDB)\v11.0;Integrated Security=true;");
			Driver.FindElement(By.CssSelector("div.continue input")).Click();

			// step 3
			Driver.FindElement(By.CssSelector("div.continue input")).Click();

			// step 3b
			Driver.FindElement(By.Id("AdminEmail")).SendKeys("admin@localhost");
			Driver.FindElement(By.Id("AdminPassword")).SendKeys("password");
			Driver.FindElement(By.Id("password2")).SendKeys("password");
			Driver.FindElement(By.CssSelector("div.continue input")).Click();

			// step 4
			Driver.FindElement(By.CssSelector("input[id=UseObjectCache]")).Click();
			Driver.FindElement(By.CssSelector("div.continue input")).Click();

			// step5
			//Assert.That(Driver.FindElement(By.CssSelector("div#installsuccess h1")).Text, Is.EqualTo("Installation successful"), Driver.PageSource);
			Driver.FindElement(By.CssSelector("div#installsuccess a")).Click();

			// login, create a page
			LoginAsAdmin();
			CreatePageWithTitleAndTags("Homepage", "homepage");

			//
			// ***Assert***
			//
			Driver.Navigate().GoToUrl(BaseUrl);
			Assert.That(Driver.FindElement(By.CssSelector(".pagetitle")).Text, Contains.Substring("Homepage"));
			Assert.That(Driver.FindElement(By.CssSelector("#pagecontent p")).Text, Contains.Substring("Some content goes here"));
		}
	}
}
