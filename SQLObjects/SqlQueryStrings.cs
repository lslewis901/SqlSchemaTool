using System;
using System.Collections.Generic;
using System.Text;

namespace Lewis.SST.SQLObjects
{
    /// <summary>
    /// const strings class
    /// </summary>
    public static class SqlQueryStrings
    {
        /// <summary>
        /// const string to get all database objects names, types, and id
        /// expected object types for {1} are a delimited string of 'U','P','FN','TF','V'
        /// ignores all dt_xxx, sysxxx, and syncobj_xxx database objects
        /// </summary>
        public const string GetDataBaseObjects =
@"SELECT [name], xtype, id 
FROM [{0}].dbo.sysobjects 
WHERE 
        xtype IN ({1}) 
    --AND PATINDEX('sys%', [name])      = 0 
    --AND PATINDEX('dt_%', [name])      = 0 
    --AND PATINDEX('syncobj_%', [name]) = 0
ORDER BY [name]";

        /// <summary>
        /// const ssql statement
        /// </summary>
        public const string GetUDDTsList =
@"USE [{0}]
CREATE TABLE #temp1
(
[name] sysname COLLATE SQL_Latin1_General_CP1_CI_AS
)

INSERT INTO #temp1 SELECT [name] from master.dbo.systypes

CREATE TABLE #temp2
(
[uddt_name] sysname COLLATE SQL_Latin1_General_CP1_CI_AS,
[user_name] sysname COLLATE SQL_Latin1_General_CP1_CI_AS,
[type] sysname COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[prec] int NULL,
[scale] int NULL,
[allownulls] int NULL,
[default] sysname COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[rule] sysname COLLATE SQL_Latin1_General_CP1_CI_AS NULL
)

INSERT INTO #temp2
SELECT 
	a.name as UDDT_NAME,  
	b.name as USER_NAME,  
	(SELECT TOP 1 [name] FROM [{0}].dbo.systypes AS c WHERE c.xusertype = a.xtype) AS type,  
	a.prec,  
	a.scale,  
	a.allownulls,  
	(SELECT TOP 1 [name] from [{0}].dbo.sysobjects AS d WHERE d.id = a.tdefault) as [default],  
	(SELECT TOP 1 [name] FROM [{0}].dbo.sysobjects AS e WHERE e.type = 'R' and e.id = a.domain) AS [Rule] 
FROM [{0}].dbo.systypes AS a INNER JOIN [{0}].dbo.sysusers AS b ON b.uid = a.uid 

SELECT * FROM #temp2 as a 
WHERE a.UDDT_NAME NOT IN (SELECT [name] FROM #temp1) ORDER BY UDDT_NAME, USER_NAME

IF (SELECT object_id('tempdb.dbo.#temp1')) IS NOT NULL
    DROP TABLE #temp1
IF (SELECT object_id('tempdb.dbo.#temp2')) IS NOT NULL
    DROP TABLE #temp2
";

        /// <summary>
        /// const SQL statement
        /// </summary>
        public const string GetTriggersSchema =
            @"USE [{0}]
EXEC sp_helptext '{1}'
";

        /// <summary>
        /// const SQL statement
        /// </summary>
        public const string GetTriggersList =
@"USE [{0}]

DECLARE @crlf char(2), @tab char(1), @cr char(1), @lf char(1)

SET @cr = char(13)
SET @lf = char(10)
SET @crlf = @cr + @lf
SET @tab = char(9)

SELECT 
	TRIGGER_NAME = o.name, 
	USER_NAME = user_name(o.uid),
	o.category,
	IsExecuted = (case when (OBJECTPROPERTY(o.id, N'IsExecuted')=1) then 1 else 0 end), 
	ExecIsAnsiNullsOn = (case when (OBJECTPROPERTY(o.id, N'ExecIsAnsiNullsOn')=1) then 1 else 0 end), 
	ExecIsQuotedIdentOn = (case when (OBJECTPROPERTY(o.id, N'ExecIsQuotedIdentOn')=1) then 1 else 0 end), 
	IsAnsiNullsOn = (case when (OBJECTPROPERTY(o.id, N'IsAnsiNullsOn')=1) then 1 else 0 end),
	IsQuotedIdentOn = (case when (OBJECTPROPERTY(o.id, N'IsQuotedIdentOn')=1) then 1 else 0 end),
	ExecIsAfterTrigger = (case when (OBJECTPROPERTY(o.id, N'ExecIsAfterTrigger')=1) then 1 else 0 end), 
	ExecIsDeleteTrigger = (case when (OBJECTPROPERTY(o.id, N'ExecIsDeleteTrigger')=1) then 1 else 0 end),
	ExecIsFirstDeleteTrigger = (case when (OBJECTPROPERTY(o.id, N'ExecIsFirstDeleteTrigger')=1) then 1 else 0 end),
	ExecIsFirstInsertTrigger = (case when (OBJECTPROPERTY(o.id, N'ExecIsFirstInsertTrigger')=1) then 1 else 0 end),
	ExecIsFirstUpdateTrigger = (case when (OBJECTPROPERTY(o.id, N'ExecIsFirstUpdateTrigger')=1) then 1 else 0 end),
	ExecIsInsertTrigger = (case when (OBJECTPROPERTY(o.id, N'ExecIsInsertTrigger')=1) then 1 else 0 end),
	ExecIsInsteadOfTrigger = (case when (OBJECTPROPERTY(o.id, N'ExecIsInsteadOfTrigger')=1) then 1 else 0 end),
	ExecIsLastDeleteTrigger = (case when (OBJECTPROPERTY(o.id, N'ExecIsLastDeleteTrigger')=1) then 1 else 0 end),
	ExecIsLastInsertTrigger = (case when (OBJECTPROPERTY(o.id, N'ExecIsLastInsertTrigger')=1) then 1 else 0 end),
	ExecIsLastUpdateTrigger = (case when (OBJECTPROPERTY(o.id, N'ExecIsLastUpdateTrigger')=1) then 1 else 0 end),
	ExecIsTriggerDisabled = (case when (OBJECTPROPERTY(o.id, N'ExecIsTriggerDisabled')=1) then 1 else 0 end),
	ExecIsUpdateTrigger = (case when (OBJECTPROPERTY(o.id, N'ExecIsUpdateTrigger')=1) then 1 else 0 end),
	ExecIsTriggerDisabled = (case when (OBJECTPROPERTY(o.id, N'ExecIsTriggerDisabled')=1) then 1 else 0 end),
    Check_Sum = (SELECT TOP 1 BINARY_CHECKSUM(SUBSTRING(LOWER( RTRIM(LTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(s1.Text, @crlf, ''), @tab, ''), @lf, ''), @cr, ''), ' ', '')))),1,4000))
				 FROM SYSCOMMENTS AS s1 WHERE o.id = s1.id) +
				(SELECT SUM(LEN(LOWER(RTRIM(LTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(s2.Text, @crlf, ''), @tab, ''), @lf, ''), @cr, ''), ' ', '')))))) as text_Len
				 FROM SYSCOMMENTS AS s2 WHERE o.id = s2.id )


