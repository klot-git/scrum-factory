<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet
    version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"    
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/xps/2005/06/resourcedictionary-key"
    xmlns:SF="clr-namespace:ScrumFactory.Backlog;assembly=ScrumFactory.Backlog">
  
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

          <!--<BlockUIContainer Margin="0,0,0,0">
              <Viewbox StretchDirection="DownOnly" Stretch="Uniform" HorizontalAlignment="Left" VerticalAlignment="Top">
                  <SF:SprintGrid x:Name="sprintGrid" IsEnabled="false" />
              </Viewbox>
          </BlockUIContainer>-->

        
        <BlockUIContainer>
          <Viewbox StretchDirection="DownOnly" Stretch="Uniform" HorizontalAlignment="Left" VerticalAlignment="Top" MaxHeight="480">
            <StackPanel Orientation="Horizontal">
              <xsl:for-each select="Project/Sprints/Sprint[SprintNumber >= /ReportData/ProjectCurrentSprintNumber]">
                <xsl:sort select="SprintNumber" data-type="number"/>
                <xsl:variable name="sprintNumber" select="SprintNumber"/>
                <xsl:variable name="totalSprintHours" select="sum(/ReportData/ArrayOfBacklogItem/BacklogItem[SprintNumber = $sprintNumber]/CurrentPlannedHours/PlannedHour/Hours)" />

                <StackPanel MaxWidth="150">
                  <Grid>
                    <TextBlock VerticalAlignment="Center" FontSize="40" FontWeight="Bold" TextAlignment="Center" Foreground="Gray" Opacity="0.5" Padding="0">
                      <xsl:value-of select="SprintNumber"/>
                    </TextBlock>
                    <StackPanel Margin="0,0,4,0">
                      <TextBlock FontSize="16" FontWeight="Bold" TextAlignment="Right">
                        <xsl:value-of select="$totalSprintHours"/> hrs
                      </TextBlock>
                      <DockPanel>
                        <TextBlock DockPanel.Dock="Left">
                          <xsl:call-template name="formatShortDate">
                            <xsl:with-param name="dateTime" select="StartDate"/>
                          </xsl:call-template>
                        </TextBlock>
                        <TextBlock DockPanel.Dock="Right" HorizontalAlignment="Right">
                          <xsl:call-template name="formatShortDate">
                            <xsl:with-param name="dateTime" select="EndDate"/>
                          </xsl:call-template>
                        </TextBlock>
                      </DockPanel>
                    </StackPanel>
                  </Grid>


                  <xsl:call-template name="sprintItemsBlock">
                    <xsl:with-param name="sprintNumber" select="SprintNumber"/>
                  </xsl:call-template>
                </StackPanel>

              </xsl:for-each>
            </StackPanel>
          </Viewbox>
        </BlockUIContainer>


        <Paragraph Style="{{StaticResource GroupParagraph}}">
            <xsl:value-of select="$_STRUCTURES"/>
        </Paragraph>


        <xsl:call-template name="groupLegend"/>          
          
        

        </FlowDocument>
    </xsl:template>
  


</xsl:stylesheet>
