﻿<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet
    version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"    
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/xps/2005/06/resourcedictionary-key"
    xmlns:SF="clr-namespace:ScrumFactory.Projects;assembly=ScrumFactory.Projects">
  
    <xsl:output method="xml" indent="yes"/>
    <xsl:include href="../include/locale.xslt"/>
    <xsl:include href="../include/styles.xslt"/>
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
          <xsl:with-param name="title" select="concat('SPRINT ', /ReportData/ProjectPreviousSprintNumber, ' - ', $_REVIEW)"/>
        </xsl:call-template>

        <Paragraph Style="{{StaticResource GroupParagraph}}">
          <xsl:value-of select="$_PROJECT_BURNDOWN"/>          
        </Paragraph>
        <Paragraph>
          <xsl:value-of select="$_Project_scheduled_to"/>          
          <xsl:call-template name="formatDate">
            <xsl:with-param name="dateTime" select="/ReportData/ProjectEndDate" />
          </xsl:call-template>
        </Paragraph>

        <BlockUIContainer Margin="0,20,0,20">
          <SF:Burndown x:Name="burndown" Height="250" IsReadOnly="True" HorizontalAlignment="Stretch"/>
        </BlockUIContainer>


        <Paragraph Style="{{StaticResource GroupParagraph}}">
          <xsl:value-of select="$_RISKS"/>          
        </Paragraph>
        
        <Table>
          <Table.Columns>
            <TableColumn Width="60" />
            <TableColumn Width="60" />
          </Table.Columns>
          <TableRowGroup>
            <TableRow>
              <TableCell Style="{{StaticResource headerItemCell}}">
                <Paragraph FontWeight="Bold" TextAlignment="Center">
                  <xsl:value-of select="$_Prob"/>
                </Paragraph>
              </TableCell>
              <TableCell Style="{{StaticResource headerItemCell}}">
                <Paragraph FontWeight="Bold" TextAlignment="Center">
                  <xsl:value-of select="$_Impact"/>
                </Paragraph>
              </TableCell>
              <TableCell Style="{{StaticResource headerItemCell}}">
                <Paragraph FontWeight="Bold">
                  <xsl:value-of select="$_Risk"/>
                </Paragraph>
              </TableCell>              
            </TableRow>
            <xsl:for-each select="//ArrayOfRisk/Risk[IsPrivate='false' and Probability &gt;= 1]">
              <TableRow>
                <TableCell>
                  <xsl:call-template name="levelImage">
                    <xsl:with-param name="level" select="Probability"/>
                  </xsl:call-template>                  
                </TableCell>
                <TableCell>
                  <xsl:call-template name="levelImage">
                    <xsl:with-param name="level" select="Impact"/>
                  </xsl:call-template>
                </TableCell>
                <TableCell>
                  <Paragraph FontWeight="Bold">
                    <xsl:value-of select="RiskDescription"/>                                        
                  </Paragraph>
                  <Paragraph FontSize="11" FontStyle="Italic"  LineHeight="Auto">
                    <xsl:call-template name="breakLines">
                      <xsl:with-param name="text" select="RiskAction" />
                    </xsl:call-template>
                  </Paragraph>
                </TableCell>                
              </TableRow>
            </xsl:for-each>
            <xsl:if test="count(//ArrayOfRisk/Risk[IsPrivate='false' and Probability &gt;= 1]) = 0">
              <TableRow>
                <TableCell ColumnSpan="3">
                  <Paragraph FontWeight="Bold" Margin="0,10,0,0">
                    <xsl:value-of select="$_No_risks_were_identified"/>          
                  </Paragraph>
                </TableCell>
              </TableRow>
            </xsl:if>
          </TableRowGroup>
        </Table>

        
        <Paragraph Style="{{StaticResource GroupParagraph}}">
          <xsl:value-of select="$_PREVIOUS_ITEMS"/>          
        </Paragraph>
        <Table>
          <TableRowGroup>
            
            <TableRow>
              <TableCell>                
                <xsl:call-template name="sprintDeliveries">
                  <xsl:with-param name="sprintNumber" select="/ReportData/ProjectPreviousSprintNumber"/>
                  <xsl:with-param name="usePreviousPlan" select="1"/>
                  <xsl:with-param name="hideDeliveryDate" select="1"/>
                </xsl:call-template>
              </TableCell>
            </TableRow>            
          </TableRowGroup>
        </Table>

        <Paragraph Style="{{StaticResource GroupParagraph}}">
          <xsl:value-of select="$_NEXT_SPRINT_ITEMS"/> -
	  <xsl:call-template name="formatDate">
            <xsl:with-param name="dateTime" select="/ReportData/Project/Sprints/Sprint[SprintNumber = /ReportData/ProjectCurrentSprintNumber]/EndDate" />
          </xsl:call-template>
        </Paragraph>
        <Table>
          <TableRowGroup>
            <TableRow>
              <TableCell>
                <xsl:call-template name="sprintDeliveries">
                  <xsl:with-param name="sprintNumber" select="/ReportData/ProjectCurrentSprintNumber"/>
                  <xsl:with-param name="hideDeliveryDate" select="1"/>
                </xsl:call-template>
              </TableCell>
            </TableRow>
          </TableRowGroup>
        </Table>

      </FlowDocument>
    </xsl:template>
  


</xsl:stylesheet>
