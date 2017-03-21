<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

	<xsl:template match="DataBase_Schema">
		<xsl:apply-templates select="VIEW"/>
	</xsl:template>

	<!-- template to output View statements -->
	<xsl:template match="VIEW">
		<xsl:if test="@Action='Add'">
	if exists (select * from dbo.sysobjects where id = object_id(N'[<xsl:value-of select="TABLE_OWNER"/>].[<xsl:value-of select="VIEW_NAME"/>]') and OBJECTPROPERTY(id, N'IsView') = 1)
	BEGIN
		DROP VIEW [<xsl:value-of select="TABLE_OWNER"/>].[<xsl:value-of select="VIEW_NAME"/>]
	END
	GO

		<xsl:for-each select="VIEW_DEPENDS[string-length(depname)>0]">
			<xsl:if test="position()=1">if (</xsl:if> OBJECT_ID(N'<xsl:value-of select="depname"/>') IS NULL <xsl:if test="position()!=last()">OR</xsl:if> <xsl:if test="position()=last()"> )
		BEGIN
			RAISERROR ('Required view object dependency is missing, unable to create view', 16, 1)
		END

			</xsl:if>
		</xsl:for-each> 
	SET QUOTED_IDENTIFIER, ANSI_NULLS ON 
	GO

				<xsl:call-template name="CREATE_Text"><xsl:with-param name="elem" select="CREATE_TEXT"/></xsl:call-template>

		</xsl:if>
		<xsl:if test="@Action='Alter'">
			<xsl:text> 
			</xsl:text>
	SET QUOTED_IDENTIFIER, ANSI_NULLS ON 
	GO

			<xsl:call-template name="ALTER_Text"><xsl:with-param name="elem" select="CREATE_TEXT"/><xsl:with-param name="type">VIEW</xsl:with-param></xsl:call-template>
		</xsl:if>
	GO

	SET QUOTED_IDENTIFIER, ANSI_NULLS OFF
	GO
	</xsl:template>

	<!-- output unchanged create text -->
	<xsl:template name="CREATE_Text">
		<xsl:param name="elem"/>
		<xsl:for-each select="$elem">
			<xsl:call-template name="dblquot-replace">
				<xsl:with-param name="string">
					<xsl:value-of disable-output-escaping="yes" select="Text"/>
				</xsl:with-param>
				<xsl:with-param name="counter" select="0"/>
			</xsl:call-template>
		</xsl:for-each>
	</xsl:template>

	<!-- replace 'create' with 'alter' for the passed in type, VIEW, PROCEDURE, FUNCTION, TRIGGER -->
	<xsl:template name="ALTER_Text">
		<xsl:param name="elem"/>
		<xsl:param name="type"/>
		
	<!-- parse all versions of standard case for key words -->
		<xsl:variable name="_lowertype">
			<xsl:call-template name='convertcase'>
				<xsl:with-param name='toconvert' select='$type' />
				<xsl:with-param name='conversion' select="string('lower')" />
			</xsl:call-template>
		</xsl:variable>
		
		<xsl:variable name="_propertype">
			<xsl:call-template name='convertcase'>
				<xsl:with-param name='toconvert' select='$type' />
				<xsl:with-param name='conversion' select="string('proper')" />
			</xsl:call-template>
		</xsl:variable>
		
		<xsl:variable name="_UpperTypeIn"><xsl:value-of select="concat('CREATE ',$type)"/></xsl:variable>
		<xsl:variable name="_UpperTypeOut"><xsl:value-of select="concat('ALTER ',$type)"/></xsl:variable>
		<xsl:variable name="_LowerTypeIn"><xsl:value-of select="concat('create ',$_lowertype)"/></xsl:variable>
		<xsl:variable name="_ProperTypeIn"><xsl:value-of select="concat('Create ',$_propertype)"/></xsl:variable>
		<xsl:variable name="_MixedType1In"><xsl:value-of select="concat('CREATE ',$_lowertype)"/></xsl:variable>
		<xsl:variable name="_MixedType2In"><xsl:value-of select="concat('Create ',$_lowertype)"/></xsl:variable>
		<xsl:variable name="_MixedType3In"><xsl:value-of select="concat('Create ',$type)"/></xsl:variable>
		<xsl:variable name="_MixedType4In"><xsl:value-of select="concat('create ',$type)"/></xsl:variable>
		<xsl:variable name="_MixedType5In"><xsl:value-of select="concat('CREATE ',$_propertype)"/></xsl:variable>
		<xsl:variable name="_MixedType6In"><xsl:value-of select="concat('create ',$_propertype)"/></xsl:variable>
		
		<xsl:for-each select="$elem">
			<xsl:variable name="_firstPass">
				<xsl:call-template name="StartsWithReplace">
					<xsl:with-param name="stringIn" select="Text"/>
					<xsl:with-param name="substringIn" select="$_UpperTypeIn"/>
					<xsl:with-param name="substringOut" select="$_UpperTypeOut"/>
					<xsl:with-param name="once" select="0"/>
				</xsl:call-template>
			</xsl:variable>

			<xsl:variable name="_secondPass">
				<xsl:call-template name="StartsWithReplace">
					<xsl:with-param name="stringIn" select="$_firstPass"/>
					<xsl:with-param name="substringIn" select="$_LowerTypeIn"/>
					<xsl:with-param name="substringOut" select="$_UpperTypeOut"/>
					<xsl:with-param name="once" select="0"/>
				</xsl:call-template>
			</xsl:variable>
			
			<xsl:variable name="_thirdPass">
				<xsl:call-template name="StartsWithReplace">
					<xsl:with-param name="stringIn" select="$_secondPass"/>
					<xsl:with-param name="substringIn" select="$_ProperTypeIn"/>
					<xsl:with-param name="substringOut" select="$_UpperTypeOut"/>
					<xsl:with-param name="once" select="0"/>
				</xsl:call-template>
			</xsl:variable>

			<xsl:variable name="_fourthPass">
				<xsl:call-template name="StartsWithReplace">
					<xsl:with-param name="stringIn" select="$_thirdPass"/>
					<xsl:with-param name="substringIn" select="$_MixedType1In"/>
					<xsl:with-param name="substringOut" select="$_UpperTypeOut"/>
					<xsl:with-param name="once" select="0"/>
				</xsl:call-template>
			</xsl:variable>

			<xsl:variable name="_fifthPass">
				<xsl:call-template name="StartsWithReplace">
					<xsl:with-param name="stringIn" select="$_fourthPass"/>
					<xsl:with-param name="substringIn" select="$_MixedType2In"/>
					<xsl:with-param name="substringOut" select="$_UpperTypeOut"/>
					<xsl:with-param name="once" select="0"/>
				</xsl:call-template>
			</xsl:variable>

			<xsl:variable name="_sixthPass">
				<xsl:call-template name="StartsWithReplace">
					<xsl:with-param name="stringIn" select="$_fifthPass"/>
					<xsl:with-param name="substringIn" select="$_MixedType3In"/>
					<xsl:with-param name="substringOut" select="$_UpperTypeOut"/>
					<xsl:with-param name="once" select="0"/>
				</xsl:call-template>
			</xsl:variable>

			<xsl:variable name="_seventhPass">
				<xsl:call-template name="StartsWithReplace">
					<xsl:with-param name="stringIn" select="$_sixthPass"/>
					<xsl:with-param name="substringIn" select="$_MixedType4In"/>
					<xsl:with-param name="substringOut" select="$_UpperTypeOut"/>
					<xsl:with-param name="once" select="0"/>
				</xsl:call-template>
			</xsl:variable>

			<xsl:variable name="_eighthPass">
				<xsl:call-template name="StartsWithReplace">
					<xsl:with-param name="stringIn" select="$_seventhPass"/>
					<xsl:with-param name="substringIn" select="$_MixedType5In"/>
					<xsl:with-param name="substringOut" select="$_UpperTypeOut"/>
					<xsl:with-param name="once" select="0"/>
				</xsl:call-template>
			</xsl:variable>

			<xsl:variable name="_ninethPass">
				<xsl:call-template name="StartsWithReplace">
					<xsl:with-param name="stringIn" select="$_eighthPass"/>
					<xsl:with-param name="substringIn" select="$_MixedType6In"/>
					<xsl:with-param name="substringOut" select="$_UpperTypeOut"/>
					<xsl:with-param name="once" select="0"/>
				</xsl:call-template>
			</xsl:variable>

			<xsl:call-template name="dblquot-replace">
				<xsl:with-param name="string">
					<xsl:value-of disable-output-escaping="yes" select="$_firstPass"/>
				</xsl:with-param>
				<xsl:with-param name="counter" select="0"/>
			</xsl:call-template>			
			<xsl:text> </xsl:text>
		</xsl:for-each>
	</xsl:template>

	<!-- functional equivalent of starts with / replace substring method -->
		<xsl:template name="StartsWithReplace">
			<xsl:param name="stringIn" /> 
			<xsl:param name="substringIn" /> 
			<xsl:param name="substringOut" />
			<xsl:param name="once" />
			<xsl:variable name="spacedIn" select="normalize-space($stringIn)"/>
			<xsl:choose>
				<xsl:when test="starts-with($spacedIn,$substringIn) and $once=0">
					<xsl:value-of select="concat(substring-before($spacedIn,$substringIn),$substringOut)" /> 
					<xsl:call-template name="SubstringReplace">
						<xsl:with-param name="stringIn" select="substring-after($spacedIn,$substringIn)" /> 
						<xsl:with-param name="substringIn" select="$substringIn" /> 
						<xsl:with-param name="substringOut" select="$substringOut" /> 
						<xsl:with-param name="once" select="$once + 1"/>
					</xsl:call-template>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$stringIn" /> 
				</xsl:otherwise>
			</xsl:choose>
		</xsl:template>

	<!-- functional equivalent of change case method -->
	<xsl:template name='convertcase'>
		<xsl:param name='toconvert' />
		<xsl:param name='conversion' />
		
		<xsl:variable name="lcletters">abcdefghijklmnopqrstuvwxyz</xsl:variable>
		<xsl:variable name="ucletters">ABCDEFGHIJKLMNOPQRSTUVWXYZ</xsl:variable>

		<xsl:choose>
			<xsl:when test='$conversion="lower"'>
				<xsl:value-of select="translate($toconvert,$ucletters,$lcletters)"/>
			</xsl:when>
			<xsl:when test='$conversion="upper"'>
				<xsl:value-of select="translate($toconvert,$lcletters,$ucletters)"/>
			</xsl:when>
			<xsl:when test='$conversion="proper"'>
				<xsl:call-template name='convertpropercase'>
				<xsl:with-param name='toconvert'>
					<xsl:value-of select="translate($toconvert,$ucletters,$lcletters)"/>
				</xsl:with-param>
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select='$toconvert' />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name='convertpropercase'>
		<xsl:param name='toconvert' />

		<xsl:if test="string-length($toconvert) > 0">
			<xsl:variable name='f' select='substring($toconvert, 1, 1)' />
			<xsl:variable name='s' select='substring($toconvert, 2)' />
			
			<xsl:call-template name='convertcase'>
				<xsl:with-param name='toconvert' select='$f' />
				<xsl:with-param name='conversion'>upper</xsl:with-param>
			</xsl:call-template>

			<xsl:choose>
				<xsl:when test="contains($s,' ')">
					<xsl:value-of select='substring-before($s," ")'/>
					&#160;
					<xsl:call-template name='convertpropercase'>
					<xsl:with-param name='toconvert' select='substring-after($s," ")' />
					</xsl:call-template>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select='$s'/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:if>
	</xsl:template>