FROM dbo.sysobjects o
-- get all non-system procs
WHERE 
	(OBJECTPROPERTY(o.id, N'IsTrigger') = 1  ) 
	AND OBJECTPROPERTY(o.id, N'IsMSShipped') = 0   
ORDER BY TRIGGER_NAME, USER_NAME
";


        /// <summary>
        /// const SQL statement
        /// </summary>
        public const string GetTablesList =
@"USE [{0}]
select
	TABLE_NAME = convert(sysname,o.name),	/* make nullable */
	TABLE_OWNER = convert(sysname,user_name(o.uid)),
	TABLE_TYPE = convert(varchar(32),rtrim(substring('SYSTEM TABLE            TABLE       VIEW       ',(ascii(o.type)-83)*12+1,12))),	/* 'S'=0,'U'=2,'V'=3 */
	REMARKS = convert(varchar(254),null)	/* Remarks are NULL */
from sysobjects o
where
	o.name not in ('dtproperties','sysconstraints','syssegments')
	and o.type = 'U'
    {1}
order by TABLE_NAME
";

        /// <summary>
        /// const SQL statement
        /// </summary>
        public const string GetTableSchema =
@" -- could potentially use EXEC sp_columns_rowset 'customer' 
USE [{0}] 

SELECT 
	Table_Name = o.name, 
	User_Name = user_name(o.uid),
	N'SystemObj' = (case when (OBJECTPROPERTY(o.id, N'IsMSShipped')=1) then 1 else OBJECTPROPERTY(o.id, N'IsSystemTable') end),        
	o.category, 
	TableHasActiveFulltextIndex = ObjectProperty(o.id, N'TableHasActiveFulltextIndex'), 
	TableFulltextCatalogId = ObjectProperty(o.id, N'TableFulltextCatalogId'), 
	N'FakeTable' = (case when (OBJECTPROPERTY(o.id, N'tableisfake')=1) then 1 else 0 end),        
	IsQuotedIdentOn = (case when (OBJECTPROPERTY(o.id, N'IsQuotedIdentOn')=1) then 1 else 0 end), 
	IsAnsiNullsOn = (case when (OBJECTPROPERTY(o.id, N'IsAnsiNullsOn')=1) then 1 else 0 end)
FROM dbo.sysobjects o, dbo.sysindexes i 
WHERE OBJECTPROPERTY(o.id, N'IsTable') = 1 and i.id = o.id and i.indid < 2 and o.name Like N'{1}'   
ORDER BY Table_Name, User_Name

SET QUOTED_IDENTIFIER OFF
{2}
SET QUOTED_IDENTIFIER ON

{4}

-- EXEC sp_MStablerefs N'[{1}]', N'actualkeycols', N'both'
SET QUOTED_IDENTIFIER OFF
{5}
SET QUOTED_IDENTIFIER ON

SET QUOTED_IDENTIFIER OFF
{3}
SET QUOTED_IDENTIFIER ON


SELECT
	--db_name()			as CONSTRAINT_CATALOG,
	user_name(a.uid)  	as CONSTRAINT_OWNER,
	a.name				as CONSTRAINT_NAME,
	isnull(d.name, '')	AS COLUMN_NAME,
	com.text			as CONSTRAINT_CLAUSE,
	--object_name(a.parent_obj) AS PARENT_NAME, 
	a.xtype				AS CONSTRAINT_TYPE
