--uses the temp table created in '004 Drop Indexes + Relations.sql'

declare	@key_name sysname,
	@table_name sysname,
	@referenced_table sysname,
	@const_id int,
	@col_1 sysname,
	@col_2 sysname,
	@col_list_1 nvarchar(2000),
	@col_list_2 nvarchar(2000),
	@cnst_is_update_cascade bit,
	@cnst_is_not_repl bit,
	@cnst_is_delete_cascade bit,
	@cnst_is_disabled bit,
	@c2 cursor,
	@c cursor,
	@sql_segment nvarchar(4000)

-- SET UP SOME CONSTANT VALUES FOR OUTPUT QUERY
	
declare @ix_empty varchar(1) select @ix_empty = ''
declare @ix_des1			varchar(35),	-- 35 matches spt_values
		@ix_des2		varchar(35),
		@ix_des4		varchar(35),
		@ix_des32		varchar(35),
		@ix_des64		varchar(35),
		@ix_des2048		varchar(35),
		@ix_des4096		varchar(35),
		@ix_des8388608		varchar(35),
		@ix_des16777216	varchar(35)

select @ix_des1 = name from master.dbo.spt_values where type = 'I' and number = 1 --ignoor duplicate keys

select @ix_des2 = name from master.dbo.spt_values where type = 'I' and number = 2 --unique
select @ix_des4 = name from master.dbo.spt_values where type = 'I' and number = 4 --ignoor duplicate rows
select @ix_des32 = name from master.dbo.spt_values where type = 'I' and number = 32 --hypothetical
select @ix_des64 = name from master.dbo.spt_values where type = 'I' and number = 64 --statistics
select @ix_des2048 = name from master.dbo.spt_values where type = 'I' and number = 2048  --primary key
select @ix_des4096 = name from master.dbo.spt_values where type = 'I' and number = 4096  --unique key
select @ix_des8388608 = name from master.dbo.spt_values where type = 'I' and number = 8388608  --auto create
select @ix_des16777216 = name from master.dbo.spt_values where type = 'I' and number = 16777216 --stats no recompute




insert into #sql(sql)
select case when (stats & 4096)<>0 or (stats & 2048) <> 0 then
	--Constraint		
	'ALTER TABLE '+objectname+' ADD CONSTRAINT ['+index_name+'] '
	+ case when (stats & 2048)<>0 then 'PRIMARY KEY ' else 'UNIQUE ' end
	+ case when (stats & 16)<>0 then 'clustered' else 'nonclustered' end
	+ ' ('+index_keys+')'
	+ case when OrigFillFactor >0 then ' WITH FILLFACTOR =' + cast(OrigFillFactor as nvarchar(3)) else @ix_empty end
	+ ' ON ['+groupname+']
' collate database_default

	when (stats & 64) <> 0 then
	
	--statistics
	'CREATE STATISTICS ['+index_name+'] on '+objectname+' ('+index_keys+')'
	+ case when (stats & 16777216)<>0 then ' WITH ' else @ix_empty end
	+ case when (stats & 16777216)<>0 then @ix_des16777216 else @ix_empty end
	else
	-- index

	'CREATE ' + case when (stats & 2)<>0 then @ix_des2 +' ' else @ix_empty end +case when (stats & 16)<>0 then 'clustered' else 'nonclustered' end +' INDEX'
	+ ' ['+ index_name +'] on '+objectname+' ('+index_keys+')'
	+ case when OrigFillFactor >0 or (stats & 1) <> 0 or (stats & 16777216) <> 0 then ' WITH ' else @ix_empty end
	+ case when OrigFillFactor >0 then 'PAD_INDEX, FILLFACTOR = ' +cast(OrigFillFactor as nvarchar(3) ) else @ix_empty end
	+ case when (stats & 1) <> 0 then ', '+ @ix_des1  else @ix_empty end
	+ case when (stats & 16777216) <> 0 then ', '+ @ix_des16777216  else @ix_empty end
	+ ' ON ['+groupname+']
'
	end
from 	#spindtab 
where	 IsAutoStatistic=0  --do not recreate auto statistics
	
	
	
-- script out foreign keys

	

set @C = cursor for 

