using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using HaemophilusWeb.Models;
using HaemophilusWeb.Utils;
using HaemophilusWeb.Views.Utils;

namespace HaemophilusWeb.Tools
{
    public class ExportDefinition<T>
    {
        private readonly List<Tuple<Func<T, object>, string, Type>> exportedFields = new();

        public void AddField<TMember>(Expression<Func<T, TMember>> field)
        {
            AddField(field, field.GetDisplayName());
        }

        public void AddField<TMember>(Expression<Func<T, TMember>> field, string headerName)
        {
            var compiledField = field.Compile();
            Func<T, object> wrappedExpression = arg => compiledField(arg);
            exportedFields.Add(Tuple.Create(wrappedExpression, headerName, typeof(TMember)));
        }


        protected void AddFieldOnPositiveTestResult<TMember>(Func<T, NativeMaterialTestResult> testResult, Func<T, TMember> field, string headerName)
        {
            AddField(s => testResult(s) == NativeMaterialTestResult.Positive 
                ? ExportToString(field(s)) 
                : testResult(s) == NativeMaterialTestResult.Negative
                    ? EnumUtils.GetEnumDescription<NativeMaterialTestResult>(NativeMaterialTestResult.Negative)
                    : null, 
                headerName);
        }

        public DataTable ToDataTable(IEnumerable<T> entries)
        {
            var dataTable = new DataTable();

            AddColumnDefinitions(dataTable);
            AddRows(entries, dataTable);

            return dataTable;
        }

        protected object ExportChildProperty<TProperty>(TProperty property, Func<TProperty, object> accessValue)
        {
            // ReSharper disable once IntroduceOptionalParameters.Global as used in expression tree
            return ExportChildProperty(property, accessValue, null);
        }

        protected object ExportChildProperty<TProperty>(TProperty property, Func<TProperty, object> accessValue, string nullValue)
        {
            return property != null
                ? ExportToStringIfEnum(accessValue(property))
                : nullValue;
        }

        protected static object ExportToStringIfEnum<TValue>(TValue value)
        {
            if (value != null && value.GetTypeOrNullableType().IsEnum)
            {
                return EnumEditor.GetEnumDescription(value);
            }
            return value;
        }

        protected static string ExportToString<TValue>(TValue value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            var type = value.GetTypeOrNullableType();
            if (type.IsEnum)
            {
                var enumDescription = EnumEditor.GetEnumDescription(value);
                if ((int)(object)value == 0 && (enumDescription.Equals("0") || string.IsNullOrEmpty(enumDescription)))
                {
                    return null;
                }

                return enumDescription;
            }
            return value.ToString();
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
                var expression = exportedField.Item1;
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