FROM sysobjects AS a
INNER JOIN syscomments AS com ON com.id = a.id
LEFT OUTER JOIN dbo.syscolumns d on a.id = d.cdefault
WHERE
	permissions(a.parent_obj) != 0
	and a.xtype IN ('C','D')
	and object_name(a.parent_obj) = '{1}'
";


        /// <summary>
        /// const SQL statement
        /// </summary>
        public const string GetIndexes =
@"
SET NOCOUNT ON
IF (SELECT object_id('tempdb.dbo.#spindtab')) IS NOT NULL
	DROP TABLE #spindtab

DECLARE @objname nvarchar(776),
		@objid int,		-- the object id of the table
		@indid smallint,	-- the index id of an index
		@groupid smallint,  	-- the filegroup id of an index
		@indname sysname,
		@groupname sysname,
		@status int,
		@keys nvarchar(2126),	--Length (16*max_identifierLength)+(15*2)+(16*3)
		@dbname	sysname

SET @objname = '{0}'
-- Check to see that the object names are local to the current database.
select @dbname = parsename(@objname,3)

-- Check to see the the table exists and initialize @objid.
select @objid = object_id(@objname)

-- OPEN CURSOR OVER INDEXES (skip stats: bug shiloh_51196)
declare ms_crs_ind cursor local static for
	select indid, groupid, name, status from sysindexes
		where id = @objid and indid > 0 and indid < 255 and (status & 64)=0 order by indid
open ms_crs_ind
fetch ms_crs_ind into @indid, @groupid, @indname, @status

-- create temp table
create table #spindtab
(
	index_name			sysname	collate database_default NOT NULL,
	stats				int,
	groupname			sysname collate database_default NOT NULL,
	index_keys			nvarchar(2126)	collate database_default NOT NULL -- see @keys above for length descr
)

-- Now check out each index, figure out its type and keys and
--	save the info in a temporary table that we'll print out at the end.
while @@fetch_status >= 0
begin
	-- First we'll figure out what the keys are.
	declare @i int, @thiskey nvarchar(131) -- 128+3

	select @keys = index_col(@objname, @indid, 1), @i = 2
	if (indexkey_property(@objid, @indid, 1, 'isdescending') = 1)
		select @keys = @keys  + '(-)'

	select @thiskey = index_col(@objname, @indid, @i)
	if ((@thiskey is not null) and (indexkey_property(@objid, @indid, @i, 'isdescending') = 1))
		select @thiskey = @thiskey + '(-)'

	while (@thiskey is not null )
	begin
		select @keys = @keys + ', ' + @thiskey, @i = @i + 1
		select @thiskey = index_col(@objname, @indid, @i)
		if ((@thiskey is not null) and (indexkey_property(@objid, @indid, @i, 'isdescending') = 1))
			select @thiskey = @thiskey + '(-)'
	end

	select @groupname = {1}

	-- INSERT ROW FOR INDEX
	insert into #spindtab values (@indname, @status, @groupname, @keys)

	-- Next index
	fetch ms_crs_ind into @indid, @groupid, @indname, @status
end
deallocate ms_crs_ind

-- SET UP SOME CONSTANT VALUES FOR OUTPUT QUERY
declare @empty varchar(1) select @empty = ''
declare @des1			varchar(35),	-- 35 matches spt_values
		@des2			varchar(35),
		@des4			varchar(35),
		@des32			varchar(35),
		@des64			varchar(35),
		@des2048		varchar(35),
		@des4096		varchar(35),
		@des8388608		varchar(35),
		@des16777216	varchar(35)
select @des1 = name from master.dbo.spt_values where type = 'I' and number = 1
select @des2 = name from master.dbo.spt_values where type = 'I' and number = 2
select @des4 = name from master.dbo.spt_values where type = 'I' and number = 4
select @des32 = name from master.dbo.spt_values where type = 'I' and number = 32
select @des64 = name from master.dbo.spt_values where type = 'I' and number = 64
select @des2048 = name from master.dbo.spt_values where type = 'I' and number = 2048
select @des4096 = name from master.dbo.spt_values where type = 'I' and number = 4096
select @des8388608 = name from master.dbo.spt_values where type = 'I' and number = 8388608
select @des16777216 = name from master.dbo.spt_values where type = 'I' and number = 16777216

-- DISPLAY THE RESULTS
select
	'index_name' = index_name,
	'index_description' = convert(varchar(210), --bits 16 off, 1, 2, 16777216 on, located on group
			case when (stats & 16)<>0 then 'clustered' else 'nonclustered' end
			+ case when (stats & 1)<>0 then ', '+@des1 else @empty end
			+ case when (stats & 2)<>0 then ', '+@des2 else @empty end
			+ case when (stats & 4)<>0 then ', '+@des4 else @empty end
			+ case when (stats & 64)<>0 then ', '+@des64 else case when (stats & 32)<>0 then ', '+@des32 else @empty end end
			+ case when (stats & 2048)<>0 then ', '+@des2048 else @empty end
			+ case when (stats & 4096)<>0 then ', '+@des4096 else @empty end
			+ case when (stats & 8388608)<>0 then ', '+@des8388608 else @empty end
			+ case when (stats & 16777216)<>0 then ', '+@des16777216 else @empty end
			+ ' located on ' + groupname),
	'index_keys' = index_keys
