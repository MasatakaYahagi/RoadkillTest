﻿---- Mono on Linux (Ubuntu) support ----

Roadkill works with Mono 2.10.8.1 on Ubuntu 12.10 x64 with MongoDB, mySQL or Postgres.
For the easiest friction free install MongoDB is lightening fast and the only one tested so far.

NOTE: the instructions below are for testing/development boxes. You should harden the security of your machine if
you are intending to use it for anything other than testing and development. The bash script creates a website in 
the Apache root folder and sets the www root folder to be owned by the www-data user and group, and also sets up the firewall.

NOTE 2: Mono support is experimental. Whilst most of the functionality is fine, Roadkill is tested using IIS (Express) so you 
may find some issues that aren't issues with the Windows version.

## Amazon AMI images

There are two public Amazon AMI images you can launch Roadkill using:

xyz1 - Roadkill (not installed) on Ubuntu, with MongoDB installed.
xyz2 - Roadkill (installed) on Ubuntu, with MongoDB installed. Login using admin@localhost/Password1

### Setting up the server manually
A good cheap provider is Digitalocean.com - $5 a month for an Ubuntu VM.

See the mono.sh bash script in the root folder for installing - the bash script automatically downloads the Mono build from Bitbucket and unzips it for you.

To run the bash script automatically on your server use the following:

sudo wget --no-check-certificate --no-cache https://gist.github.com/yetanotherchris/5426167/raw
sudo sh raw

The mono.sh script will leave Roadkill uninstalled. The default connection string for MongoDB is: "mongodb://roadkill:password@localhost/local".
If you need to FTP to your site, WinScp on Windows is a good FTP over SSH client.

### Known issues
* You might need to run "sudo sh raw" twice (close SSH, and log back in), as the first time the script will hang when installing mod_mono with this message:
	" ... waiting .apache2: Could not reliably determine the server's fully qualified domain name, using 127.0.0.1 for ServerName"
or at the firewall state, when it prompts you to enable the Firewall.

* Installer complains about connection string, and then installed=true is set but the database has no data (logins fail). Set installed=false
and start again. Then do sudo /etc/init.d/apache2 restart afterwards.

* Sometimes the installer gets stuck (from what looks like a cache issue with Mono). Restart Apache to get around this:
sudo /etc/init.d/apache2 restart

* System.ApplicationException: Failed to acquire lock after errors appear to be Mono related. Restart Apache to get around this:
sudo /etc/init.d/apache2 restart


### Building the solution for Mono
If you are building the solution, there are a couple of things you will need to do:

- Publish the project using the Mono configuration.
- Delete System.Web.dll
- Delete Microsoft.Web.Infrastructure.dll, Microsoft.Web.Administration.dll from the bin folder.
- Delete any .txt files from the root folder (they interfere with the MVC RouteTables).

- SQL Server, SQL Server Express, SQL Server CE are obviously not supported.
- SQLite isn't supported, as it would required 3 different binaries to float around.
- Copy lib/LightSpeed/Mindscape.LightSpeed.MetaData.dll to the bin folder
- Copy lib/LightSpeed/Providers/log4net.dll to the bin folder
- Copy lib/LightSpeed/Providers/Memcached.ClientLibrary.dll to the bin folder
- Copy lib/Configs/apache.txt to site root

### References
A lot of help came from:
http://www.toptensoftware.com/Articles/6/Setting-up-a-Ubuntu-Apache-MySQL-Mono-ASP-NET-MVC-2-Development-Server
http://devblog.rayonnant.net/2012/11/mvc3-working-in-mono-ubuntu-1210.html