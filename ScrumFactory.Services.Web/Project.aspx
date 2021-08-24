<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Project.aspx.cs" Inherits="ScrumFactory.Services.Web.Project" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <link rel="stylesheet" href="./css/style.css" type="text/css" />        
    <title><%=DefaultCompanyName.ToLower()%> Scrum Factory Server</title>    
</head>
<body>
    <div id="mainBanner">
        <h1>The Scrum Factory</h1>
    </div>

    <div id="content">

        <center>
            <br />
            <br />
            <a href="scrum-factory://<%=ProjectNumber %>">
                <h1>Open project <%=ProjectNumber %></h1>                
            </a>
        </center>
        
    </div>

</body>
</html>
