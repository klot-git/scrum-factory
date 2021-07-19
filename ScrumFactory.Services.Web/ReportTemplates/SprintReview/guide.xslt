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
    <xsl:include href="../include/deliveryHelpers.xslt"/>

  
    
  

    <xsl:template match="/ReportData">
      <FlowDocument        
        PageWidth="21cm"
        PageHeight="29.7cm"      
        PagePadding="60,40,40,40"      
        LineHeight="25"
        ColumnWidth="21cm">
        
        <xsl:call-template name="styles"/>


        <xsl:call-template name="reportHeader">
          <xsl:with-param name="title" select="$_PROJECT_GUIDE"/>
        </xsl:call-template>

        <Paragraph Style="{{StaticResource GroupParagraph}}">
          <xsl:value-of select="$_THIS_GUIDE"/>
        </Paragraph>
        <Paragraph>
          <xsl:value-of select="$_THIS_GUIDE_text"/>
        </Paragraph>

        <Paragraph Style="{{StaticResource GroupParagraph}}">
          <xsl:value-of select="$_PROJECT_LIFE_CYCLE"/>
        </Paragraph>
        <Paragraph>
          <xsl:value-of select="$_PROJECT_LIFE_CYCLE_text"/>
          <LineBreak/>
          <Hyperlink>
            <Hyperlink.NavigateUri>
              <xsl:value-of select="$_PROJECT_LIFE_CYCLE_url"/>
            </Hyperlink.NavigateUri>
            <xsl:value-of select="$_PROJECT_LIFE_CYCLE_url"/>
          </Hyperlink>
        </Paragraph>

        <Paragraph Style="{{StaticResource GroupParagraph}}">
          <xsl:value-of select="$_PLATFORM"/>
        </Paragraph>
        <Paragraph>          
            <xsl:value-of select="Project/Platform"/>          
        </Paragraph>
        <Paragraph Style="{{StaticResource GroupParagraph}}">
          <xsl:value-of select="$_GUIDE_SCOPE"/>
        </Paragraph>
        <Paragraph>          
          "<xsl:call-template name="breakLines">
          <xsl:with-param name="text" select="Project/Description" />
        </xsl:call-template>"<LineBreak/>          
          <xsl:value-of select="$_GUIDE_SCOPE_text"/>          
        </Paragraph>

        <BlockUIContainer Margin="0,10,0,0">

          <!--<Viewbox StretchDirection="DownOnly" Stretch="Fill" HorizontalAlignment="Stretch" VerticalAlignment="Top">-->
            <Grid HorizontalAlignment="Stretch" >
              <Grid.ColumnDefinitions>
                <xsl:for-each select="//ArrayOfBacklogItemGroup/BacklogItemGroup">
                  <ColumnDefinition />
                </xsl:for-each>
              </Grid.ColumnDefinitions>
              <Grid.RowDefinitions>
                <RowDefinition Height="auto"/>
                <RowDefinition Height="*"/>
              </Grid.RowDefinitions>
              <StackPanel Grid.Row="0" Grid.ColumnSpan="{count(//ArrayOfBacklogItemGroup/BacklogItemGroup)}">
                <Border
                  Background="LightGray" Padding="4" BorderThickness="2" BorderBrush="Black" HorizontalAlignment="Center">
                  <TextBlock>
                    <xsl:value-of select="Project/ProjectName"/>
                  </TextBlock>
                </Border>
                <Line X1="0" Y1="0" X2="0" Y2="10" Stroke="Black" StrokeThickness="2" HorizontalAlignment="Center" />
              </StackPanel>

              <xsl:for-each select="//ArrayOfBacklogItemGroup/BacklogItemGroup">
                <xsl:sort select="DefaultGroup" data-type="number"/>
                <xsl:sort select="GroupName"/>

                <xsl:variable name="groupUId" select="GroupUId"/>
                <xsl:variable name="groupColor" select="GroupColor"/>
                <xsl:variable name="textColor">
                  <xsl:choose>
                    <xsl:when test="$groupColor = 'Black'">
                      <xsl:value-of select="'White'"/>
                    </xsl:when>
                    <xsl:when test="$groupColor = 'OliveDrab'">
                      <xsl:value-of select="'White'"/>
                    </xsl:when>
                    <xsl:when test="$groupColor = 'Crimson'">
                      <xsl:value-of select="'White'"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="'Black'"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:variable>
                <xsl:variable name="groupItems" select="//ArrayOfBacklogItem/BacklogItem[GroupUId=$groupUId]"/>

                <StackPanel Grid.Row="1" Grid.Column="{position()-1}">

                  <Grid>
                    <Grid.ColumnDefinitions>
                      <ColumnDefinition Width=".5*" />
                      <ColumnDefinition Width=".5*" />
                    </Grid.ColumnDefinitions>
                    <xsl:if test="position() != 1">                    
                    <Rectangle Grid.Column="0" HorizontalAlignment="Stretch" Fill="Black" Height="2" VerticalAlignment="Top"/>
                    </xsl:if>
                    <xsl:if test="position() != last()">
                      <Rectangle Grid.Column="1" HorizontalAlignment="Stretch" Fill="Black" Height="2" VerticalAlignment="Top"/>
                    </xsl:if>
                  </Grid>

                  <Line X1="0" Y1="0" X2="0" Y2="10" Stroke="Black" StrokeThickness="2" HorizontalAlignment="Center" />
                  <Border Background="{$groupColor}" Padding="4" BorderThickness="2" BorderBrush="Black" Margin="0,0,4,0" HorizontalAlignment="Stretch">
                    <TextBlock HorizontalAlignment="Center" Foreground="{$textColor}"  FontSize="12" FontWeight="Bold" TextWrapping="Wrap" LineHeight="15">
                      <xsl:value-of select="GroupName"/>
                    </TextBlock>
                  </Border>

                  <xsl:for-each select="$groupItems">
                    <xsl:variable name="itemUId" select="BacklogItemUId"/>
                    <xsl:variable name="item" select="//ArrayOfBacklogItem/BacklogItem[BacklogItemUId=$itemUId]"/>

                    
                    <Line X1="0" Y1="0" X2="0" Y2="10" Stroke="Black" StrokeThickness="2" HorizontalAlignment="Center" />
                    <Border Background="{$groupColor}" Padding="3" BorderThickness="2" BorderBrush="Black" Margin="0,0,4,0" HorizontalAlignment="Stretch">
                      <TextBlock FontSize="10" Foreground="{$textColor}" TextWrapping="Wrap" LineHeight="10" TextAlignment="Left">
                        <xsl:value-of select="$item/Name"/>
                        (<xsl:value-of select="$item/BacklogItemNumber"/>)

                      </TextBlock>
                    </Border>
                  </xsl:for-each>

                </StackPanel>




              </xsl:for-each>
            </Grid>        
          <!--</Viewbox>-->
    
        </BlockUIContainer>
  


        <Paragraph Style="{{StaticResource GroupParagraph}}">
          <xsl:value-of select="$_PEOPLE_AND_COMMUNICATION"/>
        </Paragraph>
        <Paragraph>
          <xsl:value-of select="$_PEOPLE_AND_COMMUNICATION_text1"/>
        </Paragraph>
        <xsl:call-template name="projectTeam"/>
        <Paragraph>
          <LineBreak/>
          <xsl:value-of select="$_PEOPLE_AND_COMMUNICATION_text2"/>
          <LineBreak/>
          <xsl:for-each select="Project/Memberships/ProjectMembership[Role/PermissionSet = 0 or Role/PermissionSet = 2]">
            <xsl:if test="position() = last() and position() != 0">
              <xsl:value-of select="$_and_"/>
            </xsl:if>
            <xsl:if test="position() != last() and position() > 1" >, </xsl:if>
            <Bold>
              <xsl:value-of select="Role/RoleName" />(s)
            </Bold>
          </xsl:for-each>
          <xsl:value-of select="$_PEOPLE_AND_COMMUNICATION_text3"/>
        </Paragraph>

        <Paragraph Style="{{StaticResource GroupParagraph}}">
          <xsl:value-of select="$_DATA_MANAGEMENT"/>
        </Paragraph>
        <Paragraph>
          <xsl:value-of select="$_DATA_MANAGEMENT_text"/><LineBreak/>
          <xsl:value-of select="$_DATA_MANAGEMENT_codeFolder"/>:
          <Hyperlink>
            <Hyperlink.NavigateUri>
              <xsl:value-of select="Project/CodeRepositoryPath"/>
            </Hyperlink.NavigateUri>
            <xsl:value-of select="Project/CodeRepositoryPath"/>
          </Hyperlink>
          <LineBreak/>
          <xsl:value-of select="$_DATA_MANAGEMENT_docFolder"/>:
          <Hyperlink>
            <Hyperlink.NavigateUri>
              <xsl:value-of select="Project/DocRepositoryPath"/>
            </Hyperlink.NavigateUri>
            <xsl:value-of select="Project/DocRepositoryPath"/>
          </Hyperlink>
        </Paragraph>

        <Paragraph Style="{{StaticResource GroupParagraph}}">
          <xsl:value-of select="$_IMPORTANT_DATES"/>
        </Paragraph>
        <Paragraph>
          <xsl:value-of select="$_IMPORTANT_DATES_projectStart"/>:
          <Bold>
            <xsl:call-template name="formatDate">
              <xsl:with-param name="dateTime" select="Project/Sprints/Sprint[SprintNumber = 1]/StartDate" />
            </xsl:call-template>
          </Bold>
          <LineBreak/>

          <xsl:value-of select="$_IMPORTANT_DATES_projectEnd"/>:
          <Bold>
            <xsl:call-template name="formatDate">
              <xsl:with-param name="dateTime" select="/ReportData/ProjectEndDate" />
            </xsl:call-template>
          </Bold>
          <LineBreak/>
          <xsl:value-of select="$_IMPORTANT_DATES_text"/>
        </Paragraph>


        <Paragraph Style="{{StaticResource GroupParagraph}}">
          <xsl:value-of select="$_RISKS_AND_VIABILITY"/>          
        </Paragraph>
        <Paragraph>
          <xsl:value-of select="$_RISKS_AND_VIABILITY_text"/>
        </Paragraph>
        <Table>
          <Table.Columns>
            <TableColumn Width="60" />
            <TableColumn Width="60" />
          </Table.Columns>
          <TableRowGroup>
            <xsl:if test="count(//ArrayOfRisk/Risk[Probability &gt;= 1]) &gt; 0">
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
            </xsl:if>
            <xsl:for-each select="//ArrayOfRisk/Risk[Probability &gt;= 1]">
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
            <xsl:if test="count(//ArrayOfRisk/Risk[Probability &gt;= 1]) = 0">
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
        
     
      </FlowDocument>
    </xsl:template>
  


</xsl:stylesheet>