select 	'[' + u.name + '].[' + o.name + ']' as TableName,
	object_name(constid) as KeyName,
	(	select distinct '['+fu.name+'].['+ro.name+']'
		from 		sysforeignkeys fk 
		join		sysobjects ro
		on		ro.id = fk.rkeyid
		join		sysusers fu
		on		fu.uid = ro.uid
		where 		fk.constid = c.constid) as ReferencedTable,
	constid,
	objectproperty(constid,'CnstIsUpdateCascade') CnstIsUpdateCascade,
	objectproperty(constid,'CnstIsDeleteCascade') CnstIsDeleteCascade,
	objectproperty(constid,'CnstIsNotRepl') CnstIsNotRepl,
	objectproperty(constid,'CnstIsDisabled') CnstIsDisabled
from 	sysconstraints c
join	sysobjects o
on	c.id = o.id
join	sysusers u
on	u.uid = o.uid
where 	objectproperty(constid,'IsForeignKey')=1 
and 	constid in  (

	select 	fk.constid
	from 	sysforeignkeys fk
	join 	syscolumns fc
	on	fc.colid = fk.fkey
	and	fc.id = fk.fkeyid
	join 	syscolumns rc
	on	rc.colid = fk.rkey
	and	rc.id = fk.rkeyid
	where	(fc.collationid is not null and fc.collationid<>0)
	or 	(rc.collationid is not null and rc.collationid<>0)
	or	1={2} --paramater allows all constraints to be dropped
	
	union 
	
	select 	constid 
	from 	sysreferences  r
	join	#spindtab  i
	on	r.fkeyid = i.id)  
	
open @C 

fetch next from @C into @table_name, @key_name, @referenced_table,@const_id, @cnst_is_update_cascade, @cnst_is_delete_cascade, @cnst_is_not_repl,@cnst_is_disabled
while @@fetch_Status =0
begin
	set @col_list_1 = ''
	set @col_list_2 = ''

	set @c2 = Cursor for
	select  fc.name,
		rc.name

	from 	sysforeignkeys fk
	join 	syscolumns fc
	on	fc.colid = fk.fkey
	and	fc.id = fk.fkeyid
	join 	syscolumns rc
	on	rc.colid = fk.rkey
	and	rc.id = fk.rkeyid
	where 	fk.constid = @const_id 

	open @c2
	fetch next from @c2 into @col_1, @col_2
	while @@Fetch_status=0
	begin
		if len(@col_list_1) > 0 
			set @col_list_1 = @col_list_1 collate database_default+', '
		if len(@col_list_2) > 0 
			set @col_list_2 = @col_list_2 collate database_default+', '

		set @col_list_1 = @col_list_1 collate database_default +'[' + @col_1 collate database_default + ']'
		set @col_list_2 = @col_list_2 collate database_default +'[' + @col_2 collate database_default + ']'

		fetch next from @c2 into @col_1, @col_2
	end
	close @c2
	deallocate @c2

	set @sql_segment = 'Alter table '+ @table_name collate database_default + ' WITH NOCHECK ADD CONSTRAINT [' + @key_name collate database_default + '] FOREIGN KEY ('+@col_list_1 collate database_default + ') REFERENCES ' + @referenced_table collate database_default+' ('+ @col_list_2 collate database_default +')'
	if @cnst_is_update_cascade =1
		set @sql_segment =@sql_segment + ' ON UPDATE CASCADE'

	if @cnst_is_delete_cascade =1
		set @sql_segment =@sql_segment + ' ON DELETE CASCADE'

	if @cnst_is_not_repl =1
		set @sql_segment =@sql_segment + ' NOT FOR REPLICATION'
	set @sql_segment = @sql_segment +'
'

	insert into #sql (sql) values (@sql_segment)

	if @cnst_is_disabled=1
	begin
		set @sql_segment = 'Alter table '+ @table_name + ' NOCHECK CONSTRAINT [' + @key_name + ']
'
		insert into #sql values (@sql_segment)
	end

	fetch next from @C into @table_name, @key_name, @referenced_table,@const_id, @cnst_is_update_cascade, @cnst_is_delete_cascade, @cnst_is_not_repl,@cnst_is_disabled
end

close @C
deallocate @C 