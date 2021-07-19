Scrum Factory
=============

Scrum Factory is a client-server application to manage Scrum projects.
This repository contains the .Net server-side source-code.

**Server Installation**<br>
You can download and install the server binaries using this link:<br>
https://github.com/klot-git/scrum-factory/releases

Follow the instructions to install the server:

	a)	Unzip the app_root.zip into your IIS website root or into a virtual 
		application directory.

	b)	Make sure it runs under a .Net Framework 4.0 application pool.

	c)	Execute the SQL script DB_SCRIPT.sql located at the App_Data 
		to create the Scrum Factory Database.
	
	d)	Change the web.config connection string to your database.  


**Client Download**<br>
The 32-bits client application is no longer supported, but you can install a Windows 10 UWP application at the Microsoft Store:<br>
https://www.microsoft.com/store/apps/9NBLGGH68DVB
