using ClosedXML.Excel;
using davproj.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

public class ReportsController : Controller
{
    private readonly DBContext _db;

    public ReportsController(DBContext context) => _db = context;

    public byte[] ExportInventoryToExcel(IEnumerable<Building> buildings)
    {
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Инвентаризация");
            var row = 1;

            var headers = new string[] {
            "Здание", "Этаж", "Кабинет", "РМ", "Пользователь",
            "Модель монитора", "S/N монитора", "Диагональ"
        };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(row, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2F75B5");
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            foreach (var building in buildings)
            {
                int buildingStartRow = row + 1;

                foreach (var floor in building.Floors ?? Enumerable.Empty<Floor>())
                {
                    int floorStartRow = row + 1;

                    foreach (var office in floor.Offices ?? Enumerable.Empty<Office>())
                    {
                        foreach (var wp in office.Workplaces ?? Enumerable.Empty<Workplace>())
                        {
                            row++;
                            worksheet.Cell(row, 1).Value = building.Name;
                            worksheet.Cell(row, 2).Value = floor.FloorNum;
                            worksheet.Cell(row, 3).Value = office.Name;
                            worksheet.Cell(row, 4).Value = wp.Name;
                            worksheet.Cell(row, 5).Value = wp.User?.FullName ?? "Вакантно";

                            if (wp.PC?.DisplayList != null && wp.PC.DisplayList.Any())
                            {
                                var firstMon = true;
                                foreach (var monitor in wp.PC.DisplayList)
                                {
                                    if (!firstMon)
                                    {
                                        row++;
                                        /* worksheet.Cell(row, 1).Value = building.Name;
                                        worksheet.Cell(row, 2).Value = floor.FloorNum;
                                        worksheet.Cell(row, 3).Value = office.Name;
                                        worksheet.Cell(row, 4).Value = wp.Name + " (доп)"; */
                                    }
                                  
                                    worksheet.Cell(row, 6).Value = monitor.Model;
                                    worksheet.Cell(row, 7).Value = monitor.Serial;
                                    worksheet.Cell(row, 8).Value = monitor.Diagonal;
                                    worksheet.Cell(row, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                    firstMon = false;
                                }
                            }

                            if (row % 2 == 0)
                                worksheet.Range(row, 1, row, 8).Style.Fill.BackgroundColor = XLColor.FromHtml("#DEEBF7");
                        }
                    }

                    if (row >= floorStartRow)
                    {
                        row++;
                        var floorRange = worksheet.Range(row, 1, row, 8);
                        floorRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#BDD7EE");
                        floorRange.Style.Font.Italic = true;

                        worksheet.Cell(row, 2).FormulaA1 = $"=\"Итого по этажу {floor.FloorNum}: \" & COUNTA(F{floorStartRow}:F{row - 1}) & \" шт.\"";
                        worksheet.Range(row, 2, row, 5).Merge();

                        worksheet.Rows(floorStartRow, row - 1).Group();
                    }
                }

                if (row >= buildingStartRow)
                {
                    row++;
                    var buildRange = worksheet.Range(row, 1, row, 8);
                    buildRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#9BC2E6");
                    buildRange.Style.Font.Bold = true;
                    buildRange.Style.Border.TopBorder = XLBorderStyleValues.Medium;

                    worksheet.Cell(row, 1).FormulaA1 = $"=\"ИТОГО ПО ЗДАНИЮ {building.Name}: \" & COUNTIFS(F{buildingStartRow}:F{row - 1}, \"<>\", F{buildingStartRow}:F{row - 1}, \"<>Итого*\") & \" шт.\"";
                    worksheet.Range(row, 1, row, 5).Merge();

                    worksheet.Rows(buildingStartRow, row - 1).Group();
                }
            }

            worksheet.Columns().AdjustToContents();
            worksheet.SheetView.FreezeRows(1);

            worksheet.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
        }
    }

    public async Task<IActionResult> Index()
    {
        var buildings = await _db.Buildings
            .Include(b => b.Floors!).ThenInclude(f => f.Offices!)
                .ThenInclude(o => o.Workplaces!).ThenInclude(w => w.User!)
            .Include(b => b.Floors!).ThenInclude(f => f.Offices!)
                .ThenInclude(o => o.Workplaces!).ThenInclude(w => w.PC!)
                    .ThenInclude(p => p.CurrentHardwareInfo)
            .Include(b => b.Location)
        .ToListAsync();
        byte[] filecontent = ExportInventoryToExcel(buildings);
        using (var stream = new MemoryStream())
        {
            return File(filecontent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "MonitorsReport.xlsx");
        }
    }
}
    