<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/xps/2005/06/resourcedictionary-key">

  <xsl:template name="styles">
    <FlowDocument.Resources>      
      <Style TargetType="{{x:Type FlowDocument}}">
        <Setter Property="Background" Value="White"/>
      </Style>

      <Style TargetType="{{x:Type Table}}">
        <Setter Property="CellSpacing" Value="0"/>
        <Setter Property="Margin" Value="0"/>
      </Style>

      <Style x:Key="headerTable" TargetType="{{x:Type Table}}">
        <Setter Property="Background" Value="#EEEEEE"/>
        <Setter Property="Margin" Value="0"/>
      </Style>

      <Style TargetType="{{x:Type Paragraph}}">
        <Setter Property="FontSize" Value="14"/>
        <Setter Property="Foreground" Value="Black"/>
        <Setter Property="FontFamily" Value="Calibri"/>
        <Setter Property="Margin" Value="0"/>        
      </Style>


        <Style TargetType="{{x:Type TextBlock}}">
            <Setter Property="FontSize" Value="14"/>
            <Setter Property="Foreground" Value="Black"/>
            <Setter Property="FontFamily" Value="Calibri"/>
            <Setter Property="Margin" Value="0"/>
        </Style>

      <Style x:Key="NormalIndicatorParagraph" TargetType="{{x:Type Paragraph}}">
        <Setter Property="FontSize" Value="32"/>
        <Setter Property="FontFamily" Value="Calibri"/>
        <Setter Property="FontWeight" Value="Bold"/>
       
        <Setter Property="Foreground" Value="Black"/>
      </Style>

      <Style x:Key="MediumIndicatorParagraph" TargetType="{{x:Type Paragraph}}">
        <Setter Property="FontSize" Value="32"/>
        <Setter Property="FontFamily" Value="Calibri"/>
        <Setter Property="Foreground" Value="Orange"/>
      </Style>

      <Style x:Key="HighIndicatorParagraph" TargetType="{{x:Type Paragraph}}">
        <Setter Property="FontSize" Value="32"/>
        <Setter Property="FontFamily" Value="Calibri"/>
        <Setter Property="Foreground" Value="Red"/>
      </Style>

      <Style TargetType="{{x:Type TableCell}}">
        <Setter Property="Padding" Value="3"/>
      </Style>

      <Style x:Key="normalItemCell" TargetType="{{x:Type TableCell}}">
        <Setter Property="Padding" Value="3,3,3,3"/>
        <Setter Property="BorderThickness" Value="0"/>
      </Style>

      <Style x:Key="headerItemCell" TargetType="{{x:Type TableCell}}">
        <Setter Property="Padding" Value="3"/>
        <Setter Property="BorderBrush" Value="Black"/>
        <Setter Property="BorderThickness" Value="0"/>
      </Style>


      <Style x:Key="deliveryItemCell" TargetType="{{x:Type TableCell}}">
        <Setter Property="Padding" Value="3"/>        
        <Setter Property="BorderThickness" Value="0"/>
        <Setter Property="Background" Value="#DDDDDD"/>
        <Setter Property="Foreground" Value="Black"/>        
      </Style>

      <Style x:Key="criticalItemCell" TargetType="{{x:Type TableCell}}">
        <Setter Property="Padding" Value="3"/>        
        <Setter Property="BorderThickness" Value="0"/>
        <Setter Property="Background" Value="Red"/>
        <Setter Property="Foreground" Value="White"/>
      </Style>

      <Style x:Key="TitleParagraph" TargetType="{{x:Type Paragraph}}" BasedOn="{{StaticResource {{x:Type Paragraph}}}}">
        <Setter Property="FontSize" Value="30"/>
        <Setter Property="Margin" Value="0,40,0,20"/>
      </Style>

      <Style x:Key="SubTitleParagraph" TargetType="{{x:Type Paragraph}}" BasedOn="{{StaticResource {{x:Type Paragraph}}}}">
        <Setter Property="FontSize" Value="20"/>
      </Style>

      <Style x:Key="GroupParagraph" TargetType="{{x:Type Paragraph}}" BasedOn="{{StaticResource {{x:Type Paragraph}}}}">
        <Setter Property="FontWeight" Value="Bold"/>
        <Setter Property="FontSize" Value="18"/>
        <Setter Property="Margin" Value="0,15,0,10"/>
      </Style>

      <Style x:Key="ValueParagraph" TargetType="{{x:Type Paragraph}}" BasedOn="{{StaticResource {{x:Type Paragraph}}}}">
        <Setter Property="FontWeight" Value="Bold"/>
      </Style>


      <Style x:Key="BacklogItemGroupTextBlock" TargetType="TextBlock">
        <Setter Property="FontFamily" Value="Calibri"/>
        <Setter Property="Foreground" Value="Black"/>
        <Style.Triggers>
          <Trigger Property="Background" Value="Black">
            <Setter Property="Foreground" Value="White"/>
          </Trigger>
          <Trigger Property="Background" Value="MediumSeaGreen">
            <Setter Property="Foreground" Value="PaleGreen"/>
          </Trigger>
          <Trigger Property="Background" Value="PaleGreen">
            <Setter Property="Foreground" Value="OliveDrab"/>
          </Trigger>
          <Trigger Property="Background" Value="OliveDrab">
            <Setter Property="Foreground" Value="PaleGreen"/>
          </Trigger>
          <Trigger Property="Background" Value="Crimson">
            <Setter Property="Foreground" Value="Pink"/>
          </Trigger>
          <Trigger Property="Background" Value="CornflowerBlue">
            <Setter Property="Foreground" Value="LightBlue"/>
          </Trigger>
          <Trigger Property="Background" Value="LightBlue">
            <Setter Property="Foreground" Value="DarkBlue"/>
          </Trigger>
          <Trigger Property="Background" Value="Gold">
            <Setter Property="Foreground" Value="Brown"/>
          </Trigger>
          <Trigger Property="Background" Value="Khaki">
            <Setter Property="Foreground" Value="Brown"/>
          </Trigger>
        </Style.Triggers>
      </Style>

    </FlowDocument.Resources>
  </xsl:template>


</xsl:stylesheet>