from #spindtab
order by index_name

";

        /// <summary>
        /// const SQL statement
        /// </summary>
        public const string GetColumns =
@"
DECLARE  @tablename nvarchar(517), @flags int, @obj_id int

SET @tablename = '{0}'
SET @flags = 0

SELECT @obj_id = object_id(@tablename)
	
SET NOCOUNT ON

SELECT 
	Column_Name = c.name, 
	--Column_ID = c.colid, 
	Type = st.name,
	Base_Type = bt.name,
    Length = case when bt.name in (N'nchar', N'nvarchar') then c.length/2 else c.length end,
	Prec = ISNULL(ColumnProperty(@obj_id, c.name, N'Precision'), 0),
	Scale = ISNULL(ColumnProperty(@obj_id, c.name, N'Scale'), 0),
	-- Identity seed and increment
	Seed = case when (ColumnProperty(@obj_id, c.name, N'IsIdentity') <> 0) then CONVERT(nvarchar(40), ident_seed(@tablename)) else 0 end,
	Increment = case when (ColumnProperty(@obj_id, c.name, N'IsIdentity') <> 0) then CONVERT(nvarchar(40), ident_incr(@tablename)) else 0 end,
	-- Nullable
	isNullable = ISNULL(ColumnProperty(@obj_id, c.name, N'AllowsNull'), 0),
	-- Identity
	isIdentity = case when (@flags & 0x40000000 = 0) then ISNULL(ColumnProperty(@obj_id, c.name, N'IsIdentity'), 0) else 0 end,
    isComputed = ISNULL(c.iscomputed, 0),
	isRowGuidCol = ISNULL(ColumnProperty(@obj_id, c.name, N'IsRowGuidCol'), 0),
	Default_Constraint = case when (c.cdefault = 0) then '' when (OBJECTPROPERTY(c.cdefault, N'IsDefaultCnst') <> 0) then user_name(d.uid) + N'.' + d.name else '' end,
	-- Non-DRI Default (make sure it's not a DRI constraint).
	Default_Name = case when (c.cdefault = 0) then '' when (OBJECTPROPERTY(c.cdefault, N'IsDefaultCnst') <> 0) then '' else user_name(d.uid) + N'.' + d.name end,
	-- Non-DRI Rule
	Rule_Name = case when (c.domain = 0) then '' else r.name end,
	-- Physical base datatype
	-- Non-DRI Default owner and name
	Default_Owner = case when (c.cdefault = 0) then '' when (OBJECTPROPERTY(c.cdefault, N'IsDefaultCnst') <> 0) then '' else user_name(d.uid) end,
	Default_Value = case when t.text is null then '' else replace(replace(case when (CHARINDEX('AS ', t.text)=0) then RTRIM(t.text) else RTRIM(SUBSTRING( t.text, CHARINDEX('AS ', t.text) + 3, LEN(t.text) - (CHARINDEX('AS ', t.text) + 3))) end, char(13), ''), char(10), '') end,
	-- Non-DRI Rule owner and name
	Rule_Owner = case when (c.domain = 0) then '' else user_name(r.uid) end,
	-- Not For Replication
	NotforRepl = ISNULL(ColumnProperty(@obj_id, c.name, N'IsIdNotForRepl'), 0),
    FullText = ISNULL(ColumnProperty(@obj_id, c.name, N'IsFulltextIndexed'), 0),
    AnsiPad = ISNULL(ColumnProperty(@obj_id, c.name, N'UsesAnsiTrim'), 0),
    -- column level collation
    Collation = ISNULL(c.collation, ''),
	Calc_Text = ISNULL(com.text, '')

FROM dbo.syscolumns c
	LEFT OUTER JOIN syscomments as com ON c.id = com.id and c.colid = com.number
	-- NonDRI Default and Rule filters
	LEFT OUTER JOIN dbo.sysobjects d ON c.cdefault = d.id
	LEFT OUTER JOIN dbo.sysobjects r ON c.domain = r.id 
	-- Fully derived data type name
	JOIN dbo.systypes st ON st.xusertype = c.xusertype
	-- Physical base data type name
	JOIN dbo.systypes bt ON bt.xusertype = c.xtype
	-- DRIDefault text, if it's only one row.
	LEFT OUTER JOIN dbo.syscomments t ON c.cdefault = t.id AND t.colid = 1
		AND NOT EXISTS (SELECT * FROM dbo.syscomments WHERE id = c.cdefault AND colid = 2)

WHERE c.id = @obj_id
ORDER BY c.colid
";

        /// <summary>
        /// const SQL statement
        /// </summary>
        public const string GetTableFileGroups =
@"-- select the filegroup information for the table
SELECT 
    s.groupname 
FROM dbo.sysfilegroups s, dbo.sysindexes i 
WHERE i.groupid = s.groupid AND i.id = object_id(N'[{0}]') AND i.indid in (0, 1)
";

        /// <summary>
        /// const SQL statement
        /// </summary>
        public const string ForceTableFileGroups =
@"-- for now, we are just going to fix the TABLE FileGroup to 'PRIMARY'
SELECT 'PRIMARY' AS groupname
";

        /// <summary>
        /// const SQL statement
        /// </summary>
        public const string GetTableReferences =
@"SET @tablename = '{0}'
SET @flags = 0
 
   /* tablename: table whose references are being evaluated */
   /* type     : '[actual | all][tables | keys | keycols]'; all candidates, or only those actually referenced */
   /* direction: look for references from @tablename to 'primary' table(s), or to @tablename from 'foreign' table(s) */
   /* reftable : limit scope to this table, if non-null */
   /*** @flags added for DaVinci uses.  If the bit isn't set, use 6.5 ***/
   /*** sp_MStablerefs '%s', null, 'both'                             ***/

	/* @flags is for daVinci */
	if (@flags is null)
		select @flags = 0

	select
        N'PK_Table' = PKT.name,
        N'FK_Table' = FKT.name,
        N'Constraint' = object_name(r.constid),
		--c.status,
		cKeyCol1 = convert(nvarchar(132), col_name(r.fkeyid, r.fkey1)),
		cKeyCol2 = convert(nvarchar(132), col_name(r.fkeyid, r.fkey2)),
		cKeyCol3 = convert(nvarchar(132), col_name(r.fkeyid, r.fkey3)),
		cKeyCol4 = convert(nvarchar(132), col_name(r.fkeyid, r.fkey4)),
		cKeyCol5 = convert(nvarchar(132), col_name(r.fkeyid, r.fkey5)),
		cKeyCol6 = convert(nvarchar(132), col_name(r.fkeyid, r.fkey6)),
		cKeyCol7 = convert(nvarchar(132), col_name(r.fkeyid, r.fkey7)),
		cKeyCol8 = convert(nvarchar(132), col_name(r.fkeyid, r.fkey8)),
		cKeyCol9 = convert(nvarchar(132), col_name(r.fkeyid, r.fkey9)),
		cKeyCol10 = convert(nvarchar(132), col_name(r.fkeyid, r.fkey10)),
		cKeyCol11 = convert(nvarchar(132), col_name(r.fkeyid, r.fkey11)),
		cKeyCol12 = convert(nvarchar(132), col_name(r.fkeyid, r.fkey12)),
		cKeyCol13 = convert(nvarchar(132), col_name(r.fkeyid, r.fkey13)),
		cKeyCol14 = convert(nvarchar(132), col_name(r.fkeyid, r.fkey14)),
		cKeyCol15 = convert(nvarchar(132), col_name(r.fkeyid, r.fkey15)),
		cKeyCol16 = convert(nvarchar(132), col_name(r.fkeyid, r.fkey16)),
		cRefCol1 = convert(nvarchar(132), col_name(r.rkeyid, r.rkey1)),
		cRefCol2 = convert(nvarchar(132), col_name(r.rkeyid, r.rkey2)),	
		cRefCol3 = convert(nvarchar(132), col_name(r.rkeyid, r.rkey3)),
		cRefCol4 = convert(nvarchar(132), col_name(r.rkeyid, r.rkey4)),
		cRefCol5 = convert(nvarchar(132), col_name(r.rkeyid, r.rkey5)),
		cRefCol6 = convert(nvarchar(132), col_name(r.rkeyid, r.rkey6)),
		cRefCol7 = convert(nvarchar(132), col_name(r.rkeyid, r.rkey7)),
		cRefCol8 = convert(nvarchar(132), col_name(r.rkeyid, r.rkey8)),
		cRefCol9 = convert(nvarchar(132), col_name(r.rkeyid, r.rkey9)),
		cRefCol10 = convert(nvarchar(132), col_name(r.rkeyid, r.rkey10)),
		cRefCol11 = convert(nvarchar(132), col_name(r.rkeyid, r.rkey11)),
		cRefCol12 = convert(nvarchar(132), col_name(r.rkeyid, r.rkey12)),
		cRefCol13 = convert(nvarchar(132), col_name(r.rkeyid, r.rkey13)),
		cRefCol14 = convert(nvarchar(132), col_name(r.rkeyid, r.rkey14)),
		cRefCol15 = convert(nvarchar(132), col_name(r.rkeyid, r.rkey15)),
		cRefCol16 = convert(nvarchar(132), col_name(r.rkeyid, r.rkey16)),
		N'PK_Table_Owner' = user_name(PKT.uid),
		N'FK_Table_Owner' = user_name(FKT.uid),
        N'DeleteCascade' = OBJECTPROPERTY( r.constid, N'CnstIsDeleteCascade'),
        N'UpdateCascade' = OBJECTPROPERTY( r.constid, N'CnstIsUpdateCascade')
    from dbo.sysreferences r, dbo.sysconstraints c, dbo.sysobjects PKT, dbo.sysobjects FKT
    where r.constid = c.constid and (@tablename is null or
        (r.rkeyid = object_id(@tablename) or r.fkeyid = object_id(@tablename)))
    and PKT.id = r.rkeyid and FKT.id = r.fkeyid
";

        /// <summary>
        /// const SQL statement
        /// </summary>
        public const string GetSprocSchema =
@"USE [{0}] 
EXEC sp_helptext '{1}'

-- get all the dependency objects
select
	'depname' = ISNULL(o1.name, ''),
	--o1.id,
	owner = ISNULL(s6.name, ''),
	type = ISNULL(substring(v2.name, 5, 16), ''),
	updated = ISNULL(substring(u4.name, 1, 7), ''),
	selected = ISNULL(substring(w5.name, 1, 8), ''),
	'column' = ISNULL(col_name(d3.depid, d3.depnumber), '')
from sysobjects		o1
	,master.dbo.spt_values	v2
	,sysdepends		d3
	,master.dbo.spt_values	u4
	,master.dbo.spt_values	w5 --11667
	,sysusers		s6
where	 o1.id = d3.depid
	and	 o1.xtype = substring(v2.name,1,2) collate database_default and v2.type = 'O9T'
	and	 u4.type = 'B' and u4.number = d3.resultobj
	and	 w5.type = 'B' and w5.number = d3.readobj|d3.selall
	and	 d3.id = object_id('[{1}]')
	and	 o1.uid = s6.uid
	and deptype < 2 
	and substring(v2.name, 5, 16) <> 'user table'
	and substring(v2.name, 5, 16) <> 'system table'
order by depname
";
        /// <summary>
        /// const SQL statement
        /// </summary>
        public const string GetSprocsList =
@"USE [{0}]

DECLARE @crlf char(2), @tab char(1), @cr char(1), @lf char(1)

SET @cr = char(13)
SET @lf = char(10)
SET @crlf = @cr + @lf
SET @tab = char(9)
-- implemented changes based on CodeProject feedback from TheBigKahuna
SELECT 	
	SPROC_NAME = s.name + '.' + o.name,  	
	USER_NAME = user_name(o.uid),	
	o.category,	
	ExecIsAnsiNullsOn = (case when (OBJECTPROPERTY(o.id, N'ExecIsAnsiNullsOn')=1) then 1 else 0 end), 	
	ExecIsQuotedIdentOn = (case when (OBJECTPROPERTY(o.id, N'ExecIsQuotedIdentOn')=1) then 1 else 0 end), 	
	ExecIsStartup = (case when (OBJECTPROPERTY(o.id, N'ExecIsStartup')=1) then 1 else 0 end), 	
	IsAnsiNullsOn = (case when (OBJECTPROPERTY(o.id, N'IsAnsiNullsOn')=1) then 1 else 0 end),	
	IsQuotedIdentOn = (case when (OBJECTPROPERTY(o.id, N'IsQuotedIdentOn')=1) then 1 else 0 end),	
	IsExecuted = (case when (OBJECTPROPERTY(o.id, N'IsExecuted')=1) then 1 else 0 end),	
	IsExtendedProc = (case when (OBJECTPROPERTY(o.id, N'IsExtendedProc')=1) then 1 else 0 end),	
	IsReplProc = (case when (OBJECTPROPERTY(o.id, N'IsReplProc')=1) then 1 else 0 end),	
	IsSystemProc = (case when (OBJECTPROPERTY(o.id, N'IsMSShipped')=1) then 1 else 0 end),    
	Check_Sum = (SELECT TOP 1 BINARY_CHECKSUM(SUBSTRING(LOWER( RTRIM(LTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(s1.Text, @crlf, ''), @tab, ''), @lf, ''), @cr, ''), ' ', '')))),1,4000))				 
		FROM SYSCOMMENTS AS s1 
		WHERE o.id = s1.id) +				
			(SELECT SUM(LEN(LOWER(RTRIM(LTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(s2.Text, @crlf, ''), @tab, ''), @lf, ''), @cr, ''), ' ', '')))))) as text_Len				 
			FROM SYSCOMMENTS AS s2 WHERE o.id = s2.id )
