/* we are just using this for chopping up a space-delimited list of database objects. If you
have embedded spaces in your object names then tough. You'll have to do it differently*/
 
if exists (Select * from sys.xml_schema_collections where name like 'ObjectListParameter')
  drop XML SCHEMA COLLECTION ObjectListParameter
go
create xml schema collection ObjectListParameter as '
<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">
<xs:element name="Object">
       <xs:simpleType>
              <xs:list itemType="xs:string" />
       </xs:simpleType>
</xs:element>
</xs:schema>'
 
go
/*
 
*/
go
    -- does a particular procedure  exist
IF EXISTS ( SELECT 1 FROM sys.objects
              WHERE  object_ID= object_ID('dbo.CreatePlantUMLCode'))
  SET NOEXEC ON
GO
-- if the routine exists this isn't executed
CREATE PROCEDURE dbo.CreatePlantUMLCode
AS Select 'created, but not implemented yet.'--just anything will do
GO
-- the following section will be always executed
SET NOEXEC OFF
GO
Alter procedure CreatePlantUMLCode
@ObjectsToShow NVarchar(400)=null, -- space-delimited list of database objects
@dependenciesOf NVarchar(400)=null, --show the first order objects that reference or otherwise depend on it
@dependsOn  NVarchar(400)=null, --show the objects that it depends on
@MyPlantUMLStatement varchar(max) output --the code to use for the diagram
/*
Examples of use:
Declare @ThePlantUMLCode Varchar(max)
execute CreatePlantUMLCode @ObjectsToShow='HumanResources.employee person.person',
       @MyPlantUMLStatement=@ThePlantUMLCode output
select @ThePlantUMLCode
 
Declare @ThePlantUMLCode Varchar(max)
execute CreatePlantUMLCode @DependsOn='person.address',
       @MyPlantUMLStatement=@ThePlantUMLCode output
select @ThePlantUMLCode
 
Declare @ThePlantUMLCode Varchar(max)
execute CreatePlantUMLCode @DependenciesOf='person.address',
       @MyPlantUMLStatement=@ThePlantUMLCode output
select @ThePlantUMLCode
 
Declare @ThePlantUMLCode Varchar(max)
execute CreatePlantUMLCode @ObjectsToShow='HumanResources.employee person.person HumanResources.vEmployeeDepartmentHistory  dbo.ufnGetContactInformation',
       @MyPlantUMLStatement=@ThePlantUMLCode output
select @ThePlantUMLCode
 
execute CreatePlantUMLCode @ObjectsToShow='dbo.ufnGetContactInformation',
       @MyPlantUMLStatement=@ThePlantUMLCode output
select @ThePlantUMLCode
 
Declare @ThePlantUMLCode Varchar(max)
execute CreatePlantUMLCode @ObjectsToShow='HumanResources.employee person.person HumanResources.vEmployeeDepartmentHistory ',
       @MyPlantUMLStatement=@ThePlantUMLCode output
select @ThePlantUMLCode
 
execute CreatePlantUMLCode @ObjectsToShow='HumanResources.vEmployeeDepartmentHistory  dbo.ufnGetContactInformation',
       @MyPlantUMLStatement=@ThePlantUMLCode output
select @ThePlantUMLCode
*/
as
--has the user given us a list of objects?
Declare @ObjectsToDo table(Object_ID int primary key) --check only specified once!
declare @xml_data xml(ObjectListParameter)
set @xml_data='<Object>'+ @ObjectsToShow +'</Object>'
Declare @ii int
Select @ii=0 --what has been specified in the parameters
if (@ObjectsToShow is not null) set @ii=@ii+1
if (@DependenciesOf is not null) set @ii=@ii+1
if (@DependsOn is not null) set @ii=@ii+1
if (@ii<1)
  begin raiserror('Sorry, but you''ll need  to specify what to draw!', 16,1);return;end
if (@ii>1)
  begin raiserror('Sorry, only one parameter can be used at a time', 16,1);return;end
