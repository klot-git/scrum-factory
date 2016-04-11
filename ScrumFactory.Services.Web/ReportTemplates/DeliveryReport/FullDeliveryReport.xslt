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

    <xsl:template name="bar">
      <xsl:param name="isFirst"/>
      <xsl:param name="isLast"/>
      <xsl:param name="status"/>
      <xsl:param name="hours"/>
      <xsl:param name="constraint"/>
      <xsl:variable name="radius">
        <xsl:choose>
          <xsl:when test="$isFirst and $isLast">
            <xsl:value-of select="'4,4,4,4'"/>
          </xsl:when>
          <xsl:when test="$isFirst">
            <xsl:value-of select="'4,0,0,4'"/>
          </xsl:when>
          <xsl:when test="$isLast">
            <xsl:value-of select="'0,4,4,0'"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="'0,0,0,0'"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="thick">
        <xsl:choose>
          <xsl:when test="$isFirst and $isLast">
            <xsl:value-of select="'1'"/>
          </xsl:when>
          <xsl:when test="$isFirst">
            <xsl:value-of select="'1,1,0,1'"/>
          </xsl:when>
          <xsl:when test="$isLast">
            <xsl:value-of select="'0,1,1,1'"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="'0,1,0,1'"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="borderColor">
        <xsl:choose>
          <xsl:when test="$status = 2">
            <xsl:value-of select="'Green'"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="'Blue'"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="color">
        <xsl:choose>          
          <xsl:when test="$status = 2">
            <xsl:value-of select="'DarkGreen'"/>
          </xsl:when>          
          <xsl:otherwise>
            <xsl:value-of select="'DarkBlue'"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="align">
        <xsl:choose>
          <xsl:when test="$constraint = 0">
            <xsl:value-of select="'Left'"/>
          </xsl:when>
          <xsl:when test="$constraint = 2">
            <xsl:value-of select="'Right'"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="'Stretch'"/>            
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="textColor">
        <xsl:choose>
          <xsl:when test="$isLast and $status != 2">
            <xsl:value-of select="'White'"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$color"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:if test="$hours">
        <BlockUIContainer>
          <Border HorizontalAlignment="{$align}" VerticalAlignment="Center" Height="12" BorderBrush="{$borderColor}" Background="{$color}" BorderThickness="{$thick}" CornerRadius="{$radius}">            
              <TextBlock Foreground="{$textColor}" FontSize="6" HorizontalAlignment="Right" VerticalAlignment="Center" Margin="0,0,4,0">
                <xsl:value-of select="$hours"/>hrs
              </TextBlock>            
          </Border>
        </BlockUIContainer>
      </xsl:if>
    </xsl:template>
  
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

        
        

        <Table BorderThickness="0" BorderBrush="#000000" FontSize="8">
          <Table.Resources>
            <Style TargetType="{{x:Type Paragraph}}">
              <Setter Property="FontSize" Value="8"/>
              <Setter Property="Foreground" Value="Black"/>
              <Setter Property="FontFamily" Value="Calibri"/>
              <Setter Property="Margin" Value="0"/>
            </Style>
            <Style TargetType="{{x:Type TableCell}}">
              <Setter Property="Padding" Value="3"/>
              <Setter Property="BorderThickness" Value="0,0,0,1"/>
              <Setter Property="BorderBrush" Value="Gray"/>
            </Style>
            <Style x:Key="headerCell" TargetType="{{x:Type TableCell}}">
              <Setter Property="Padding" Value="0,3,0,3"/>
              <Setter Property="BorderThickness" Value="0,0,0,2"/>
              <Setter Property="BorderBrush" Value="Black"/>
            </Style>
            <Style x:Key="barCell" TargetType="{{x:Type TableCell}}">
              <Setter Property="Padding" Value="0,3,0,3"/>
              <Setter Property="BorderThickness" Value="0,0,0,1"/>
              <Setter Property="BorderBrush" Value="Gray"/>
            </Style>
            
          </Table.Resources>

          <Table.Columns>
            <TableColumn Width="5" />
            <TableColumn Width="40*" />            
            <xsl:for-each select="Project/Sprints/Sprint">
              <TableColumn Width="40" />
            </xsl:for-each>
          </Table.Columns>

            <TableRowGroup>


            <TableRow>
              <TableCell Style="{{StaticResource headerCell}}"></TableCell>
              <TableCell Style="{{StaticResource headerCell}}" >
                <Paragraph FontWeight="Bold" >
                  <xsl:value-of select="$_Item"/>                  
                </Paragraph>
              </TableCell>                            
              <xsl:for-each select="Project/Sprints/Sprint">
                <xsl:sort select="SprintNumber" data-type="number"/>
                <xsl:variable name="sprintNumber" select="SprintNumber"/>

                <xsl:variable name="cellBG">
                  <xsl:choose>                    
                    <xsl:when test="$sprintNumber = /ReportData/ProjectCurrentSprintNumber">
                      <xsl:value-of select="'#F0F0F0'"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="'White'"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:variable>

                <TableCell Style="{{StaticResource headerCell}}" Background="{$cellBG}">
                  <BlockUIContainer>
                    <Grid>		
                      <TextBlock VerticalAlignment="Center" FontSize="14" FontWeight="Bold" HorizontalAlignment="Center" Foreground="Gray" Padding="0">
                        <xsl:value-of select="SprintNumber"/>
                      </TextBlock>
		      <xsl:if test="$sprintNumber = 1">
                      <TextBlock  FontWeight="Bold" FontSize="8" VerticalAlignment="Center" HorizontalAlignment="Left">
                        <xsl:call-template name="formatShortDate">
                          <xsl:with-param name="dateTime" select="StartDate"/>
                        </xsl:call-template>
                      </TextBlock>
                      </xsl:if>
                      <TextBlock  FontWeight="Bold" FontSize="8" VerticalAlignment="Center"  HorizontalAlignment="Right">
                        <xsl:call-template name="formatShortDate">
                          <xsl:with-param name="dateTime" select="EndDate"/>
                        </xsl:call-template>
                      </TextBlock>
                    </Grid>
                  </BlockUIContainer>
                </TableCell>                
              </xsl:for-each>
            </TableRow>
            
            <xsl:for-each select="/ReportData/ArrayOfBacklogItem/BacklogItem[Status !=3]">              
              <xsl:sort select="OrderSprintWorked" data-type="number"/>
              <xsl:sort select="OccurrenceConstraint" data-type="number"/>
              <xsl:sort select="Status" data-type="number"  order="descending"/>
              <xsl:variable name="itemUId" select="BacklogItemUId"/>
              <xsl:variable name="status" select="Status"/>

              <xsl:variable name="groupColor" select="Group/GroupColor"/>

                <xsl:variable name="color">
                    <xsl:choose>
                        <xsl:when test="IssueType = 3">
                            <xsl:value-of select="'Red'"/>
                        </xsl:when>
                        <xsl:otherwise>
                            <xsl:value-of select="'Black'"/>
                        </xsl:otherwise>
                    </xsl:choose>
                </xsl:variable>
              

              <TableRow>

                <TableCell Background="{$groupColor}"></TableCell>
                
                <TableCell>
                  <Paragraph TextAlignment="Left" Foreground="{$color}">
                    <xsl:value-of select="Name"/>[<xsl:value-of select="BacklogItemNumber"/>]
                    <xsl:if test="string-length(DeliveryDate) &gt; 0">
                        <TextBlock Background="{$color}" Foreground="White" FontSize="8" Margin="5,0,0,0" Padding="2,0,2,0">
                            <xsl:call-template name="formatShortDate">
                                <xsl:with-param name="dateTime" select="DeliveryDate"/>
                            </xsl:call-template>
                        </TextBlock>
                    </xsl:if>
                  </Paragraph>
                </TableCell>                
                
                <xsl:for-each select="/ReportData/Project/Sprints/Sprint">
                <xsl:sort select="SprintNumber" data-type="number"/>
                  <xsl:variable name="sprintNumber" select="SprintNumber"/>

                  <xsl:variable name="hours" select="sum(/ReportData/ArrayOfBacklogItem/BacklogItem[BacklogItemUId = $itemUId]/ValidPlannedHours/PlannedHour[SprintNumber = $sprintNumber]/Hours)"/>

                  <xsl:variable name="cellBG">
                    <xsl:choose>                    
                      <xsl:when test="$sprintNumber = /ReportData/ProjectCurrentSprintNumber">
                        <xsl:value-of select="'#F0F0F0'"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="'White'"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:variable>
                  
                  <TableCell Background="{$cellBG}" Style="{{StaticResource barCell}}">
                   
                    <xsl:call-template name="bar">
                      <xsl:with-param name="isFirst" select="$sprintNumber = /ReportData/ArrayOfBacklogItem/BacklogItem[BacklogItemUId = $itemUId]/FirstSprintWorked" />
                      <xsl:with-param name="isLast" select="$sprintNumber = /ReportData/ArrayOfBacklogItem/BacklogItem[BacklogItemUId = $itemUId]/LastSprintWorked" />
                      <xsl:with-param name="status" select="$status" />
                      <xsl:with-param name="hours" select="$hours" />
                      <xsl:with-param name="constraint" select="/ReportData/ArrayOfBacklogItem/BacklogItem[BacklogItemUId = $itemUId]/OccurrenceConstraint" />
                    </xsl:call-template>                    
                  </TableCell>
                </xsl:for-each>

                

              </TableRow>
            </xsl:for-each>
          </TableRowGroup>
        </Table>

        <Paragraph Style="{{StaticResource GroupParagraph}}">
          <xsl:value-of select="$_STRUCTURES"/>
        </Paragraph>

        <xsl:call-template name="groupLegend">
          <!--<xsl:with-param name="devOnly" select="1"/>-->
        </xsl:call-template>
        
        
      </FlowDocument>
    </xsl:template>
  


</xsl:stylesheet>
