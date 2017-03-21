<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:include href="type.xsl"/>
<xsl:output method="text"/>

<xsl:template match="/">
<xsl:apply-templates select="/routine/parameter" mode="declare">
	<xsl:sort select="@ordinal_position"/>
</xsl:apply-templates>

<xsl:apply-templates select="/routine/parameter" mode="set">
	<xsl:sort select="@ordinal_position"/>
</xsl:apply-templates>

EXEC [<xsl:value-of select="/routine/@name"/>]&#9;<xsl:apply-templates select="/routine/parameter" mode="list"><xsl:sort select="@ordinal_position"/></xsl:apply-templates>
</xsl:template>

<xsl:template match="parameter" mode="declare">
<xsl:variable name="data_type">
	<xsl:choose>
		<xsl:when test="string(@data_type) = 'text' or string(@data_type) = 'TEXT'">varchar</xsl:when>
		<xsl:when test="string(@data_type) = 'ntext' or string(@data_type) = 'NTEXT'">nvarchar</xsl:when>
		<xsl:when test="string(@data_type) = 'image' or string(@data_type) = 'IMAGE'">nvarchar</xsl:when>
		<xsl:otherwise><xsl:value-of select="@data_type"/></xsl:otherwise>
	</xsl:choose>
</xsl:variable>
<xsl:variable name="character_maximum_length">
	<xsl:choose>
		<xsl:when test="string(@data_type) = 'text' or string(@data_type) = 'TEXT'">8000</xsl:when>
		<xsl:when test="string(@data_type) = 'ntext' or string(@data_type) = 'NTEXT'">4000</xsl:when>
		<xsl:when test="string(@data_type) = 'image' or string(@data_type) = 'IMAGE'">4000</xsl:when>
		<xsl:otherwise><xsl:value-of select="@character_maximum_length"/></xsl:otherwise>
	</xsl:choose>
</xsl:variable>
DECLARE <xsl:value-of select="@parameter_name"/>&#9;<xsl:value-of select="$data_type"/><xsl:call-template name="data-length-precision-scale2"><xsl:with-param name="data-type" select="$data_type"/><xsl:with-param name="length" select="$character_maximum_length"/><xsl:with-param name="precision" select="@numeric_precision"/><xsl:with-param name="scale" select="@numeric_scale"/></xsl:call-template>
</xsl:template>

<xsl:template match="parameter" mode="list">
<xsl:if test="position() > 1">, </xsl:if><xsl:value-of select="@parameter_name"/><xsl:if test="@parameter_mode='INOUT'"> OUTPUT</xsl:if>
</xsl:template>

<xsl:template match="parameter" mode="set">
SET <xsl:value-of select="@parameter_name"/> = <xsl:call-template name="select_primitive_type_default_SQL"><xsl:with-param name="data_type" select="@data_type"/></xsl:call-template>
</xsl:template>

</xsl:stylesheet>