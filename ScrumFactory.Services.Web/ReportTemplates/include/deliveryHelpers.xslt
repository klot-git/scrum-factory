<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                xmlns:exslt="http://exslt.org/common"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/xps/2005/06/resourcedictionary-key">
    <xsl:output method="xml" indent="yes"/>

  <xsl:include href="helpers.xslt"/>


  <xsl:template name="_sprintItems">
    <xsl:param name="sprintItems"/>
    <xsl:param name="usePreviousPlan"/>
    <xsl:param name="hideDeliveryDate" select="0"/>
    <xsl:param name="endDate"/>
    <xsl:param name="startDate"/>
    <xsl:param name="sprintNumber"/>
    <xsl:for-each select="$sprintItems/BacklogItem">
      <xsl:sort select="OccurrenceConstraint" data-type="number"/>      
      <xsl:sort select="BusinessPriority" data-type="number"/>
      <xsl:variable name="itemStyle">
        <xsl:choose>
          <xsl:when test="IssueType = 3 and $hideDeliveryDate = 0" >{StaticResource criticalItemCell}</xsl:when>
          <!--<xsl:when test="(OccurrenceConstraint = 2 or string-length(DeliveryDate) &gt; 0) and $hideDeliveryDate = 0" >{StaticResource deliveryItemCell}</xsl:when>
          <xsl:when test="position() = 1 and $hideDeliveryDate = 0" >{StaticResource deliveryItemCell}</xsl:when>-->
          <xsl:otherwise>{StaticResource normalItemCell}</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="bgColor" select="Group/GroupColor"/>
      
      <TableRow>
        <xsl:if test="$hideDeliveryDate = 0">
          <TableCell Style="{$itemStyle}" TextAlignment="Center">

              <xsl:if test="IssueType != 3 and string-length(DeliveryDate) &gt; 0">
                <Paragraph FontWeight="Bold">
                  <xsl:call-template name="formatShortDate">
                    <xsl:with-param name="dateTime" select="DeliveryDate" />
                  </xsl:call-template>
                </Paragraph>                
              </xsl:if>

            <xsl:if test="IssueType = 3 and string-length(DeliveryDate) &gt; 0">
              <Paragraph FontWeight="Bold"  Foreground="White">
                <xsl:call-template name="formatShortDate">
                  <xsl:with-param name="dateTime" select="DeliveryDate" />
                </xsl:call-template>
              </Paragraph>
            </xsl:if>

              <xsl:if test="IssueType = 3 and string-length(DeliveryDate) = 0">
                <Paragraph FontWeight="Bold" Foreground="White">
                  <xsl:call-template name="formatShortDate">
                    <xsl:with-param name="dateTime" select="$endDate" />
                  </xsl:call-template>
                </Paragraph>
              </xsl:if>
              

          </TableCell>
        </xsl:if>        
        
        <TableCell Style="{{StaticResource normalItemCell}}" BorderThickness="0,0,0,1" BorderBrush="#EEEEEE">
          <Paragraph TextAlignment="Left" Margin="10,5,0,5">
            <xsl:if test="$hideDeliveryDate = 0">
              <TextBlock Style="{{StaticResource BacklogItemGroupTextBlock}}" Background="{$bgColor}" TextWrapping="Wrap" FontSize="10">
                <xsl:value-of select="Group/GroupName"/>
              </TextBlock>
              <LineBreak/>
            </xsl:if>
            <TextBlock FontWeight="Bold" TextWrapping="Wrap">
              <xsl:value-of select="Name"/>
              #<xsl:value-of select="BacklogItemNumber"/>              
            </TextBlock>            
            
            <xsl:if test="Description and $hideDeliveryDate = 0">
            <LineBreak/>
            <TextBlock FontSize="10" TextWrapping="Wrap" Margin="0,5,0,0">
              <xsl:call-template name="breakLines">
                <xsl:with-param name="text" select="Description" />
              </xsl:call-template>              
            </TextBlock>  
            </xsl:if>
            

          </Paragraph>
        </TableCell>
        <TableCell Style="{{StaticResource normalItemCell}}" TextAlignment="Right" BorderThickness="0,0,0,1" BorderBrush="#EEEEEE">
          <Paragraph Margin="0,5,0,5">
            <xsl:variable name="totalHours" select="sum(ValidPlannedHours/PlannedHour[SprintNumber =  $sprintNumber]/Hours)"/>
            
            <xsl:value-of select="format-number($totalHours, $decimalN1, 'numberFormat')"/> hrs
          </Paragraph>
        </TableCell>
        <TableCell Style="{{StaticResource normalItemCell}}" TextAlignment="Center" BorderThickness="0,0,0,1" BorderBrush="#EEEEEE">
          <Paragraph FontSize="10"  Margin="0,5,0,5" >
                <xsl:call-template name="itemStatus" />  
          </Paragraph>
        </TableCell>
    
      </TableRow>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="sprintDeliveries">    
    <xsl:param name="sprintNumber"/>
    <xsl:param name="usePreviousPlan"/>
    <xsl:param name="hideDeliveryDate" select="0"/>

    <xsl:variable name="sprint" select="/ReportData/Project/Sprints/Sprint[SprintNumber = $sprintNumber]" />
    
    <Table>
      <Table.Columns>
        <xsl:if test="$hideDeliveryDate = 0">
          <TableColumn Width="80" />
        </xsl:if>        
        <TableColumn />
        <TableColumn Width="60" />
        <TableColumn Width="120" />        
      </Table.Columns>
      <TableRowGroup>
        <xsl:if test="$hideDeliveryDate = 0">
          <TableRow>
            <TableCell Background="Black" TextAlignment="Center">
              <Paragraph FontWeight="Bold"  Foreground="White">
                <!--<xsl:value-of select="$_starts"/>&#160;-->
                <xsl:call-template name="formatShortDate">
                  <xsl:with-param name="dateTime" select="$sprint/StartDate" />
                </xsl:call-template>
              </Paragraph>
            </TableCell>
          </TableRow>
        </xsl:if>
        
        <xsl:variable name="sprintItems">
          <xsl:copy-of select="/ReportData/ArrayOfBacklogItem/BacklogItem[ValidPlannedHours/PlannedHour/SprintNumber = $sprintNumber and Status !=3]"/>          
        </xsl:variable>

        <xsl:call-template name="_sprintItems">
          <xsl:with-param name="sprintItems" select="exslt:node-set($sprintItems)"/>
          <xsl:with-param name="usePreviousPlan" select="$usePreviousPlan"/>
          <xsl:with-param name="hideDeliveryDate" select="$hideDeliveryDate"/>
          <xsl:with-param name="endDate" select="/ReportData/Project/Sprints/Sprint[SprintNumber = $sprintNumber]/EndDate"/>
          <xsl:with-param name="startDate" select="/ReportData/Project/Sprints/Sprint[SprintNumber = $sprintNumber]/StartDate"/>
          <xsl:with-param name="sprintNumber" select="$sprintNumber"/>
        </xsl:call-template>

        <xsl:if test="$hideDeliveryDate = 0">
          <TableRow>
            <TableCell Background="Black" TextAlignment="Center">
              <Paragraph FontWeight="Bold" Foreground="White">
                <!--<xsl:value-of select="$_ends"/>&#160;-->
                <xsl:call-template name="formatShortDate">
                  <xsl:with-param name="dateTime" select="$sprint/EndDate" />
                </xsl:call-template>
              </Paragraph>
            </TableCell>
          </TableRow>
        </xsl:if>
     
      </TableRowGroup>
    </Table>
  </xsl:template>


  <xsl:template name="sprintItemsBlock">
    <xsl:param name="sprintNumber"/>        

        <xsl:for-each select="/ReportData/ArrayOfBacklogItem/BacklogItem[SprintNumber = $sprintNumber]">
          <xsl:sort select="OccurrenceConstraint" data-type="number"/>          
          <xsl:sort select="BusinessPriority" data-type="number"/>

          <xsl:variable name="totalHours" select="sum(CurrentPlannedHours/PlannedHour/Hours)" />
          <xsl:variable name="boxHeight">
            <xsl:choose>
              <xsl:when test="$totalHours &lt; 20">
                <xsl:value-of select="20"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$totalHours"/>                
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <xsl:variable name="bgColor" select="Group/GroupColor"/>

                  <Border BorderThickness="2" BorderBrush="Gray" Margin="0,0,4,4" CornerRadius="3" Height="{$boxHeight}" Padding="3" Background="{$bgColor}">
                    <Grid>
                      <TextBlock Style="{{StaticResource BacklogItemGroupTextBlock}}" Background="{$bgColor}" TextWrapping="Wrap" FontSize="10">
                        <xsl:value-of select="Name"/>
                        [<xsl:value-of select="BacklogItemNumber"/>]
                      </TextBlock>
                      <TextBlock HorizontalAlignment="Right" VerticalAlignment="Bottom" Foreground="Gray" Opacity="0.7" FontWeight="Bold" FontSize="10">
                        <xsl:value-of select="$totalHours"/> hrs
                      </TextBlock>
                    </Grid>
                  </Border>
                
        </xsl:for-each>

  </xsl:template>


  <xsl:template name="groupLegend">
    <xsl:param name="devOnly" select="0"/>
    <BlockUIContainer>
      <WrapPanel Orientation="Horizontal"  HorizontalAlignment="Left">
        <xsl:for-each select="/ReportData/ArrayOfBacklogItemGroup/BacklogItemGroup">
          <xsl:sort select="DefaultGroup" data-type="number"/>
          <xsl:variable name="bgColor" select="GroupColor"/>
          <xsl:if test="not($devOnly) or DefaultGroup = 1">
            <StackPanel Orientation="Horizontal">
              <Border Width="10" Height="10" BorderBrush="Gray" BorderThickness="1" Background="{$bgColor}" Margin="0,3,10,0"/>
              <TextBlock Margin="0,0,20,0">
                <xsl:value-of select="GroupName"/>
              </TextBlock>
            </StackPanel>
          </xsl:if>
        </xsl:for-each>
      </WrapPanel>
    </BlockUIContainer>
  </xsl:template>

  <xsl:template name="bar">
      <xsl:param name="isFirst"/>
      <xsl:param name="isLast"/>
      <xsl:param name="shouldBeDone"/>
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
            <xsl:value-of select="'Gray'"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="color">
        <xsl:choose>          
          <xsl:when test="$status = 2">
            <xsl:value-of select="'DarkGreen'"/>
          </xsl:when>          
          <xsl:when test="$shouldBeDone = 2">
            <xsl:value-of select="'Orange'"/>
          </xsl:when>          
          <xsl:otherwise>
            <xsl:value-of select="'Gray'"/>
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
      <xsl:variable name="margin">        
        <xsl:choose>
          <xsl:when test="$constraint = 1 and $isFirst and $isLast">
            <xsl:value-of select="'15,0,15,0'"/>
          </xsl:when>
          <xsl:when test="$constraint = 1 and $isFirst and not($isLast)">
            <xsl:value-of select="'15,0,0,0'"/>
          </xsl:when>
          <xsl:when test="$constraint = 1 and not($isFirst) and $isLast">
            <xsl:value-of select="'0,0,15,0'"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="'0,0,0,0'"/>              
          </xsl:otherwise>
        </xsl:choose>        
      </xsl:variable>  
      <xsl:if test="$hours">
        <BlockUIContainer>
          <Border HorizontalAlignment="{$align}" Margin="{$margin}" VerticalAlignment="Center" Height="12" BorderBrush="{$borderColor}" Background="{$color}" BorderThickness="0" CornerRadius="{$radius}">            
              <TextBlock Foreground="{$textColor}" FontSize="7" HorizontalAlignment="Right" VerticalAlignment="Center" Margin="4,2,4,0" FontWeight="Bold">
                <xsl:value-of select="$hours"/> hrs
              </TextBlock>            
          </Border>
        </BlockUIContainer>
      </xsl:if>
    </xsl:template>

  <xsl:template name="timeLine">
    <xsl:param name="landscape" select="0"/>
    <xsl:param name="autoFit" select="0"/>
    
    <Paragraph>
      <xsl:value-of select="$_The_project_will_be_divided_in"/>
      <Bold><xsl:value-of select="count(Project/Sprints/Sprint)"/></Bold>      
      <xsl:value-of select="$_sprints"/>
            
      <xsl:value-of select="$_The_project_will_start_at"/>
      <Bold><xsl:call-template name="formatDate"><xsl:with-param name="dateTime" select="Project/Sprints/Sprint[1]/StartDate" /></xsl:call-template></Bold>
            
      <xsl:value-of select="$_The_project_should_be_done_by"/>
      <Bold><xsl:call-template name="formatDate"><xsl:with-param name="dateTime" select="Project/Sprints/Sprint[last()]/EndDate" /></xsl:call-template></Bold>.
      <LineBreak/>
    
    </Paragraph>
    
    <Table BorderThickness="0" BorderBrush="#000000" FontSize="8">
      <Table.Resources>
        <Style TargetType="{{x:Type Paragraph}}">
          <Setter Property="FontSize" Value="8"/>
          <Setter Property="Foreground" Value="Black"/>
          <Setter Property="FontFamily" Value="Calibri"/>
          <Setter Property="Margin" Value="0"/>
          <Setter Property="LineHeight" Value="12" />
        </Style>
        <Style TargetType="{{x:Type TableCell}}">
          <Setter Property="Padding" Value="3"/>
          <Setter Property="BorderThickness" Value="0,0,0,1"/>
          <Setter Property="BorderBrush" Value="Gray"/>
          <Setter Property="LineHeight" Value="12" />
        </Style>
        <Style x:Key="headerCell" TargetType="{{x:Type TableCell}}">
          <Setter Property="Padding" Value="0,3,0,3"/>
          <Setter Property="BorderThickness" Value="0,0,0,3"/>
          <Setter Property="BorderBrush" Value="Black"/>
          <Setter Property="LineHeight" Value="12" />
        </Style>
        <Style x:Key="barCell" TargetType="{{x:Type TableCell}}">
          <Setter Property="Padding" Value="0,3,0,3"/>
          <Setter Property="BorderThickness" Value="0,0,0,1"/>
          <Setter Property="BorderBrush" Value="Gray"/>
          <Setter Property="LineHeight" Value="12" />
        </Style>
        <Style x:Key="sprintBarCell" TargetType="{{x:Type TableCell}}">
          <Setter Property="Padding" Value="0,3,0,3"/>
          <Setter Property="BorderThickness" Value="0,0,0,3"/>
          <Setter Property="BorderBrush" Value="Black"/>
          <Setter Property="LineHeight" Value="12" />
        </Style>
        <Style x:Key="sprintHeaderCell" TargetType="{{x:Type TableCell}}">
          <Setter Property="Padding" Value="6"/>
          <Setter Property="BorderThickness" Value="0"/>
          <Setter Property="BorderBrush" Value="Black"/>
          <Setter Property="LineHeight" Value="12" />
        </Style>

      </Table.Resources>
      
       <xsl:variable name="sprintCellWidth">
        <xsl:choose>
          <xsl:when test="not($landscape) and count(Project/Sprints/Sprint) &gt; 7">
            <xsl:value-of select="'1.5cm'"/>
          </xsl:when>          
          <xsl:when test="not($landscape) and count(Project/Sprints/Sprint) = 7">
            <xsl:value-of select="'1.8cm'"/>
          </xsl:when>
          <xsl:when test="not($landscape) and count(Project/Sprints/Sprint) &gt;= 5">
            <xsl:value-of select="'2cm'"/>
          </xsl:when>
          <xsl:when test="not($landscape)">
            <xsl:value-of select="'2.5cm'"/>
          </xsl:when>
          <xsl:when test="$landscape and count(Project/Sprints/Sprint) &gt; 7">
            <xsl:value-of select="'2.4cm'"/>
          </xsl:when>          
          <xsl:when test="$landscape and count(Project/Sprints/Sprint) = 7">
            <xsl:value-of select="'2.8cm'"/>
          </xsl:when>
          <xsl:when test="$landscape and count(Project/Sprints/Sprint) &gt;= 5">
            <xsl:value-of select="'3cm'"/>
          </xsl:when>          
          <xsl:otherwise>
            <xsl:value-of select="'3cm'"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      
      <xsl:variable name="startSprint"  >
          <xsl:choose>
            <xsl:when test="$autoFit and not($landscape) and count(Project/Sprints/Sprint) &gt;= 8">
              <xsl:value-of select="/ReportData/ProjectCurrentSprintNumber"/>
            </xsl:when>          
            <xsl:when test="$autoFit and $landscape and count(Project/Sprints/Sprint) &gt;= 10">
              <xsl:value-of select="/ReportData/ProjectCurrentSprintNumber"/>
            </xsl:when>          
            <xsl:otherwise>
              <xsl:value-of select="1" />
            </xsl:otherwise>
          </xsl:choose>
      </xsl:variable>
        

      <Table.Columns>
        <TableColumn Width="1cm" />
        <TableColumn Width="100*" />
        <xsl:for-each select="Project/Sprints/Sprint[SprintNumber &gt;= $startSprint]">
          <TableColumn Width="{$sprintCellWidth}" />
        </xsl:for-each>
      </Table.Columns>
      

      <TableRowGroup>


     

        
        <TableRow>
          <TableCell Style="{{StaticResource headerCell}}"></TableCell>
          <TableCell Style="{{StaticResource headerCell}}" Padding="0,0,0,0" >
            <BlockUIContainer>              
                <Border Background="Black" BorderThickness="0" CornerRadius="4,4,0,0" Padding="4" HorizontalAlignment="Right">
                  <TextBlock  FontWeight="Bold" FontSize="8" VerticalAlignment="Center" Foreground="White">
                    <xsl:call-template name="formatShortDate">
                      <xsl:with-param name="dateTime" select="Project/Sprints/Sprint[SprintNumber = 1]/StartDate"/>
                    </xsl:call-template>
                  </TextBlock>
                </Border>                            
            </BlockUIContainer>
          </TableCell>
          <xsl:for-each select="Project/Sprints/Sprint[SprintNumber &gt;= $startSprint]">
            <xsl:sort select="SprintNumber" data-type="number"/>
            <xsl:variable name="sprintNumber" select="SprintNumber"/>

            <xsl:variable name="cellBG">
                <xsl:choose>
                  <xsl:when test="$sprintNumber = /ReportData/ProjectCurrentSprintNumber">
                    <xsl:value-of select="'#FFF9C6'"/>
                  </xsl:when>
                  <xsl:when test="$sprintNumber mod 2 = 0">
                    <xsl:value-of select="'#FAFAFA'"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="'White'"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:variable>

            <TableCell Style="{{StaticResource headerCell}}" Background="{$cellBG}" Padding="0,0,0,0">
              <BlockUIContainer>
                <Grid>
                  <TextBlock Text="{$sprintNumber}" HorizontalAlignment="Left" Margin="6,0,0,0" />
                  <Border Background="Black" BorderThickness="0"  CornerRadius="4,4,0,0"  HorizontalAlignment="Right" Padding="4">
                    <TextBlock  FontWeight="Bold" FontSize="8" VerticalAlignment="Center" Foreground="White">
                      <xsl:call-template name="formatShortDate">
                        <xsl:with-param name="dateTime" select="EndDate"/>
                      </xsl:call-template>
                    </TextBlock>
                  </Border>
                </Grid>
              </BlockUIContainer>
            </TableCell>
          </xsl:for-each>
        </TableRow>


          
        <xsl:for-each select="/ReportData/ArrayOfBacklogItem/BacklogItem[Status !=3 and string-length(SprintNumber) &gt; 0 and SprintNumber &gt;= $startSprint]">
          <xsl:sort select="OrderSprintWorked" data-type="number"/>
          <xsl:sort select="OccurrenceConstraint" data-type="number"/>
          <xsl:sort select="Status" data-type="number"  order="descending"/>
          <xsl:variable name="itemUId" select="BacklogItemUId"/>
          <xsl:variable name="status" select="Status"/>
          <xsl:variable name="itemSprintNumber" select="SprintNumber"/>

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
          
          <xsl:variable name="fontWeight">
            <xsl:choose>
              <xsl:when test="IssueType = 3">
                <xsl:value-of select="'Bold'"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'Normal'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <xsl:variable name="cellStyle">
            <xsl:choose>
              <xsl:when test="OccurrenceConstraint = 2">
                <xsl:value-of select="'{StaticResource sprintBarCell}'"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'{StaticResource barCell}'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <xsl:if test="OccurrenceConstraint = 0">
            <TableRow>
              <TableCell Style="{{StaticResource sprintHeaderCell}}">
                <Paragraph>
                  <TextBlock FontWeight="Bold" TextWrapping="NoWrap" FontSize="8">                    
                      <xsl:call-template name="formatShortDate">
                        <xsl:with-param name="dateTime" select="//ReportData/Project/Sprints/Sprint[SprintNumber=$itemSprintNumber]/StartDate"/>
                      </xsl:call-template>                    
                  </TextBlock>
                </Paragraph>
              </TableCell>
              
              <TableCell Style="{{StaticResource sprintHeaderCell}}" ColumnSpan="{count(//ReportData/Project/Sprints/Sprint) + 1}">
                <Paragraph>
                  <Bold>
                    Sprint <xsl:value-of select="$itemSprintNumber"/>
                  </Bold>
                </Paragraph>
              </TableCell>
            </TableRow>
          </xsl:if>


          <TableRow>

            <TableCell Background="{$groupColor}" Style="{$cellStyle}" Padding="6,3,6,3">
              
                <Paragraph Foreground="White">
                  <TextBlock FontWeight="Bold" TextWrapping="NoWrap" FontSize="8" Foreground="White">
                    <xsl:if test="OccurrenceConstraint = 2">
                      <xsl:call-template name="formatShortDate">
                        <xsl:with-param name="dateTime" select="//ReportData/Project/Sprints/Sprint[SprintNumber=$itemSprintNumber]/EndDate"/>
                      </xsl:call-template>
                    </xsl:if>  
                    <xsl:if test="string-length(DeliveryDate) &gt; 0">                  
                      <xsl:call-template name="formatShortDate">
                        <xsl:with-param name="dateTime" select="DeliveryDate"/>
                      </xsl:call-template>                  
                    </xsl:if>
                  </TextBlock>
                </Paragraph>              
              
              
            </TableCell>

            <TableCell Style="{$cellStyle}" Padding="3">
              <Paragraph TextAlignment="Left" Foreground="{$color}" FontWeight="{$fontWeight}">
                <xsl:value-of select="Name"/>&#160;[<xsl:value-of select="BacklogItemNumber"/>]                
              </Paragraph>
            </TableCell>

            <xsl:for-each select="/ReportData/Project/Sprints/Sprint[SprintNumber &gt;= $startSprint]">
              <xsl:sort select="SprintNumber" data-type="number"/>
              <xsl:variable name="sprintNumber" select="SprintNumber"/>

              <xsl:variable name="hours" select="sum(/ReportData/ArrayOfBacklogItem/BacklogItem[BacklogItemUId = $itemUId]/ValidPlannedHours/PlannedHour[SprintNumber = $sprintNumber]/Hours)"/>

              <xsl:variable name="cellBG">
                <xsl:choose>
                  <xsl:when test="$sprintNumber = /ReportData/ProjectCurrentSprintNumber">
                    <xsl:value-of select="'#FFF9C6'"/>
                  </xsl:when>
                  <xsl:when test="$sprintNumber mod 2 = 0">
                    <xsl:value-of select="'#FAFAFA'"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="'White'"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:variable>

              <TableCell Background="{$cellBG}" Style="{$cellStyle}">

                <xsl:call-template name="bar">
                  <xsl:with-param name="isFirst" select="$sprintNumber = /ReportData/ArrayOfBacklogItem/BacklogItem[BacklogItemUId = $itemUId]/FirstSprintWorked" />
                  <xsl:with-param name="isLast" select="$sprintNumber = /ReportData/ArrayOfBacklogItem/BacklogItem[BacklogItemUId = $itemUId]/LastSprintWorked" />
                  <xsl:with-param name="shouldBeDone" select="$sprintNumber &lt; /ReportData/ArrayOfBacklogItem/BacklogItem[BacklogItemUId = $itemUId]/LastSprintWorked" />
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
  </xsl:template>


</xsl:stylesheet>
