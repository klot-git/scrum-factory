<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet
    version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"    
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/xps/2005/06/resourcedictionary-key">
  
    <xsl:output method="xml" indent="yes"/>
    <xsl:include href="../include/locale.xslt"/>
    <xsl:include href="../include/styles.xslt"/>
    <xsl:include href="../include/deliveryHelpers.xslt"/>

    
  
    <xsl:template match="/ReportData">
      <FlowDocument                
        PageWidth="29.7cm" 
        PageHeight="21cm"
        PagePadding="60,40,40,40"              
        ColumnWidth="29.7 cm">
        
        <xsl:call-template name="styles"/>


        <xsl:call-template name="reportHeader">
          <xsl:with-param name="title" select="$_PROJECT_SCHEDULE"/>
        </xsl:call-template>

        
        

        <xsl:call-template name="timeLine">
          <xsl:with-param name="landscape" select="1" />
          <xsl:with-param name="autoFit" select="1" />
        </xsl:call-template>
   

        <Paragraph Style="{{StaticResource GroupParagraph}}">
          <xsl:value-of select="$_STRUCTURES"/>
        </Paragraph>

        <xsl:call-template name="groupLegend">
          <!--<xsl:with-param name="devOnly" select="1"/>-->
        </xsl:call-template>
        
        
      </FlowDocument>
    </xsl:template>
  


</xsl:stylesheet>
