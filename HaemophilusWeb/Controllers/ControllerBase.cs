using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web.Mvc;
using HaemophilusWeb.Tools;
using HaemophilusWeb.ViewModels;

namespace HaemophilusWeb.Controllers
{
    public class ControllerBase : Controller
    {
        protected ActionResult ExportToExcel<T>(FromToQuery query, List<T> list, ExportDefinition<T> exportDefinition,
            string prefix, Action<DataTable> postProcessDataTable = null)
        {
            var tempFile = Path.GetTempFileName();
            CreateExcelFile.CreateExcelDocument(list, exportDefinition, tempFile, postProcessDataTable);
            return File(System.IO.File.ReadAllBytes(tempFile),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{prefix}-Export_{query.From:yyyyMMdd}-{query.To:yyyyMMdd}.xlsx");
        }
    }
}