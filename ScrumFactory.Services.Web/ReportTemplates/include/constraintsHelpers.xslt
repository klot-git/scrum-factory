<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
    <xsl:output method="xml" indent="yes"/>


  <xsl:template name="projectConstraints">
    <xsl:param name="index" select="'8'"/>
    <xsl:for-each select="//ArrayOfProjectConstraint/ProjectConstraint">
    <xsl:sort select="ConstraintId" />
      <Paragraph Margin="0,0,0,10">
        <TextBlock TextWrapping="Wrap">
          <Run FontWeight="Bold">
            <xsl:value-of select="ConstraintId"/>.
          </Run>
          <xsl:text>&#x20;</xsl:text>
          <xsl:call-template name="breakLines">
            <xsl:with-param name="text" select="Constraint" />
          </xsl:call-template>
          
        </TextBlock>        
      </Paragraph>
    </xsl:for-each>      
  </xsl:template>

  
</xsl:stylesheet>
