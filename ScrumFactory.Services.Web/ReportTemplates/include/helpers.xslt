<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
    <xsl:output method="xml" indent="yes"/>


  <xsl:template name="breakLines">
    <xsl:param name="text" select="string(.)"/>
    <xsl:choose>
      <xsl:when test="contains($text, '&#xa;')">
        <xsl:value-of select="substring-before($text, '&#xa;')"/>
        <LineBreak />
        <xsl:call-template name="breakLines">
          <xsl:with-param
            name="text"
            select="substring-after($text, '&#xa;')"
        />
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$text"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

    <xsl:template name="formatTime">
      <xsl:param name="dateTime" />
      <xsl:value-of select="substring-after($dateTime, 'T')" />
    </xsl:template>

  <xsl:template name="levelImage">
    <xsl:param name="level"/>
    <xsl:variable name="imageUrl">
      <xsl:choose>
        <xsl:when test="$level = 0">
          <xsl:value-of select="concat($ServerUrl,'/Images/RiskIndicators/GreenSquare.png')"/>
        </xsl:when>
        <xsl:when test="$level = 1">
          <xsl:value-of select="concat($ServerUrl,'/Images/RiskIndicators/BlueSquare.png')"/>
        </xsl:when>
        <xsl:when test="$level = 2">
          <xsl:value-of select="concat($ServerUrl,'/Images/RiskIndicators/YellowSquare.png')"/>
        </xsl:when>
        <xsl:when test="$level = 3">
          <xsl:value-of select="concat($ServerUrl,'/Images/RiskIndicators/RedSquare.png')"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="text">
      <xsl:choose>
        <xsl:when test="$level = 0">
          <xsl:value-of select="$_NONE"/>
        </xsl:when>
        <xsl:when test="$level = 1">
          <xsl:value-of select="$_LOW"/>
        </xsl:when>
        <xsl:when test="$level = 2">
          <xsl:value-of select="$_MEDIUM"/>
        </xsl:when>
        <xsl:when test="$level = 3">
          <xsl:value-of select="$_HIGH"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <BlockUIContainer>
      <Image Source="{$imageUrl}" Width="32" Height="32"/>
    </BlockUIContainer>
    <Paragraph TextAlignment="Center">
      <xsl:value-of select="$text" />
    </Paragraph>
  </xsl:template>


  <xsl:template name="itemStatus">
    <xsl:choose>
      <xsl:when test="Status = 0" >
        <xsl:value-of select="$_PLANNED"/>
      </xsl:when>
      <xsl:when test="Status = 1" >
        <xsl:value-of select="$_WORKING_ON"/>
      </xsl:when>
      <xsl:when test="Status = 2" >
        <xsl:value-of select="$_DONE"/>
      </xsl:when>
      <xsl:when test="Status = 3" >
        <xsl:value-of select="$_CANCELED"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <xsl:variable name="ServerUrl" select="/ReportData/ServerUrl"/>


  <xsl:template name="reportHeader">
    <xsl:param name="title"/>
    <xsl:param name="showSprintsInfo" select="'true'"/>
    <Table Style="{{StaticResource headerTable}}">
      <Table.Columns>
        <TableColumn/>
        <TableColumn  Width="130" />
      </Table.Columns>
      <TableRowGroup>
        <TableRow>
          <TableCell>
            <Paragraph Style="{{StaticResource TitleParagraph}}" Margin="0">
              <xsl:value-of select="$title"/><LineBreak/>
              <TextBlock FontSize="20">
                <xsl:value-of select="Project/ProjectName"/> (<xsl:value-of select="Project/ProjectNumber"/>)<LineBreak/>
                <xsl:value-of select="Project/ClientName"/>
              </TextBlock>
            </Paragraph>            
          </TableCell>
          <TableCell >
            <BlockUIContainer>
              <Image HorizontalAlignment="Right" VerticalAlignment="Bottom" Source="{$ServerUrl}/Images/Companylogo.png" Width="105" Margin="0,5,5,0"/>
            </BlockUIContainer>
          </TableCell>
        </TableRow>
        <xsl:if test="$showSprintsInfo">
          <TableRow>
            <TableCell ColumnSpan="2">
              <Paragraph  Margin="0,0,0,0" Padding="0,0,0,0">
                <Floater HorizontalAlignment="Left" Margin="0,0,0,0" Padding="0,0,0,0">
                  <Paragraph>
                    Scrum Master(s):
                    <xsl:for-each select="Project/Memberships/ProjectMembership[Role/PermissionSet=0]">
                      <xsl:value-of select="Member/FullName"/>
                      <xsl:if test="position() != last()" >,</xsl:if>
                    </xsl:for-each>
                  </Paragraph>
                </Floater>
                <Floater HorizontalAlignment="Right"  Margin="0,0,0,0" Padding="0,0,0,0">
                  <xsl:call-template name="sprintHeaderIndicator"/>
                </Floater>
              </Paragraph>
            </TableCell>          
          </TableRow>  
        </xsl:if>
        
      </TableRowGroup>
    </Table>
    <BlockUIContainer>
      <Rectangle Fill="Black" Height="4" />
    </BlockUIContainer>
    <Paragraph FontStyle="Italic" TextAlignment="Right" Margin="0">
      <xsl:call-template name="formatDate">
        <xsl:with-param name="dateTime" select="Today" />
      </xsl:call-template>
      &#x20;,
      <xsl:call-template name="formatTime">
        <xsl:with-param name="dateTime" select="Today" />
      </xsl:call-template>
    </Paragraph>
  </xsl:template>

  <xsl:template name="sprintHeaderIndicator">
    <Paragraph TextAlignment="Right" Margin="0">
      <xsl:if test="/ReportData/ProjectCurrentSprintNumber">
        <TextBlock Padding="2" FontSize="12" Margin="0,0,5,0">Sprint</TextBlock>
        <xsl:for-each select="Project/Sprints/Sprint">
          <xsl:sort select="SprintNumber" data-type="number"/>
          <xsl:variable name="background">
            <xsl:choose>
              <xsl:when test="SprintNumber = /ReportData/ProjectCurrentSprintNumber">Black</xsl:when>
              <xsl:otherwise>Transparent</xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <xsl:variable name="foreground">
            <xsl:choose>
              <xsl:when test="SprintNumber = /ReportData/ProjectCurrentSprintNumber">White</xsl:when>
              <xsl:otherwise>Black</xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <TextBlock Background="{$background}" Foreground="{$foreground}" Margin="1,0,1,0" Padding="2,0,2,0" FontSize="12">
            <xsl:value-of select="SprintNumber"/>
          </TextBlock>
        </xsl:for-each>
      </xsl:if>
    </Paragraph>
  </xsl:template>

  <xsl:template name="projectTeam">
    <Table>
      <Table.Columns>
        <TableColumn Width=".35*" />
        <TableColumn Width=".65*"/>
      </Table.Columns>
      <TableRowGroup>
        <xsl:for-each select="Project/Memberships/ProjectMembership">
          <xsl:sort select="Role/PermissionSet" data-type="number"/>
          <xsl:sort select="Member/FullName" data-type="text"/>
          <xsl:variable name="roleUId" select="RoleUId"/>
          <xsl:variable name="role" select="Role"/>
          <TableRow>
            <TableCell>
              <Paragraph>                
                <xsl:value-of select="$role/RoleName"/>                
              </Paragraph>
            </TableCell>
            <TableCell>
              <Paragraph  Style="{{StaticResource ValueParagraph}}" TextAlignment="Right">                
                <xsl:value-of select="Member/FullName"/>
                <xsl:if test="Member/EmailAccount"> (<xsl:value-of select="Member/EmailAccount"/>)</xsl:if>                
              </Paragraph>
            </TableCell>
          </TableRow>
        </xsl:for-each>
      </TableRowGroup>
    </Table>
  </xsl:template>

  

</xsl:stylesheet>
