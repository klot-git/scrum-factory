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



  <xsl:template match="/ReportData">
    <FlowDocument
      PageWidth="21cm"
      PageHeight="29.7cm"
      PagePadding="80,40,80,40"
      ColumnWidth="21 cm"
      LineHeight="22"
      Tag="sf-header:yes">

      <xsl:call-template name="styles"/>

        <BlockUIContainer>
        <Image HorizontalAlignment="Center" Source="{$ServerUrl}/Images/Companylogo.png" Stretch="None"  Margin="0,180,0,0"/>
      </BlockUIContainer>

      <BlockUIContainer>
        <Border Background="Black" HorizontalAlignment="Stretch" Height="20"  Margin="0,70,0,0"/>
      </BlockUIContainer>

      <Paragraph FontSize="30" TextAlignment="Left" Margin="0,40,0,0">
        <xsl:value-of select="Project/ProjectName"/> (<xsl:value-of select="Project/ProjectNumber"/>)<LineBreak/>
        <xsl:value-of select="Project/ClientName"/>
      </Paragraph>

      <Paragraph FontSize="24" TextAlignment="Left" Margin="0,70,0,0">
        Proposta Comercial Foster
      </Paragraph>
      
      <BlockUIContainer>
        <Border Background="Black" HorizontalAlignment="Stretch" Height="20" Margin="0,20,0,0"/>
      </BlockUIContainer>
      <Paragraph FontStyle="Italic" TextAlignment="Right" Margin="0">
        <xsl:call-template name="formatDate">
          <xsl:with-param name="dateTime" select="Today" />
        </xsl:call-template>
        &#x20;,
        <xsl:call-template name="formatTime">
          <xsl:with-param name="dateTime" select="Today" />
        </xsl:call-template>
      </Paragraph>

      <Paragraph Style="{{StaticResource TitleParagraph}}" BreakPageBefore="true" Margin="0,0,0,20">
        1. APRESENTAÇÃO
      </Paragraph>

      <Paragraph >
        A Foster é uma agência de comunicação especializada no cenário digital.<LineBreak/>
        Com mais de 20 anos de existência, nossa história confunde-se com o início da internet e acompanha o seu crescimento.
      </Paragraph>

      <Paragraph Margin="0,20,0,20">
        Somos parte do grupo WPP, o maior grupo de marketing do mundo, composto por 158 mil pessoas em mais de 2 mil escritórios espalhados por 106 países.
      </Paragraph>

      <BlockUIContainer Margin="0,20,0,20">
        <Image Source="{$ServerUrl}/images/foster/proposta/wpp.png" Width="600" HorizontalAlignment="Center"/>
      </BlockUIContainer>

      <Paragraph Style="{{StaticResource GroupParagraph}}">
        1.1 ESTRUTURA
      </Paragraph>
      <BlockUIContainer Margin="0,20,0,20">
        <Image Source="{$ServerUrl}/images/foster/proposta/estrutura.jpg" Width="200" HorizontalAlignment="Center"/>
      </BlockUIContainer>

      <Paragraph>
        A Foster é estruturada em diferentes áreas que estudarão o seu projeto para elaborar uma solução completa de comunicação.<LineBreak/>
        <LineBreak/>
        Nossa Fábrica de Software foi avaliada com sucesso no MPS.Br em 2009 mesclando processos de qualidade de software com um conjunto de práticas ágeis de desenvolvimento.
      </Paragraph>

      <BlockUIContainer Margin="0,20,0,20">
        <Image Source="{$ServerUrl}/images/foster/proposta/mpsbr.jpg" Width="80" HorizontalAlignment="Right"/>
      </BlockUIContainer>




      <Paragraph Style="{{StaticResource TitleParagraph}}" BreakPageBefore="true" Margin="0,0,0,20">
        2. ESCOPO
      </Paragraph>

      <Paragraph Style="{{StaticResource GroupParagraph}}">
        2.1 <xsl:value-of select="$_PROJECT_DESCRIPTION"/>
      </Paragraph>
      <Paragraph>
        <xsl:call-template name="breakLines">
          <xsl:with-param name="text" select="Proposal/Description" />
        </xsl:call-template>
      </Paragraph>

      <Paragraph Style="{{StaticResource GroupParagraph}}">
        2.2 <xsl:value-of select="$_TECHNOLOGY_and_PLATFORM"/>
      </Paragraph>
      <Paragraph>
        <xsl:value-of select="Project/Platform"/>
      </Paragraph>

      <Paragraph Style="{{StaticResource GroupParagraph}}">
        2.3 WORK BREAKDOWN STRUCTURE (WBS)
      </Paragraph>
      <BlockUIContainer Margin="0,10,0,0">

        <Grid HorizontalAlignment="Stretch">
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
            <!--<xsl:variable name="groupItems" select="//ArrayOfBacklogItem/BacklogItem[GroupUId=$groupUId]"/>-->
            
            <xsl:variable name="groupItems" select="//ArrayOfProposalItemWithPrice/ProposalItemWithPrice[BacklogItemUId=//ArrayOfBacklogItem/BacklogItem[GroupUId=$groupUId]/BacklogItemUId]"/>

            <!--<xsl:if test="count($groupItems) &gt; 0">-->
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
            <!--</xsl:if>-->



          </xsl:for-each>
        </Grid>


      </BlockUIContainer>

      <xsl:variable name="scopeIndex">
        <xsl:choose>
          <xsl:when test="count(//ArrayOfBacklogItem/BacklogItem[Description != '' and  OccurrenceConstraint=1]) &gt; 0">
            <xsl:value-of select="'2.5. '"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="'2.4. '"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable> 

      <xsl:if test="$scopeIndex = '2.5. '">
        <Paragraph Style="{{StaticResource GroupParagraph}}">
          2.4. <xsl:value-of select="$_FUNCTIONAL_REQUIREMENTS"/>
        </Paragraph>
        <Paragraph>
          <xsl:value-of select="$_FUNCTIONAL_REQUIREMENTS_tooltip"/>
        </Paragraph>
        <xsl:call-template name="scopeOnly"/>  
      </xsl:if>
      

      <xsl:if test="count(//ArrayOfProjectConstraint/ProjectConstraint) &gt; 0">
        <Paragraph Style="{{StaticResource GroupParagraph}}">
          <xsl:value-of select="$scopeIndex"/>
          <xsl:value-of select="$_NON_FUNCTIONAL_REQUIREMENTS"/>
        </Paragraph>
        <xsl:call-template name="projectConstraints"/>
      </xsl:if>
  

      <Paragraph Style="{{StaticResource TitleParagraph}}" BreakPageBefore="true">
        3. METODOLOGIA
      </Paragraph>

      <Paragraph>
        A Fábrica da Foster trabalha com uma metodologia ágil de desenvolvimento de software.
      </Paragraph>
      
      <Paragraph Margin="0,20,0,0">
        Com a finalidade de permitir um melhor gerenciamento da aplicação em desenvolvimento, a sua implementação será regulada por meio de <Bold>“Sprints”</Bold> que representarão o cronograma de implantação e as atividades a serem realizadas.        
      </Paragraph>

      <Paragraph  Margin="0,20,0,0">
        Cada <Bold>“Sprint”</Bold> terá um dimensionamento quinzenal e já estarão estabelecidos no cronograma inicial do projeto.<LineBreak/>
        Ao final do qual, será objeto de uma avaliação dos representantes do cliente, com a finalidade de validar os objetos realizados e a sua adequação aos serviços contratados.
      </Paragraph>
      <Paragraph Margin="0,20,0,0">
        No caso de não cumprimento das atividades estabelecidas em um determinado <Bold>“Sprint”</Bold>, os serviços considerados não-conformes serão re-planejados nos <Bold>“Sprints”</Bold> subsequentes.
      </Paragraph>
      <Paragraph Margin="0,20,0,0">
        Durante a realização da reunião de avaliação do resultado do <Bold>“Sprint”</Bold> o cliente tem liberdade para reorganizar o cronograma, definindo a prioridade dos itens que deverão ser executados nos próximos <Bold>“Sprints”</Bold>.
      </Paragraph>
      <List>
        <ListItem>
          <Paragraph >
            Como boa prática, evita-se que os itens do cronograma sejam alterados no meio do <Bold>“Sprint”</Bold>.
          </Paragraph>
        </ListItem>
        <ListItem>
          <Paragraph >
            É responsabilidade do cliente, validar os protótipos no prazo estabelecido no cronograma de modo a garantir e bom andamento dos prazos do projeto.
          </Paragraph>
        </ListItem>
        <ListItem>
          <Paragraph >
            Um relatório com percentual de andamento do projeto, horas estimadas para conclusão e riscos identificados também será entregue pelo Gerente de Projeto ao final de cada <Bold>“Sprint”</Bold>.
          </Paragraph>
        </ListItem>
      </List>
      
      

      <Paragraph Style="{{StaticResource TitleParagraph}}" >
        4. <xsl:value-of select="$_PEOPLE_AND_COMMUNICATION"/>
      </Paragraph>
      <xsl:call-template name="projectTeam"/>



      <Paragraph Style="{{StaticResource TitleParagraph}}" BreakPageBefore="true">
        5. CUSTOS
      </Paragraph>
      <Paragraph>
