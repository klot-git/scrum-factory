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
        PageWidth="21cm"
        PageHeight="29.7cm"      
        PagePadding="60,40,40,40"      
        ColumnWidth="21 cm">
        
        <xsl:call-template name="styles"/>


        <xsl:call-template name="reportHeader">
          <xsl:with-param name="title" select="$_PROJECT_SCHEDULE"/>
        </xsl:call-template>

        <xsl:variable name="deliveriedItems" select="Project/Sprints/Sprint[SprintNumber &lt; /ReportData/ProjectCurrentSprintNumber]"/>

        <!--<Paragraph Style="{{StaticResource GroupParagraph}}">
          <xsl:value-of select="$_Project_start"/>
          <xsl:call-template name="formatDate">
            <xsl:with-param name="dateTime" select="Project/Sprints/Sprint[SprintNumber = 1]/StartDate" />
          </xsl:call-template>
        </Paragraph>-->


  


        <Paragraph Style="{{StaticResource GroupParagraph}}">
          <xsl:value-of select="$_SCHEDULED_ITEMS"/>          
        </Paragraph>
        
            <xsl:for-each select="Project/Sprints/Sprint[SprintNumber &gt;= /ReportData/ProjectCurrentSprintNumber]">
              <xsl:sort select="SprintNumber" data-type="number"/>

                <Paragraph Style="{{StaticResource GroupParagraph}}" FontWeight="Bold" Margin="0,25,0,10" TextAlignment="Left" >
                  Sprint <xsl:value-of select="SprintNumber"/>
                </Paragraph>
              
                  <xsl:call-template name="sprintDeliveries">
                    <xsl:with-param name="sprintNumber" select="SprintNumber"/>
                  </xsl:call-template>
              
            </xsl:for-each>            
          
        <!--<Table CellSpacing="4" Margin="00,60,0,0">
          <Table.Columns>            
            <TableColumn Width="20" />
            <TableColumn />
          </Table.Columns>
          <TableRowGroup>
            <TableRow>              
              <TableCell Style="{{StaticResource deliveryItemCell}}">
                <Paragraph></Paragraph>
              </TableCell>
              <TableCell>
                <Paragraph FontSize="10">
                  <xsl:value-of select="$_Delivery_item"/>                  
                </Paragraph>
              </TableCell>
            </TableRow>
            <TableRow>
              
              <TableCell Style="{{StaticResource criticalItemCell}}">
                <Paragraph></Paragraph>
              </TableCell>
              <TableCell>
                <Paragraph  FontSize="10">
                  <xsl:value-of select="$_Critical_item"/>
                </Paragraph>
              </TableCell>
            </TableRow>
          </TableRowGroup>
        </Table>-->


        <xsl:if test="count($deliveriedItems) &gt; 0">
          <Paragraph Style="{{StaticResource GroupParagraph}}" Foreground="Gray">
            <LineBreak/>
            <xsl:value-of select="$_PREVIOUS_ITEMS"/>
          </Paragraph>

          <xsl:for-each select="$deliveriedItems">
            <xsl:sort select="SprintNumber" data-type="number" order="descending"/>

            <Paragraph Style="{{StaticResource GroupParagraph}}" FontWeight="Bold" Margin="0,25,0,10" TextAlignment="Left" >
              Sprint <xsl:value-of select="SprintNumber"/>
            </Paragraph>
            <xsl:call-template name="sprintDeliveries">
              <xsl:with-param name="sprintNumber" select="SprintNumber"/>
            </xsl:call-template>

          </xsl:for-each>

        </xsl:if>


      </FlowDocument>
    </xsl:template>
  


</xsl:stylesheet>
