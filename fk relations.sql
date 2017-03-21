DECLARE @OnClauseTemplate VARCHAR(8000)
	SET @OnClauseTemplate = '[<@pTable>].[<@pCol>] = [<@cTable>].[<@cCol>] AND '
	
	DECLARE @str VARCHAR(8000)
	SET @str = ''
	SELECT	
	/*	fk_name = OBJECT_NAME(constid),	-- constraint name
		cTableId = OBJECT_NAME(fkeyid),	-- child table
		c_col_id = COL_NAME(fkeyid, fkey),	-- child table column
		p_table_id = OBJECT_NAME(rkeyid),	-- parent table
		p_col_id = COL_NAME(rkeyid, rkey)	-- parent table column
	,	
	*/	@str = @str +
			REPLACE(
				REPLACE(
					REPLACE(
						REPLACE(@OnClauseTemplate,
							'<@pTable>', 
							OBJECT_NAME(rkeyid)
						),
						'<@pCol>',
						COL_NAME(rkeyid, rkey)
					),
					'<@cTable>',
					OBJECT_NAME(fkeyid)
				),
				'<@cCol>',
				COL_NAME(fkeyid, fkey)
			)
	FROM	dbo.sysforeignkeys fk
	--WHERE	fk.constid = @fkNameId 

print @str



SELECT	
  fk_name = OBJECT_NAME(constid),	-- constraint name
  cTableId = OBJECT_NAME(fkeyid),	-- child table
  c_col_id = COL_NAME(fkeyid, fkey),	-- child table column
  p_table_id = OBJECT_NAME(rkeyid),	-- parent table
  p_col_id = COL_NAME(rkeyid, rkey)	-- parent table column
FROM	dbo.sysforeignkeys fk
where OBJECT_NAME(rkeyid) = 'person'
order by OBJECT_NAME(rkeyid), OBJECT_NAME(fkeyid)

select DISTINCT c.*
from person as p
left outer join company as c on c.person_id = p.person_id
where c.person_id is null