<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="ScrumFactory.Services.Web.WindowsAuth.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head >
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta name="viewport" content="width=device-width, initial-scale=0.8, maximum-scale=0.8, user-scalable=0" />
    <title>Scrum Factory - Windows Authentication</title>
    <link rel="stylesheet" href="../css/style.css" type="text/css" />    
</head>
<body>
        
    <div id="content" style="margin-left: auto; margin-right: auto; width: 300px">
        <h2>Windows Authentication</h2>
   
    <form id="form1" runat="server">
    
    
            <h3><asp:Literal ID="domainLiteral" runat="server" /></h3>
                

            <span style="color: Red">
                <asp:Literal ID="messageLiteral" runat="server"></asp:Literal>
            </span>
            

            
            <asp:Panel ID="loginPanel" runat="server">

                <label>User</label>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator2"
                    ControlToValidate="userTextBox"
                    EnableClientScript="true"
                    Width="20"
                    runat="server">
                        <span style="color: Red; margin-left: 10px;">*</span>
                </asp:RequiredFieldValidator>
               
                <asp:TextBox ID="userTextBox" runat="server" Width="100%"/>
               

            <br /><br />
            <label>Password</label>        
            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" ControlToValidate="passwordTextBox"
                EnableClientScript="true" runat="server" Width="20" ><span style="color: Red;  margin-left: 10px;">*</span></asp:RequiredFieldValidator>
                
            <asp:TextBox ID="passwordTextBox" TextMode="Password" runat="server" Width="100%" />

            <br />
            <br />
            
            <asp:Button Text="Sign in" runat="server" OnClick="Signin_Click" Width="100%" Height="80" Font-Size="XX-Large"/>
            
            </asp:Panel>

            <asp:Panel ID="windowsAuthPanel" runat="server">
            
            Sign in with <span style="font-size: 16px; font-weight: bold"><asp:Literal ID="widowsUserLiteral" runat="server" /></span><br /><br />
            
                <asp:Button Text="Sign in" runat="server" OnClick="SigninWindowsUser_Click"  Width="100%" Height="80"/>
            
           

            
            </asp:Panel>
        
         <br /><br />
            <asp:LinkButton ID="LinkButton1" runat="server" OnClick="Show_LoginPanel_Click">Or click here to sign in with another user</asp:LinkButton>

            
    
    </form>
    </div>

   
</body>
</html>
