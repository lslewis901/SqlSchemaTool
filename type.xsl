<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<xsl:template name="data-length-precision-scale">
	<xsl:param name="data-type"/>
	<xsl:param name="length"/>
	<xsl:param name="precision"/>
	<xsl:param name="scale"/>
	<xsl:choose>
		<xsl:when test="$length > 0 and not(contains('text ntext image timestamp', string($data-type)))">, <xsl:value-of select="$length"/></xsl:when>
		<xsl:otherwise><xsl:if test="contains('decimal numeric', string($data-type))">, <xsl:value-of select="$precision"/>, <xsl:value-of select="$scale"/></xsl:if></xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template name="data-length-precision-scale2">
	<xsl:param name="data-type"/>
	<xsl:param name="length"/>
	<xsl:param name="precision"/>
	<xsl:param name="scale"/>
	<xsl:choose>
		<xsl:when test="$length > 0 and not(contains('text ntext image timestamp', string($data-type)))">(<xsl:value-of select="$length"/>)</xsl:when>
		<xsl:otherwise><xsl:if test="contains('decimal numeric', string($data-type))">(<xsl:value-of select="$precision"/>, <xsl:value-of select="$scale"/>)</xsl:if></xsl:otherwise>
	</xsl:choose>
</xsl:template>
	
