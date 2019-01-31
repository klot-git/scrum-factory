<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/xps/2005/06/resourcedictionary-key">
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
          <xsl:sort select="GroupOrder" data-type="number"/>
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
              <xsl:sort select="Item/SprintNumber" data-type="number"/>
              <xsl:sort select="Item/BusinessPriority" data-type="number"/>
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
                      <Paragraph TextAlignment="Right" Foreground="Gray"  LineHeight="Auto">
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
                <Paragraph TextAlignment="Right" Foreground="Gray">
                  
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

  <xsl:template name="proposalScopeNoValues">
    <xsl:param name="showDetail" select="1"/>
    <Table>
      <Table.Columns>
        <TableColumn  />        
        <xsl:for-each select="$proposalRoles">
          <TableColumn Width="50" />
        </xsl:for-each>        
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

        </TableRow>

        <xsl:for-each select="//ArrayOfBacklogItemGroup/BacklogItemGroup">
          <xsl:sort select="DefaultGroup" data-type="number"/>
          <xsl:sort select="GroupOrder" data-type="number"/>
          <xsl:sort select="GroupName"/>
          <xsl:variable name="groupUId" select="GroupUId"/>

          <xsl:variable name="groupItems" select="//ArrayOfProposalItemWithPrice/ProposalItemWithPrice[BacklogItemUId=//ArrayOfBacklogItem/BacklogItem[GroupUId=$groupUId]/BacklogItemUId]"/>

          <xsl:if test="count($groupItems) &gt; 0">
            <xsl:variable name="columnSpan">
              <xsl:choose>
                <xsl:when test="//Proposal/UseCalcPrice='true'">
                  <xsl:value-of select="count($proposalRoles) + 1"/>
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
              <xsl:sort select="Item/SprintNumber" data-type="number"/>
              <xsl:sort select="Item/BusinessPriority" data-type="number"/>
              <xsl:variable name="itemUId" select="BacklogItemUId"/>
              <xsl:variable name="item" select="//ArrayOfBacklogItem/BacklogItem[BacklogItemUId=$itemUId]"/>
              <TableRow>
                <TableCell BorderThickness="0,0,0,1" BorderBrush="LightGray">
                  <Paragraph Margin="20,0,0,0" LineHeight="Auto">
                    <xsl:value-of select="$item/Name"/>
                    <Run Text=" "/>
                    <Run FontSize="10">
                      (<xsl:value-of select="$item/BacklogItemNumber"/>)
                    </Run>
                  </Paragraph>
                  <xsl:if test="$item/Description and $showDetail">
                    <Paragraph Margin="20,4,0,0" FontSize="11" FontStyle="Italic"  LineHeight="Auto">
                      <xsl:call-template name="breakLines">
                        <xsl:with-param name="text" select="$item/Description" />
                      </xsl:call-template>
                    </Paragraph>
                  </xsl:if>

                </TableCell>


                  <xsl:for-each select="$proposalRoles">
                    <xsl:variable name="roleUId" select="RoleUId"/>
                    <TableCell BorderThickness="0,0,0,1" BorderBrush="LightGray">
                      <Paragraph TextAlignment="Right" Foreground="Gray"  LineHeight="Auto">
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


              </TableRow>
            </xsl:for-each>
          </xsl:if>

        </xsl:for-each>

          <TableRow>
            <TableCell>
              <Paragraph></Paragraph>
            </TableCell>
            <xsl:for-each select="$proposalRoles">
              <xsl:variable name="roleUId" select="RoleUId"/>
              <TableCell>
                <Paragraph TextAlignment="Right" Foreground="Gray">

                  <xsl:variable name="proposalItems" select="//ArrayOfBacklogItem/BacklogItem[BacklogItemUId = //ArrayOfProposalItemWithPrice/ProposalItemWithPrice/BacklogItemUId]"/>

                  <xsl:variable name="totalHours" select="sum($proposalItems/CurrentPlannedHours/PlannedHour[RoleUId=$roleUId]/Hours)"/>
                  <xsl:value-of select="format-number($totalHours, $decimalN1, 'numberFormat')"/>
                </Paragraph>
              </TableCell>
            </xsl:for-each>
          </TableRow>


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
          <xsl:sort select="GroupOrder" data-type="number"/>
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
              <xsl:sort select="Item/SprintNumber" data-type="number"/>
              <xsl:sort select="Item/BusinessPriority" data-type="number"/>       
              <xsl:variable name="itemUId" select="BacklogItemUId"/>
              <xsl:variable name="item" select="//ArrayOfBacklogItem/BacklogItem[BacklogItemUId=$itemUId]"/>
              
              <xsl:if test="$item/Description">
              
                  <Paragraph Margin="30,0,0,0" FontWeight="Bold" FontSize="16" LineHeight="24">

                    <xsl:value-of select="$item/Name"/>
                    <Run Text=" "/>
                    <Run FontSize="10">
                      (<xsl:value-of select="$item/BacklogItemNumber"/>)
                    </Run>
                  </Paragraph>
                  
                    <Paragraph Margin="30,0,0,15" LineHeight="22">
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
      <Paragraph Style="{{StaticResource GroupParagraph}}">        
          <xsl:value-of select="$index"/>.<xsl:value-of select="ClauseOrder"/><xsl:text>&#x20;</xsl:text><xsl:value-of select="ClauseName"/>        
        </Paragraph>
        <Paragraph>
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

  <xsl:template name="proposalPrice2">
    <Table>
      <Table.Columns>
        <TableColumn />
        <TableColumn />
        <TableColumn />
      </Table.Columns>
      <TableRowGroup>
        <xsl:if test="//Proposal/UseCalcPrice = 'true'">

          <xsl:for-each select="//ArrayOfBacklogItemGroup/BacklogItemGroup">
            <xsl:sort select="DefaultGroup" data-type="number"/>
            <xsl:sort select="GroupOrder" data-type="number"/>
            <xsl:sort select="GroupName"/>
            <xsl:variable name="groupUId" select="GroupUId"/>
            <xsl:variable name="groupName" select="GroupName"/>

            <xsl:variable name="groupValue" select="sum(//ArrayOfProposalItemWithPrice/ProposalItemWithPrice[BacklogItemUId=//ArrayOfBacklogItem/BacklogItem[GroupUId=$groupUId]/BacklogItemUId]/Price)"/>

            <TableRow>
              <TableCell>
                <Paragraph>
                  <xsl:value-of select="$groupName"/>
                </Paragraph>
              </TableCell>
              <TableCell>
                <Paragraph  TextAlignment="Right">
                  <xsl:value-of select="//Proposal/CurrencySymbol"/>
                  <xsl:text>&#x20;</xsl:text>
                  <xsl:value-of select="format-number($groupValue div $currencyRate, $moneyN, 'numberFormat')"/>
                </Paragraph>
              </TableCell>
            </TableRow>
          </xsl:for-each>
          
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


  <xsl:template name="bar">
    <xsl:param name="isFirst"/>
    <xsl:param name="isLast"/>
    <xsl:param name="status"/>
    <xsl:param name="hours"/>
    <xsl:param name="constraint"/>
    <xsl:variable name="radius">
      <xsl:choose>
        <xsl:when test="$isFirst and $isLast">
          <xsl:value-of select="'4,4,4,4'"/>
        </xsl:when>
        <xsl:when test="$isFirst">
          <xsl:value-of select="'4,0,0,4'"/>
        </xsl:when>
        <xsl:when test="$isLast">
          <xsl:value-of select="'0,4,4,0'"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'0,0,0,0'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="thick">
      <xsl:choose>
        <xsl:when test="$isFirst and $isLast">
          <xsl:value-of select="'1'"/>
        </xsl:when>
        <xsl:when test="$isFirst">
          <xsl:value-of select="'1,1,0,1'"/>
        </xsl:when>
        <xsl:when test="$isLast">
          <xsl:value-of select="'0,1,1,1'"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'0,1,0,1'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="borderColor">
      <xsl:choose>
        <xsl:when test="$status = 2">
          <xsl:value-of select="'Green'"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'Gray'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="color">
      <xsl:choose>
        <xsl:when test="$status = 2">
          <xsl:value-of select="'DarkGreen'"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'Gray'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="align">
      <xsl:choose>
        <xsl:when test="$constraint = 0">
          <xsl:value-of select="'Left'"/>
        </xsl:when>
        <xsl:when test="$constraint = 2">
          <xsl:value-of select="'Right'"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'Stretch'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="textColor">
      <xsl:choose>
        <xsl:when test="$isLast and $status != 2">
          <xsl:value-of select="'White'"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$color"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:if test="$hours">
      <BlockUIContainer>
        <Border HorizontalAlignment="{$align}" VerticalAlignment="Center" Height="12" BorderBrush="{$borderColor}" Background="{$color}" BorderThickness="0" CornerRadius="{$radius}">
          <TextBlock Foreground="{$textColor}" FontSize="6" HorizontalAlignment="Right" VerticalAlignment="Center" Margin="4,0,4,0">
            <xsl:value-of select="$hours"/> hrs
          </TextBlock>
        </Border>
      </BlockUIContainer>
    </xsl:if>
  </xsl:template>

  <xsl:template name="timeLine">
    <Table BorderThickness="0" BorderBrush="#000000" FontSize="8">
      <Table.Resources>
        <Style TargetType="{{x:Type Paragraph}}">
          <Setter Property="FontSize" Value="8"/>
          <Setter Property="Foreground" Value="Black"/>
          <Setter Property="FontFamily" Value="Calibri"/>
          <Setter Property="Margin" Value="0"/>
          <Setter Property="LineHeight" Value="12" />
        </Style>
        <Style TargetType="{{x:Type TableCell}}">
          <Setter Property="Padding" Value="3"/>
          <Setter Property="BorderThickness" Value="0,0,0,1"/>
          <Setter Property="BorderBrush" Value="Gray"/>
          <Setter Property="LineHeight" Value="12" />
        </Style>
        <Style x:Key="headerCell" TargetType="{{x:Type TableCell}}">
          <Setter Property="Padding" Value="0,3,0,3"/>
          <Setter Property="BorderThickness" Value="0,0,0,2"/>
          <Setter Property="BorderBrush" Value="Black"/>
          <Setter Property="LineHeight" Value="12" />
        </Style>
        <Style x:Key="barCell" TargetType="{{x:Type TableCell}}">
          <Setter Property="Padding" Value="0,3,0,3"/>
          <Setter Property="BorderThickness" Value="0,0,0,1"/>
          <Setter Property="BorderBrush" Value="Gray"/>
          <Setter Property="LineHeight" Value="12" />
        </Style>
        <Style x:Key="sprintBarCell" TargetType="{{x:Type TableCell}}">
          <Setter Property="Padding" Value="0,3,0,3"/>
          <Setter Property="BorderThickness" Value="0,0,0,2"/>
          <Setter Property="BorderBrush" Value="Black"/>
          <Setter Property="LineHeight" Value="12" />
        </Style>
        <Style x:Key="sprintHeaderCell" TargetType="{{x:Type TableCell}}">
          <Setter Property="Padding" Value="6"/>
          <Setter Property="BorderThickness" Value="0"/>
          <Setter Property="BorderBrush" Value="Black"/>
          <Setter Property="LineHeight" Value="12" />
        </Style>

      </Table.Resources>

      <Table.Columns>
        <TableColumn Width="5" />
        <TableColumn Width="40*" />
        <xsl:for-each select="Project/Sprints/Sprint">
          <TableColumn Width="40" />
        </xsl:for-each>
      </Table.Columns>

      <TableRowGroup>


        <TableRow>
          <TableCell Style="{{StaticResource headerCell}}"></TableCell>
          <TableCell Style="{{StaticResource headerCell}}" Padding="0,0,0,0" >            
            <BlockUIContainer>                            
                <Border Background="Black" BorderThickness="0"  Padding="4" HorizontalAlignment="Right">
                    <TextBlock  FontWeight="Bold" FontSize="8" VerticalAlignment="Center" Foreground="White">
                      <xsl:call-template name="formatShortDate">
                        <xsl:with-param name="dateTime" select="Project/Sprints/Sprint[SprintNumber = 1]/StartDate"/>
                      </xsl:call-template>                        
                    </TextBlock>
                  </Border>                
              </BlockUIContainer>            
          </TableCell>
          <xsl:for-each select="Project/Sprints/Sprint">
            <xsl:sort select="SprintNumber" data-type="number"/>
            <xsl:variable name="sprintNumber" select="SprintNumber"/>

            <xsl:variable name="cellBG">
              <xsl:choose>
                <xsl:when test="$sprintNumber = /ReportData/ProjectCurrentSprintNumber">
                  <xsl:value-of select="'#F0F0F0'"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="'White'"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>

            <TableCell Style="{{StaticResource headerCell}}" Background="{$cellBG}" Padding="0,0,0,0">
              <BlockUIContainer>
                <Grid>
                  <!--<TextBlock VerticalAlignment="Center" FontSize="14" FontWeight="Bold" HorizontalAlignment="Center" Foreground="Gray" Padding="0">
                    <xsl:value-of select="SprintNumber"/>
                  </TextBlock>-->
                  <!-- <xsl:if test="$sprintNumber = 1">
                    <Border Background="Black" BorderThickness="0"  HorizontalAlignment="Left" Padding="4">
                      <TextBlock   FontWeight="Bold" FontSize="8" VerticalAlignment="Center" Foreground="White">
                        <xsl:call-template name="formatShortDate">
                          <xsl:with-param name="dateTime" select="StartDate"/>
                        </xsl:call-template>
                      </TextBlock>
                    </Border>
                  </xsl:if> -->
                  <Border Background="Black" BorderThickness="0"  HorizontalAlignment="Right" Padding="4">
                    <TextBlock  FontWeight="Bold" FontSize="8" VerticalAlignment="Center" Foreground="White">
                      <xsl:call-template name="formatShortDate">
                        <xsl:with-param name="dateTime" select="EndDate"/>
                      </xsl:call-template>
                    </TextBlock>
                  </Border>
                </Grid>
              </BlockUIContainer>
            </TableCell>
          </xsl:for-each>
        </TableRow>

        <xsl:for-each select="/ReportData/ArrayOfBacklogItem/BacklogItem[Status !=3]">
          <xsl:sort select="OrderSprintWorked" data-type="number"/>
          <xsl:sort select="OccurrenceConstraint" data-type="number"/>
          <xsl:sort select="Status" data-type="number"  order="descending"/>
          <xsl:variable name="itemUId" select="BacklogItemUId"/>
          <xsl:variable name="status" select="Status"/>
          <xsl:variable name="itemSprintNumber" select="SprintNumber"/>

          <xsl:variable name="groupColor" select="Group/GroupColor"/>

          <xsl:variable name="color">
            <xsl:choose>
              <xsl:when test="IssueType = 3">
                <xsl:value-of select="'Red'"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'Black'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <xsl:variable name="cellStyle">
            <xsl:choose>
              <xsl:when test="OccurrenceConstraint = 2">
                <xsl:value-of select="'{StaticResource sprintBarCell}'"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'{StaticResource barCell}'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <xsl:if test="OccurrenceConstraint = 0">
            <TableRow>
              <TableCell></TableCell>
              <TableCell Style="{{StaticResource sprintHeaderCell}}" ColumnSpan="{count(//ReportData/Project/Sprints/Sprint) + 1}">
                <Paragraph>
                  <Bold>
                    Sprint <xsl:value-of select="$itemSprintNumber"/> (até <xsl:call-template name="formatShortDate"><xsl:with-param name="dateTime" select="//ReportData/Project/Sprints/Sprint[SprintNumber=$itemSprintNumber]/EndDate"/></xsl:call-template>)
                  </Bold>
                </Paragraph>
              </TableCell>
            </TableRow>
          </xsl:if>




          <TableRow>

            <TableCell Background="{$groupColor}" Style="{$cellStyle}"></TableCell>

            <TableCell Style="{$cellStyle}" Padding="3">
              <Paragraph TextAlignment="Left" Foreground="{$color}">
                <xsl:value-of select="Name"/>[<xsl:value-of select="BacklogItemNumber"/>]
                <xsl:if test="string-length(DeliveryDate) &gt; 0">
                  <TextBlock Background="{$color}" Foreground="White" FontSize="8" Margin="5,0,0,0" Padding="2,0,2,0">
                    <xsl:call-template name="formatShortDate">
                      <xsl:with-param name="dateTime" select="DeliveryDate"/>
                    </xsl:call-template>
                  </TextBlock>
                </xsl:if>
              </Paragraph>
            </TableCell>

            <xsl:for-each select="/ReportData/Project/Sprints/Sprint">
              <xsl:sort select="SprintNumber" data-type="number"/>
              <xsl:variable name="sprintNumber" select="SprintNumber"/>

              <xsl:variable name="hours" select="sum(/ReportData/ArrayOfBacklogItem/BacklogItem[BacklogItemUId = $itemUId]/ValidPlannedHours/PlannedHour[SprintNumber = $sprintNumber]/Hours)"/>

              <xsl:variable name="cellBG">
                <xsl:choose>
                  <xsl:when test="$sprintNumber = /ReportData/ProjectCurrentSprintNumber">
                    <xsl:value-of select="'#F0F0F0'"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="'White'"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:variable>

              <TableCell Background="{$cellBG}" Style="{$cellStyle}">

                <xsl:call-template name="bar">
                  <xsl:with-param name="isFirst" select="$sprintNumber = /ReportData/ArrayOfBacklogItem/BacklogItem[BacklogItemUId = $itemUId]/FirstSprintWorked" />
                  <xsl:with-param name="isLast" select="$sprintNumber = /ReportData/ArrayOfBacklogItem/BacklogItem[BacklogItemUId = $itemUId]/LastSprintWorked" />
                  <xsl:with-param name="status" select="$status" />
                  <xsl:with-param name="hours" select="$hours" />
                  <xsl:with-param name="constraint" select="/ReportData/ArrayOfBacklogItem/BacklogItem[BacklogItemUId = $itemUId]/OccurrenceConstraint" />
                </xsl:call-template>
              </TableCell>
            </xsl:for-each>



          </TableRow>
        </xsl:for-each>
      </TableRowGroup>
    </Table>
  </xsl:template>




</xsl:stylesheet>
