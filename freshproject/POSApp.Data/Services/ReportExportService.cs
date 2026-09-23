using System;
using System.Collections.Generic;
using System.Linq;

namespace POSApp.Data.Services
{
    /// <summary>
    /// Helper service for exporting reports to various formats
    /// Note: Requires ClosedXML for Excel and QuestPDF for PDF
    /// </summary>
    public class ReportExportService
    {
        /// <summary>
        /// Convert list of objects to dictionary format suitable for export
        /// </summary>
        public static List<Dictionary<string, object>> ConvertToExportFormat<T>(List<T> items) where T : class
        {
            var result = new List<Dictionary<string, object>>();

            if (items == null || items.Count == 0)
                return result;

            var properties = typeof(T).GetProperties();

            foreach (var item in items)
            {
                var row = new Dictionary<string, object>();
                foreach (var property in properties)
                {
                    var value = property.GetValue(item);
                    row[property.Name] = value ?? "";
                }
                result.Add(row);
            }

            return result;
        }

        /// <summary>
        /// Format report name for file export
        /// </summary>
        public static string FormatFileName(string reportName)
        {
            var sanitized = System.Text.RegularExpressions.Regex.Replace(reportName, @"[^a-zA-Z0-9_\- ]", "");
            return $"{sanitized}_{DateTime.Now:yyyyMMdd_HHmmss}";
        }

        /// <summary>
        /// Get column names for a report type
        /// </summary>
        public static List<string> GetColumnNames<T>() where T : class
        {
            return typeof(T).GetProperties().Select(p => p.Name).ToList();
        }
    }
}
