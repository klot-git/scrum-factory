<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
    <xsl:output method="xml" indent="yes"/>

  <xsl:variable name="ServerUrl" select="/ReportData/ServerUrl"/>

  <xsl:template name="projectTeam">
    <table>
      <xsl:for-each select="Project/Memberships/ProjectMembership">
        <xsl:sort select="Role/PermissionSet" data-type="number"/>
        <xsl:sort select="Member/FullName" data-type="text"/>
        <xsl:variable name="roleUId" select="RoleUId"/>
        <xsl:variable name="role" select="Role"/>        
        <xsl:variable name="memberUId" select="MemberUId"/>
        <tr>
          <td>
            <img src="{$ServerUrl}/MemberImage.aspx?memberUId={$memberUId}" width="60"/>
          </td>          
          <td style="font-family: Segoe UI">
            <xsl:value-of select="Member/FullName"/> (<xsl:value-of select="$role/RoleName"/>)<br/>
            <xsl:if test="Member/EmailAccount">
              <xsl:value-of select="Member/EmailAccount"/>
            </xsl:if>
          </td>
        </tr>              
      </xsl:for-each>
    </table>
  </xsl:template>

  <xsl:template name="projectScrumMaster">
    <table>
      <xsl:for-each select="Project/Memberships/ProjectMembership">
        <xsl:sort select="//Project/Roles/Role/PermissionSet" data-type="number"/>
        <xsl:variable name="roleUId" select="RoleUId"/>
        <xsl:variable name="role" select="//Project/Roles/Role[RoleUId=$roleUId]"/>
        <xsl:variable name="memberUId" select="MemberUId"/>
        <xsl:if test="$role/PermissionSet=0">
          <tr>
            <td>
              <img src="{$ServerUrl}/MemberImage.aspx?memberUId={$memberUId}" width="60"/>
            </td>
            <td style="font-family: Segoe UI">
              <xsl:value-of select="Member/FullName"/> (<xsl:value-of select="$role/RoleName"/>)<br/>
              <xsl:if test="Member/EmailAccount">
                <xsl:value-of select="Member/EmailAccount"/>
              </xsl:if>
            </td>
          </tr>
        </xsl:if>
      </xsl:for-each>
    </table>
  </xsl:template>

</xsl:stylesheet>
