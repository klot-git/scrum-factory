<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/xps/2005/06/resourcedictionary-key">

  <xsl:template name="formatDate">
    <xsl:param name="dateTime" />
    <xsl:variable name="date" select="substring-before($dateTime, 'T')" />
    <xsl:variable name="year" select="substring-before($date, '-')" />
    <xsl:variable name="month" select="substring-before(substring-after($date, '-'), '-')" />
    <xsl:variable name="day" select="substring-after(substring-after($date, '-'), '-')" />
    <xsl:value-of select="concat($day, '.', $month, '.', $year)" />
  </xsl:template>

  <xsl:template name="formatShortDate">
    <xsl:param name="dateTime" />
    <xsl:variable name="date" select="substring-before($dateTime, 'T')" />
    <xsl:variable name="year" select="substring-before($date, '-')" />
    <xsl:variable name="month" select="substring-before(substring-after($date, '-'), '-')" />
    <xsl:variable name="monthName">
      <xsl:choose>
        <xsl:when test="$month = '01'">
          <xsl:value-of select="'jan'"/>
        </xsl:when>
        <xsl:when test="$month = '02'">
          <xsl:value-of select="'fev'"/>
        </xsl:when>
        <xsl:when test="$month = '03'">
          <xsl:value-of select="'mar'"/>
        </xsl:when>
        <xsl:when test="$month = '04'">
          <xsl:value-of select="'abr'"/>
        </xsl:when>
        <xsl:when test="$month = '05'">
          <xsl:value-of select="'maio'"/>
        </xsl:when>
        <xsl:when test="$month = '06'">
          <xsl:value-of select="'jun'"/>
        </xsl:when>
        <xsl:when test="$month = '07'">
          <xsl:value-of select="'jul'"/>
        </xsl:when>
        <xsl:when test="$month = '08'">
          <xsl:value-of select="'ago'"/>
        </xsl:when>
        <xsl:when test="$month = '09'">
          <xsl:value-of select="'set'"/>
        </xsl:when>
        <xsl:when test="$month = '10'">
          <xsl:value-of select="'out'"/>
        </xsl:when>
        <xsl:when test="$month = '11'">
          <xsl:value-of select="'nov'"/>
        </xsl:when>
        <xsl:when test="$month = '12'">
          <xsl:value-of select="'dez'"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="day" select="substring-after(substring-after($date, '-'), '-')" />
    <xsl:value-of select="concat($day, ', ', $monthName)" />
  </xsl:template>

  <xsl:decimal-format name="numberFormat" decimal-separator="," grouping-separator="."/>

  <xsl:variable name="moneyN" select="'###.##0,00'" />
  <xsl:variable name="decimalN" select="'0,00'" />
  <xsl:variable name="decimalN1" select="'0,0'" />

  <xsl:variable name="_PLANNED" select="'PLANEJADO'" />
  <xsl:variable name="_WORKING_ON" select="'EM ANDAMENTO'" />
  <xsl:variable name="_DONE" select="'FEITO'" />
  <xsl:variable name="_CANCELED" select="'CANCELADO'" />

  <xsl:variable name="_PROJECT_SCHEDULE" select="'CRONOGRAMA DO PROJETO'" />
  <xsl:variable name="_DELIVERED_ITEMS" select="'ITENS ENTREGUES'" />
  <xsl:variable name="_Items_started_at" select="'Itens entregues em '" />
  <xsl:variable name="_SCHEDULED_ITEMS" select="'ITENS PLANEJADOS'" />
  <xsl:variable name="_Items_planned_to_start_at" select="'Itens planejados para iniciar em '" />

  <xsl:variable name="_NONE" select="'NENHUMA'" />
  <xsl:variable name="_LOW" select="'BAIXO'" />
  <xsl:variable name="_MEDIUM" select="'MÉDIO'" />
  <xsl:variable name="_HIGH" select="'ALTO'" />

  <xsl:variable name="_REVIEW" select="'REVISÃO'" />
  <xsl:variable name="_PROJECT_BURNDOWN" select="'BURNDOWN DO PROJETO'" />
  <xsl:variable name="_Project_scheduled_to" select="'Projeto planejado para:'" />
  <xsl:variable name="_RISKS" select="'RISCOS'" />
  <xsl:variable name="_Prob" select="'Prob.'" />
  <xsl:variable name="_Impact" select="'Impacto'" />
  <xsl:variable name="_Risk" select="'Riscos'" />
  <xsl:variable name="_No_risks_were_identified" select="'Nenhum risco identificado.'" />
  <xsl:variable name="_PREVIOUS_ITEMS" select="'ITENS ANTERIORES'" />
  <xsl:variable name="_NEXT_SPRINT_ITEMS" select="'ITENS DA PRÓXIMA SPRINT'" />

  <xsl:variable name="_PROJECT_DESCRIPTION" select="'DESCRIÇÃO DO PROJETO'" />
  <xsl:variable name="_TECHNOLOGY_and_PLATFORM" select="'TECNOLOGIA E PLATAFORMA'" />
  <xsl:variable name="_SCOPE" select="'ESCOPO'" />
  <xsl:variable name="_PRICE" select="'PREÇO'" />
  <xsl:variable name="_PRICE_Hour_Value" select="'PREÇO - Valor hora'" />
  <xsl:variable name="_DEADLINE" select="'PRAZO'" />
  <xsl:variable name="_STAKEHOLDERS" select="'ENVOLVIDOS'" />
  <xsl:variable name="_CLAUSES" select="'CLÁUSULAS'" />

  <xsl:variable name="_Total_Price" select="'Preço total'" />
  <xsl:variable name="_Discount" select="'Desconto'" />
  <xsl:variable name="_Scope_Price" select="'Preço do escopo'" />
  <xsl:variable name="_Estimated_End_Date" select="'Data final planejada'" />
  <xsl:variable name="_Estimated_Start_Date" select="'Data inicial planejada'" />
  <xsl:variable name="_Proposal_Work_Days_Count" select="'Dias úteis'" />
  <xsl:variable name="_days" select="'dias'" />

  <xsl:variable name="_Item" select="'Item'" />
  <xsl:variable name="_Cost" select="'Custo'" />

  <xsl:variable name="_Project_start" select="'Ínicio do projeto: '" />
  <xsl:variable name="_Delivery_item" select="'entrega do projeto.'" />
  <xsl:variable name="_Critical_item" select="'entrega crítica. Essa entrega compromete a data de todas as outras entregas do projeto.'" />


  <!-- PROJECT GUIDE -->

  <xsl:variable name="_and_" select="' e '" />

  <xsl:variable name="_PROJECT_GUIDE" select="'GUIA DO PROJETO'" />
  <xsl:variable name="_THIS_GUIDE" select="'ESSE GUIA'" />
  <xsl:variable name="_THIS_GUIDE_text" select="'Esse guia contém orientações quanto a condução do projeto, responsabilidades, escopo, datas de entrega e riscos. Deve ser utilizado como fonte de referência para o Scrum Master e para sua equipe.'" />
  <xsl:variable name="_PROJECT_LIFE_CYCLE" select="'CICLO DE VIDA DO PROJETO'" />
  <xsl:variable name="_PROJECT_LIFE_CYCLE_text" select="'Esse projeto será conduzido segundo o modelo iterativo “SF Fast Project  1.0” baseado em práticas agéis de desenvolvimento. Para mais detalhes quanto ao modelo verifique o endereço:'" />
  <xsl:variable name="_PROJECT_LIFE_CYCLE_url" select="'http://www.scrum-factory.com/doc/lifecycles/fp10'" />
  <xsl:variable name="_PEOPLE_AND_COMMUNICATION" select="'PESSOAS E COMUNICAÇÃO'" />
  <xsl:variable name="_PEOPLE_AND_COMMUNICATION_text1" select="'As seguinte pessoas estão envolvidas nesse projeto.'" />
  <xsl:variable name="_PEOPLE_AND_COMMUNICATION_text2" select="'Todas as comunicações e decisões relevantes do projeto devem ser registradas através de e-mails ou atas de reuniões.'" />
  <xsl:variable name="_PEOPLE_AND_COMMUNICATION_text3" select="' devem ser copiados/estarem presentes nessas comunicações.'" />

  <xsl:variable name="_DATA_MANAGEMENT" select="'GERENCIAMENTO DE DADOS'" />
  <xsl:variable name="_DATA_MANAGEMENT_text" select="'Todos os dados/arquivos do projeto são armazendados respeitando os níveis de segurança adequados e não estarão disponíveis para pessoas não envolvidas no projeto.'" />
  <xsl:variable name="_DATA_MANAGEMENT_codeFolder" select="'Repositório de código'" />
  <xsl:variable name="_DATA_MANAGEMENT_docFolder" select="'Repositório de documentos'" />

  <xsl:variable name="_PLATFORM" select="'PLATAFORMA'" />

  <xsl:variable name="_IMPORTANT_DATES" select="'DATAS IMPORTANTES'" />
  <xsl:variable name="_IMPORTANT_DATES_projectStart" select="'Ínicio do projeto'" />
  <xsl:variable name="_IMPORTANT_DATES_projectEnd" select="'Final do projeto'" />
  <xsl:variable name="_IMPORTANT_DATES_text" select="'Informações mais detalhadas referente a datas e entregas do projeto podem ser encontradas no cronograma do projeto.'" />

  <xsl:variable name="_RISKS_AND_VIABILITY" select="'RISCOS E VIABILIDADE'" />
  <xsl:variable name="_RISKS_AND_VIABILITY_text" select="'Esse projeto foi considerado viável depois de realizada uma análise de viabilidade e riscos. Os riscos identificados, bem como suas probabilidade e impactos estão listados abaixo.'" />

  <xsl:variable name="_GUIDE_SCOPE" select="'ESCOPO'" />
  <xsl:variable name="_GUIDE_SCOPE_text" select="'O escopo do projeto está representado no WBS (Work Breakdown Structure) abaixo.'" />
  <xsl:variable name="_GUIDE_SCOPE_platform" select="' será utilizado como plataforma de desenvolvimento desse projeto.'" />



  <xsl:variable name="_PROJECT_INDICATORS" select="'INDICADORES'" />
  <xsl:variable name="_WORKED_HOURS" select="'HORAS TRABALHADAS'" />
  <xsl:variable name="_budget_indicator" select="'orçamento'" />
  <xsl:variable name="_quality_indicator" select="'qualidade'" />
  <xsl:variable name="_velocity_indicator" select="'velocidade'" />

    <xsl:variable name="_STRUCTURES" select="'ESTRUTURAS'" />

  <xsl:variable name="_CONSTRAINTS" select="'PREMISSAS'" />

  <xsl:variable name="_Total_project_hours" select="'Total de horas do projeto:'"/>

  <xsl:variable name="_hours" select="'horas'"/>

  <xsl:variable name="_Delivery_dates" select="'Data das entregas'"/>

  <xsl:variable name="_FUNCTIONAL_REQUIREMENTS" select="'REQUISITOS FUNCIONAIS'"/>
  <xsl:variable name="_FUNCTIONAL_REQUIREMENTS_tooltip" select="'Os requisitos funcionais que compõem o escopo do projeto são apresentados abaixo e identificados por seu respectivo número entre parênteses.'"/>
  <xsl:variable name="_NON_FUNCTIONAL_REQUIREMENTS" select="'REQUISITOS NÃO FUNCIONAIS E PREMISSAS'"/>


</xsl:stylesheet>
