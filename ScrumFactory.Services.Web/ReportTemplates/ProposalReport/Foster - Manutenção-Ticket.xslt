<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet
    version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"    
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/xps/2005/06/resourcedictionary-key">
  
    <xsl:output method="xml" indent="yes"/>
  
    <xsl:include href="../include/locale.pt-br.xslt"/>
    <xsl:include href="../include/ProposalHelpers.xslt"/>    
    <xsl:include href="../include/styles.xslt"/>
  

    <xsl:template name="sprintDeliveries">
      <xsl:param name="sprintNumber"/>
      <Table>
        <Table.Columns>
          <TableColumn Width="40" />
          <TableColumn />
          <TableColumn Width="60" />
          <TableColumn Width="100" />
          <TableColumn Width="100" />
        </Table.Columns>
        <TableRowGroup>
          <xsl:for-each select="/ReportData/Project/BacklogItems/BacklogItem[SprintNumber = $sprintNumber]">
            <xsl:sort select="OccurrenceConstraint" data-type="number"/>
            <xsl:sort select="BusinessPriority" data-type="number"/>
            <xsl:variable name="itemStyle">
              <xsl:choose>
                <xsl:when test="OccurrenceConstraint = 2" >{StaticResource deliveryItemCell}</xsl:when>
                <xsl:otherwise>{StaticResource normalItemCell}</xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <TableRow>
              <TableCell Style="{$itemStyle}" TextAlignment="Right">
                <Paragraph>
                  <xsl:value-of select="BacklogItemNumber"/>
                </Paragraph>
              </TableCell>
              <TableCell Style="{$itemStyle}">
                <Paragraph>
                  <xsl:value-of select="Name"/>
                </Paragraph>
              </TableCell>
              <TableCell Style="{$itemStyle}" TextAlignment="Right">
                <Paragraph>
                  <xsl:value-of select="sum(PlannedHours/PlannedHour/Hours)"/> hrs
                </Paragraph>
              </TableCell>
              <TableCell Style="{$itemStyle}" TextAlignment="Center">
                <Paragraph>
                  <xsl:call-template name="itemStatus" />
                </Paragraph>
              </TableCell>
              <TableCell Style="{$itemStyle}" TextAlignment="Right">
                <Paragraph FontWeight="Bold" FontSize="16">
                  <xsl:if test="OccurrenceConstraint = 2">
                    <xsl:call-template name="formatDate">
                      <xsl:with-param name="dateTime" select="/ReportData/Project/Sprints/Sprint[SprintNumber = $sprintNumber]/EndDate" />
                    </xsl:call-template>                    
                  </xsl:if>
                  <xsl:if test="string-length(DeliveryDate) &gt; 0">
                    <xsl:call-template name="formatDate">
                      <xsl:with-param name="dateTime" select="DeliveryDate" />
                    </xsl:call-template>
                  </xsl:if>
                </Paragraph>
              </TableCell>
            </TableRow>
          </xsl:for-each>
        </TableRowGroup>
      </Table>
    </xsl:template>

  
    <xsl:template match="/ReportData">
      <FlowDocument        
        PageWidth="21cm"
        PageHeight="29.7cm"      
        PagePadding="80,40,80,40"
        LineHeight="25"
        ColumnWidth="21 cm">
      
        <xsl:call-template name="styles"/>

        <xsl:call-template name="reportHeader">
          <xsl:with-param name="title" select="Proposal/ProposalName"/>
          <xsl:with-param name="showSprintsInfo" select="false"/>
        </xsl:call-template>

        <Paragraph Style="{{StaticResource GroupParagraph}}">
          1. <xsl:value-of select="$_PROJECT_DESCRIPTION"/>           
        </Paragraph>
        <Paragraph>
          <xsl:call-template name="breakLines">
            <xsl:with-param name="text" select="Proposal/Description" />
          </xsl:call-template>
        </Paragraph>

        <Paragraph Style="{{StaticResource GroupParagraph}}">
          2. <xsl:value-of select="$_TECHNOLOGY_and_PLATFORM"/>           
      </Paragraph>
        <Paragraph>
          <xsl:value-of select="Project/Platform"/>
        </Paragraph>

        
        <Paragraph Style="{{StaticResource GroupParagraph}}">
          3. CUSTO MENSAL
      </Paragraph>

        <xsl:call-template name="proposalPriceTicket"/>


        <Paragraph Style="{{StaticResource GroupParagraph}}">
          3.1 Valor hora
        </Paragraph>
        <xsl:call-template name="proposalHourCosts"/>

        <Paragraph Style="{{StaticResource GroupParagraph}}">
          4. PERÍODO
        </Paragraph>
        <xsl:call-template name="proposalScheduleTicket"/>

        <Paragraph Style="{{StaticResource GroupParagraph}}">
          5. <xsl:value-of select="$_STAKEHOLDERS"/>
        </Paragraph>        
         <xsl:call-template name="projectTeam"/>

        
        
          <Paragraph Style="{{StaticResource GroupParagraph}}">
            6. CONDIÇÕES COMERCIAIS
          </Paragraph>
        <Paragraph Margin="0,0,0,10">
          <TextBlock FontWeight="Bold">
            6.1<xsl:text>&#x20;</xsl:text>Reajuste
          </TextBlock>
          <LineBreak/>
          Os valores acima serão reajustados anualmente, tendo como base a variação do IGP-M (Índice Geral de Preços de Mercado) da Fundação Getúlio Vargas,
          ocorrida no período.
        </Paragraph>
        <Paragraph Margin="0,0,0,10">
          <TextBlock FontWeight="Bold">
            6.2<xsl:text>&#x20;</xsl:text>Despesas de locomoção
          </TextBlock>
          <LineBreak/>
          As despesas de locomoção, caso existam, deverão ser reembolsadas pela CONTRATANTE, mediante Nota de débito.
        </Paragraph>
        <Paragraph Margin="0,0,0,10">
          <TextBlock FontWeight="Bold">
            6.3<xsl:text>&#x20;</xsl:text>Créditos e saldo
          </TextBlock>
          <LineBreak/>
          As horas adquiridas e não usadas em um determinado mês podem ser usadas até o final do contrato. Ao final do contrato, havendo saldo devedor (foram usadas mais
          horas que as adquiridas), a CONTRATANTE deve quitar imediatamente o saldo devedor. Ao final do contrato, havendo saldo credor (foram usadas menos horas que
          as adquiridas), a CONTRATANTE poderá usá-las solicitando serviços da CONTRATADA nos três meses seguintes.
        </Paragraph>

        <xsl:if test="count(//Proposal/Clauses/ProposalClause) &gt; 0">
          <Paragraph Style="{{StaticResource GroupParagraph}}">
            7. <xsl:value-of select="$_CLAUSES"/>
          </Paragraph>
          <xsl:call-template name="proposalClauses"/>
        </xsl:if>

        



      </FlowDocument>
    </xsl:template>

  <xsl:template name="proposalScheduleTicket">
    <xsl:param name="showDate" select="1"/>
    <Table>
      <Table.Columns>
        <TableColumn />        
      </Table.Columns>
      <TableRowGroup>
        <xsl:if test="$showDate">
          <TableRow>
            <TableCell>
              <Paragraph>
                De
                <Bold>
                <xsl:call-template name="formatDate">
                  <xsl:with-param name="dateTime" select="//Proposal/EstimatedStartDate" />
                </xsl:call-template>
                </Bold>
              até
            <Bold>
                <xsl:call-template name="formatDate">
                  <xsl:with-param name="dateTime" select="//Proposal/EstimatedEndDate" />
                </xsl:call-template>
            </Bold>
              </Paragraph>
            </TableCell>
          </TableRow>
        </xsl:if>
     

      </TableRowGroup>
    </Table>
  </xsl:template>

  <xsl:template name="proposalPriceTicket">


    <Table>
      <Table.Columns>
        <TableColumn />
        <TableColumn />
        <TableColumn />
      </Table.Columns>
      <TableRowGroup>       
        <TableRow>
          <TableCell BorderThickness="0,1,0,1" BorderBrush="Black">
            <Paragraph>
              Valor mensal da manutenção (12x)
            </Paragraph>
          </TableCell>
          <TableCell BorderThickness="0,1,0,1" BorderBrush="Black">
            <Paragraph Style="{{StaticResource ValueParagraph}}" TextAlignment="Right">
              <xsl:value-of select="//Proposal/CurrencySymbol"/>
              <xsl:text>&#x20;</xsl:text>
              <xsl:value-of select="format-number(//Proposal/TotalValue div $currencyRate div 12, $moneyN, 'numberFormat')"/>
            </Paragraph>
          </TableCell>
        </TableRow>
      </TableRowGroup>
    </Table>

  </xsl:template>
  


</xsl:stylesheet>