<xsl:template name="select_type">
	<xsl:param name="data_type"></xsl:param>
	<xsl:choose>
		<xsl:when test="contains('int Integer', string($data_type))">FieldTypeInteger</xsl:when>
		<xsl:when test="contains('bigint', string($data_type))">FieldTypeLong</xsl:when>
		<xsl:when test="contains('smallint', string($data_type))">FieldTypeInt16</xsl:when>
		<xsl:when test="contains('tinyint', string($data_type))">FieldTypeInt8</xsl:when>
		<xsl:when test="contains('char varchar nchar nvarchar binary varbinary uniqueidentifier String', string($data_type))">FieldTypeString</xsl:when>
		<xsl:when test="contains('datetime smalldatetime DateTime', string($data_type))">FieldTypeDateTime</xsl:when>
		<xsl:when test="contains('money smallmoney float real decimal numeric Decimal', string($data_type))">FieldTypeDecimal</xsl:when>
		<xsl:when test="contains('text ntext image', string($data_type))">FieldTypeByte</xsl:when>
		<xsl:when test="contains('bit Bool', string($data_type))">FieldTypeBool</xsl:when>
		<xsl:when test="contains('timestamp', string($data_type))">FieldTypeTimeStamp</xsl:when>
		<xsl:when test="contains('uniqueidentifier', string($data_type))">FieldTypeGuid</xsl:when>
		<xsl:otherwise>FieldTypeString</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template name="select_primitive_type">
	<xsl:param name="data_type"></xsl:param>
	<xsl:choose>
		<xsl:when test="contains('int smallint tinyint bigint Integer', string($data_type))">Integer</xsl:when>
		<xsl:when test="contains('char varchar nchar nvarchar binary varbinary uniqueidentifier String', string($data_type))">String</xsl:when>
		<xsl:when test="contains('datetime smalldatetime DateTime', string($data_type))">DateTime</xsl:when>
		<xsl:when test="contains('money smallmoney float real decimal numeric Decimal', string($data_type))">Decimal</xsl:when>
		<xsl:when test="contains('image', string($data_type))">Byte()</xsl:when>
		<xsl:when test="contains('bit Bool', string($data_type))">Bool</xsl:when>
		<xsl:when test="contains('text ntext', string($data_type))">String</xsl:when>
		<xsl:otherwise>String</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template name="select_primitive_type_CSharp">
	<xsl:param name="data_type"></xsl:param>
	<xsl:choose>
		<xsl:when test="contains('int smallint tinyint bigint Integer', string($data_type))">int</xsl:when>
		<xsl:when test="contains('char varchar nchar nvarchar binary varbinary uniqueidentifier String', string($data_type))">string</xsl:when>
		<xsl:when test="contains('datetime smalldatetime DateTime', string($data_type))">DateTime</xsl:when>
		<xsl:when test="contains('money smallmoney float real decimal numeric Decimal', string($data_type))">decimal</xsl:when>
		<xsl:when test="contains('image', string($data_type))">byte[]</xsl:when>
		<xsl:when test="contains('bit Bool', string($data_type))">bool</xsl:when>
		<xsl:when test="contains('sql_variant Variant', string($data_type))">object</xsl:when>
		<xsl:when test="contains('text ntext', string($data_type))">string</xsl:when>
		<xsl:otherwise>string</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template name="select_primitive_type_VisualBasic">
	<xsl:param name="data_type"></xsl:param>
	<xsl:choose>
		<xsl:when test="contains('int smallint tinyint bigint Integer', string($data_type))">Integer</xsl:when>
		<xsl:when test="contains('char varchar nchar nvarchar binary varbinary uniqueidentifier String', string($data_type))">String</xsl:when>
		<xsl:when test="contains('datetime smalldatetime DateTime', string($data_type))">DateTime</xsl:when>
		<xsl:when test="contains('money smallmoney float real decimal numeric Decimal', string($data_type))">Decimal</xsl:when>
		<xsl:when test="contains('image', string($data_type))">Byte()</xsl:when>
		<xsl:when test="contains('bit Bool', string($data_type))">Boolean</xsl:when>
		<xsl:when test="contains('sql_variant Variant', string($data_type))">Object</xsl:when>
		<xsl:when test="contains('text ntext', string($data_type))">String</xsl:when>
		<xsl:otherwise>String</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template name="select_primitive_type_VB60">
	<xsl:param name="data_type"></xsl:param>
	<xsl:choose>
		<xsl:when test="contains('int smallint tinyint bigint Integer', string($data_type))">Integer</xsl:when>
		<xsl:when test="contains('char varchar nchar nvarchar binary varbinary uniqueidentifier String', string($data_type))">String</xsl:when>
		<xsl:when test="contains('datetime smalldatetime DateTime', string($data_type))">Date</xsl:when>
		<xsl:when test="contains('money smallmoney float real decimal numeric Decimal', string($data_type))">Double</xsl:when>
		<xsl:when test="contains('image', string($data_type))">Byte()</xsl:when>
		<xsl:when test="contains('bit Bool', string($data_type))">Boolean</xsl:when>
		<xsl:when test="contains('sql_variant Variant', string($data_type))">Object</xsl:when>
		<xsl:when test="contains('text ntext', string($data_type))">String</xsl:when>
		<xsl:otherwise>String</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template name="select_primitive_type_cast_CSharp">
	<xsl:param name="data_type"></xsl:param>
	<xsl:param name="object"></xsl:param>
	<xsl:choose>
		<xsl:when test="contains('int smallint tinyint bigint Integer', string($data_type))">int.Parse(<xsl:value-of select="$object"/>.ToString())</xsl:when>
		<xsl:when test="contains('char varchar nchar nvarchar binary varbinary uniqueidentifier String', string($data_type))"><xsl:value-of select="$object"/>.ToString()</xsl:when>
		<xsl:when test="contains('datetime smalldatetime DateTime', string($data_type))">DateTime.Parse(<xsl:value-of select="$object"/>.ToString())</xsl:when>
		<xsl:when test="contains('money smallmoney float real decimal numeric Decimal', string($data_type))">decimal.Parse(<xsl:value-of select="$object"/>.ToString())</xsl:when>
		<xsl:when test="contains('image', string($data_type))"><xsl:value-of select="$object"/>.ToString()</xsl:when>
		<xsl:when test="contains('bit Bool', string($data_type))">bool.Parse(<xsl:value-of select="$object"/>.ToString())</xsl:when>
		<xsl:when test="contains('text ntext', string($data_type))">.ToString()</xsl:when>
		<xsl:otherwise><xsl:value-of select="$object"/>.ToString()</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template name="select_primitive_type_cast_VisualBasic">
	<xsl:param name="data_type"></xsl:param>
	<xsl:param name="object"></xsl:param>
	<xsl:choose>
		<xsl:when test="contains('int smallint tinyint bigint Integer', string($data_type))">CInt(<xsl:value-of select="$object"/>)</xsl:when>
		<xsl:when test="contains('char varchar nchar nvarchar binary varbinary uniqueidentifier String', string($data_type))"><xsl:value-of select="$object"/>.ToString()</xsl:when>
		<xsl:when test="contains('datetime smalldatetime DateTime', string($data_type))">CDate(<xsl:value-of select="$object"/>)</xsl:when>
		<xsl:when test="contains('money smallmoney float real decimal numeric Decimal', string($data_type))">CDec(<xsl:value-of select="$object"/>)</xsl:when>
		<xsl:when test="contains('image', string($data_type))">CByte(<xsl:value-of select="$object"/>)</xsl:when>
		<xsl:when test="contains('bit Bool', string($data_type))">CBool(<xsl:value-of select="$object"/>)</xsl:when>
		<xsl:when test="contains('text ntext', string($data_type))">.ToString()</xsl:when>
		<xsl:otherwise><xsl:value-of select="$object"/>.ToString()</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template name="select_primitive_type_cast_VB60">
	<xsl:param name="data_type"></xsl:param>
	<xsl:param name="object"></xsl:param>
	<xsl:choose>
		<xsl:when test="contains('int smallint tinyint bigint Integer', string($data_type))">CInt(<xsl:value-of select="$object"/>)</xsl:when>
		<xsl:when test="contains('char varchar nchar nvarchar binary varbinary uniqueidentifier String', string($data_type))">CStr(<xsl:value-of select="$object"/>)</xsl:when>
		<xsl:when test="contains('datetime smalldatetime DateTime', string($data_type))">CDate(<xsl:value-of select="$object"/>)</xsl:when>
		<xsl:when test="contains('money smallmoney float real decimal numeric Decimal', string($data_type))">CDbl(<xsl:value-of select="$object"/>)</xsl:when>
		<xsl:when test="contains('image', string($data_type))">CByte(<xsl:value-of select="$object"/>)</xsl:when>
		<xsl:when test="contains('bit Bool', string($data_type))">CBool(<xsl:value-of select="$object"/>)</xsl:when>
		<xsl:when test="contains('text ntext', string($data_type))">CStr(<xsl:value-of select="$object"/>)</xsl:when>
		<xsl:otherwise><xsl:value-of select="$object"/>CStr(<xsl:value-of select="$object"/>)</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template name="select_primitive_type_default_CSharp">
	<xsl:param name="data_type"></xsl:param>
	<xsl:choose>
		<xsl:when test="contains('int smallint tinyint bigint Integer', string($data_type))">-1</xsl:when>
		<xsl:when test="contains('char varchar nchar nvarchar binary varbinary uniqueidentifier String', string($data_type))">string.Empty</xsl:when>
		<xsl:when test="contains('datetime smalldatetime DateTime', string($data_type))">DateTime.Now</xsl:when>
		<xsl:when test="contains('money smallmoney float real decimal numeric Decimal', string($data_type))">-1</xsl:when>
		<xsl:when test="contains('image', string($data_type))">null</xsl:when>
		<xsl:when test="contains('bit Bool', string($data_type))">false</xsl:when>
		<xsl:when test="contains('sql_variant Variant', string($data_type))">null</xsl:when>
		<xsl:when test="contains('text ntext', string($data_type))">string.Empty</xsl:when>
		<xsl:otherwise>string.Empty</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template name="select_primitive_type_default_VisualBasic">
	<xsl:param name="data_type"></xsl:param>
	<xsl:choose>
		<xsl:when test="contains('int smallint tinyint bigint Integer', string($data_type))">-1</xsl:when>
		<xsl:when test="contains('char varchar nchar nvarchar binary varbinary uniqueidentifier String', string($data_type))">String.Empty</xsl:when>
		<xsl:when test="contains('datetime smalldatetime DateTime', string($data_type))">DateTime.Now</xsl:when>
		<xsl:when test="contains('money smallmoney float real decimal numeric Decimal', string($data_type))">-1</xsl:when>
		<xsl:when test="contains('image', string($data_type))">Nothing</xsl:when>
		<xsl:when test="contains('bit Bool', string($data_type))">False</xsl:when>
		<xsl:when test="contains('sql_variant Variant', string($data_type))">Nothing</xsl:when>
		<xsl:when test="contains('text ntext', string($data_type))">String.Empty</xsl:when>
		<xsl:otherwise>String.Empty</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template name="select_primitive_type_default_VB60">
	<xsl:param name="data_type"></xsl:param>
	<xsl:choose>
		<xsl:when test="contains('int smallint tinyint bigint Integer', string($data_type))">-1</xsl:when>
		<xsl:when test="contains('char varchar nchar nvarchar binary varbinary uniqueidentifier String', string($data_type))">""</xsl:when>
		<xsl:when test="contains('datetime smalldatetime DateTime', string($data_type))">Now</xsl:when>
		<xsl:when test="contains('money smallmoney float real decimal numeric Decimal', string($data_type))">-1</xsl:when>
		<xsl:when test="contains('image', string($data_type))">Nothing</xsl:when>
		<xsl:when test="contains('bit Bool', string($data_type))">False</xsl:when>
		<xsl:when test="contains('sql_variant Variant', string($data_type))">Nothing</xsl:when>
		<xsl:when test="contains('text ntext', string($data_type))">""</xsl:when>
		<xsl:otherwise>""</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template name="select_primitive_type_default_JScript">
	<xsl:param name="data_type"></xsl:param>
	<xsl:choose>
		<xsl:when test="contains('int smallint tinyint bigint Integer', string($data_type))">-1</xsl:when>
		<xsl:when test="contains('char varchar nchar nvarchar binary varbinary uniqueidentifier String', string($data_type))">""</xsl:when>
		<xsl:when test="contains('datetime smalldatetime DateTime', string($data_type))">new Date()</xsl:when>
		<xsl:when test="contains('money smallmoney float real decimal numeric Decimal', string($data_type))">-1</xsl:when>
		<xsl:when test="contains('image', string($data_type))">null</xsl:when>
		<xsl:when test="contains('bit Bool', string($data_type))">false</xsl:when>
		<xsl:when test="contains('sql_variant Variant', string($data_type))">null</xsl:when>
		<xsl:when test="contains('text ntext', string($data_type))">""</xsl:when>
		<xsl:otherwise>""</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template name="select_primitive_type_default_SQL">
	<xsl:param name="data_type"></xsl:param>
	<xsl:choose>
		<xsl:when test="contains('int smallint tinyint bigint Integer', string($data_type))">-1</xsl:when>
		<xsl:when test="contains('char varchar nchar nvarchar binary varbinary uniqueidentifier String', string($data_type))">NULL</xsl:when>
		<xsl:when test="contains('datetime smalldatetime DateTime', string($data_type))">NULL</xsl:when>
		<xsl:when test="contains('money smallmoney float real decimal numeric Decimal', string($data_type))">-1</xsl:when>
		<xsl:when test="contains('image', string($data_type))">NULL</xsl:when>
		<xsl:when test="contains('bit Bool', string($data_type))">0</xsl:when>
		<xsl:when test="contains('sql_variant Variant', string($data_type))">NULL</xsl:when>
		<xsl:when test="contains('text ntext', string($data_type))">NULL</xsl:when>
		<xsl:otherwise>NULL</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template name="select_primitive_type_comparison_SQL">
	<xsl:param name="data_type"></xsl:param>
	<xsl:param name="object"></xsl:param>
	<xsl:choose>
		<xsl:when test="contains('int smallint tinyint bigint Integer', string($data_type))"> = <xsl:value-of select="$object"/></xsl:when>
		<xsl:when test="contains('char varchar nchar nvarchar binary varbinary uniqueidentifier String', string($data_type))"> LIKE ''%'' + <xsl:value-of select="$object"/> + ''%''</xsl:when>
		<xsl:when test="contains('datetime smalldatetime DateTime', string($data_type))"> = <xsl:value-of select="$object"/></xsl:when>
		<xsl:when test="contains('money smallmoney float real decimal numeric Decimal', string($data_type))"> = <xsl:value-of select="$object"/></xsl:when>
		<xsl:when test="contains('image', string($data_type))"> = <xsl:value-of select="$object"/></xsl:when>
		<xsl:when test="contains('bit Bool', string($data_type))"> = <xsl:value-of select="$object"/></xsl:when>
		<xsl:when test="contains('sql_variant Variant', string($data_type))"> = <xsl:value-of select="$object"/></xsl:when>
		<xsl:when test="contains('text ntext', string($data_type))"> LIKE ''%'' + <xsl:value-of select="$object"/> + ''%''</xsl:when>
		<xsl:otherwise> = <xsl:value-of select="$object"/></xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template name="select_primitive_type_default_comparison_SQL">
	<xsl:param name="data_type"></xsl:param>
	<xsl:choose>
		<xsl:when test="contains('int smallint tinyint bigint Integer', string($data_type))"> != -1</xsl:when>
		<xsl:when test="contains('char varchar nchar nvarchar binary varbinary uniqueidentifier String', string($data_type))"> IS NOT NULL</xsl:when>
		<xsl:when test="contains('datetime smalldatetime DateTime', string($data_type))"> IS NOT NULL</xsl:when>
		<xsl:when test="contains('money smallmoney float real decimal numeric Decimal', string($data_type))"> != -1</xsl:when>
		<xsl:when test="contains('image', string($data_type))"> IS NOT NULL</xsl:when>
		<xsl:when test="contains('bit Bool', string($data_type))"> != 0</xsl:when>
		<xsl:when test="contains('sql_variant Variant', string($data_type))"> IS NOT NULL</xsl:when>
		<xsl:when test="contains('text ntext', string($data_type))"> IS NOT NULL</xsl:when>
		<xsl:otherwise> IS NOT NULL</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template name="select_primitive_type_parse_CSharp">
	<xsl:param name="data_type"></xsl:param>
	<xsl:param name="object"></xsl:param>
	<xsl:choose>
		<xsl:when test="contains('int smallint tinyint bigint Integer', string($data_type))">int.Parse(<xsl:value-of select="$object"/>.ToString())</xsl:when>
		<xsl:when test="contains('char varchar nchar nvarchar binary varbinary uniqueidentifier String', string($data_type))"><xsl:value-of select="$object"/></xsl:when>
		<xsl:when test="contains('datetime smalldatetime DateTime', string($data_type))"><xsl:value-of select="$object"/></xsl:when>
		<xsl:when test="contains('money smallmoney float real decimal numeric Decimal', string($data_type))">decimal.Parse(<xsl:value-of select="$object"/>.ToString())</xsl:when>
		<xsl:when test="contains('image', string($data_type))">(byte[])<xsl:value-of select="$object"/></xsl:when>
		<xsl:when test="contains('bit Bool', string($data_type))">(bool)<xsl:value-of select="$object"/></xsl:when>
		<xsl:when test="contains('text ntext', string($data_type))"><xsl:value-of select="$object"/></xsl:when>
		<xsl:otherwise><xsl:value-of select="$object"/>.ToString()</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template name="select_primitive_type_parse_VisualBasic">
	<xsl:param name="data_type"></xsl:param>
	<xsl:param name="object"></xsl:param>
	<xsl:choose>
		<xsl:when test="contains('int smallint tinyint bigint Integer', string($data_type))">CInt(<xsl:value-of select="$object"/>)</xsl:when>
		<xsl:when test="contains('char varchar nchar nvarchar binary varbinary uniqueidentifier String', string($data_type))"><xsl:value-of select="$object"/></xsl:when>
		<xsl:when test="contains('datetime smalldatetime DateTime', string($data_type))"><xsl:value-of select="$object"/></xsl:when>
		<xsl:when test="contains('money smallmoney float real decimal numeric Decimal', string($data_type))">CDec(<xsl:value-of select="$object"/>)</xsl:when>
		<xsl:when test="contains('image', string($data_type))">CByte(<xsl:value-of select="$object"/>)</xsl:when>
		<xsl:when test="contains('bit Bool', string($data_type))">CBool(<xsl:value-of select="$object"/>)</xsl:when>
		<xsl:when test="contains('text ntext', string($data_type))"><xsl:value-of select="$object"/></xsl:when>
		<xsl:otherwise><xsl:value-of select="$object"/>.ToString()</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template name="select_primitive_type_parse_VB60">
	<xsl:param name="data_type"></xsl:param>
	<xsl:param name="object"></xsl:param>
	<xsl:choose>
		<xsl:when test="contains('int smallint tinyint bigint Integer', string($data_type))">CInt(<xsl:value-of select="$object"/>)</xsl:when>
		<xsl:when test="contains('char varchar nchar nvarchar binary varbinary uniqueidentifier String', string($data_type))"><xsl:value-of select="$object"/></xsl:when>
		<xsl:when test="contains('datetime smalldatetime DateTime', string($data_type))"><xsl:value-of select="$object"/></xsl:when>
		<xsl:when test="contains('money smallmoney float real decimal numeric Decimal', string($data_type))">CDbl(<xsl:value-of select="$object"/>)</xsl:when>
		<xsl:when test="contains('image', string($data_type))">CByte(<xsl:value-of select="$object"/>)</xsl:when>
		<xsl:when test="contains('bit Bool', string($data_type))">CBool(<xsl:value-of select="$object"/>)</xsl:when>
		<xsl:when test="contains('text ntext', string($data_type))"><xsl:value-of select="$object"/></xsl:when>
		<xsl:otherwise><xsl:value-of select="$object"/>CStr(<xsl:value-of select="$object"/>)</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template name="select_primitive_type_ToString">
	<xsl:param name="data_type"></xsl:param>
	<xsl:param name="object"></xsl:param>
	<xsl:choose>
		<xsl:when test="contains('int smallint tinyint bigint Integer', string($data_type))"><xsl:value-of select="$object"/>.ToString()</xsl:when>
		<xsl:when test="contains('char varchar nchar nvarchar binary varbinary uniqueidentifier String', string($data_type))"><xsl:value-of select="$object"/></xsl:when>
		<xsl:when test="contains('datetime smalldatetime DateTime', string($data_type))"><xsl:value-of select="$object"/></xsl:when>
		<xsl:when test="contains('money smallmoney float real decimal numeric Decimal', string($data_type))"><xsl:value-of select="$object"/>.ToString()</xsl:when>
		<xsl:when test="contains('image', string($data_type))"><xsl:value-of select="$object"/></xsl:when>
		<xsl:when test="contains('bit Bool', string($data_type))"><xsl:value-of select="$object"/></xsl:when>
		<xsl:when test="contains('text ntext', string($data_type))"><xsl:value-of select="$object"/></xsl:when>
		<xsl:otherwise><xsl:value-of select="$object"/>.ToString()</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template name="select_primitive_type_SqlDbType">
	<xsl:param name="data_type"></xsl:param>
	<xsl:choose>
		<xsl:when test="contains('int smallint tinyint bigint Integer', string($data_type))">SqlDbType.Int</xsl:when>
		<xsl:when test="contains('char varchar nchar nvarchar binary varbinary uniqueidentifier String', string($data_type))">SqlDbType.VarChar</xsl:when>
		<xsl:when test="contains('datetime smalldatetime DateTime', string($data_type))">SqlDbType.DateTime</xsl:when>
		<xsl:when test="contains('money smallmoney float real decimal numeric Decimal', string($data_type))">SqlDbType.Decimal</xsl:when>
		<xsl:when test="contains('image', string($data_type))">SqlDbType.Binary</xsl:when>
		<xsl:when test="contains('bit Bool', string($data_type))">SqlDbType.Bit</xsl:when>
		<xsl:when test="contains('sql_variant Variant', string($data_type))">SqlDbType.Variant</xsl:when>
		<xsl:when test="contains('text', string($data_type))">SqlDbType.Text</xsl:when>
		<xsl:when test="contains('ntext', string($data_type))">SqlDbType.NText</xsl:when>
		<xsl:otherwise>string</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template name="select_primitive_type_OleDbType">
	<xsl:param name="data_type"></xsl:param>
	<xsl:choose>
		<xsl:when test="contains('int smallint tinyint bigint Integer', string($data_type))">OleDbType.Integer</xsl:when>
		<xsl:when test="contains('char varchar nchar nvarchar binary varbinary uniqueidentifier String', string($data_type))">OleDbType.VarChar</xsl:when>
		<xsl:when test="contains('datetime smalldatetime DateTime', string($data_type))">OleDbType.Date</xsl:when>
		<xsl:when test="contains('money smallmoney float real decimal numeric Decimal', string($data_type))">OleDbType.Decimal</xsl:when>
		<xsl:when test="contains('text ntext image', string($data_type))">OleDbType.VarBinary</xsl:when>
		<xsl:when test="contains('bit Bool', string($data_type))">OleDbType.Boolean</xsl:when>
		<xsl:when test="contains('sql_variant Variant', string($data_type))">OleDbType.Variant</xsl:when>
		<xsl:otherwise>string</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template name="select_primitive_type_OdbcType">
	<xsl:param name="data_type"></xsl:param>
	<xsl:choose>
		<xsl:when test="contains('int smallint tinyint bigint Integer', string($data_type))">OdbcType.Int</xsl:when>
		<xsl:when test="contains('char varchar nchar nvarchar binary varbinary uniqueidentifier String', string($data_type))">OdbcType.VarChar</xsl:when>
		<xsl:when test="contains('datetime smalldatetime DateTime', string($data_type))">OdbcType.DateTime</xsl:when>
		<xsl:when test="contains('money smallmoney float real decimal numeric Decimal', string($data_type))">OdbcType.Decimal</xsl:when>
		<xsl:when test="contains('text ntext image', string($data_type))">OdbcType.VarBinary</xsl:when>
		<xsl:when test="contains('bit Bool', string($data_type))">OdbcType.Bit</xsl:when>
		<xsl:when test="contains('sql_variant Variant', string($data_type))">OdbcType.Variant</xsl:when>
		<xsl:otherwise>string</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template name="select_primitive_type_ADODBType">
	<xsl:param name="data_type"></xsl:param>
	<xsl:choose>
		<xsl:when test="contains('int smallint tinyint bigint Integer', string($data_type))">adInteger</xsl:when>
		<xsl:when test="contains('char varchar nchar nvarchar binary varbinary uniqueidentifier String', string($data_type))">adVarChar</xsl:when>
		<xsl:when test="contains('datetime smalldatetime DateTime', string($data_type))">adDate</xsl:when>
		<xsl:when test="contains('money smallmoney float real decimal numeric Decimal', string($data_type))">adDecimal</xsl:when>
		<xsl:when test="contains('text ntext image', string($data_type))">adBinary</xsl:when>
		<xsl:when test="contains('bit Bool', string($data_type))">adBoolean</xsl:when>
		<xsl:when test="contains('sql_variant Variant', string($data_type))">adVariant</xsl:when>
		<xsl:otherwise>adVariant</xsl:otherwise>
	</xsl:choose>
</xsl:template>

<xsl:template name="select_primitive_type_msdata">
	<xsl:param name="data_type"></xsl:param>
	<xsl:choose>
		<xsl:when test="contains('int smallint tinyint bigint Integer', string($data_type))">int</xsl:when>
		<xsl:when test="contains('char varchar nchar nvarchar binary varbinary uniqueidentifier String', string($data_type))">string</xsl:when>
		<xsl:when test="contains('datetime smalldatetime DateTime', string($data_type))">dateTime</xsl:when>
		<xsl:when test="contains('money smallmoney float real decimal numeric Decimal', string($data_type))">double</xsl:when>
		<xsl:when test="contains('text ntext image', string($data_type))">string</xsl:when>
		<xsl:when test="contains('bit Bool', string($data_type))">boolean</xsl:when>
		<xsl:otherwise></xsl:otherwise>
	</xsl:choose>
</xsl:template>

</xsl:stylesheet>

  