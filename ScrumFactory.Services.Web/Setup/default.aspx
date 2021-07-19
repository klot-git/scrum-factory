<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="ScrumFactory.Services.Web.Setup._default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <link rel="stylesheet" href="../css/style.css" type="text/css" />
    <title>The Scrum Factory - Where is my db?</title>    
</head>
<body>

    <div id="mainBanner">
        <h1>The Scrum Factory</h1>
    </div>
   
    
    <div id="content">
        
    
    <center>
        <h2>Where is my db?</h2>
        <div class="alert">
        <img src="../images/alert.png" alt="alert image" style="vertical-align: middle" />
        Scrum Factory Database has not been installed yet.
    </div>
    <p>
    Before you start to use <i>Scrum Factory</i> the database should be created.        
    </p>

    </center>

    <ul style="width: 600px; margin-left: -300px; position: absolute; left:50%">
        <li>if the database was created at your MS Sql Server, check the connection string at the web.config file<br /><br /></li>        
        <li>if the database has not been created yet, you can manually create the database using the DB_SCRIPT.sql script located at the app_data folder<br /><br /></li>        
        <li>or you can re-install the server</li>
    </ul>
    

    </div>

</body>
</html>
