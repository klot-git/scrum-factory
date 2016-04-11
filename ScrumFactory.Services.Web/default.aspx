<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="ScrumFactory.Services.Web._default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <link rel="stylesheet" href="./css/style.css" type="text/css" />
    <script type='text/javascript' src="./scripts/knockout-3.1.0.js"></script>
    <script type='text/javascript' src="./scripts/jquery-1.11.1.min.js"></script>
    <script type='text/javascript' src="./scripts/projectSearch.js"></script>
    <title><%=DefaultCompanyName.ToLower()%> hub</title>
    <script type="text/javascript">
        pageVM.webapp = '<%=ResolveUrl("~")%>';
    </script>
</head>
<body>
     <header></header>
    <div id="mainBanner">
        <h1>The Scrum Factory</h1>
    </div>

    <div id="content">

        <ul id="topMenu">
            <%if(IsPublicHub) { %>
            <li><a href="http://scrum-factory.com/">home</a></li>
            <li><a href="#">public hub</a></li>
            <li><a href="http://scrum-factory.com/documentation">documentation</a></li>
            <li><a href="http://scrum-factory.com/support">support</a></li>
            <li><a href="http://scrum-factory.com/features">features</a></li>
            <li><a href="http://scrum-factory.com/download">for companies</a></li>
            <li><a href="http://scrum-factory.com/contribute">contribute</a></li>
            <%} %>
            <%else { %>           
            <li><a href="#"><%=DefaultCompanyName.ToLower()%> hub</a></li>
            <li><a href="http://scrum-factory.com/documentation">documentation</a></li>
            <li><a href="http://scrum-factory.com/">scrum factory web-site</a></li>
            <%} %>
    </ul>
        
      
       
            <h2><%=DefaultCompanyName%> hub</h2>
            <div>                
                <label>search for a project</label>
                <input type="text" 
                    onkeyup="pageVM.keyPressed(); setTimeout(function(){window.scrollTo(0, 220)}, 1000);"
                    data-bind="value: tagFilter, valueUpdate:'afterkeydown'" 
                    placeholder="type a for project number, name or client to search" style="width: 880px; margin-right: 5px"/>                
                <button onclick="pageVM.search()" >Search</button>                
            </div>
            
                
            <div  style="margin-top: 20px; margin-bottom: 20px;">   
                <span data-bind="visible: projects().length ==0 && firstSearch()"><i>Sorry, we could not find any project with this number, client or name</i></span>       
                <span data-bind="visible: projects().length >=20"><i>showing top 20 results</i></span>       
                <ul data-bind="foreach: projects" class="projectList">
                    <li>
                        
                        <div data-bind="css:{ 'prjImage0': Status==0, 'prjImage3': Status==3, 'prjImage5': Status==5}"></div>

                        <div class="prjData">

                            <a class="openFactoryLink" data-bind="attr: { href: '<%=Page.ResolveUrl("~")%>' + ProjectNumber}, text: ClientName + ' - ' + ProjectName + ' [' + ProjectNumber + ']'"></a>                            
                            <a data-bind="attr: { href: CodeRepositoryPath, title: CodeRepositoryPath }, visible: CodeRepositoryPath!=null">
                                <img src="./images/tail.png" class="repoLink" />
                            </a> 
                            
                            
                            <br style="clear: both" />
                            
                            <div style="color: Gray; font-style: italic; width: 600px; word-wrap:break-word; font-size: 12px;" data-bind="text: Description"></div>                            
                      
                            
                            <div style="position: absolute; right: 5px; top: 5px; text-align: center; width: 200px">
                                <img class="memberImage" data-bind="attr: { src: 'memberImage.aspx?memberUId=' + CreateBy, title: CreateBy }"  /><br />                                
                                <a style="font-size: 10px" data-bind="attr: { href: 'mailto:' + CreateBy }, text: CreateBy"></a>
                            </div>
                            
                            
                        </div>
                                                
                        
                    </li>
                </ul>
            </div>

            <br style="clear: both"/>

            <hr data-bind="visible: projects().length >=1" style="display: none" />
            <a href="<%=SFClientServer %>/SFClient2012/ScrumFactory.application?server=<%=SFServer %>" style="font-size: 22px;">                
                or click here to start Scrum Factory                
            </a>
            
            <p>
                <b style="font-weight: bold">Requirements</b><br />
                This server supports clients of following versions: <%=SFClientServerVersion %>.*.*<br />
                Scrum Factory is a Click-Once Windows Client application and requires .Net Framework 4.0 to run.                


            </p>
            <p>
            
                <img src="<%=ResolveUrl("~/images/chrome_logo.png")%>" style="float: left; margin-right: 15px;" /> 
            
            
                <div style="float: left">
                    <b style="font-weight: bold">For Chorme web-browsers, please install the</b><br />
                    <a href="https://chrome.google.com/webstore/detail/clickonce-for-google-chro/eeifaoomkminpbeebjdmdojbhmagnncl">
                    Click-Once Google Extension
                    </a>
                </div>
                
                <br style="clear: both"/>
            </p>

      

        

    </div>



    <footer>
    <div style="width: 980px; position: relative; left: 50%; margin-left: -490px;">
    <img src="<%=ResolveUrl("~/images/factoryLogoFooter.png") %>" style="position: absolute; right: 0px; top:-70px; width: 130px;" />
     <div style="float:left;  margin-top: 10px;margin-right: 50px;">
        <a href="http://scrum-factory.com/privacy-policy.aspx">privacy police</a>
    </div>
     <div style="float:right;  margin-top: 10px;margin-right: 50px;">
        <h3>support</h3>
        <ul>
            <li><a href="http://scrum-factory.com/support/faq.aspx">FAQ</a></li>
             <li><a href="https://thescrumfactory2012.codeplex.com/discussions">Forum</a></li>
            <li><a href="mailto:support@scrum-factory">E-mail</a></li>
        </ul>
    </div>
    <div style="float:right; margin-top: 10px; margin-right: 50px;">
        <h3>documentation</h3>
        <ul>
            <li><a href="http://scrum-factory.com/documentation/sf-user-guide.aspx">SF user guide</a></li>
             <li><a href="http://scrum-factory.com/documentation/sf-server-guide.aspx">SF server setup guide</a></li>
        </ul>
    </div>
   
    


    
</div>
</footer>

</body>
</html>
