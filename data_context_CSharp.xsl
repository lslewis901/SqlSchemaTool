<?xsl version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:include href="type.xsl"/>
<xsl:output method="xml" omit-xml-declaration="yes"/>
<xsl:preserve-space elements="*" />

<xsl:param name="db_name"></xsl:param>
<xsl:param name="use_mapping">0</xsl:param>
<xsl:param name="base_table">BaseTable</xsl:param>
<xsl:param name="view">View</xsl:param>
<xsl:param name="procedure">Procedure</xsl:param>
<xsl:param name="function">Function</xsl:param>
<xsl:param name="rowset">Rowset</xsl:param>
<xsl:param name="return_value">ReturnValue</xsl:param>
<xsl:param name="table">TABLE</xsl:param>

<xsl:template match="/">namespace <xsl:value-of select="$db_name"/>
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Reflection;
	using System.Query;
	using System.Expressions;
	using System.Data;
	using System.Data.DLinq;


	public partial class <xsl:value-of select="$db_name"/>DataContext : DataContext 
	{
		public <xsl:value-of select="$db_name"/>DataContext(string connection) : base(connection)
		{
		}
  
		public <xsl:value-of select="$db_name"/>DataContext(System.Data.IDbConnection connection) : base(connection) 
		{
		}
  
		public <xsl:value-of select="$db_name"/>DataContext(string connection, System.Data.DLinq.MappingSource mappingSource) : base(connection, mappingSource) 
		{
		}
  
		public <xsl:value-of select="$db_name"/>DataContext(System.Data.IDbConnection connection, System.Data.DLinq.MappingSource mappingSource) : base(connection, mappingSource) 
		{
		}

	<xsl:apply-templates select="/dbobjects/dbobject[@type=$base_table]" mode="table"/>
	<xsl:apply-templates select="/dbobjects/dbobject[@type=$view]" mode="table"/>
	<xsl:apply-templates select="/dbobjects/dbobject[@type=$procedure and @procedure_type=$rowset]" mode="routine_rowset"/>
	<xsl:apply-templates select="/dbobjects/dbobject[@type=$procedure and @procedure_type=$return_value]" mode="routine_return_value"/>
	<xsl:apply-templates select="/dbobjects/dbobject[@type=$function and @data_type!=$table]" mode="scalar_function"/>
	<xsl:apply-templates select="/dbobjects/dbobject[@type=$function and @data_type=$table]" mode="table_function"/>
	}
}

</xsl:template>

<xsl:template match="dbobject" mode="table">
		public Table<xsl:value-of select="'&lt;'" disable-output-escaping="yes"/><xsl:value-of select="@friendly_name"/><xsl:value-of select="'&gt;'" disable-output-escaping="yes"/>&#9;<xsl:value-of select="@friendly_name"/>;
</xsl:template>

<xsl:template match="dbobject" mode="routine_rowset">
		<xsl:if test="$use_mapping!=1">[StoredProcedure(Name="<xsl:value-of select="@name"/>")]</xsl:if>
		public StoredProcedureResult<xsl:value-of select="'&lt;'" disable-output-escaping="yes"/><xsl:value-of select="@friendly_name"/>Rowset<xsl:value-of select="'&gt;'" disable-output-escaping="yes"/>	<xsl:value-of select="@friendly_name"/>(<xsl:apply-templates select="parameter[@parameter_name!='']" mode="list1"><xsl:sort select="@ordinal_position"/></xsl:apply-templates>)
		{
			return this.ExecuteStoredProcedure<xsl:value-of select="'&lt;'" disable-output-escaping="yes"/><xsl:value-of select="@friendly_name"/>Rowset<xsl:value-of select="'&gt;'" disable-output-escaping="yes"/>(((MethodInfo)(MethodInfo.GetCurrentMethod()))<xsl:if test="parameter[@parameter_name!='']">, </xsl:if><xsl:apply-templates select="parameter[@parameter_name!='']" mode="list2"><xsl:sort select="@ordinal_position"/></xsl:apply-templates>);
		}
</xsl:template>

<xsl:template match="dbobject" mode="routine_return_value">
		<xsl:if test="$use_mapping!=1">[StoredProcedure(Name="<xsl:value-of select="@name"/>")]</xsl:if>
		public int <xsl:value-of select="@friendly_name"/>(<xsl:apply-templates select="parameter[@parameter_name!='']" mode="list1"><xsl:sort select="@ordinal_position"/></xsl:apply-templates>)
		{
			StoredProcedureResult result = this.ExecuteStoredProcedure(((MethodInfo)(MethodInfo.GetCurrentMethod()))<xsl:if test="parameter[@parameter_name!='']">, </xsl:if><xsl:apply-templates select="parameter[@parameter_name!='']" mode="list2"><xsl:sort select="@ordinal_position"/></xsl:apply-templates>);
			return result.ReturnValue.Value;
		}
</xsl:template>

