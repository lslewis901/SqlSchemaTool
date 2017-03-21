<?xml version='1.0' encoding='utf-8' ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:output method="xml"/>
	<xsl:template match="DocumentElement">
		<xsl:element name="DataExport">
			<xsl:apply-templates select="*"/>
		</xsl:element>
	</xsl:template>
		
	<xsl:template match="*">
		<xsl:element name="{local-name()}">
			<xsl:for-each select="*">
				<xsl:element name="{local-name()}">
					<xsl:if test="string-length(normalize-space(text()))=0">
            <xsl:text xml:space="preserve"> </xsl:text>
					</xsl:if>
          <xsl:if test="string-length(normalize-space(text()))>0">
            <xsl:value-of select="."/>
          </xsl:if>
				</xsl:element>
			</xsl:for-each>
		</xsl:element>
	</xsl:template>
</xsl:stylesheet>