As horas necessárias para a implementação de cada requisito do projeto é apresentada por recurso (em cinza) ao lado de sua descrição.<LineBreak/>
O custo de cada item é exibido ao final de cada linha.
      </Paragraph>
      <xsl:call-template name="proposalScope">
        <xsl:with-param name="showDetail" select="0"/>
      </xsl:call-template>
      <Paragraph>

        <xsl:variable name="totalHours" select="sum(//ArrayOfBacklogItem/BacklogItem[BacklogItemUId = //ArrayOfProposalItemWithPrice/ProposalItemWithPrice/BacklogItemUId]/CurrentPlannedHours/PlannedHour/Hours)"/>
        Total de Horas do projeto:
        <xsl:value-of select="format-number($totalHours, $decimalN1, 'numberFormat')"/> horas
      </Paragraph>



      <Paragraph Style="{{StaticResource GroupParagraph}}">
        5.1 <xsl:value-of select="$_PRICE"/>
      </Paragraph>
      <xsl:call-template name="proposalPrice"/>

      <Paragraph Style="{{StaticResource GroupParagraph}}">
        5.2 <xsl:value-of select="$_PRICE_Hour_Value"/>
      </Paragraph>
      <xsl:call-template name="proposalHourCosts"/>

      <Paragraph Style="{{StaticResource TitleParagraph}}" KeepWithNext="True" BreakPageBefore="true">
        6. CRONOGRAMA e ENTREGAS
      </Paragraph>
      <xsl:call-template name="proposalSchedule" >
        <xsl:with-param name="showSprints" select="1"/>
      </xsl:call-template>

      <Paragraph Style="{{StaticResource TitleParagraph}}" KeepWithNext="True">
        7. <xsl:value-of select="$_RISKS_AND_VIABILITY"/>
      </Paragraph>
      <Paragraph KeepWithNext="True">
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
                <Paragraph >
                  <xsl:value-of select="RiskAction"/>
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


      <xsl:if test="count(//Proposal/Clauses/ProposalClause) &gt; 0">
          <Paragraph Style="{{StaticResource GroupParagraph}}">
            8. <xsl:value-of select="$_CLAUSES"/>
          </Paragraph>
          <xsl:call-template name="proposalClauses">
            <xsl:with-param name="index" select="'8'"/>
	  </xsl:call-template>
        </xsl:if>

      
     



    </FlowDocument>
  </xsl:template>



</xsl:stylesheet>
