using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable disable

namespace a2n.DynData
{
    public static class DefaultExport
    {
        public static void OnExport(string format, string viewName, 
            Type valueType, Metadata[] metadataArr,
            IQueryable<dynamic> qry,
            out byte[] buffer, out string mimeType, out string fileName)
        {
            format = format.ToLower();
            switch (format)
            {
                case "xlsx":
                    {
                        using (var ms = new MemoryStream())
                        {
                            qry.ExportToExcel(valueType, metadataArr, ms);
                            buffer = ms.ToArray();
                        }
                        mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        fileName = $"{viewName.ToHumanReadable()}.xlsx";
                    }
                    break;
                case "csv":
                    {
                        using (var ms = new MemoryStream())
                        {
                            using (var sr = new StreamWriter(ms))
                            {
                                qry.ExportToCSV(metadataArr, sr);
                            }
                            buffer = ms.ToArray();
                        }
                        mimeType = "text/csv";
                        fileName = $"{viewName.ToHumanReadable()}.csv";
                    }
                    break;
                default:
                    mimeType = "";
                    fileName = "";
                    buffer = null;
                    break;
            }
        }
    }
}
