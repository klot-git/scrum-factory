<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet
    version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"    
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/xps/2005/06/resourcedictionary-key">
  
    <xsl:output method="xml" indent="yes"/>
  
    <xsl:include href="../include/locale.xslt"/>
    <xsl:include href="../include/ProposalHelpers.xslt"/>    
    <xsl:include href="../include/styles.xslt"/>
  
    <xsl:template match="/ReportData">
      <FlowDocument        
        PageWidth="21cm"
        PageHeight="29.7cm"      
        PagePadding="80,40,80,40"
        LineHeight="25"
        ColumnWidth="21 cm">
      
        <xsl:call-template name="styles"/>

        <xsl:call-template name="reportHeader">
          <xsl:with-param name="title" select="Proposal/ProposalName"/>
          <xsl:with-param name="showSprintsInfo" select="false"/>
        </xsl:call-template>

        <Paragraph Style="{{StaticResource GroupParagraph}}">
          1. <xsl:value-of select="$_PROJECT_DESCRIPTION"/>           
        </Paragraph>
        <Paragraph>
          <xsl:call-template name="breakLines">
            <xsl:with-param name="text" select="Proposal/Description" />
          </xsl:call-template>          
        </Paragraph>

        <Paragraph Style="{{StaticResource GroupParagraph}}">
          2. <xsl:value-of select="$_TECHNOLOGY_and_PLATFORM"/>           
      </Paragraph>
        <Paragraph>
          <xsl:value-of select="Project/Platform"/>
        </Paragraph>

        <Paragraph Style="{{StaticResource GroupParagraph}}">
          3. <xsl:value-of select="$_SCOPE"/>
      </Paragraph>        
        <xsl:call-template name="proposalScope"/>

      <Paragraph>
        <xsl:variable name="totalHours" select="sum(//ArrayOfBacklogItem/BacklogItem[BacklogItemUId = //ArrayOfProposalItemWithPrice/ProposalItemWithPrice/BacklogItemUId]/CurrentPlannedHours/PlannedHour/Hours)"/>
        <xsl:value-of select="$_Total_project_hours"/><xsl:text>&#x20;</xsl:text>
        <xsl:value-of select="format-number($totalHours, $decimalN1, 'numberFormat')"/><xsl:text>&#x20;</xsl:text>
        <xsl:value-of select="$_hours"/>
      </Paragraph>
        
        <Paragraph Style="{{StaticResource GroupParagraph}}">
          4. <xsl:value-of select="$_PRICE"/>
      </Paragraph>
        <xsl:call-template name="proposalPrice"/>

        <Paragraph Style="{{StaticResource GroupParagraph}}">
          4.1 <xsl:value-of select="$_PRICE_Hour_Value"/>
        </Paragraph>
        <xsl:call-template name="proposalHourCosts"/>

        <Paragraph Style="{{StaticResource GroupParagraph}}">
          5. <xsl:value-of select="$_DEADLINE"/>
        </Paragraph>
        <!--<xsl:call-template name="proposalSchedule"/>-->
        <xsl:call-template name="timeLine"/>

        <Paragraph Style="{{StaticResource GroupParagraph}}">
          6. <xsl:value-of select="$_STAKEHOLDERS"/>
        </Paragraph>        
         <xsl:call-template name="projectTeam"/>

        <xsl:if test="count(//ArrayOfProjectConstraint/ProjectConstraint) &gt; 0">
          <Paragraph Style="{{StaticResource GroupParagraph}}">
            7. <xsl:value-of select="$_CONSTRAINTS"/>
          </Paragraph>
          <xsl:call-template name="projectConstraints"/>  
        </xsl:if>
        
        <xsl:variable name="clauseIdx">
          <xsl:choose>
            <xsl:when test="count(//ArrayOfProjectConstraint/ProjectConstraint) &gt; 0">
              <xsl:value-of select="'8'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'7'"/>
            </xsl:otherwise>
          </xsl:choose>  
        </xsl:variable>
        
        <xsl:if test="count(//Proposal/Clauses/ProposalClause) &gt; 0">
          <Paragraph Style="{{StaticResource GroupParagraph}}">
            <xsl:value-of select="$clauseIdx"/>. <xsl:value-of select="$_CLAUSES"/>
          </Paragraph>
          <xsl:call-template name="proposalClauses"/>
        </xsl:if>
        



      </FlowDocument>
    </xsl:template>
  


</xsl:stylesheet>
