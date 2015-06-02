using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using HaemophilusWeb.Utils;

namespace HaemophilusWeb.Tools
{
    public class ExportDefinition<T>
    {
        private readonly List<Tuple<Expression<Func<T, object>>, string, Type>> exportedFields =
            new List<Tuple<Expression<Func<T, object>>, string, Type>>();

        public void AddField<TMember>(Expression<Func<T, TMember>> field)
        {
            AddField(field, field.GetDisplayName());
        }

        public void AddField<TMember>(Expression<Func<T, TMember>> field, string headerName)
        {
            Expression<Func<T, object>> wrapExpression = arg => field.Compile()(arg);
            exportedFields.Add(Tuple.Create(wrapExpression, headerName, typeof (TMember)));
        }

        public DataTable ToDataTable(IEnumerable<T> entries)
        {
            var dataTable = new DataTable();

            AddColumnDefinitions(dataTable);
            AddRows(entries, dataTable);

            return dataTable;
        }

        private void AddColumnDefinitions(DataTable dataTable)
        {
            foreach (var exportedField in exportedFields)
            {
                var header = exportedField.Item2;
                var type = exportedField.Item3;
                dataTable.Columns.Add(new DataColumn(header, GetColumnType(type)));
            }
        }

        private static Type GetColumnType(Type type)
        {
            var memberType = GetNullableType(type);
            if (memberType.IsEnum)
            {
                return typeof (string);
            }
            return memberType;
        }

        private void AddRows(IEnumerable<T> entries, DataTable dataTable)
        {
            foreach (var entry in entries)
            {
                AddRow(dataTable, entry);
            }
        }

        private void AddRow(DataTable dataTable, T entry)
        {
            var row = dataTable.NewRow();
            foreach (var exportedField in exportedFields)
            {
                var columnName = exportedField.Item2;
                var expression = exportedField.Item1.Compile();
                row[columnName] = expression(entry) ?? DBNull.Value;
            }
            dataTable.Rows.Add(row);
        }


        private static Type GetNullableType(Type t)
        {
            return Nullable.GetUnderlyingType(t) ?? t;
        }
    }
}