/* if he has given a list, then we need to parse the list and find the IDs of reach
database object that has been specified */
if  @ObjectsToShow is not null
       begin
       insert into @ObjectsToDo
         select object_ID(T.ref.value('.', 'sysname'))
        from (Select @xml_data.query('
                     for $i in data(/Object) return
                     element item { $i }
              '))  A(list)
        cross apply A.List.nodes('/item') T(ref)
       end
--does the user want to see the dependencies
if @DependenciesOf is not null
       begin --get all the foreign key references
       insert into @ObjectsToDo
         Select referenced_object_ID
           from sys.foreign_keys
           where parent_object_id=object_ID(@DependenciesOf)
       union all --and all the objects that refer to it
         Select referenced_ID from sys.sql_expression_dependencies
           where referencing_id=object_ID(@DependenciesOf)
             and referenced_ID is not null
             and is_schema_bound_reference =0
    union --and insert the object itself
         Select object_ID(@DependenciesOf)
       end
if @DependsOn is not null
       begin --does the user want a diagram of all the objects that the object depends on?
       insert into @ObjectsToDo --insert all the foreign key relationships
      Select parent_object_ID
        from sys.foreign_keys
        where referenced_Object_id=object_ID(@DependsOn)
      union all --and all the references this object makes
      Select referencing_ID from sys.sql_expression_dependencies
        where referenced_id=object_ID(@DependsOn)
          and is_schema_bound_reference =0
    union
         Select object_ID(@DependsOn) --and add the object itself
       end
 
Select @MyPlantUMLStatement='!define table(x) class x << (T,mistyrose) >> 
!define view(x) class x << (V,lightblue) >> 
!define table(x) class x << (T,mistyrose) >>
!define tr(x) class x << (R,red) >>
!define tf(x) class x << (F,darkorange) >> 
!define af(x) class x << (F,white) >> 
!define fn(x) class x << (F,plum) >> 
!define fs(x) class x << (F,tan) >> 
!define ft(x) class x << (F,wheat) >> 
!define if(x) class x << (F,gaisboro) >> 
!define p(x) class x << (P,indianred) >> 
!define pc(x) class x << (P,lemonshiffon) >> 
!define x(x) class x << (P,linen) >>
 
hide methods 
hide stereotypes
skinparam classarrowcolor gray
 
'
/* firstly, we'll create all the UML table diagrams. */
SELECT @MyPlantUMLStatement =
  @MyPlantUMLStatement + 'table(' + Object_Schema_Name(allTables.object_id) + '.' + name + ') { 
'
  +
  (
  SELECT DISTINCT 
    c.name + ': ' + t.name
       + CASE WHEN PrimaryKeyColumns.Object_ID IS NOT NULL THEN ' <<pk>>' ELSE '' END
       + CASE WHEN fk.parent_object_id IS NOT NULL THEN ' <<fk>>' ELSE '' END + ' 
'
    FROM sys.columns AS c --give the column names and the data types but no dimensions
      INNER JOIN sys.types AS t
        ON c.user_type_id = t.user_type_id
      LEFT OUTER JOIN sys.foreign_key_columns AS fk
        ON parent_object_id = c.object_id AND parent_column_id = c.column_id
      LEFT OUTER JOIN --the primary keys are a bit awkward to get
           (SELECT i.object_id, column_id
              FROM sys.indexes AS i
                INNER JOIN sys.index_columns AS ic
                  ON ic.object_id = i.object_id AND ic.index_id = i.index_id
                INNER JOIN sys.key_constraints AS k
                  ON k.parent_object_id = ic.object_id AND i.index_id = k.unique_index_id
              WHERE ic.object_id = allTables.object_id AND k.type = 'pk'
           ) AS PrimaryKeyColumns(Object_ID, Column_ID)
        ON c.object_id = PrimaryKeyColumns.Object_ID AND c.column_id = PrimaryKeyColumns.Column_ID
    WHERE c.object_id = allTables.object_id
  FOR XML PATH(''), TYPE
  ).value(N'(./text())[1]', N'varchar(max)') /* so now we can add any triggers. We could do indexes as well
but I somehow felt this wasn't appropriate*/
  + Coalesce('__ trigger __
' +
  (SELECT name + '
'
     FROM sys.triggers
     WHERE parent_id = allTables.object_id
  FOR XML PATH(''), TYPE
  ).value('.', 'varchar(max)'), '') + '}
'
  FROM sys.tables AS allTables
    INNER JOIN @ObjectsToDo AS ObjectsToDo
      ON allTables.object_id = ObjectsToDo.Object_ID;
 
/* now let's do the views */
 
SELECT @MyPlantUMLStatement =
  @MyPlantUMLStatement + 'view(' + Object_Schema_Name(allViews.object_id) + '.' + name + ') {
'                             +
  (SELECT c.name + ': ' + t.name + '
'
     FROM sys.columns AS c
       INNER JOIN sys.types AS t
         ON c.user_type_id = t.user_type_id
     WHERE c.object_id = allViews.object_id
  FOR XML PATH(''), TYPE
  ).value(N'(./text())[1]', N'varchar(max)') + '}
'
  FROM sys.views AS allViews
    INNER JOIN @ObjectsToDo AS ObjectsToDo
      ON allViews.object_id = ObjectsToDo.Object_ID;
 
/* now we do anything that is capable of having parameters */
SELECT @MyPlantUMLStatement =
  @MyPlantUMLStatement + RTrim(Lower(Allroutines.type)) + '('
  + Object_Schema_Name(Allroutines.object_id) + '.' + Allroutines.name + ') {
'
  /* note, a routine can exist without a parameter */
  + Coalesce(
  (SELECT p.name + ': ' + Type_Name(p.user_type_id) + '
'
     FROM sys.objects AS o
       INNER JOIN sys.parameters AS p
         ON o.object_id = p.object_id
     WHERE o.object_id = Allroutines.object_id
  FOR XML PATH(''), TYPE
  ).value(N'(./text())[1]', N'varchar(max)'), '') + '}
'
  FROM sys.objects AS Allroutines
    INNER JOIN @ObjectsToDo AS ObjectsToDo
      ON Allroutines.object_id = ObjectsToDo.Object_ID AND type IN
('AF', 'FN', 'FS', 'FT', 'IF', 'P', 'PC', 'TF', 'X');
 
/* just the types that can have parameters */
/* and now that we have a class diagram for every object,
we now do the arrows.*/
SELECT @MyPlantUMLStatement =
  @MyPlantUMLStatement
  + Coalesce(
  (
  SELECT Coalesce(Object_Schema_Name(referencing_id) + '.', '') + Object_Name(referencing_id)
     + ' -|> ' + referenced_schema_name + '.' + referenced_entity_name + ':References
'
    --AS reference
    FROM sys.sql_expression_dependencies
      INNER JOIN @ObjectsToDo AS ObjectsToDo
        ON referencing_id = ObjectsToDo.Object_ID
      INNER JOIN @ObjectsToDo AS ObjectsToDo2
        ON referenced_id = ObjectsToDo2.Object_ID
    WHERE is_schema_bound_reference = 0
  FOR XML PATH(''), TYPE
  ).value(N'(./text())[1]', N'varchar(max)'), '');
 
SELECT @MyPlantUMLStatement =
  @MyPlantUMLStatement
  + Coalesce(
  (
  SELECT Object_Schema_Name(parent_object_id) + '.' + Object_Name(parent_object_id) + ' -|> '
     + Object_Schema_Name(referenced_object_id) + '.' + Object_Name(referenced_object_id)
     + ':FK
'
    FROM sys.foreign_keys
      INNER JOIN @ObjectsToDo AS ObjectsToDo
        ON parent_object_id = ObjectsToDo.Object_ID
      INNER JOIN @ObjectsToDo AS ObjectsToDo2
        ON referenced_object_id = ObjectsToDo2.Object_ID
  FOR XML PATH(''), TYPE
  ).value(N'(./text())[1]', N'varchar(max)'), '');
 
go