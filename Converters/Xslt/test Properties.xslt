<?xml version="1.0" encoding="UTF-8" ?>
<!--
 |
 | XSLT REC Compliant Version of IE5 Default Stylesheet
 |
 | Original version by Jonathan Marsh (jmarsh@microsoft.com)
 | http://msdn.microsoft.com/xml/samples/defaultss/defaultss.xsl
 |
 | Conversion to XSLT 1.0 REC Syntax by Steve Muench (smuench@oracle.com)
 |
 +-->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
   <xsl:output indent="yes" method="html"/>
 
   <xsl:template match="/">
   </xsl:template>
 
   <xsl:template match="processing-instruction()">
            <xsl:call-template name="entity-ref">
               <xsl:with-param name="name">nbsp</xsl:with-param>
            </xsl:call-template>
            <xsl:text>&lt;?</xsl:text>
            <xsl:value-of select="name(.)"/>
            <xsl:value-of select="."/>
            <xsl:text>?></xsl:text>
   </xsl:template>
 
   <xsl:template match="processing-instruction('xml')">
            <xsl:call-template name="entity-ref">
               <xsl:with-param name="name">nbsp</xsl:with-param>
            </xsl:call-template>
            <xsl:text>&lt;?</xsl:text>
            <xsl:text>xml </xsl:text>
            <xsl:for-each select="@*">
               <xsl:value-of select="name(.)"/>
               <xsl:text>="</xsl:text>
               <xsl:value-of select="."/>
               <xsl:text>" </xsl:text>
            </xsl:for-each>
            <xsl:text>?></xsl:text>
   </xsl:template>
 
   <xsl:template match="@*">	  
         <xsl:attribute name="class">
            <xsl:if test="xsl:*/@*">
              <xsl:text>x</xsl:text>              
            </xsl:if>
            <xsl:text>t</xsl:text>
         </xsl:attribute>&#xA0;
         <xsl:value-of select="name(.)"/>
         <xsl:value-of select="."/>
   </xsl:template>
 
   <xsl:template match="text()">
            <xsl:value-of select="."/>
   </xsl:template>
 
   <xsl:template match="comment()">
               <xsl:text>&lt;!--</xsl:text>
               <xsl:value-of select="."/>
            <xsl:call-template name="entity-ref">
               <xsl:with-param name="name">nbsp</xsl:with-param>
            </xsl:call-template>
            <xsl:text>--></xsl:text>
   </xsl:template>
 
   <xsl:template match="*">
               <xsl:call-template name="entity-ref">
                  <xsl:with-param name="name">nbsp</xsl:with-param>
               </xsl:call-template>
               <xsl:attribute name="class">
                  <xsl:if test="xsl:*">
                     <xsl:text>x</xsl:text>
                  </xsl:if>
                  <xsl:text>t</xsl:text>
               </xsl:attribute>
               <xsl:value-of select="name(.)"/>
               <xsl:if test="@*">
                  <xsl:text> </xsl:text>
               </xsl:if>
            <xsl:apply-templates select="@*"/>
               <xsl:text>/></xsl:text>
   </xsl:template>
 
   <xsl:template match="*[node()]">
               <xsl:attribute name="class">
                  <xsl:if test="xsl:*">
                     <xsl:text>x</xsl:text>
                  </xsl:if>
                  <xsl:text>t</xsl:text>
               </xsl:attribute>
               <xsl:value-of select="name(.)"/>
               <xsl:if test="@*">
                  <xsl:text> </xsl:text>
               </xsl:if>
            <xsl:apply-templates select="@*"/>
               <xsl:text>></xsl:text>
            <xsl:apply-templates/>
                  <xsl:call-template name="entity-ref">
                     <xsl:with-param name="name">nbsp</xsl:with-param>
                  </xsl:call-template>
                   <xsl:text>&lt;/</xsl:text>
                  <xsl:attribute name="class">
                     <xsl:if test="xsl:*">
                        <xsl:text>x</xsl:text>
                     </xsl:if>
                     <xsl:text>t</xsl:text>
                  </xsl:attribute>
                  <xsl:value-of select="name(.)"/>
                  <xsl:text>></xsl:text>
   </xsl:template>
 
   <xsl:template match="*[text() and not (comment() or processing-instruction())]">
               <xsl:call-template name="entity-ref">
                  <xsl:with-param name="name">nbsp</xsl:with-param>
               </xsl:call-template>
               <xsl:text>&lt;</xsl:text>
               <xsl:attribute name="class">
                  <xsl:if test="xsl:*">
                     <xsl:text>x</xsl:text>
                  </xsl:if>
                  <xsl:text>t</xsl:text>
               </xsl:attribute>
               <xsl:value-of select="name(.)"/>
               <xsl:if test="@*">
                  <xsl:text> </xsl:text>
               </xsl:if>
            <xsl:apply-templates select="@*"/>
               <xsl:text>></xsl:text>
               <xsl:value-of select="."/>
               <xsl:attribute name="class">
                  <xsl:if test="xsl:*">
                     <xsl:text>x</xsl:text>
                  </xsl:if>
                  <xsl:text>t</xsl:text>
               </xsl:attribute>
               <xsl:value-of select="name(.)"/>
               <xsl:text>></xsl:text>
   </xsl:template>
 
   <xsl:template match="*[*]" priority="20">
               <xsl:attribute name="class">
                  <xsl:if test="xsl:*">
                     <xsl:text>x</xsl:text>
                  </xsl:if>
                  <xsl:text>t</xsl:text>
               </xsl:attribute>
               <xsl:value-of select="name(.)"/>
               <xsl:if test="@*">
                  <xsl:text> </xsl:text>
               </xsl:if>
            <xsl:apply-templates select="@*"/>
               <xsl:text>></xsl:text>
            <xsl:apply-templates/>
                  <xsl:call-template name="entity-ref">
                     <xsl:with-param name="name">nbsp</xsl:with-param>
                  </xsl:call-template>
                  <xsl:text>&lt;/</xsl:text>
                  <xsl:attribute name="class">
                     <xsl:if test="xsl:*">
                        <xsl:text>x</xsl:text>
                     </xsl:if>
                     <xsl:text>t</xsl:text>
                  </xsl:attribute>
                  <xsl:value-of select="name(.)"/>
                  <xsl:text>></xsl:text>
   </xsl:template>
 
   <xsl:template name="entity-ref">
      <xsl:param name="name"/>
      <xsl:text disable-output-escaping="yes">&amp;</xsl:text>
      <xsl:value-of select="$name"/>
      <xsl:text>;</xsl:text>
   </xsl:template>
 
</xsl:stylesheet>
 
  