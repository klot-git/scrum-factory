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
      <xsl:variable name="ticketFolder" select="/ReportData/TicketFolder"/>
      <xsl:variable name="docFolder" select="Project/DocRepositoryPath"/>
      <xsl:variable name="codeFolder" select="Project/CodeRepositoryPath"/>
            
      <html>
        <body>
          <table cellpadding="10">
            <tr>
              <td style="vertical-align: top">
                <img src="{$ServerUrl}/Images/Companylogo.png" width="60"/>
              </td>
              <td>
                <p style="font-family: Segoe UI">
                  The ticket <b>#<xsl:value-of select="BacklogItem/BacklogItemNumber"/></b> was created for the project <b>
                    <xsl:value-of select="Project/ClientName"/> - <xsl:value-of select="Project/ProjectName"/> (<xsl:value-of select="$projectNumber"/>) </b>.                             
                </p>
                
                <p style="font-family: Segoe UI">
                  <xsl:value-of select="BacklogItem/Name"/>
                </p>

                <xsl:if test="string-length($docFolder) &gt; 0">
                <p style="font-family: Segoe UI">
                  <b>Project documents folder:</b>
                  <br/>
                  <a href="{$docFolder}">
                    <xsl:value-of select="$docFolder"/>
                  </a>
                </p>
                </xsl:if>
                
                <xsl:if test="string-length($codeFolder) &gt; 0">
                  <p style="font-family: Segoe UI">
                    <b>Project repository:</b>
                    <br/>
                    <a href="{$codeFolder}">
                      <xsl:value-of select="$codeFolder"/>
                    </a>
                  </p>
                </xsl:if>
                
                <p style="font-family: Segoe UI">
                  You can find more information about the project at:<br/>
                  <a href="scrum-factory://{$projectNumber}/">
                    scrum-factory://<xsl:value-of select="$projectNumber"/>/                    
                  </a>
                </p>
                
              </td>
            </tr>
          </table>
        </body>
      </html>
    </xsl:template>
  


</xsl:stylesheet>
