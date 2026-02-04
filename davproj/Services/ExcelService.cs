using ClosedXML.Excel;
using davproj.Models;

namespace davproj.Services
{
    public class ExcelService : IExcelService
    {
        public byte[] GetFullReport(List<Building> buildings)
        {
            using (var workbook = new XLWorkbook())
            {
                AddMonitorsSheet(workbook, buildings);

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
        public byte[] GetMonitorsReport(List<Building> buildings)
        {
            using (var workbook = new XLWorkbook())
            {
                AddMonitorsSheet(workbook, buildings);

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        private void AddMonitorsSheet(IXLWorkbook workbook, List<Building> buildings)
        {
            var worksheet = workbook.Worksheets.Add("Мониторы");
            var currentRow = 1;

            var headers = new[] { "Здание", "Этаж", "Кабинет", "РМ", "Пользователь", "Модель монитора", "S/N монитора", "Диагональ" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(currentRow, i + 1);
                cell.Value = headers[i];
                ApplyHeaderStyle(cell);
            }

            foreach (var building in buildings)
            {
                int buildingStartRow = currentRow + 1;

                foreach (var floor in building.Floors ?? Enumerable.Empty<Floor>())
                {
                    int floorStartRow = currentRow + 1;

                    foreach (var office in floor.Offices ?? Enumerable.Empty<Office>())
                    {
                        foreach (var wp in office.Workplaces ?? Enumerable.Empty<Workplace>())
                        {
                            currentRow++;
                            FillWorkplaceRow(worksheet, currentRow, building, floor, office, wp);

                            if (wp.PC?.DisplayList?.Any() == true)
                            {
                                var isFirst = true;
                                foreach (var monitor in wp.PC.DisplayList)
                                {
                                    if (!isFirst) currentRow++;

                                    worksheet.Cell(currentRow, 6).Value = monitor.Model;
                                    worksheet.Cell(currentRow, 7).Value = monitor.Serial;
                                    worksheet.Cell(currentRow, 8).Value = monitor.Diagonal;
                                    isFirst = false;
                                }
                            }

                            if (currentRow % 2 == 0)
                                worksheet.Range(currentRow, 1, currentRow, 8).Style.Fill.BackgroundColor = XLColor.FromHtml("#DEEBF7");
                        }
                    }

                    if (currentRow >= floorStartRow)
                    {
                        currentRow++;
                        AddFloorSummary(worksheet, currentRow, floor, floorStartRow);
                    }
                }

                if (currentRow >= buildingStartRow)
                {
                    currentRow++;
                    AddBuildingSummary(worksheet, currentRow, building, buildingStartRow);
                }
            }

            worksheet.Columns().AdjustToContents();
            worksheet.SheetView.FreezeRows(1);
            worksheet.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        }

        #region Стили, вспомогательные методы для формирования листов
        private void ApplyHeaderStyle(IXLCell cell)
        {
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2F75B5");
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        private void FillWorkplaceRow(IXLWorksheet sheet, int row, Building b, Floor f, Office o, Workplace wp)
        {
            sheet.Cell(row, 1).Value = b.Name;
            sheet.Cell(row, 2).Value = f.FloorNum;
            sheet.Cell(row, 3).Value = o.Name;
            sheet.Cell(row, 4).Value = wp.Name;
            sheet.Cell(row, 5).Value = wp.User?.FullName ?? "Вакантно";
        }

        private void AddFloorSummary(IXLWorksheet sheet, int row, Floor floor, int startRow)
        {
            var range = sheet.Range(row, 1, row, 8);
            range.Style.Fill.BackgroundColor = XLColor.FromHtml("#BDD7EE");
            range.Style.Font.Italic = true;
            sheet.Cell(row, 2).FormulaA1 = $"=\"Итого по этажу {floor.FloorNum}: \" & COUNTA(F{startRow}:F{row - 1}) & \" шт.\"";
            sheet.Range(row, 2, row, 5).Merge();
            sheet.Rows(startRow, row - 1).Group();
        }

        private void AddBuildingSummary(IXLWorksheet sheet, int row, Building building, int startRow)
        {
            var range = sheet.Range(row, 1, row, 8);
            range.Style.Fill.BackgroundColor = XLColor.FromHtml("#9BC2E6");
            range.Style.Font.Bold = true;
            range.Style.Border.TopBorder = XLBorderStyleValues.Medium;
            sheet.Cell(row, 1).FormulaA1 = $"=\"ИТОГО ПО ЗДАНИЮ {building.Name}: \" & COUNTIFS(F{startRow}:F{row - 1}, \"<>\", F{startRow}:F{row - 1}, \"<>Итого*\") & \" шт.\"";
            sheet.Range(row, 1, row, 5).Merge();
            sheet.Rows(startRow, row - 1).Group();
        }

        #endregion
    }
}