FROM dbo.sysobjects o, sys.schemas s -- get all non-system procs
	WHERE o.uid = s.schema_id AND OBJECTPROPERTY(o.id, N'IsProcedure') = 1 AND OBJECTPROPERTY(o.id, N'IsMSShipped') = 0   
ORDER BY SPROC_NAME, USER_NAME
";

        /// <summary>
        /// const SQL statement
        /// </summary>
        public const string GetRulesList =
@"USE [{0}]
SELECT 
	RULE_NAME = [name],
	USER_NAME = user_name(o.uid),
	o.category
FROM sysobjects AS o 
WHERE Type = 'R'
ORDER BY RULE_NAME, USER_NAME
";

        /// <summary>
        /// const SQL statement
        /// </summary>
        public const string GetRulesSchema =
@"USE [{0}]
EXEC sp_helptext '{1}'
";

        /// <summary>
        /// const SQL statement
        /// </summary>
        public const string GetFuncsList =
@" USE [{0}]

DECLARE @crlf char(2), @tab char(1), @cr char(1), @lf char(1)

SET @cr = char(13)
SET @lf = char(10)
SET @crlf = @cr + @lf
SET @tab = char(9)

SELECT 
	FUNC_NAME = o.name, 
	USER_NAME = user_name(o.uid),
	o.category,
	ExecIsAnsiNullsOn = (case when (OBJECTPROPERTY(o.id, N'ExecIsAnsiNullsOn')=1) then 1 else 0 end), 
	ExecIsQuotedIdentOn = (case when (OBJECTPROPERTY(o.id, N'ExecIsQuotedIdentOn')=1) then 1 else 0 end), 
	ExecIsStartup = (case when (OBJECTPROPERTY(o.id, N'ExecIsStartup')=1) then 1 else 0 end), 
	IsAnsiNullsOn = (case when (OBJECTPROPERTY(o.id, N'IsAnsiNullsOn')=1) then 1 else 0 end),
	IsQuotedIdentOn = (case when (OBJECTPROPERTY(o.id, N'IsQuotedIdentOn')=1) then 1 else 0 end),
	IsDeterministic = (case when (OBJECTPROPERTY(o.id, N'IsDeterministic')=1) then 1 else 0 end),
	IsInlineFunction = (case when (OBJECTPROPERTY(o.id, N'IsInlineFunction')=1) then 1 else 0 end),
	IsScalarFunction = (case when (OBJECTPROPERTY(o.id, N'IsScalarFunction')=1) then 1 else 0 end),
	IsTableFunction = (case when (OBJECTPROPERTY(o.id, N'IsTableFunction')=1) then 1 else 0 end),
    Check_Sum = (SELECT TOP 1 BINARY_CHECKSUM(SUBSTRING(LOWER( RTRIM(LTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(s1.Text, @crlf, ''), @tab, ''), @lf, ''), @cr, ''), ' ', '')))),1,4000))
				 FROM SYSCOMMENTS AS s1 WHERE o.id = s1.id) +
				(SELECT SUM(LEN(LOWER(RTRIM(LTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(s2.Text, @crlf, ''), @tab, ''), @lf, ''), @cr, ''), ' ', '')))))) as text_Len
				 FROM SYSCOMMENTS AS s2 WHERE o.id = s2.id )

