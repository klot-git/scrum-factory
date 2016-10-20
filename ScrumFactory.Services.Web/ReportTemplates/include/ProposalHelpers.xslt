<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
    <xsl:output method="xml" indent="yes"/>

  <xsl:include href="helpers.xslt"/>
  <xsl:include href="constraintsHelpers.xslt"/>

  <xsl:variable name="proposalRoles" select="//Project/Roles/Role[PermissionSet=0 or PermissionSet=1]"/>

  <xsl:variable name="currencyRate">
    <xsl:choose>
      <xsl:when test="//Proposal/CurrencyRate">
        <xsl:value-of select="//Proposal/CurrencyRate"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="1"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:template name="proposalHourCosts">
    <Table>
      <Table.Columns>
        <TableColumn />
        <TableColumn />
      </Table.Columns>
      <TableRowGroup>
        <xsl:for-each select="$proposalRoles">
          <xsl:variable name="roleUId" select="RoleUId"/>
          <xsl:variable name="cost" select="//ArrayOfRoleHourCost/RoleHourCost[RoleUId=$roleUId]"/>
          <TableRow>
            <TableCell>
              <Paragraph>
                <xsl:value-of select="RoleName"/> (<xsl:value-of select="RoleShortName"/>)
              </Paragraph>
            </TableCell>
            <TableCell>
              <Paragraph  Style="{{StaticResource ValueParagraph}}"  TextAlignment="Right">
                <xsl:value-of select="//Proposal/CurrencySymbol"/>
                <xsl:text>&#x20;</xsl:text><xsl:value-of select="format-number($cost/Price  div $currencyRate, $moneyN, 'numberFormat')"/>
              </Paragraph>
            </TableCell>
          </TableRow>
        </xsl:for-each>
      </TableRowGroup>
    </Table>
  </xsl:template>
  
  <xsl:template name="proposalSchedule">
    <xsl:param name="showDate" select="1"/>
    <xsl:param name="showSprints" select="0"/>
    
    <Table>
      <Table.Columns>
        <TableColumn  Width="8*" />
        <TableColumn  Width="2*"/>
      </Table.Columns>
      <TableRowGroup>
        <xsl:if test="$showDate">
          <TableRow>
            <TableCell>
              <Paragraph>
                <xsl:value-of select="$_Estimated_Start_Date"/>
              </Paragraph>
            </TableCell>
            <TableCell>
              <Paragraph Style="{{StaticResource ValueParagraph}}" TextAlignment="Right">
                <xsl:call-template name="formatDate">
                  <xsl:with-param name="dateTime" select="//Proposal/EstimatedStartDate" />
                </xsl:call-template>
              </Paragraph>
            </TableCell>
          </TableRow>
          <TableRow>
            <TableCell>
              <Paragraph>
                <xsl:value-of select="$_Estimated_End_Date"/>
              </Paragraph>
            </TableCell>
            <TableCell>
              <Paragraph Style="{{StaticResource ValueParagraph}}" TextAlignment="Right">
                <xsl:call-template name="formatDate">
                  <xsl:with-param name="dateTime" select="//Proposal/EstimatedEndDate" />
                </xsl:call-template>
              </Paragraph>
            </TableCell>
          </TableRow>
        </xsl:if>
        <xsl:if test="/ReportData/workDaysCount">
          <TableRow>
            <TableCell>
              <Paragraph>
                <xsl:value-of select="$_Proposal_Work_Days_Count"/>
              </Paragraph>
            </TableCell>
            <TableCell>
              <Paragraph Style="{{StaticResource ValueParagraph}}" TextAlignment="Right">
                <xsl:value-of select="/ReportData/workDaysCount"/>&#160;<xsl:value-of select="$_days"/>
              </Paragraph>
            </TableCell>
          </TableRow>
        </xsl:if>
        <xsl:if test="$showSprints and count(/ReportData/Project/Sprints/Sprint) &gt; 0 ">
          <TableRow>
            <TableCell ColumnSpan="2">
              <Paragraph Style="{{StaticResource GroupParagraph}}">
              <xsl:value-of select="$_Delivery_dates"/>
              
              </Paragraph>
            </TableCell>
          </TableRow>
          <xsl:for-each select="/ReportData/Project/Sprints/Sprint">
            <xsl:sort data-type="number" select="SprintNumber"/>
            <xsl:variable name="sprintNumber" select="SprintNumber"/>
            <TableRow>
              <TableCell>
              <Paragraph FontWeight="Bold">
              Sprint <xsl:value-of select="SprintNumber"/>
              </Paragraph>
              <Paragraph>
                <xsl:for-each select="/ReportData/ArrayOfBacklogItem/BacklogItem[SprintNumber = $sprintNumber and OccurrenceConstraint=1 and Status &lt; 2]">                  
                  <xsl:sort select="OccurrenceConstraint" data-type="number"/>
                  <xsl:sort select="BusinessPriority" data-type="number"/>
                  <xsl:variable name="itemUId" select="BacklogItemUId"/>
                  <xsl:if test="/ReportData/ArrayOfProposalItemWithPrice/ProposalItemWithPrice[BacklogItemUId = $itemUId]">
                  <xsl:if test="position() &gt; 1">,</xsl:if><xsl:value-of select="Name"/> [<xsl:value-of select="BacklogItemNumber"/>]
                  </xsl:if>
                </xsl:for-each>
                <LineBreak/>
              </Paragraph>
            </TableCell>
            <TableCell>
              <Paragraph  Style="{{StaticResource ValueParagraph}}" TextAlignment="Right">
                <xsl:call-template name="formatDate">
                  <xsl:with-param name="dateTime" select="EndDate" />
                </xsl:call-template>                
              </Paragraph>
            </TableCell>
            
          </TableRow>
          
          </xsl:for-each>            
        </xsl:if>
  
      </TableRowGroup>
    </Table>
  </xsl:template>

  <xsl:template name="proposalScope">
    <xsl:param name="showDetail" select="1"/>
    <Table>
      <Table.Columns>        
        <TableColumn  />
        <xsl:if test="//Proposal/UseCalcPrice='true'">
          <xsl:for-each select="$proposalRoles">
            <TableColumn Width="50" />
          </xsl:for-each>
          <TableColumn Width="120" />
        </xsl:if>
      </Table.Columns>
      <TableRowGroup>

        <TableRow>
          <TableCell>
            <Paragraph></Paragraph>
          </TableCell>

          <xsl:if test="//Proposal/UseCalcPrice='true'">
            <xsl:for-each select="$proposalRoles">
              <xsl:variable name="roleUId" select="RoleUId"/>
              <TableCell>
                <Paragraph TextAlignment="Right" Style="{{StaticResource ValueParagraph}}">
                  <xsl:value-of select="RoleShortName"/>
                </Paragraph>
              </TableCell>
            </xsl:for-each>

            <TableCell>
              <Paragraph TextAlignment="Right" Style="{{StaticResource ValueParagraph}}">
                <xsl:value-of select="$_Cost"/>
              </Paragraph>
            </TableCell>
          </xsl:if>
        </TableRow>
        
        <xsl:for-each select="//ArrayOfBacklogItemGroup/BacklogItemGroup">
          <xsl:sort select="DefaultGroup" data-type="number"/>
          <xsl:sort select="GroupName"/>
          <xsl:variable name="groupUId" select="GroupUId"/>
          
          <xsl:variable name="groupItems" select="//ArrayOfProposalItemWithPrice/ProposalItemWithPrice[BacklogItemUId=//ArrayOfBacklogItem/BacklogItem[GroupUId=$groupUId]/BacklogItemUId]"/>
      
          <xsl:if test="count($groupItems) &gt; 0">
            <xsl:variable name="columnSpan">
              <xsl:choose>
                <xsl:when test="//Proposal/UseCalcPrice='true'">
                  <xsl:value-of select="count($proposalRoles) + 2"/>
                </xsl:when>
                <xsl:otherwise>1</xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <TableRow>
              <TableCell ColumnSpan="{$columnSpan}">
                <Paragraph Style="{{StaticResource ValueParagraph}}" Margin="0,10,0,0"  LineHeight="Auto">
                  <xsl:value-of select="GroupName"/>
                </Paragraph>
              </TableCell>
            </TableRow>
            <xsl:for-each select="$groupItems">
              <xsl:variable name="itemUId" select="BacklogItemUId"/>
              <xsl:variable name="item" select="//ArrayOfBacklogItem/BacklogItem[BacklogItemUId=$itemUId]"/>
              <TableRow>                
                <TableCell BorderThickness="0,0,0,1" BorderBrush="LightGray">
                  <Paragraph Margin="20,0,0,0" LineHeight="Auto">
                    <xsl:value-of select="$item/Name"/>
                    <Run Text=" "/>
                    <Run FontSize="10"> (<xsl:value-of select="$item/BacklogItemNumber"/>)</Run>                          
                  </Paragraph>
                  <xsl:if test="$item/Description and $showDetail">
                    <Paragraph Margin="20,4,0,0" FontSize="11" FontStyle="Italic"  LineHeight="Auto">
                      <xsl:call-template name="breakLines">
                        <xsl:with-param name="text" select="$item/Description" />
                      </xsl:call-template>                      
                    </Paragraph>  
                  </xsl:if>
                  
                </TableCell>

                <xsl:if test="//Proposal/UseCalcPrice='true'">
                                  
                  <xsl:for-each select="$proposalRoles">
                    <xsl:variable name="roleUId" select="RoleUId"/>                  
                    <TableCell BorderThickness="0,0,0,1" BorderBrush="LightGray">
                      <Paragraph TextAlignment="Right" Foreground="LightGray"  LineHeight="Auto">
                        <xsl:variable name="hours">
                          <xsl:choose>
                            <xsl:when test="$item/CurrentPlannedHours/PlannedHour[RoleUId=$roleUId]/Hours">                            
                              <xsl:value-of select="$item/CurrentPlannedHours/PlannedHour[RoleUId=$roleUId]/Hours"/>
                            </xsl:when>
                            <xsl:otherwise>0</xsl:otherwise>
                          </xsl:choose>
                        </xsl:variable> 
                       
                        <xsl:value-of select="format-number($hours, $decimalN1, 'numberFormat')"/>
                      
                        </Paragraph>
                    </TableCell>
                  </xsl:for-each>

                  <TableCell BorderThickness="0,0,0,1" BorderBrush="LightGray">
                    <Paragraph  TextAlignment="Right"  LineHeight="Auto">
                      <xsl:value-of select="//Proposal/CurrencySymbol"/><xsl:text>&#x20;</xsl:text><xsl:value-of select="format-number(Price div $currencyRate, $moneyN, 'numberFormat')"/>                      
                    </Paragraph>
                  </TableCell>

                </xsl:if>
              </TableRow>
            </xsl:for-each>
          </xsl:if>
          
        </xsl:for-each>


        <xsl:if test="//Proposal/UseCalcPrice='true'">
          <TableRow>
            <TableCell>
              <Paragraph></Paragraph>
            </TableCell>
            <xsl:for-each select="$proposalRoles">
              <xsl:variable name="roleUId" select="RoleUId"/>
              <TableCell>
                <Paragraph TextAlignment="Right" Foreground="LightGray">
                  
                  <xsl:variable name="proposalItems" select="//ArrayOfBacklogItem/BacklogItem[BacklogItemUId = //ArrayOfProposalItemWithPrice/ProposalItemWithPrice/BacklogItemUId]"/>

                  <xsl:variable name="totalHours" select="sum($proposalItems/CurrentPlannedHours/PlannedHour[RoleUId=$roleUId]/Hours)"/>
                  <xsl:value-of select="format-number($totalHours, $decimalN1, 'numberFormat')"/>
                </Paragraph>
              </TableCell>
            </xsl:for-each>
          </TableRow>
        </xsl:if>
        
      </TableRowGroup>
    </Table>
  </xsl:template>

  <xsl:template name="proposalScopeSimple">
    <Table>
      <Table.Columns>
        <TableColumn  />
        <xsl:for-each select="$proposalRoles">
          <TableColumn Width="50" />
        </xsl:for-each>
        <TableColumn Width="120" />
      </Table.Columns>
      <TableRowGroup>

        <TableRow>
          <TableCell>
            <Paragraph></Paragraph>
          </TableCell>

          <xsl:for-each select="$proposalRoles">
            <xsl:variable name="roleUId" select="RoleUId"/>
            <TableCell>
              <Paragraph TextAlignment="Right" Style="{{StaticResource ValueParagraph}}">
                <xsl:value-of select="RoleShortName"/>
              </Paragraph>
            </TableCell>
          </xsl:for-each>

          <TableCell>
            <Paragraph TextAlignment="Right" Style="{{StaticResource ValueParagraph}}">
              <xsl:value-of select="$_Cost"/>
            </Paragraph>
          </TableCell>
        </TableRow>
        
        <xsl:for-each select="//ArrayOfBacklogItemGroup/BacklogItemGroup">
          <xsl:sort select="DefaultGroup" data-type="number"/>
          <xsl:variable name="groupUId" select="GroupUId"/>

          <xsl:variable name="groupItems" select="//ArrayOfProposalItemWithPrice/ProposalItemWithPrice[BacklogItemUId=//ArrayOfBacklogItem/BacklogItem[GroupUId=$groupUId]/BacklogItemUId]"/>

          <xsl:if test="count($groupItems) &gt; 0">
            <TableRow>
              <TableCell  BorderThickness="0,0,0,1" BorderBrush="LightGray">
                <Paragraph Style="{{StaticResource ValueParagraph}}">
                  <xsl:value-of select="GroupName"/>
                </Paragraph>
              </TableCell>

              <xsl:for-each select="$proposalRoles">
                <xsl:variable name="roleUId" select="RoleUId"/>
                <TableCell BorderThickness="0,0,0,1" BorderBrush="LightGray">
                  <Paragraph TextAlignment="Right" Foreground="LightGray">
                    <xsl:variable name="hours">
                      <xsl:choose>
                        <xsl:when test="sum(//ArrayOfBacklogItem/BacklogItem[GroupUId=$groupUId]/CurrentPlannedHours/PlannedHour[RoleUId=$roleUId]/Hours)">
                          <xsl:value-of select="sum(//ArrayOfBacklogItem/BacklogItem[GroupUId=$groupUId]/CurrentPlannedHours/PlannedHour[RoleUId=$roleUId]/Hours)"/>
                        </xsl:when>
                        <xsl:otherwise>0</xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>

                    <xsl:value-of select="format-number($hours, $decimalN, 'numberFormat')"/>

                  </Paragraph>
                </TableCell>
              </xsl:for-each>


              <TableCell BorderThickness="0,0,0,1" BorderBrush="LightGray">
                <Paragraph  TextAlignment="Right">
                  <xsl:value-of select="//Proposal/CurrencySymbol"/>
                  <xsl:text>&#x20;</xsl:text>
                  <xsl:value-of select="format-number(sum($groupItems/Price) div $currencyRate, $moneyN, 'numberFormat')"/>
                </Paragraph>
              </TableCell>
            </TableRow>
     
          </xsl:if>

        </xsl:for-each>
      </TableRowGroup>
    </Table>
  </xsl:template>

  <xsl:template name="scopeOnly">

    
        <xsl:for-each select="//ArrayOfBacklogItemGroup/BacklogItemGroup[DefaultGroup=1]">
          <xsl:sort select="DefaultGroup" data-type="number"/>
          <xsl:sort select="GroupName"/>
          
          <xsl:variable name="groupUId" select="GroupUId"/>
          <xsl:variable name="groupColor" select="GroupColor"/>

          
          <xsl:variable name="groupItems" select="//ArrayOfProposalItemWithPrice/ProposalItemWithPrice[BacklogItemUId=//ArrayOfBacklogItem/BacklogItem[GroupUId=$groupUId]/BacklogItemUId]"/>

          <xsl:if test="count($groupItems) &gt; 0">
            
            
              <Paragraph Style="{{StaticResource GroupParagraph}}" Margin="0,20,0,10">
                <InlineUIContainer>
                  <Border Background="{$groupColor}" Width="20" Height="18" Margin="5,0,5,-3" BorderThickness="1" BorderBrush="Black"></Border>
                </InlineUIContainer>
                <xsl:value-of select="GroupName"/>
              </Paragraph>
            
            <xsl:for-each select="$groupItems">              
              <xsl:variable name="itemUId" select="BacklogItemUId"/>
              <xsl:variable name="item" select="//ArrayOfBacklogItem/BacklogItem[BacklogItemUId=$itemUId]"/>
              
              <xsl:if test="$item/Description">
              
                  <Paragraph Margin="30,0,0,0" FontWeight="Bold" FontSize="16">

                    <xsl:value-of select="$item/Name"/>
                    <Run Text=" "/>
                    <Run FontSize="10">
                      (<xsl:value-of select="$item/BacklogItemNumber"/>)
                    </Run>
                  </Paragraph>
                  
                    <Paragraph Margin="30,0,0,15" LineHeight="Auto">
                      <xsl:call-template name="breakLines">
                        <xsl:with-param name="text" select="$item/Description" />
                      </xsl:call-template>
                    </Paragraph>
              </xsl:if>
                  
            </xsl:for-each>
          </xsl:if>

        </xsl:for-each>


  </xsl:template>
  
  <xsl:template name="proposalClauses">
    <xsl:param name="index" select="'7'"/>
    <xsl:for-each select="//Proposal/Clauses/ProposalClause">
    <xsl:sort select="ClauseOrder" data-type="number"/>
      <Paragraph Margin="0,0,0,10">
        <TextBlock FontWeight="Bold">
          <xsl:value-of select="$index"/>.<xsl:value-of select="ClauseOrder"/><xsl:text>&#x20;</xsl:text><xsl:value-of select="ClauseName"/>
        </TextBlock>
        <LineBreak/>
        <xsl:call-template name="breakLines">
          <xsl:with-param name="text" select="ClauseText" />
        </xsl:call-template>        
      </Paragraph>
    </xsl:for-each>      
  </xsl:template>

  <xsl:template name="proposalPrice">


    <Table>
      <Table.Columns>
        <TableColumn />
        <TableColumn />
        <TableColumn />
      </Table.Columns>
      <TableRowGroup>
        <xsl:if test="//Proposal/UseCalcPrice = 'true'">        
          <TableRow>
            <TableCell>
              <Paragraph>
                <xsl:value-of select="$_Scope_Price"/>                
              </Paragraph>
            </TableCell>
            <TableCell>
              <Paragraph  TextAlignment="Right">
                <xsl:value-of select="//Proposal/CurrencySymbol"/>
                <xsl:text>&#x20;</xsl:text>
                <xsl:value-of select="format-number(sum(//ArrayOfProposalItemWithPrice/ProposalItemWithPrice/Price) div $currencyRate, $moneyN, 'numberFormat')"/>
              </Paragraph>
            </TableCell>
          </TableRow>
          <xsl:for-each select="//Proposal/FixedCosts/ProposalFixedCost[RepassToClient = 'true']">
            <xsl:sort select="CostDescription" data-type="text"/>
            <TableRow>
              <TableCell>
                <Paragraph>
                  <xsl:value-of select="CostDescription"/>
                </Paragraph>
              </TableCell>
              <TableCell>
                <Paragraph TextAlignment="Right">
                  <xsl:value-of select="//Proposal/CurrencySymbol"/>
                  <xsl:text>&#x20;</xsl:text>
                  <xsl:value-of select="format-number(Cost div $currencyRate, $moneyN, 'numberFormat')"/>
                </Paragraph>
              </TableCell>
            </TableRow>
          </xsl:for-each>
          <xsl:if test="//Proposal/Discount &gt; 0">                  
          <TableRow>
            <TableCell>
              <Paragraph>
                <xsl:value-of select="$_Discount"/>                
              </Paragraph>
            </TableCell>
            <TableCell>
              <Paragraph Style="{{StaticResource ValueParagraph}}" TextAlignment="Right">              
                - <xsl:value-of select="format-number(//Proposal/Discount, $decimalN, 'numberFormat')"/> %
              </Paragraph>
            </TableCell>
          </TableRow>
          </xsl:if>
        </xsl:if>
        <TableRow>
          <TableCell BorderThickness="0,1,0,0" BorderBrush="Black">
            <Paragraph>
              <xsl:value-of select="$_Total_Price"/>              
            </Paragraph>
          </TableCell>
          <TableCell BorderThickness="0,1,0,0" BorderBrush="Black">
            <Paragraph Style="{{StaticResource ValueParagraph}}" TextAlignment="Right">
              <xsl:value-of select="//Proposal/CurrencySymbol"/>
              <xsl:text>&#x20;</xsl:text>              
              <xsl:value-of select="format-number(//Proposal/TotalValue div $currencyRate, $moneyN, 'numberFormat')"/>
            </Paragraph>
          </TableCell>
        </TableRow>
      </TableRowGroup>
    </Table>

  </xsl:template>


</xsl:stylesheet>
