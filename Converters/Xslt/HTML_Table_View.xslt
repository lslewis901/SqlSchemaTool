<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
<!-- this is the html output xslt for either a schema xml snapshot or a diff xml snapshot -->
<xsl:template match="DataBase_Schema">
<HTML><HEAD></HEAD>
  <STYLE TYPE='text/css' MEDIA='screen'>
    BODY { font-family:tahoma; font-size:8pt }
    TD   { font-size:8pt }
    TABLE { font-family:tahoma; font-size:8pt }
    H5 { background-color:lightblue; }
    span.Guion { font-family: Webdings; }
    .add { background-color:lightgreen; }
    .alter { background-color:yellow; }
    .drop { background-color:red; }
    .addkey { background-color:lightgreen; text-align:center; }
    .alterkey { background-color:yellow; text-align:center; }
    .dropkey { background-color:red; text-align:center; }
    .normal { background-color:white; }
  </STYLE>
  <BODY>
    <xsl:if test="(.)/text()='DiffData'">
      <xsl:variable name="output" select="Database/Name"/>
      <xsl:variable name="output2" select="concat(substring-before($output, '-- '), ' ')"/>
      <xsl:variable name="output3" select="substring-after($output, '-- ')"/>
      <H5>
        <xsl:if test="contains($output, '-- ')">
          <xsl:value-of select="concat($output2, substring-before($output3, '-- '))"/>
        </xsl:if>
        <xsl:if test="not(contains($output, '-- '))">
          <xsl:value-of select="$output"/>
        </xsl:if>
      </H5>
      <BR/>
      <H6>
        <xsl:value-of select="substring-after($output3, '-- ')"/>
      </H6>
      <TABLE BORDER="0" width="40%">
        <TR>
          <TH>
            <B>Legend: </B>
          </TH>
          <TD class="addkey" width ="30%"> added </TD>
          <TD class="alterkey" width ="30%"> altered </TD>
          <TD class="dropkey" width ="30%"> removed </TD>
        </TR>
      </TABLE>
      <HR/>
    </xsl:if>
    <xsl:if test="(.)/text()!='DiffData'">
      <H5>
        Database: <xsl:value-of select="Database/Name"/>
      </H5>
    </xsl:if>
    <H6>
      Process Date/Time: (<xsl:value-of select="Database/Date"/>)/(<xsl:value-of select="Database/Time"/>)
    </H6>
    <xsl:if test="TABLE">
      <B>Tables:</B>
    </xsl:if>
    <HR/>
    <xsl:apply-templates select="TABLE">
      <xsl:sort select="TABLE_NAME" data-type="text" order="ascending"/>
    </xsl:apply-templates>
    <BR/>
    <hr></hr>
    <TABLE BORDER="0" width="100%">
      <TR>
        <xsl:if test="VIEW[not(@Action)]">
        <TD class='add' width="50%" valign="top">
          <xsl:variable name="view">Views: </xsl:variable>
          <xsl:call-template name="Standard">
            <xsl:with-param name="SelectValue" select="VIEW"/>
            <xsl:with-param name="DisplayValue" select="$view"/>
          </xsl:call-template>
        </TD>
        </xsl:if>
        <xsl:if test="VIEW[(@Action)]">
          <TD class='add' width="50%" valign="top">
          <xsl:variable name="newview">New Views: </xsl:variable>
          <xsl:call-template name="AddAlterDrop">
            <xsl:with-param name="SelectValue" select="VIEW[@Action='Add']"/>
            <xsl:with-param name="DisplayValue" select="$newview"/>
          </xsl:call-template>
        </TD>
        <TD class='alter' width="50%" valign="top">
          <xsl:variable name="alteredview">Altered Views: </xsl:variable>
          <xsl:call-template name="AddAlterDrop">
            <xsl:with-param name="SelectValue" select="VIEW[@Action='Alter']"/>
            <xsl:with-param name="DisplayValue" select="$alteredview"/>
          </xsl:call-template>
        </TD>
        </xsl:if>
      </TR>
    </TABLE>
    <TABLE BORDER="0" width="100%">
      <TR>
        <xsl:if test="SPROC[not(@Action)]">
          <TD class='add' width="50%" valign="top">
          <xsl:variable name="sproc">Stored Procedures: </xsl:variable>
          <xsl:call-template name="Standard">
            <xsl:with-param name="SelectValue" select="SPROC"/>
            <xsl:with-param name="DisplayValue" select="$sproc"/>
          </xsl:call-template>
        </TD>
        </xsl:if>
        <xsl:if test="VIEW[(@Action)]">
          <TD class='drop' width="50%" valign="top">
            <xsl:variable name="dropview">Dropped Views: </xsl:variable>
            <xsl:call-template name="AddAlterDrop">
              <xsl:with-param name="SelectValue" select="VIEW[@Action='Drop']"/>
              <xsl:with-param name="DisplayValue" select="$dropview"/>
            </xsl:call-template>
          </TD>
        </xsl:if>
        <xsl:if test="SPROC[(@Action)]">
          <TD class='add' width="50%" valign="top">
          <xsl:variable name="addsproc">New Stored Procedures: </xsl:variable>
          <xsl:call-template name="AddAlterDrop">
            <xsl:with-param name="SelectValue" select="SPROC[@Action='Add']"/>
            <xsl:with-param name="DisplayValue" select="$addsproc"/>
          </xsl:call-template>
        </TD>
        </xsl:if>
      </TR>
    </TABLE>
    <TABLE BORDER="0" width="100%">
      <TR>
        <xsl:if test="FUNC[not(@Action)]">
          <TD class='add' width="50%" valign="top">
            <xsl:variable name="func">Functions: </xsl:variable>
            <xsl:call-template name="Standard">
              <xsl:with-param name="SelectValue" select="FUNC"/>
              <xsl:with-param name="DisplayValue" select="$func"/>
            </xsl:call-template>
          </TD>
        </xsl:if>
        <xsl:if test="SPROC[(@Action)]">
          <TD class='alter' width="50%" valign="top">
          <xsl:variable name="altersproc">Altered Stored Procedures: </xsl:variable>
          <xsl:call-template name="AddAlterDrop">
            <xsl:with-param name="SelectValue" select="SPROC[@Action='Alter']"/>
            <xsl:with-param name="DisplayValue" select="$altersproc"/>
          </xsl:call-template>
          </TD>
          <TD class='drop' width="50%" valign="top">
            <xsl:variable name="dropsproc">Dropped Stored Procedures: </xsl:variable>
            <xsl:call-template name="AddAlterDrop">
              <xsl:with-param name="SelectValue" select="SPROC[@Action='Drop']"/>
              <xsl:with-param name="DisplayValue" select="$dropsproc"/>
            </xsl:call-template>
          </TD>
        </xsl:if>
      </TR>
    </TABLE>
    <TABLE BORDER="0" width="100%">
      <TR>
        <xsl:if test="TRIGGER[not(@Action)]">
          <TD class='add' width="50%" valign="top">
            <xsl:variable name="trigger">Triggers: </xsl:variable>
            <xsl:call-template name="Standard">
              <xsl:with-param name="SelectValue" select="TRIGGER"/>
              <xsl:with-param name="DisplayValue" select="$trigger"/>
            </xsl:call-template>
          </TD>
        </xsl:if>
        <xsl:if test="FUNC[(@Action)]">
          <TD class='add' width="50%" valign="top">
            <xsl:variable name="addfunc">New Functions: </xsl:variable>
            <xsl:call-template name="AddAlterDrop">
              <xsl:with-param name="SelectValue" select="FUNC[@Action='Add']"/>
              <xsl:with-param name="DisplayValue" select="$addfunc"/>
            </xsl:call-template>
          </TD>
          <TD class='alter' width="50%" valign="top">
            <xsl:variable name="alterfunc">Altered Functions: </xsl:variable>
            <xsl:call-template name="AddAlterDrop">
              <xsl:with-param name="SelectValue" select="FUNC[@Action='Alter']"/>
              <xsl:with-param name="DisplayValue" select="$alterfunc"/>
            </xsl:call-template>
          </TD>
        </xsl:if>
      </TR>
    </TABLE>
    <TABLE BORDER="0" width="100%">
      <TR>
        <xsl:if test="DEFAULT[not(@Action)]">
          <TD class='add' width="50%" valign="top">
            <xsl:variable name="default">Defaults: </xsl:variable>
            <xsl:call-template name="Standard">
              <xsl:with-param name="SelectValue" select="DEFAULT"/>
              <xsl:with-param name="DisplayValue" select="$default"/>
            </xsl:call-template>
          </TD>
        </xsl:if>
        <xsl:if test="FUNC[(@Action)]">
          <TD class='drop' width="50%" valign="top">
            <xsl:variable name="dropfunc">Dropped Functions: </xsl:variable>
            <xsl:call-template name="AddAlterDrop">
              <xsl:with-param name="SelectValue" select="FUNC[@Action='Drop']"/>
              <xsl:with-param name="DisplayValue" select="$dropfunc"/>
            </xsl:call-template>
          </TD>
        </xsl:if>
        <xsl:if test="TRIGGER[(@Action)]">
          <TD class='add' width="50%" valign="top">
            <xsl:variable name="addtrig">New Triggers: </xsl:variable>
            <xsl:call-template name="AddAlterDrop">
              <xsl:with-param name="SelectValue" select="TRIGGER[@Action='Add']"/>
              <xsl:with-param name="DisplayValue" select="$addtrig"/>
            </xsl:call-template>
          </TD>
        </xsl:if>
      </TR>
    </TABLE>
    <TABLE BORDER="0" width="100%">
      <TR>
        <xsl:if test="RULE[not(@Action)]">
          <TD class='add' width="50%" valign="top">
            <xsl:variable name="rule">Rules: </xsl:variable>
            <xsl:call-template name="Standard">
              <xsl:with-param name="SelectValue" select="RULE"/>
              <xsl:with-param name="DisplayValue" select="$rule"/>
            </xsl:call-template>
          </TD>
        </xsl:if>
        <xsl:if test="TRIGGER[(@Action)]">
          <TD class='alter' width="50%" valign="top">
            <xsl:variable name="altertrig">Altered Triggers: </xsl:variable>
            <xsl:call-template name="AddAlterDrop">
              <xsl:with-param name="SelectValue" select="TRIGGER[@Action='Alter']"/>
              <xsl:with-param name="DisplayValue" select="$altertrig"/>
            </xsl:call-template>
          </TD>
          <TD class='drop' width="50%" valign="top">
            <xsl:variable name="droptrig">Dropped Triggers: </xsl:variable>
            <xsl:call-template name="AddAlterDrop">
              <xsl:with-param name="SelectValue" select="TRIGGER[@Action='Drop']"/>
              <xsl:with-param name="DisplayValue" select="$droptrig"/>
            </xsl:call-template>
          </TD>
        </xsl:if>
      </TR>
    </TABLE>
    <TABLE BORDER="0" width="100%">
      <TR>
        <xsl:if test="UDDT[not(@Action)]">
          <TD class='add' width="50%" valign="top">
            <xsl:variable name="uddt">User Defined Data Types: </xsl:variable>
            <xsl:call-template name="Standard">
              <xsl:with-param name="SelectValue" select="UDDT"/>
              <xsl:with-param name="DisplayValue" select="$uddt"/>
            </xsl:call-template>
          </TD>
        </xsl:if>
        <xsl:if test="DEFAULT[(@Action)]">
          <TD class='add' width="50%" valign="top">
            <xsl:variable name="adddef">New Defaults: </xsl:variable>
            <xsl:call-template name="AddAlterDrop">
              <xsl:with-param name="SelectValue" select="DEFAULT[@Action='add']"/>
              <xsl:with-param name="DisplayValue" select="$adddef"/>
            </xsl:call-template>
          </TD>
          <TD class='alter' width="50%" valign="top">
            <xsl:variable name="alterdef">Altered Defaults: </xsl:variable>
            <xsl:call-template name="AddAlterDrop">
              <xsl:with-param name="SelectValue" select="DEFAULT[@Action='alter']"/>
              <xsl:with-param name="DisplayValue" select="$alterdef"/>
            </xsl:call-template>
          </TD>
        </xsl:if>
      </TR>
    </TABLE>
    <TABLE BORDER="0" width="100%">
      <TR>
        <xsl:if test="DEFAULT[(@Action)]">
          <TD class='drop' width="50%" valign="top">
            <xsl:variable name="dropdef">Dropped Defaults: </xsl:variable>
            <xsl:call-template name="AddAlterDrop">
              <xsl:with-param name="SelectValue" select="DEFAULT[@Action='Drop']"/>
              <xsl:with-param name="DisplayValue" select="$dropdef"/>
            </xsl:call-template>
          </TD>
        </xsl:if>
        <xsl:if test="RULE[(@Action)]">
          <TD class='add' width="50%" valign="top">
            <xsl:variable name="addrule">New Rules: </xsl:variable>
            <xsl:call-template name="AddAlterDrop">
              <xsl:with-param name="SelectValue" select="RULE[@Action='Add']"/>
              <xsl:with-param name="DisplayValue" select="$addrule"/>
            </xsl:call-template>
          </TD>
        </xsl:if>
      </TR>
    </TABLE>
    <TABLE BORDER="0" width="100%">
      <TR>
        <xsl:if test="RULE[(@Action)]">
          <TD class='alter' width="50%" valign="top">
            <xsl:variable name="alterrule">Altered Rules: </xsl:variable>
            <xsl:call-template name="AddAlterDrop">
              <xsl:with-param name="SelectValue" select="RULE[@Action='Alter']"/>
              <xsl:with-param name="DisplayValue" select="$alterrule"/>
            </xsl:call-template>
          </TD>
          <TD class='drop' width="50%" valign="top">
            <xsl:variable name="droprule">Dropped Rules: </xsl:variable>
            <xsl:call-template name="AddAlterDrop">
              <xsl:with-param name="SelectValue" select="RULE[@Action='Drop']"/>
              <xsl:with-param name="DisplayValue" select="$droprule"/>
            </xsl:call-template>
          </TD>
        </xsl:if>
      </TR>
    </TABLE>
    <TABLE BORDER="0" width="100%">
      <TR>
        <xsl:if test="UDDT[(@Action)]">
          <TD class='add' width="50%" valign="top">
            <xsl:variable name="adduddt">New User Defined Data Types: </xsl:variable>
            <xsl:call-template name="AddAlterDrop">
              <xsl:with-param name="SelectValue" select="UDDT[@Action='add']"/>
              <xsl:with-param name="DisplayValue" select="$adduddt"/>
            </xsl:call-template>
          </TD>
          <TD class='alter' width="50%" valign="top">
            <xsl:variable name="alteruddt">Altered User Defined Data Types: </xsl:variable>
            <xsl:call-template name="AddAlterDrop">
              <xsl:with-param name="SelectValue" select="UDDT[@Action='alter']"/>
              <xsl:with-param name="DisplayValue" select="$alteruddt"/>
            </xsl:call-template>
          </TD>
        </xsl:if>
      </TR>
    </TABLE>
    <TABLE BORDER="0" width="100%">
      <TR>
        <xsl:if test="UDDT[(@Action)]">
          <TD class='drop' width="50%" valign="top">
            <xsl:variable name="dropuddt">Dropped User Defined Data Types: </xsl:variable>
            <xsl:call-template name="AddAlterDrop">
              <xsl:with-param name="SelectValue" select="UDDT[@Action='Drop']"/>
              <xsl:with-param name="DisplayValue" select="$dropuddt"/>
            </xsl:call-template>
          </TD>
          <TD class='alter' width="50%" valign="top"></TD>
        </xsl:if>
      </TR>
    </TABLE>
  </BODY></HTML>