FROM dbo.sysobjects o
LEFT OUTER JOIN dbo.syscomments c ON o.id = c.id
-- get all non-system functions
WHERE 
	(o.xtype in (N'FN', N'FS', N'IF', N'TF'))
	--(OBJECTPROPERTY(o.id, N'IsInlineFunction') = 1 OR OBJECTPROPERTY(o.id, N'IsScalarFunction') = 1 OR OBJECTPROPERTY(o.id, N'IsTableFunction') = 1 ) 
	AND OBJECTPROPERTY(o.id, N'IsMSShipped') = 0   
ORDER BY FUNC_NAME, USER_NAME
";

        /// <summary>
        /// const SQL statement
        /// </summary>
        public const string GetFuncSchema =
@"USE [{0}] 

DECLARE @version int, @xtype char(3), @text varchar(4000)
SET @version = 2000
SET @xtype = 'FN'
IF EXISTS(SELECT ver FROM(SELECT @@version as ver) AS one WHERE one.ver LIKE '%2005%')
BEGIN
    SET @version = 2005
END

-- determine if function is external CLR call, SQL 2005
IF @version = 2005
BEGIN
    SELECT o.*, ass.name as assembly_name, sql_version = @version INTO #temp FROM sysobjects AS o 
    JOIN sys.assembly_modules AS a ON o.id = a.object_id
	JOIN sys.assemblies AS ass ON a.assembly_id = ass.assembly_id
    WHERE o.xtype = 'FS' AND o.name = '{1}'

	IF EXISTS(SELECT * from #temp)
	BEGIN
		-- SELECT * from #temp
		SELECT @xtype = xtype FROM #temp as t
		SELECT @text = '-- Currently there is no way to get all the necessary information for the external function, need to manually add missing values!

'
		SELECT @text as [Text], 0 as [order] INTO #temp2
		SELECT @text = 'CREATE FUNCTION [dbo.][' + (SELECT TOP 1 t.name FROM #temp as t) + '] (''ADD PARAMETERS HERE!'') '
		INSERT INTO #temp2 ([Text], [order]) VALUES (@text, 1)
		SELECT @text = 'RETURN [''ADD RETURN TYPE HERE!''] WITH EXECUTE AS CALLER '
		INSERT INTO #temp2 ([Text], [order]) VALUES (@text, 2)
		INSERT INTO #temp2 ([Text], [order]) VALUES ('AS ', 3)
		SELECT @text = 'EXTERNAL NAME [' + (SELECT TOP 1 t.assembly_name FROM #temp as t) + '].[''INSERT CLASS NAME HERE!''].[''INSERT METHOD NAME HERE!''] '
		INSERT INTO #temp2 ([Text], [order]) VALUES (@text, 4)
		SELECT [Text] from #temp2 order by [order]
	END
IF (SELECT object_id('tempdb.dbo.#temp')) IS NOT NULL
	DROP TABLE #temp
IF (SELECT object_id('tempdb.dbo.#temp2')) IS NOT NULL
    DROP TABLE #temp2
END
IF @xtype <> 'FS'
BEGIN 
	BEGIN TRY
		EXEC sp_helptext '{1}'
    END TRY
    BEGIN CATCH
		EXEC sp_help '{1}'
    END CATCH
END

-- get all the dependency objects
select
	'depname' = ISNULL(o1.name, ''),
	owner = ISNULL(s6.name, ''),
	type = ISNULL(substring(v2.name, 5, 16), ''),
	updated = ISNULL(substring(u4.name, 1, 7), ''),
	selected = ISNULL(substring(w5.name, 1, 8), ''),
	'column' = ISNULL(col_name(d3.depid, d3.depnumber), '')
from sysobjects		o1
	,master.dbo.spt_values	v2
	,sysdepends		d3
	,master.dbo.spt_values	u4
	,master.dbo.spt_values	w5 --11667
	,sysusers		s6
where	 o1.id = d3.depid
	and	 o1.xtype = substring(v2.name,1,2) collate database_default and v2.type = 'O9T'
	and	 u4.type = 'B' and u4.number = d3.resultobj
	and	 w5.type = 'B' and w5.number = d3.readobj|d3.selall
	and	 d3.id = object_id('[{1}]')
	and	 o1.uid = s6.uid
	and deptype < 2 
	and substring(v2.name, 5, 16) <> 'user table'
	and substring(v2.name, 5, 16) <> 'system table'
order by depname
";


        /// <summary>
        /// const SQL statement
        /// </summary>
        public const string GetDefaultsList =
@"USE [{0}]
SELECT 
	DEFAULT_NAME = o.[name],
	USER_NAME = user_name(o.uid),
	o.category
FROM sysobjects o 
WHERE Type = 'D' AND o.[name] not like 'DF_%'
ORDER BY DEFAULT_NAME, USER_NAME
";

        /// <summary>
        /// const SQL statement
        /// </summary>
        public const string GetDefaultsSchema =
@"USE [{0}]
EXEC sp_helptext '{1}'
";

        /// <summary>
        /// const SQL statement
        /// </summary>
        public const string GetDTSPackagesList =
"USE [MSDB] \r\n EXEC sp_enum_dtspackages"; //requires that the selected catalog be MSDB

        /// <summary>
        /// const SQL statement
        /// </summary>
        public const string GetViewsList =
@"USE [{0}]

DECLARE @crlf char(2), @tab char(1), @cr char(1), @lf char(1)

SET @cr = char(13)
SET @lf = char(10)
SET @crlf = @cr + @lf
SET @tab = char(9)

select
	VIEW_NAME = convert(sysname,o.name),	/* make nullable */
	TABLE_OWNER = convert(sysname,user_name(o.uid)),
	TABLE_TYPE = convert(varchar(32),rtrim(substring('SYSTEM TABLE            TABLE       VIEW       ',(ascii(o.type)-83)*12+1,12))),	/* 'S'=0,'U'=2,'V'=3 */
	REMARKS = convert(varchar(254),null),	/* Remarks are NULL */
    Check_Sum = (SELECT TOP 1 BINARY_CHECKSUM(SUBSTRING(LOWER( RTRIM(LTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(s1.Text, @crlf, ''), @tab, ''), @lf, ''), @cr, ''), ' ', '')))),1,4000))
				 FROM SYSCOMMENTS AS s1 WHERE o.id = s1.id) +
				(SELECT SUM(LEN(LOWER(RTRIM(LTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(s2.Text, @crlf, ''), @tab, ''), @lf, ''), @cr, ''), ' ', '')))))) as text_Len
				 FROM SYSCOMMENTS AS s2 WHERE o.id = s2.id )

