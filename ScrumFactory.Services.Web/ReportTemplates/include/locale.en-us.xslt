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
    <xsl:value-of select="concat($month, '.', $day, '.', $year)" />
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
          <xsl:value-of select="'feb'"/>
        </xsl:when>
        <xsl:when test="$month = '03'">
          <xsl:value-of select="'mar'"/>
        </xsl:when>
        <xsl:when test="$month = '04'">
          <xsl:value-of select="'abr'"/>
        </xsl:when>
        <xsl:when test="$month = '05'">
          <xsl:value-of select="'may'"/>
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
          <xsl:value-of select="'sep'"/>
        </xsl:when>
        <xsl:when test="$month = '10'">
          <xsl:value-of select="'oct'"/>
        </xsl:when>
        <xsl:when test="$month = '11'">
          <xsl:value-of select="'nov'"/>
        </xsl:when>
        <xsl:when test="$month = '12'">
          <xsl:value-of select="'dec'"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="day" select="substring-after(substring-after($date, '-'), '-')" />
    <xsl:value-of select="concat($monthName, ', ', $day)" />
  </xsl:template>

  <xsl:decimal-format name="numberFormat" decimal-separator="." grouping-separator=","/>


  <xsl:variable name="moneyN" select="'###,##0.00'" />
  <xsl:variable name="decimalN" select="'0.00'" />
  <xsl:variable name="decimalN1" select="'0.0'" />


  <xsl:variable name="_PLANNED" select="'PLANNED'" />
  <xsl:variable name="_WORKING_ON" select="'WORKING ON'" />
  <xsl:variable name="_DONE" select="'DONE'" />
  <xsl:variable name="_CANCELED" select="'CANCELED'" />

  <xsl:variable name="_PROJECT_SCHEDULE" select="'PROJECT SCHEDULE'" />
  <xsl:variable name="_DELIVERED_ITEMS" select="'DELIVERED ITEMS'" />
  <xsl:variable name="_Items_started_at" select="'Items started at '" />
  <xsl:variable name="_SCHEDULED_ITEMS" select="'SCHEDULED ITEMS'" />
  <xsl:variable name="_Items_planned_to_start_at" select="'Items planned to start at '" />

  <xsl:variable name="_NONE" select="'NONE'" />
  <xsl:variable name="_LOW" select="'LOW'" />
  <xsl:variable name="_MEDIUM" select="'MEDIUM'" />
  <xsl:variable name="_HIGH" select="'HIGH'" />

  <xsl:variable name="_REVIEW" select="'REVIEW'" />
  <xsl:variable name="_PROJECT_BURNDOWN" select="'PROJECT BURNDOWN'" />
  <xsl:variable name="_Project_scheduled_to" select="'Project scheduled to:'" />
  <xsl:variable name="_RISKS" select="'RISKS'" />
  <xsl:variable name="_Prob" select="'Prob.'" />
  <xsl:variable name="_Impact" select="'Impact'" />
  <xsl:variable name="_Risk" select="'Risk'" />
  <xsl:variable name="_No_risks_were_identified" select="'No risks were identified.'" />
  <xsl:variable name="_PREVIOUS_ITEMS" select="'PREVIOUS ITEMS'" />
  <xsl:variable name="_NEXT_SPRINT_ITEMS" select="'NEXT SPRINT ITEMS'" />

  <xsl:variable name="_PROJECT_DESCRIPTION" select="'PROJECT DESCRIPTION'" />
  <xsl:variable name="_TECHNOLOGY_and_PLATFORM" select="'TECHNOLOGY and PLATFORM'" />
  <xsl:variable name="_SCOPE" select="'SCOPE'" />
  <xsl:variable name="_PRICE" select="'PRICE'" />
  <xsl:variable name="_PRICE_Hour_Value" select="'PRICE - Hour Value'" />
  <xsl:variable name="_DEADLINE" select="'DEADLINE'" />
  <xsl:variable name="_STAKEHOLDERS" select="'STAKEHOLDERS'" />
  <xsl:variable name="_CLAUSES" select="'CLAUSES'" />

  <xsl:variable name="_Total_Price" select="'Total Price'" />
  <xsl:variable name="_Discount" select="'Discount'" />
  <xsl:variable name="_Scope_Price" select="'Scope Price'" />
  <xsl:variable name="_Estimated_End_Date" select="'Estimated End Date'" />
  <xsl:variable name="_Estimated_Start_Date" select="'Estimated Start Date'" />
  <xsl:variable name="_Proposal_Work_Days_Count" select="'Work days'" />
  <xsl:variable name="_days" select="'days'" />

  <xsl:variable name="_Item" select="'Item'" />
  <xsl:variable name="_Cost" select="'Cost'" />

  
  <!-- EMAILS -->
  <xsl:variable name="_The_project" select="'The project '" />
  <xsl:variable name="_has_started" select="'has started!!'" />

  <xsl:variable name="_Project_start" select="'Project start: '" />
  <xsl:variable name="_Delivery_item" select="'project delivery.'" />
  <xsl:variable name="_Critical_item" select="'critical delivery. This delivery compromises all other project deliveries dates.'" />

  <!-- PROJECT GUIDE -->

  <xsl:variable name="_and_" select="' and '" />

  <xsl:variable name="_PROJECT_GUIDE" select="'PROJECT GUIDE'" />
  <xsl:variable name="_THIS_GUIDE" select="'THIS GUIDE'" />
  <xsl:variable name="_THIS_GUIDE_text" select="'This guide provides information regarding how this Project will be conducted, the people involved and their roles, the project scope, deliveries deadlines and risks. It should be used as a source of reference for the Scrum Master and the team.'" />
  <xsl:variable name="_PROJECT_LIFE_CYCLE" select="'PROJECT LIFE CYCLE'" />
  <xsl:variable name="_PROJECT_LIFE_CYCLE_text" select="'This project will be conducted according the “SF Fast Project  1.0” life cycle. It’s a based at  iterative agile model. For detail information about the cycle, please refer the following url:'" />
  <xsl:variable name="_PROJECT_LIFE_CYCLE_url" select="'http://www.scrum-factory.com/doc/lifecycles/fp10'" />
  <xsl:variable name="_PEOPLE_AND_COMMUNICATION" select="'PEOPLE AND COMMUNICATION'" />
  <xsl:variable name="_PEOPLE_AND_COMMUNICATION_text1" select="'The following people are involved at this project.'" />
  <xsl:variable name="_PEOPLE_AND_COMMUNICATION_text2" select="'All project communications and decisions should be recorded using e-mail or meeting reports.'" />
  <xsl:variable name="_PEOPLE_AND_COMMUNICATION_text3" select="' should be copied/present at those communications.'" />

  <xsl:variable name="_DATA_MANAGEMENT" select="'DATA MANAGEMENT'" />
  <xsl:variable name="_DATA_MANAGEMENT_text" select="'All project data are stored respecting security levels of access and are not available for others not involved at the project.'" />
  <xsl:variable name="_DATA_MANAGEMENT_codeFolder" select="'Code folder'" />
  <xsl:variable name="_DATA_MANAGEMENT_docFolder" select="'Documents folder'" />

  <xsl:variable name="_PLATFORM" select="'PLATFORM'" />

  <xsl:variable name="_IMPORTANT_DATES" select="'IMPORTANT DATES'" />
  <xsl:variable name="_IMPORTANT_DATES_projectStart" select="'Project Start'" />
  <xsl:variable name="_IMPORTANT_DATES_projectEnd" select="'Project End'" />
  <xsl:variable name="_IMPORTANT_DATES_text" select="'More detailed information regarding the project dates and deliveries could be found at the project timetable document.'" />

  <xsl:variable name="_RISKS_AND_VIABILITY" select="'RISKS AND VIABILITY'" />
  <xsl:variable name="_RISKS_AND_VIABILITY_text" select="'The project was considered viable after a risk and viability analysis. The major risks, its impacts and probability are listed bellow.'" />

  <xsl:variable name="_GUIDE_SCOPE" select="'SCOPE'" />
  <xsl:variable name="_GUIDE_SCOPE_text" select="'The project scope is described at the following WBS (Work Breakdown Structure).'" />
  <xsl:variable name="_GUIDE_SCOPE_platform" select="' will be used as platform do develop the project.'" />



  <xsl:variable name="_PROJECT_INDICATORS" select="'PROJECT INDICATORS'" />
  <xsl:variable name="_WORKED_HOURS" select="'WORKED HOURS'" />
  <xsl:variable name="_budget_indicator" select="'budget'" />
  <xsl:variable name="_quality_indicator" select="'quality'" />
  <xsl:variable name="_velocity_indicator" select="'speed'" />

    <xsl:variable name="_STRUCTURES" select="'STRUCTURES'" />

  <xsl:variable name="_CONSTRAINTS" select="'CONSTRAINTS'" />


  <xsl:variable name="_Total_project_hours" select="'Total project hours:'"/>

  <xsl:variable name="_hours" select="'hours'"/>

  <xsl:variable name="_Delivery_dates" select="'Delivery dates'"/>

  <xsl:variable name="_FUNCTIONAL_REQUIREMENTS" select="'FUNCTIONAL REQUIREMENTS'"/>
  <xsl:variable name="_FUNCTIONAL_REQUIREMENTS_tooltip" select="''"/>
  <xsl:variable name="_NON_FUNCTIONAL_REQUIREMENTS" select="'NON_FUNCTIONAL REQUIREMENTS'"/>

  <xsl:variable name="_GRAPH_Hours" select="'Hours to finish'" />
  <xsl:variable name="_GRAPH_Actual_hours" select="'Actual'" />
  <xsl:variable name="_GRAPH_Planned_hours" select="'Planned'" />
  <xsl:variable name="_GRAPH_DateBindShort" select="'{Binding StringFormat=MMM dd}'" />
  <xsl:variable name="_GRAPH_walked" select="'% walked'" />
  <xsl:variable name="_GRAPH_hrs_ahead" select="'hr(s) ahead'" />
  <xsl:variable name="_GRAPH_hrs_late" select="'hr(s) late'" />

</xsl:stylesheet>
