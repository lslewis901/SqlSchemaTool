<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:include href="type.xsl"/>
<xsl:output method="xml" omit-xml-declaration="yes"/>
<xsl:preserve-space elements="*" />

<xsl:param name="auto_increment" select="boolean(/table/column[@identity=1])"/>
<xsl:param name="pk">PRIMARY KEY</xsl:param>
<xsl:param name="fk">FOREIGN KEY</xsl:param>
<xsl:param name="is_one_file">0</xsl:param>
<xsl:param name="db_name"></xsl:param>
<xsl:param name="use_mapping">0</xsl:param>

<xsl:template match="/">
	<xsl:if test="$is_one_file=0">namespace <xsl:value-of select="$db_name"/>
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Reflection;
	using System.Query;
	using System.Expressions;
	using System.Data;
	using System.Data.DLinq;
	
	</xsl:if>
	<xsl:if test="$use_mapping!=1">[Table(Name="<xsl:value-of select="/table/@name"/>")]</xsl:if>
	public partial class <xsl:value-of select="/table/@friendly_name"/> : System.Data.DLinq.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		public <xsl:value-of select="/table/@friendly_name"/>()
		{
		<xsl:apply-templates select="/table/assoc" mode="init"/>
		<xsl:apply-templates select="//constraint[@constraint_type=$fk]" mode="init"/>
		}
        
		#region Private members - columns
        
		<xsl:apply-templates select="/table/column" mode="member"/>
		
		#endregion

		#region Public properties - columns
		
		<xsl:apply-templates select="/table/column" mode="property"/>

		#endregion

		#region Private members - entities referring this entity

		<xsl:apply-templates select="/table/assoc" mode="member"/>

		#endregion

		#region Public properties - entities referring this entity

		<xsl:apply-templates select="/table/assoc" mode="property"/>

		#endregion

		#region Private members - entities referred by this entity

		<xsl:apply-templates select="//constraint[@constraint_type=$fk]" mode="member"/>

		#endregion

		#region Public properties - entities referred by this entity

		<xsl:apply-templates select="//constraint[@constraint_type=$fk]" mode="property"/>

		#endregion

		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanging;
  
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
  
		protected virtual void OnPropertyChanging(string PropertyName)
		{
			if(this.PropertyChanging != null)
			{
				this.PropertyChanging(this, new PropertyChangedEventArgs(PropertyName));
			}
		}
  
		protected virtual void OnPropertyChanged(string PropertyName) 
		{
			if(this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
			}
		}

		<xsl:apply-templates select="/table/assoc" mode="attach_detach"/>

	}

		<xsl:if test="$is_one_file=0">
}
		</xsl:if>
	</xsl:template>

<xsl:template match="column" mode="member">
	<xsl:variable name="type"><xsl:call-template name="select_primitive_type_CSharp"><xsl:with-param name="data_type" select="@data_type"/></xsl:call-template></xsl:variable>
		private <xsl:if test="@is_nullable='YES' and not($type='string' or $type='object' or $type='byte[]')">System.Nullable<xsl:value-of select="'&lt;'" disable-output-escaping="yes" /></xsl:if><xsl:value-of select="$type"/><xsl:if test="@is_nullable='YES' and not($type='string' or $type='object' or $type='byte[]')"><xsl:value-of select="'&gt;'" disable-output-escaping="yes" /></xsl:if>		x<xsl:value-of select="@column_friendly_name"/>;
</xsl:template>

<xsl:template match="column" mode="property">
	<xsl:variable name="type"><xsl:call-template name="select_primitive_type_CSharp"><xsl:with-param name="data_type" select="@data_type"/></xsl:call-template></xsl:variable>
	<xsl:variable name="sql_type_attr"><xsl:call-template name="data-length-precision-scale2"><xsl:with-param name="data-type" select="@data_type"/><xsl:with-param name="length" select="@character_maximum_length"/><xsl:with-param name="precision" select="@numeric_precision"/><xsl:with-param name="scale" select="@numeric_scale"/></xsl:call-template></xsl:variable>
		<xsl:if test="$use_mapping!=1">[Column(Storage="x<xsl:value-of select="@column_friendly_name"/>", DBType="<xsl:value-of select="@data_type"/><xsl:value-of select="$sql_type_attr"/><xsl:if test="@is_nullable!='YES'"> NOT NULL</xsl:if><xsl:if test="@identity=1"> IDENTITY</xsl:if>"<xsl:if test="constraint[@constraint_type=$pk]">, Id=true</xsl:if><xsl:if test="@identity=1">, AutoGen=true</xsl:if>)]</xsl:if>
		public <xsl:if test="@is_nullable='YES' and not($type='string' or $type='object' or $type='byte[]')">System.Nullable<xsl:value-of select="'&lt;'" disable-output-escaping="yes" /></xsl:if><xsl:value-of select="$type"/><xsl:if test="@is_nullable='YES' and not($type='string' or $type='object' or $type='byte[]')"><xsl:value-of select="'&gt;'" disable-output-escaping="yes" /></xsl:if>&#160;&#160;<xsl:value-of select="@column_friendly_name"/>
		{
			get
			{
				return x<xsl:value-of select="@column_friendly_name"/>;
			}
			set
			{
				if(this.x<xsl:value-of select="@column_friendly_name"/> != value)
				{
					this.OnPropertyChanging("<xsl:value-of select="@column_friendly_name"/>");
					this.x<xsl:value-of select="@column_friendly_name"/> = value;
					this.OnPropertyChanged("<xsl:value-of select="@column_friendly_name"/>");
				}
			}
		}