from sysobjects o
where
	o.name not in ('sysconstraints','syssegments')
	and o.type = 'V'
ORDER BY VIEW_NAME, TABLE_OWNER
";

        /// <summary>
        /// const SQL statement
        /// </summary>
        public const string GetViewSchema =
@"USE [{0}] 
EXEC sp_helptext '{1}'

-- get all the dependency objects
select
	'depname' = ISNULL(o1.name, ''),
	--o1.id,
	owner = ISNULL(s6.name, ''),
	type = ISNULL(substring(v2.name, 5, 16), ''),
	updated = ISNULL(substring(u4.name, 1, 7), ''),
	selected = ISNULL(substring(w5.name, 1, 8), ''),
	'column' = ISNULL(col_name(d3.depid, d3.depnumber), '')
from sysobjects		o1
	,master.dbo.spt_values	v2
	,sysdepends		d3
	,master.dbo.spt_values	u4
	,master.dbo.spt_values	w5 --11667
	,sysusers		s6
where	 o1.id = d3.depid
	and	 o1.xtype = substring(v2.name,1,2) collate database_default and v2.type = 'O9T'
	and	 u4.type = 'B' and u4.number = d3.resultobj
	and	 w5.type = 'B' and w5.number = d3.readobj|d3.selall
	and	 d3.id = object_id('[{1}]')
	and	 o1.uid = s6.uid
	and deptype < 2 
	and substring(v2.name, 5, 16) <> 'user table'
	and substring(v2.name, 5, 16) <> 'system table'
order by depname
";


    }
}
