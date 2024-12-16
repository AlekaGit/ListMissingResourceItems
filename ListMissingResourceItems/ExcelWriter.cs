﻿using ClosedXML.Excel;
using System.Collections.Frozen;

public class ExcelWriter
{
    private static readonly FrozenDictionary<string, string> cultureDictionary = new Dictionary<string, string>
    {
        { "en", "English" },
        { "de", "German" },
        { "es", "Spanish" },
        { "fr", "French" },
        { "it", "Italian" },
        { "ko", "Korean" },
        { "pt-BR", "Portuguese (Brazil)" },
        { "zh-Hans", "Chinese (Simplified)" },
        { "zh-Hant", "Chinese (Traditional)" }
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    public void Write(Dictionary<string, string?> mainFile, Dictionary<string, Dictionary<string, string>> result)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sheet1");


        // Write the header
        worksheet.Cell(1, 1).Value = "Key";
        worksheet.Cell(1, 2).Value = cultureDictionary["En"];
        int colIndex = 3;
        foreach (var entry in result)
        {
            var lang = entry.Key;
            if (cultureDictionary.TryGetValue(entry.Key, out var value))
                lang = value;

            worksheet.Cell(1, colIndex++).Value = lang;
        }

        // Apply styles to the header
        var headerRange = worksheet.Range(1, 1, 1, colIndex - 1);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Write the data
        int rowIndex = 2;
        foreach (var entry in mainFile)
        {
            worksheet.Cell(rowIndex, 1).Value = entry.Key;
            worksheet.Cell(rowIndex, 2).Value = entry.Value;
            colIndex = 3;

            foreach (var langEntry in result)
            {
                if (langEntry.Value.TryGetValue(entry.Key, out var value))
                {
                    worksheet.Cell(rowIndex, colIndex++).Value = value;
                }
                else
                {
                    worksheet.Cell(rowIndex, colIndex++).Value = "-";
                }
            }

            rowIndex++;
        }

        worksheet.Column(1).Hide();
        worksheet.CellsUsed().Style.Alignment.WrapText = true;
        worksheet.Columns().AdjustToContents();

        // Set max width
        int maxWidth = 100;
        foreach (var column in worksheet.ColumnsUsed())
        {
            if (column.Width > maxWidth)
                column.Width = maxWidth;
        }

        worksheet.Rows().AdjustToContents();

        // Freeze the headers and the first column
        worksheet.SheetView.FreezeRows(1);
        worksheet.SheetView.FreezeColumns(2);

        workbook.SaveAs(@"c:\temp\out.xlsx");
    }
}