</xsl:template>

<xsl:template match="TABLE">
    <BR/><B>
		<xsl:value-of select="TABLE_NAME"/></B><BR/><HR/>
  <xsl:if test="@Action='Alter' and count(COLUMN) > 0">
    <tr>
      <td>
        Changed Table - Column Count: <xsl:value-of select="count(COLUMN)"/>
      </td>
    </tr>
  </xsl:if>
  <xsl:if test="@Action='Add' and count(COLUMN) > 0">
    <tr>
      <td>
        New Table - Column Count: <xsl:value-of select="count(COLUMN)"/>
      </td>
    </tr>
  </xsl:if>
  <xsl:if test="@Action='Drop' and count(COLUMN) > 0">
    <tr>
      <td>
        Dropped Table - Column Count: <xsl:value-of select="count(COLUMN)"/>
      </td>
    </tr>
  </xsl:if>
  <TABLE BORDER="0" width="100%">
		<TR>
			<TD width="150px"><u>Field name</u></TD>
			<TD width="100px"><u>Data type</u></TD>
			<TD width="100px"><u>Default value</u></TD>
			<TD width="50px"><u>Allow nulls</u></TD>
			<TD width="50px"><u>IsIdentity</u></TD>
			<TD width="50px"><u>IsPrimaryKey</u></TD>
		</TR>
	</TABLE>
	<TABLE BORDER="0" width="100%">
    <xsl:for-each select="COLUMN">
      <xsl:sort select="Column_Name" data-type="text" order="ascending"/>
      <xsl:if test="not(Action) or Action='Add'">
        <tr class="add">
          <xsl:call-template name="COLUMN_DATA">
            <xsl:with-param name="_value">
              <xsl:call-template name="PK"/>
            </xsl:with-param>
          </xsl:call-template>
        </tr>
      </xsl:if>
      <xsl:if test="Action='Alter'">
        <tr class="alter">
            <xsl:call-template name="COLUMN_DATA">
              <xsl:with-param name="_value">
                <xsl:call-template name="PK"/>
              </xsl:with-param>
            </xsl:call-template>
          </tr>
      </xsl:if>
      <xsl:if test="Action='Drop'">
        <tr class="drop">
          <xsl:call-template name="COLUMN_DATA">
            <xsl:with-param name="_value">
              <xsl:call-template name="PK"/>
            </xsl:with-param>
          </xsl:call-template>
        </tr>
      </xsl:if>
	  </xsl:for-each>
    </TABLE>
