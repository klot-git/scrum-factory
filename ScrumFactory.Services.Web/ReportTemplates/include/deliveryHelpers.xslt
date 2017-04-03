<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                xmlns:exslt="http://exslt.org/common"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
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
  
</xsl:stylesheet>
