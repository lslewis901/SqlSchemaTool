<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

<xsl:template match="TablesFields">
	<xsl:apply-templates select="MyDatabase"/>
</xsl:template>

<xsl:template match="DataBase_Schema">
<HTML><HEAD></HEAD><STYLE>
  BODY {font-family:tahoma;font-size:8pt}
  TD   {font-size:8pt}
  TABLE {font-family:tahoma;font-size:8pt}
  .Guion {	font-family: Webdings;}
  </STYLE><BODY>
 <H2><xsl:value-of select="Database/Name"/> Database</H2><H5> Date/Time: (<xsl:value-of select="Database/Date"/>)/(<xsl:value-of select="Database/Time"/>)</H5>
<xsl:apply-templates select="TABLE"/>
</BODY></HTML>
</xsl:template>

<xsl:template match="TABLE">
    <BR/><BR/><B>
		<xsl:value-of select="TABLE_NAME"/></B><BR/><HR/>
  <xsl:if test="@Action='Alter' and count(COLUMN) > 0">
    <tr>
      <td>
        Altered Column Count: <xsl:value-of select="count(COLUMN)"/>
      </td>
    </tr>
  </xsl:if>
  <xsl:if test="@Action='Add' and count(COLUMN) > 0">
    <tr>
      <td>
        New Column Count: <xsl:value-of select="count(COLUMN)"/>
      </td>
    </tr>
  </xsl:if>
  <xsl:if test="@Action='Drop' and count(COLUMN) > 0">
    <tr>
      <td>
        Dropped Column Count: <xsl:value-of select="count(COLUMN)"/>
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
		  <xsl:call-template name="COLUMN_DATA">
			  <xsl:with-param name="_value"><xsl:call-template name="PK"/></xsl:with-param>
		  </xsl:call-template>
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
    <TR>
		<TD width="150px"><i><xsl:value-of select="$_cName"/></i></TD>
		<TD width="100px"><xsl:value-of select="Type"/> (<xsl:value-of select="Length"/>)</TD>
		<TD width="100px"><xsl:value-of select="Default_Value"/></TD>
		<TD width="50px"><xsl:if test="isNullable = 1"><SPAN class="Guion">a</SPAN></xsl:if></TD>
		<TD width="50px"><xsl:if test="isIdentity = 1"><SPAN class="Guion">a</SPAN></xsl:if></TD>
		<TD width="50px"><xsl:if test="contains($_value, $_cName)"><SPAN class="Guion">a</SPAN></xsl:if></TD>
    </TR>
 </xsl:template>

</xsl:stylesheet>

  