</xsl:template>

<xsl:template match="assoc" mode="member">
		private EntitySet<xsl:value-of select="'&lt;'" disable-output-escaping="yes"/><xsl:value-of select="@assoc_table_friendly_name"/><xsl:value-of select="'&gt;'" disable-output-escaping="yes"/> x<xsl:value-of select="@assoc_table_friendly_name"/>EntitySet;
</xsl:template>

<xsl:template match="assoc" mode="property">
		<xsl:if test="$use_mapping!=1">[Association(Name="<xsl:value-of select="@constraint_name"/>", Storage="x<xsl:value-of select="@assoc_table_friendly_name"/>EntitySet", OtherKey="<xsl:value-of select="@assoc_column_friendly_name"/>")]</xsl:if>
		public EntitySet<xsl:value-of select="'&lt;'" disable-output-escaping="yes"/><xsl:value-of select="@assoc_table_friendly_name"/><xsl:value-of select="'&gt;'" disable-output-escaping="yes"/>&#9;<xsl:value-of select="@assoc_table_friendly_name"/>EntitySet
		{
			get
			{
				return this.x<xsl:value-of select="@assoc_table_friendly_name"/>EntitySet;
			}
			set
			{
				this.x<xsl:value-of select="@assoc_table_friendly_name"/>EntitySet.Assign(value);
			}
		}
</xsl:template>

<xsl:template match="constraint" mode="member">
		private EntityRef<xsl:value-of select="'&lt;'" disable-output-escaping="yes"/><xsl:value-of select="@referenced_table_friendly_name"/><xsl:value-of select="'&gt;'" disable-output-escaping="yes"/> x<xsl:value-of select="@referenced_table_friendly_name"/>EntityRef;
</xsl:template>

<xsl:template match="constraint" mode="property">
		<xsl:if test="$use_mapping!=1">[Association(Name="<xsl:value-of select="@constraint_name"/>", Storage="x<xsl:value-of select="@referenced_table_friendly_name"/>EntityRef", ThisKey="<xsl:value-of select="../@column_friendly_name"/>", IsParent=true)]</xsl:if>
		public <xsl:value-of select="@referenced_table_friendly_name"/>&#9;<xsl:value-of select="@referenced_table_friendly_name"/>EntityRef
		{
			get
			{
				return this.x<xsl:value-of select="@referenced_table_friendly_name"/>EntityRef.Entity;
			}
			set
			{
				<xsl:value-of select="@referenced_table_friendly_name"/> v = this.x<xsl:value-of select="@referenced_table_friendly_name"/>EntityRef.Entity;
				if(v != value)
				{
					this.OnPropertyChanging("<xsl:value-of select="@referenced_table_friendly_name"/>EntityRef");
					if(v != null)
					{
						this.x<xsl:value-of select="@referenced_table_friendly_name"/>EntityRef.Entity = null;
						v.<xsl:value-of select="../../@friendly_name"/>EntitySet.Remove(this);
					}
					this.x<xsl:value-of select="@referenced_table_friendly_name"/>EntityRef.Entity = value;
					if(value != null)
					{
						value.<xsl:value-of select="../../@friendly_name"/>EntitySet.Add(this);
					}
					this.OnPropertyChanged("<xsl:value-of select="@referenced_table_friendly_name"/>EntityRef");
				}
			}
		}
</xsl:template>

<xsl:template match="assoc" mode="attach_detach">
		private void attach_<xsl:value-of select="@assoc_table_friendly_name"/>EntitySet(<xsl:value-of select="@assoc_table_friendly_name"/> entity)
		{
			this.OnPropertyChanging(null);
			entity.<xsl:value-of select="../@friendly_name"/>EntityRef = this;
			this.OnPropertyChanged(null);
		}
  
		private void detach_<xsl:value-of select="@assoc_table_friendly_name"/>EntitySet(<xsl:value-of select="@assoc_table_friendly_name"/> entity)
		{
			this.OnPropertyChanging(null);
			entity.<xsl:value-of select="../@friendly_name"/>EntityRef = null;
			this.OnPropertyChanged(null);
		}
</xsl:template>

<xsl:template match="assoc" mode="init">
			this.x<xsl:value-of select="@assoc_table_friendly_name"/>EntitySet = new EntitySet<xsl:value-of select="'&lt;'" disable-output-escaping="yes"/><xsl:value-of select="@assoc_table_friendly_name"/><xsl:value-of select="'&gt;'" disable-output-escaping="yes"/>(new Notification<xsl:value-of select="'&lt;'" disable-output-escaping="yes"/><xsl:value-of select="@assoc_table_friendly_name"/><xsl:value-of select="'&gt;'" disable-output-escaping="yes"/>(this.attach_<xsl:value-of select="@assoc_table_friendly_name"/>EntitySet), new Notification<xsl:value-of select="'&lt;'" disable-output-escaping="yes"/><xsl:value-of select="@assoc_table_friendly_name"/><xsl:value-of select="'&gt;'" disable-output-escaping="yes"/>(this.detach_<xsl:value-of select="@assoc_table_friendly_name"/>EntitySet));
</xsl:template>

<xsl:template match="constraint" mode="init">
			this.x<xsl:value-of select="@referenced_table_friendly_name"/>EntityRef = default(EntityRef<xsl:value-of select="'&lt;'" disable-output-escaping="yes"/><xsl:value-of select="@referenced_table_friendly_name"/><xsl:value-of select="'&gt;'" disable-output-escaping="yes"/>);
</xsl:template>

</xsl:stylesheet>


