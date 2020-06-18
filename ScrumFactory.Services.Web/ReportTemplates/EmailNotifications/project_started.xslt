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
                  The project <b>
                    <xsl:value-of select="Project/ClientName"/> - <xsl:value-of select="Project/ProjectName"/> (<xsl:value-of select="$projectNumber"/>) </b>was started.                             
                </p>
                <p style="font-family: Segoe UI">
                  It is schedule to has <b>
                    <xsl:value-of select="count(Project/Sprints/Sprint)"/> sprint(s)</b> and should be done by
                    <b>
                      <xsl:call-template name="formatDate">
                        <xsl:with-param name="dateTime" select="Project/Sprints/Sprint[SprintUId=/ReportData/LastSprintUId]/EndDate" />
                      </xsl:call-template>                      
                    </b>.
                </p>                
                <p style="font-family: Segoe UI">
                  The following people will be working with you:
                </p>
                <xsl:call-template name="projectTeam"/>

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
