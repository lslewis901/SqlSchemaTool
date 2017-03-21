<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
<xsl:output method="xml" />
<xsl:include href="type.xsl"/>

<xsl:param name="db_name"></xsl:param>
<xsl:param name="pk">PRIMARY KEY</xsl:param>
<xsl:param name="fk">FOREIGN KEY</xsl:param>
<xsl:param name="base_table">BaseTable</xsl:param>
<xsl:param name="view">View</xsl:param>
<xsl:param name="procedure">Procedure</xsl:param>
<xsl:param name="function">Function</xsl:param>
<xsl:param name="rowset">Rowset</xsl:param>
<xsl:param name="return_value">ReturnValue</xsl:param>
<xsl:param name="table">TABLE</xsl:param>

<xsl:template match="/">
	<Database xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" Name="{$db_name}">
	<xsl:apply-templates select="/dbobjects/dbobject[@type=$base_table]" mode="table"/>
	<xsl:apply-templates select="/dbobjects/dbobject[@type=$view]" mode="table"/>
	<xsl:apply-templates select="/dbobjects/dbobject[@type=$procedure]" mode="procedure"/>
	<xsl:apply-templates select="/dbobjects/dbobject[@type=$function and @data_type!=$table]" mode="scalar_function"/>
	<xsl:apply-templates select="/dbobjects/dbobject[@type=$function and @data_type!=$table]" mode="table_function"/>
	</Database>
</xsl:template>

<xsl:template match="dbobject" mode="table">
	<Table Name="{@name}">
		<Type Name=".{@friendly_name}">
			<xsl:apply-templates select="column">
				<xsl:sort select="@ordinal_position"/>
			</xsl:apply-templates>
			<xsl:apply-templates select="assoc"/>
			<xsl:apply-templates select="column/constraint"/>
		</Type>
	</Table>
</xsl:template>

<xsl:template match="column">
	<xsl:variable name="type"><xsl:call-template name="select_primitive_type_CSharp"><xsl:with-param name="data_type" select="@data_type"/></xsl:call-template></xsl:variable>
	<xsl:variable name="sql_type_attr"><xsl:call-template name="data-length-precision-scale2"><xsl:with-param name="data-type" select="@data_type"/><xsl:with-param name="length" select="@character_maximum_length"/><xsl:with-param name="precision" select="@numeric_precision"/><xsl:with-param name="scale" select="@numeric_scale"/></xsl:call-template></xsl:variable>
	<Column Name="{@column_name}" Member="{@column_friendly_name}" Storage="x{@column_friendly_name}">
		<xsl:attribute name="DbType"><xsl:value-of select="@data_type"/><xsl:value-of select="$sql_type_attr"/><xsl:if test="@is_nullable!='YES'"> NOT NULL</xsl:if><xsl:if test="@identity=1"> IDENTITY</xsl:if></xsl:attribute>
		<xsl:if test="constraint[@constraint_type=$pk]"><xsl:attribute name="IsIdentity">True</xsl:attribute></xsl:if>
		<xsl:if test="@identity=1"><xsl:attribute name="IsAutoGen">True</xsl:attribute></xsl:if>
	</Column>
</xsl:template>

<xsl:template match="assoc">
	<xsl:variable name="constraint_name" select="@constraint_name"/>
	<Association Name="{$constraint_name}" Member="{@assoc_table_friendly_name}EntitySet" Storage="x{@assoc_table_friendly_name}EntitySet" ThisKey="{@assoc_column_name}" OtherTable="{@assoc_table_name}">
		<xsl:attribute name="OtherKey"><xsl:value-of select="/dbobjects/dbobject/column[constraint/@constraint_name=$constraint_name]/@column_name"/></xsl:attribute>
	</Association>
</xsl:template>

<xsl:template match="constraint">
	<xsl:variable name="constraint_name" select="@constraint_name"/>
	<Association Name="{$constraint_name}" Member="{@referenced_table_friendly_name}EntityRef" Storage="x{@referenced_table_friendly_name}EntityRef" ThisKey="{../@column_friendly_name}" OtherTable="{@referenced_table_name}">
		<xsl:attribute name="OtherKey"><xsl:value-of select="/dbobjects/dbobject/assoc[@constraint_name=$constraint_name]/@assoc_column_name"/></xsl:attribute>
		<xsl:attribute name="IsParent">True</xsl:attribute>
	</Association>
</xsl:template>

<xsl:template match="dbobject" mode="procedure">
	<StoredProcedure Name="{@name}">
		<xsl:apply-templates select="parameter">
			<xsl:sort select="@ordinal_position"/>
		</xsl:apply-templates>
		<xsl:if test="@procedure_type=$rowset">
		<Type Name=".{@friendly_name}Rowset">
			<xsl:apply-templates select="column">
				<xsl:sort select="@ordinal_position"/>
			</xsl:apply-templates>
		</Type>
		</xsl:if>
	</StoredProcedure>
</xsl:template>

<xsl:template match="parameter">
	<Parameter Name="{@parameter_name}" Parameter="{@parameter_friendly_name}">
		<xsl:attribute name="DBType"><xsl:value-of select="@data_type"/><xsl:call-template name="data-length-precision-scale2"><xsl:with-param name="data-type" select="@data_type"/><xsl:with-param name="length" select="@character_maximum_length"/><xsl:with-param name="precision" select="@numeric_precision"/><xsl:with-param name="scale" select="@numeric_scale"/></xsl:call-template></xsl:attribute>
	</Parameter>
</xsl:template>

<xsl:template match="dbobject" mode="scalar_function">
  <UserDefinedFunction Name="[dbo].[{@name}]" Method="{@friendly_name}">
		<xsl:apply-templates select="parameter">
			<xsl:sort select="@ordinal_position"/>
		</xsl:apply-templates>
  </UserDefinedFunction>
</xsl:template>

<xsl:template match="dbobject" mode="table_function">
	<TableValuedFunction Name="[dbo].[{@name}]" Method="{@friendly_name}">
		<xsl:apply-templates select="parameter">
			<xsl:sort select="@ordinal_position"/>
		</xsl:apply-templates>
		<Type Name=".{@friendly_name}">
			<xsl:apply-templates select="column">
				<xsl:sort select="@ordinal_position"/>
			</xsl:apply-templates>
		</Type>
	</TableValuedFunction>
</xsl:template>

</xsl:stylesheet>