<xsl:template match="dbobject" mode="scalar_function">
	<xsl:variable name="return_type"><xsl:call-template name="select_primitive_type_CSharp"><xsl:with-param name="data_type" select="@data_type"/></xsl:call-template></xsl:variable>
		<xsl:if test="$use_mapping!=1">[Function(Name="[dbo].[<xsl:value-of select="@name"/>]")]</xsl:if>
		public <xsl:value-of select="$return_type"/>&#9;<xsl:value-of select="@friendly_name"/>(<xsl:apply-templates select="parameter[@parameter_name!='']" mode="list11"><xsl:sort select="@ordinal_position"/></xsl:apply-templates>)
		{
			MethodCallExpression mc = Expression.Call(((MethodInfo)(MethodInfo.GetCurrentMethod())), Expression.Constant(this), <xsl:choose><xsl:when test="not(parameter[@parameter_name!=''])">null</xsl:when><xsl:otherwise> new Expression[]
				{
				<xsl:apply-templates select="parameter[@parameter_name!='']" mode="list3">
					<xsl:sort select="@ordinal_position"/>
				</xsl:apply-templates>
				}
				</xsl:otherwise></xsl:choose> 
			);
			return Sequence.Single(this.CreateQuery<xsl:value-of select="'&lt;'" disable-output-escaping="yes"/><xsl:value-of select="$return_type"/><xsl:value-of select="'&gt;'" disable-output-escaping="yes"/>(mc));
		}
</xsl:template>

<xsl:template match="dbobject" mode="table_function">
		<xsl:if test="$use_mapping!=1">[Function(Name="[dbo].[<xsl:value-of select="@name"/>]")]</xsl:if>
		public IQueryable<xsl:value-of select="'&lt;'" disable-output-escaping="yes"/><xsl:value-of select="@friendly_name"/>Rowset<xsl:value-of select="'&gt;'" disable-output-escaping="yes"/> <xsl:value-of select="@friendly_name"/>(<xsl:apply-templates select="parameter[@parameter_name!='']" mode="list11"><xsl:sort select="@ordinal_position"/></xsl:apply-templates>)
		{
			MethodCallExpression mc = Expression.Call(((MethodInfo)(MethodInfo.GetCurrentMethod())), Expression.Constant(this), <xsl:choose><xsl:when test="not(parameter[@parameter_name!=''])">null</xsl:when><xsl:otherwise>new Expression[]
				{
				<xsl:apply-templates select="parameter[@parameter_name!='']" mode="list3">
					<xsl:sort select="@ordinal_position"/>
				</xsl:apply-templates>
				}
				</xsl:otherwise></xsl:choose> 
			);
			return this.CreateQuery<xsl:value-of select="'&lt;'" disable-output-escaping="yes"/><xsl:value-of select="@friendly_name"/>Rowset<xsl:value-of select="'&gt;'" disable-output-escaping="yes"/>(mc);
		}
</xsl:template>

<xsl:template match="parameter" mode="list1">
	<xsl:if test="position() > 1">, </xsl:if><xsl:if test="$use_mapping!=1">[Parameter(Name="<xsl:value-of select="@parameter_name"/>", DBType="<xsl:value-of select="@data_type"/><xsl:call-template name="data-length-precision-scale2"><xsl:with-param name="data-type" select="@data_type"/><xsl:with-param name="length" select="@character_maximum_length"/><xsl:with-param name="precision" select="@numeric_precision"/><xsl:with-param name="scale" select="@numeric_scale"/></xsl:call-template>")]</xsl:if> <xsl:if test="@is_nullable='YES'">	System.Nullable<xsl:value-of select="'&lt;'" disable-output-escaping="yes" /></xsl:if><xsl:call-template name="select_primitive_type_CSharp"><xsl:with-param name="data_type" select="@data_type"/></xsl:call-template><xsl:if test="@is_nullable='YES'"><xsl:value-of select="'&gt;'" disable-output-escaping="yes" /></xsl:if>&#9;<xsl:value-of select="@parameter_friendly_name"/>
</xsl:template>

<xsl:template match="parameter" mode="list11">
	<xsl:if test="position() > 1">, </xsl:if><xsl:if test="$use_mapping!=1">[Parameter(Name="<xsl:value-of select="@parameter_name"/>")]</xsl:if> <xsl:if test="@is_nullable='YES'">	System.Nullable<xsl:value-of select="'&lt;'" disable-output-escaping="yes" /></xsl:if><xsl:call-template name="select_primitive_type_CSharp"><xsl:with-param name="data_type" select="@data_type"/></xsl:call-template><xsl:if test="@is_nullable='YES'"><xsl:value-of select="'&gt;'" disable-output-escaping="yes" /></xsl:if>&#9;<xsl:value-of select="@parameter_friendly_name"/>
</xsl:template>

<xsl:template match="parameter" mode="list2">
	<xsl:if test="position() > 1">, </xsl:if><xsl:value-of select="@parameter_friendly_name"/>
</xsl:template>

<xsl:template match="parameter" mode="list3">
	<xsl:if test="position() > 1">, </xsl:if>Expression.Constant(<xsl:value-of select="@parameter_friendly_name"/>, typeof(<xsl:call-template name="select_primitive_type_CSharp"><xsl:with-param name="data_type" select="@data_type"/></xsl:call-template>))
</xsl:template>

</xsl:stylesheet>