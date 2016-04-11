<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet
    version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"    
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/xps/2005/06/resourcedictionary-key">
  
    <xsl:output method="xml" indent="yes"/>

  <xsl:include href="../include/locale.en-us.xslt"/>
  <xsl:include href="../include/emailHelpers.xslt"/>
   
    <xsl:template match="/ReportData">
      
      <xsl:variable name="projectNumber" select="Project/ProjectNumber"/>
            
      <html>
        <body>
          <table cellpadding="10">
            <tr>
              <td style="vertical-align: top">
                <img src="{$ServerUrl}/Images/Companylogo.png" width="60"/>
              </td>
              <td>
                <p style="font-family: Segoe UI">
                  You are invited to join the  <b>
                    <xsl:value-of select="Project/ClientName"/> - <xsl:value-of select="Project/ProjectName"/> (<xsl:value-of select="$projectNumber"/>) </b> project as a <xsl:value-of select="/ReportData/RoleName"/>.
                </p>                
                <p style="font-family: Segoe UI">
                  The project will be conducted by
                </p>
                <xsl:call-template name="projectScrumMaster"/>
                
                <p style="font-family: Segoe UI">
                  Use the following link to get to know more about the project and to confirm your engage level:<br/>
                  <a href="{$ServerUrl}">
                    <xsl:value-of select="$ServerUrl"/>
                  </a>
                </p>
              </td>
            </tr>
          </table>
        </body>
      </html>
    </xsl:template>
  


</xsl:stylesheet>