<!-- functional equivalent of substring method -->
	<xsl:template name="SubstringReplace">
		<xsl:param name="stringIn" /> 
		<xsl:param name="substringIn" /> 
		<xsl:param name="substringOut" /> 
		<xsl:choose>
			<xsl:when test="contains($stringIn,$substringIn)">
				<xsl:value-of select="concat(substring-before($stringIn,$substringIn),$substringOut)" /> 
				<xsl:call-template name="SubstringReplace">
					<xsl:with-param name="stringIn" select="substring-after($stringIn,$substringIn)" /> 
					<xsl:with-param name="substringIn" select="$substringIn" /> 
					<xsl:with-param name="substringOut" select="$substringOut" /> 
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$stringIn" /> 
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

<!-- replace all occurences of the character 'dblquote'
	by the character '[' and ']' in the string 'string'.-->
	<xsl:template name="dblquot-replace" >
		<xsl:param name="string"/>
		<xsl:param name="counter"/>		
		<xsl:variable name="lt">[</xsl:variable>
		<xsl:variable name="rt">]</xsl:variable>
		<xsl:variable name="dq">"</xsl:variable>
		<xsl:variable name="use">
			<xsl:choose>
				<xsl:when test="($counter mod 2)=0">
					<xsl:value-of select="$lt"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$rt"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:choose>
			<xsl:when test="contains($string, $dq)">
				<xsl:value-of select="substring-before($string, $dq)"/>
				<xsl:value-of select="$use"/>
				<xsl:call-template name="dblquot-replace">
					<xsl:with-param name="string" select="substring-after($string, $dq)"/>
					<xsl:with-param name="counter" select="$counter + 1"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$string"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

</xsl:stylesheet>
