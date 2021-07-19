<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet
    version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"    
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/xps/2005/06/resourcedictionary-key"
    xmlns:SF="clr-namespace:ScrumFactory.Projects;assembly=ScrumFactory.Projects">
  
    <xsl:output method="xml" indent="yes"/>
    <xsl:include href="../include/locale.xslt"/>
    <xsl:include href="../include/styles.xslt"/>
    <xsl:include href="../include/constraintsHelpers.xslt"/>
    <xsl:include href="../include/deliveryHelpers.xslt"/>

    <xsl:template match="/ReportData">
      <FlowDocument        
        PageWidth="21cm"
        PageHeight="29.7cm"      
        PagePadding="60,40,40,40"      
        LineHeight="25"
        ColumnWidth="21 cm">
        
        <xsl:call-template name="styles"/>


        <xsl:call-template name="reportHeader">
          <xsl:with-param name="title" select="$_GUIDE_SCOPE"/>
        </xsl:call-template>

          <xsl:for-each select="//ArrayOfBacklogItemGroup/BacklogItemGroup[DefaultGroup = 1]">
            <xsl:sort select="DefaultGroup" data-type="number"/>
            <xsl:sort select="GroupName"/>
            <xsl:variable name="groupUId" select="GroupUId"/>

            <xsl:variable name="groupItems" select="//ArrayOfBacklogItem/BacklogItem[GroupUId=$groupUId  and Status !=3]"/>

            <xsl:if test="count($groupItems) &gt; 0">


              <Paragraph Style="{{StaticResource TitleParagraph}}">
                  <xsl:value-of select="GroupName"/>
              </Paragraph>
                <xsl:for-each select="$groupItems">
                  <xsl:variable name="itemUId" select="BacklogItemUId"/>
                  <xsl:variable name="item" select="//ArrayOfBacklogItem/BacklogItem[BacklogItemUId=$itemUId]"/>

                  <Paragraph Style="{{StaticResource GroupParagraph}}">
                    <xsl:value-of select="$item/Name"/>
                    <Run Text=" "/>
                    <Run FontSize="10">
                      (<xsl:value-of select="$item/BacklogItemNumber"/>)
                    </Run>
                  </Paragraph>
                  <xsl:if test="$item/Description">
                    <Paragraph>
                      <xsl:call-template name="breakLines">
                        <xsl:with-param name="text" select="$item/Description" />
                      </xsl:call-template>                     
                    </Paragraph>
                  </xsl:if>

                </xsl:for-each>
                
          
              
            </xsl:if>
          </xsl:for-each>



        <xsl:if test="count(//ArrayOfProjectConstraint/ProjectConstraint) >0">
          <Paragraph Style="{{StaticResource TitleParagraph}}">
            <xsl:value-of select="$_CONSTRAINTS"/>
          </Paragraph>


          <xsl:call-template name="projectConstraints"/>
        </xsl:if>





      </FlowDocument>
    </xsl:template>
  


</xsl:stylesheet>