</xsl:template>

<xsl:template name="PK">
	<xsl:for-each select="../TABLE_INDEX[contains(index_description, 'primary key')]">
		<xsl:value-of select="index_keys"/>
	</xsl:for-each>
</xsl:template>

<xsl:template name="COLUMN_DATA" match="COLUMN">  
	<xsl:param name="_value"/>
	<xsl:variable name="_cName" select="Column_Name"/>
		<TD width="150px"><i><xsl:value-of select="$_cName"/></i></TD>
		<TD width="100px"><xsl:value-of select="Type"/> (<xsl:value-of select="Length"/>)</TD>
		<TD width="100px"><xsl:value-of select="Default_Value"/></TD>
		<TD width="50px"><xsl:if test="isNullable = 1"><SPAN class="Guion">a</SPAN></xsl:if></TD>
		<TD width="50px"><xsl:if test="isIdentity = 1"><SPAN class="Guion">a</SPAN></xsl:if></TD>
		<TD width="50px"><xsl:if test="contains($_value, $_cName)"><SPAN class="Guion">a</SPAN></xsl:if></TD>
</xsl:template>

  <xsl:template name="AddAlterDrop">
    <xsl:param name="SelectValue" />
    <xsl:param name="DisplayValue" />
    <xsl:if test="($SelectValue)">
      <B>
        <SPAN class="normal" ><xsl:value-of select="$DisplayValue"/></SPAN>
      </B>
      <BR/>
      <xsl:for-each select="$SelectValue">
        <xsl:sort select="child::node()" data-type="text" order="ascending"/>
        <xsl:value-of select="child::node()"/>
        <BR/>
      </xsl:for-each>
    </xsl:if>
  </xsl:template>

  <xsl:template name="Standard">
    <xsl:param name="SelectValue" />
    <xsl:param name="DisplayValue" />
    <xsl:if test="($SelectValue)">
      <B>
        <SPAN class="normal" >
          <xsl:value-of select="$DisplayValue"/>
        </SPAN>
      </B>
      <BR/>
      <xsl:for-each select="$SelectValue">
        <xsl:sort select="child::node()" data-type="text" order="ascending"/>
        <xsl:value-of select="child::node()/text()"/>
        <BR/>
      </xsl:for-each>
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>

  