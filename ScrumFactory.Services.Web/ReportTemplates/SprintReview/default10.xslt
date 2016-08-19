<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet
    version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"    
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/xps/2005/06/resourcedictionary-key"
    
    xmlns:PresentationOptions="http://schemas.microsoft.com/winfx/2006/xaml/presentation/options"
    xmlns:visualization="clr-namespace:System.Windows.Controls.DataVisualization;assembly=System.Windows.Controls.DataVisualization.Toolkit"
    xmlns:charting="clr-namespace:System.Windows.Controls.DataVisualization.Charting;assembly=System.Windows.Controls.DataVisualization.Toolkit"
    xmlns:chartPrmtvs="clr-namespace:System.Windows.Controls.DataVisualization.Charting.Primitives;assembly=System.Windows.Controls.DataVisualization.Toolkit" >
  
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
          <Grid x:Name="burndown" HorizontalAlignment="Stretch" >
            
            <charting:Chart Margin="0,10,0,0" HorizontalAlignment="Stretch" Height="300">
            
            <charting:Chart.Resources>
              
              <Color x:Key="Blue3Color" PresentationOptions:Freeze="True" R="55" G="96" B="146" A="255" />
              <Color x:Key="Blue4Color" PresentationOptions:Freeze="True" R="25" G="46" B="106" A="255" />
              <Color x:Key="Orange2Color" PresentationOptions:Freeze="True" R="254" G="202" B="108" A="255" />

              <SolidColorBrush x:Key="GraphBGBrush1" Color="{{StaticResource Blue3Color}}" Opacity="0.8"/>
              <SolidColorBrush x:Key="GraphBorderBrush1" Color="{{StaticResource Blue4Color}}"/>

              <SolidColorBrush x:Key="GraphBGBrush2" Color="Red" Opacity="0.8"/>
              <SolidColorBrush x:Key="GraphBorderBrush2" Color="Red"/>

              <SolidColorBrush x:Key="IdealHoursBrush" Color="{{StaticResource Blue3Color}}"/>

              <Style x:Key="GraphSerie1Style" TargetType="Control">
                <Setter Property="Background" Value="{{StaticResource GraphBGBrush1}}"/>
                <Setter Property="BorderBrush" Value="{{StaticResource GraphBorderBrush1}}"/>
                <Setter Property="BorderThickness" Value="3"/>
              </Style>

              <Style x:Key="GraphSerie1ColumnStyle" BasedOn="{{StaticResource GraphSerie1Style}}" TargetType="Control">
                <Setter Property="Template">
                  <Setter.Value>
                    <ControlTemplate TargetType="{{x:Type charting:ColumnDataPoint}}">
                      <Border
                      ToolTip="{{Binding Hours}}"
                      Background="{{TemplateBinding Background}}"
                      BorderBrush="{{TemplateBinding BorderBrush}}"
                      BorderThickness="{{TemplateBinding BorderThickness}}">
                      </Border>
                    </ControlTemplate>
                  </Setter.Value>
                </Setter>
              </Style>


              <Style x:Key="GraphSerie2Style" TargetType="Control">
                <Setter Property="Background" Value="{{StaticResource GraphBGBrush2}}"/>
                <Setter Property="BorderBrush" Value="{{StaticResource GraphBorderBrush2}}"/>
                <Setter Property="BorderThickness" Value="3"/>
              </Style>

              <Style x:Key="GraphSerie2ColumnStyle" BasedOn="{{StaticResource GraphSerie2Style}}" TargetType="Control">
                <Setter Property="Template">
                  <Setter.Value>
                    <ControlTemplate TargetType="{{x:Type charting:ColumnDataPoint}}">
                      <Border
                      ToolTip="{{Binding Hours}}"
                      Background="{{TemplateBinding Background}}"
                      BorderBrush="{{TemplateBinding BorderBrush}}"
                      BorderThickness="{{TemplateBinding BorderThickness}}">
                      </Border>
                    </ControlTemplate>
                  </Setter.Value>
                </Setter>
              </Style>

              <Style x:Key="HorizontalLegendStyle" TargetType="charting:LegendItem">
                <Setter Property="Margin" Value="0,0,10,0"/>
              </Style>

              <Style x:Key="HideLegendStyle" TargetType="charting:LegendItem">
                <Setter Property="Visibility" Value="Collapsed"/>
              </Style>

            </charting:Chart.Resources>

            <charting:Chart.Template>
              <ControlTemplate TargetType="charting:Chart">
                <Grid Background="{{TemplateBinding Background}}">
                  <Grid.RowDefinitions>
                    <RowDefinition Height="*" />
                    <RowDefinition Height="Auto" />
                  </Grid.RowDefinitions>

                  <visualization:Title Content="{{TemplateBinding Title}}" Style="{{TemplateBinding TitleStyle}}" Margin="1"/>

                  <chartPrmtvs:EdgePanel x:Name="ChartArea" Style="{{TemplateBinding ChartAreaStyle}}" Background="{{TemplateBinding Background}}">
                    <Grid Canvas.ZIndex="-1" Style="{{TemplateBinding PlotAreaStyle}}" Background="{{TemplateBinding Background}}"/>
                    <Border Canvas.ZIndex="10" BorderThickness="0"  />
                  </chartPrmtvs:EdgePanel>

                  <visualization:Legend x:Name="Legend" Grid.Row="1" Style="{{TemplateBinding LegendStyle}}" Title="{{TemplateBinding LegendTitle}}"/>

                </Grid>
              </ControlTemplate>
            </charting:Chart.Template>

            <charting:Chart.LegendStyle>
              <Style TargetType="visualization:Legend">
              <Setter Property="BorderThickness"  Value="0,0,0,0"/>
              <Setter Property="Background" Value="Transparent"/>
              <Setter Property="FontFamily" Value="Calibri"/>
              <!--<Setter Property="FontSize" Value="16"/>-->              
              <Setter Property="ItemsPanel">
                <Setter.Value>
                  <ItemsPanelTemplate>
                    <StackPanel Orientation="Horizontal" />
                  </ItemsPanelTemplate>
                </Setter.Value>
              </Setter>
              </Style>
            </charting:Chart.LegendStyle>
                        
            <charting:Chart.Axes>
              <charting:DateTimeAxis Orientation="X"  IntervalType="Days"   >
                <charting:DateTimeAxis.AxisLabelStyle>
                  <Style TargetType="charting:AxisLabel">
                    <Setter Property="Template">
                      <Setter.Value>
                        <ControlTemplate TargetType="charting:AxisLabel">                          
                          <TextBlock Text="{$_GRAPH_DateBindShort}" FontSize="8" FontFamily="Calibri"/>
                        </ControlTemplate>
                      </Setter.Value>
                    </Setter>
                  </Style>
                </charting:DateTimeAxis.AxisLabelStyle>
              </charting:DateTimeAxis>
              <charting:LinearAxis Minimum="0" Orientation="Y" ShowGridLines="True" Title="{$_GRAPH_Hours}" FontFamily="Calibri">
                <charting:LinearAxis.AxisLabelStyle>
                  <Style TargetType="charting:AxisLabel">
                    <Setter Property="FontFamily" Value="Calibri"/>
                    <Setter Property="FontSize" Value="12"/>
                    <Setter Property="VerticalAlignment" Value="Center"/>
                  </Style>                    
                </charting:LinearAxis.AxisLabelStyle>
                <charting:LinearAxis.GridLineStyle>
                  <Style TargetType="{{x:Type Line}}">
                    <Setter Property="Stroke" Value="LightGray"/>
                  </Style>
                </charting:LinearAxis.GridLineStyle>
              </charting:LinearAxis>
            </charting:Chart.Axes>

            <charting:Chart.Series>

              <charting:AreaSeries
                  LegendItemStyle="{{StaticResource HideLegendStyle}}"
                  ItemsSource="{{Binding ActualHoursAhead}}"
                  IndependentValuePath="Date"
                  DependentValuePath="TotalHours">
                <charting:AreaSeries.DataPointStyle>
                  <Style TargetType="Control">
                    <Setter Property="Template">
                      <Setter.Value>
                        <ControlTemplate/>
                      </Setter.Value>
                    </Setter>
                    <Setter Property="Background" Value="LightGray"/>
                  </Style>
                </charting:AreaSeries.DataPointStyle>
              </charting:AreaSeries>

              <charting:AreaSeries
                  Title="{$_GRAPH_Actual_hours}"
                  LegendItemStyle="{{StaticResource HorizontalLegendStyle}}"
                  ItemsSource="{{Binding ActualHours}}"
                  IndependentValuePath="Date"
                  DependentValuePath="TotalHours">
                <charting:AreaSeries.DataPointStyle>
                  <Style BasedOn="{{StaticResource GraphSerie1Style}}" TargetType="Control">
                    <Setter Property="Width" Value="10"/>
                    <Setter Property="Height" Value="10"/>
                    <Setter Property="Template">
                      <Setter.Value>
                        <ControlTemplate TargetType="{{x:Type charting:AreaDataPoint}}">
                          <Ellipse
                                  Cursor="Help" Stroke="{{StaticResource GraphBorderBrush1}}"
                                  StrokeThickness="3" Width="10" Height="10">
                            <Ellipse.Fill>
                              <SolidColorBrush Color="White"/>
                            </Ellipse.Fill>                            
                          </Ellipse>
                        </ControlTemplate>
                      </Setter.Value>
                    </Setter>
                    <Style.Triggers>
                      <DataTrigger Binding="{{Binding IsToday}}" Value="False">
                        <Setter Property="Width" Value="0"/>
                        <Setter Property="Height" Value="0"/>
                      </DataTrigger>
                    </Style.Triggers>
                  </Style>
                </charting:AreaSeries.DataPointStyle>
                <charting:AreaSeries.PathStyle>
                  <Style TargetType="Path">
                    <Setter Property="StrokeThickness" Value="2" />
                    <Setter Property="Stroke">
                      <Setter.Value>
                        <SolidColorBrush Color="{{StaticResource Blue4Color}}"/>
                      </Setter.Value>
                    </Setter>
                  </Style>
                </charting:AreaSeries.PathStyle>
              </charting:AreaSeries>

              <charting:LineSeries
                  Title="{$_GRAPH_Planned_hours}"
                  LegendItemStyle="{{StaticResource HorizontalLegendStyle}}"
                  ItemsSource="{{Binding PlannedHours}}"
                    
                  IndependentValuePath="Date"
                  DependentValuePath="TotalHours">
                <charting:LineSeries.PolylineStyle>
                  <Style TargetType="{{x:Type Polyline}}">
                    <Setter Property="StrokeThickness" Value="3"/>
                  </Style>
                </charting:LineSeries.PolylineStyle>
                <charting:LineSeries.DataPointStyle>
                  <Style BasedOn="{{StaticResource GraphSerie2Style}}" TargetType="Control">
                    <Setter Property="Width" Value="10"/>
                    <Setter Property="Height" Value="10"/>
                    <Setter Property="Template">
                      <Setter.Value>
                        <ControlTemplate TargetType="{{x:Type charting:LineDataPoint}}">
                          <Ellipse
                                  Cursor="Help" x:Name="ellipse"
                                  Stroke="{{StaticResource GraphBorderBrush2}}" StrokeThickness="3" Width="10" Height="10" Fill="White">           
                          </Ellipse>
                          <ControlTemplate.Triggers>
                            <DataTrigger Binding="{{Binding IsToday}}" Value="True">                              
                              <Setter TargetName="ellipse" Property="Ellipse.Stroke">
                                <Setter.Value>
                                  <SolidColorBrush Color="{{StaticResource Orange2Color}}"/>
                                </Setter.Value>
                              </Setter>
                            </DataTrigger>
                          </ControlTemplate.Triggers>
                        </ControlTemplate>
                      </Setter.Value>
                    </Setter>
                  </Style>
                </charting:LineSeries.DataPointStyle>
              </charting:LineSeries>

            </charting:Chart.Series>

          
          </charting:Chart>
            
          <!-- MAIN INDICATORS -->
        <StackPanel HorizontalAlignment="Right" VerticalAlignment="Top" Orientation="Horizontal">
            <TextBlock Width="100" TextAlignment="Center">
                <TextBlock
                    HorizontalAlignment="Center"
                    FontFamily="Segoe UI" FontSize="40" FontWeight="Bold"
                    Text="{{Binding WalkedPct, StringFormat=0}}"/>
                <LineBreak/>
                <TextBlock
                    HorizontalAlignment="Center"
                    FontSize="10" FontWeight="Bold"
                    TextWrapping="Wrap"
                    Text="{$_GRAPH_walked}"/>
            </TextBlock>            
            <TextBlock                 
                Width="100" TextAlignment="Center"
                HorizontalAlignment="Center">
                <TextBlock
                    HorizontalAlignment="Center" TextAlignment="Center"
                    FontFamily="Segoe UI" FontSize="40" FontWeight="Bold"
                    Text="{{Binding DeadlinePosition, StringFormat=0}}"/>
                <LineBreak/>
                <TextBlock
                    HorizontalAlignment="Center" TextAlignment="Center" TextWrapping="Wrap"
                    FontSize="10" FontWeight="Bold">                    
                  <TextBlock.Style>
                      <Style TargetType="{{x:Type TextBlock}}">
                          <Style.Triggers>
                              <DataTrigger Binding="{{Binding DeadlineAhead}}" Value="false">
                                  <Setter Property="Text" Value="{$_GRAPH_hrs_late}"/>
                              </DataTrigger>
                          <DataTrigger Binding="{{Binding DeadlineAhead}}" Value="true">
                                  <Setter Property="Text" Value="{$_GRAPH_hrs_ahead}"/>
                              </DataTrigger>
                          </Style.Triggers>
                      </Style>
                  </TextBlock.Style>
                
              </TextBlock>
            </TextBlock>            
        </StackPanel>
          
          </Grid>
          
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
