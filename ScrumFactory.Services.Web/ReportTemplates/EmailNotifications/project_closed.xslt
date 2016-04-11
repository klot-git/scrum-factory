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
                  Project <b>
                    <xsl:value-of select="Project/ClientName"/> - <xsl:value-of select="Project/ProjectName"/> (<xsl:value-of select="$projectNumber"/>) </b>was closed!
                    <br/>
                  Thank you for your engagement.
                </p>
                
                <table>
                  <tr>
                    <td style="font-family: Segoe UI; font-size: 22px">
                      <b>
                        <xsl:value-of select="/ReportData/BudgetIndicator"/> %
                      </b>                      
                    </td>
                    <td style="font-family: Segoe UI;">
                      of the budget was spent.
                    </td>
                  </tr>
                  <tr>
                    <td style="font-family: Segoe UI; font-size: 22px">
                      <b>
                        <xsl:value-of select="/ReportData/QualityIndicator"/> %
                      </b>
                    </td>
                    <td style="font-family: Segoe UI;">
                      of hours were spent correcting bugs and re-work.
                    </td>
                  </tr>
                  <tr>
                    <td style="font-family: Segoe UI; font-size: 22px">
                      <b>
                        <xsl:value-of select="/ReportData/VelIndicator"/> pts/hrs
                      </b>
                    </td>
                    <td style="font-family: Segoe UI;">
                      was the team velocity at this project.
                    </td>
                  </tr>
                  
                </table>
                
              </td>
            </tr>
          </table>
        </body>
      </html>
    </xsl:template>
  


</xsl:stylesheet>
