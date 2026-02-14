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
                AddHardwareSheet(workbook, buildings);
                AddSoftwareSheet(workbook, buildings);
                AddMonitorsSheet(workbook, buildings);
                AddPrinterSheet(workbook, buildings);
                AddUsbSheet(workbook, buildings);

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
        public byte[] GetHardwareReport(List<Building> buildings)
        {
            using (var workbook = new XLWorkbook())
            {
                AddHardwareSheet(workbook, buildings);

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
        public byte[] GetSoftwareReport(List<Building> buildings)
        {
            using (var workbook = new XLWorkbook())
            {
                AddSoftwareSheet(workbook, buildings);

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
        public byte[] GetUsbReport(List<Building> buildings)
        {
            using (var workbook = new XLWorkbook())
            {
                AddUsbSheet(workbook, buildings);

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
        public byte[] GetPrinterReport(List<Building> buildings)
        {
            using (var workbook = new XLWorkbook())
            {
                AddPrinterSheet(workbook, buildings);

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

       private void AddSoftwareSheet(IXLWorkbook workbook, List<Building> buildings)
        {
            var worksheet = workbook.Worksheets.Add("Программное обеспечение");
            var headerRow = worksheet.Row(1);
            headerRow.Style.Alignment.WrapText = true;

            int currentCol = 1;
            int currentRow = 2;

            foreach (var building in buildings)
            {
                foreach (var floor in building.Floors ?? Enumerable.Empty<Floor>())
                {
                    foreach (var office in floor.Offices ?? Enumerable.Empty<Office>())
                    {
                        foreach (var wp in office.Workplaces ?? Enumerable.Empty<Workplace>())
                        {
                            var headerText = $"{building.Name}-{floor.FloorNum}-{office.Name} {Environment.NewLine}" +
                                $"{wp.Name}{Environment.NewLine}" +
                                $"{wp.User?.FullName ?? "Вакант"}";
                            worksheet.Cell(1, currentCol).Value = headerText;
                            var software = wp.PC?.CurrentHardwareInfo?.SoftwareList;
                            currentRow = 2;

                            if (software != null && software.Count > 0)
                            {
                                var sortedSoftware = software.OrderBy(s => s).ToList();

                                foreach (var programName in sortedSoftware)
                                {
                                    worksheet.Cell(currentRow, currentCol).Value = programName;
                                    currentRow++;
                                }
                            }
                            else
                            {
                                worksheet.Cell(currentRow, currentCol).Value = "Данные о ПО отсутствуют";
                                worksheet.Cell(currentRow, currentCol).Style.Font.Italic = true;
                            }
                            currentCol++;
                        }
                    }
                }
            }

            var usedHeaderRange = worksheet.Row(1).AsRange().RangeUsed();
            ApplyHeaderStyle(usedHeaderRange);
            usedHeaderRange.Style.Alignment.WrapText = true;
            worksheet.Columns().AdjustToContents();
            worksheet.Columns().Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

            worksheet.SheetView.FreezeRows(1);
            worksheet.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        }

        private void AddUsbSheet(IXLWorkbook workbook, List<Building> buildings)
        {
            var worksheet = workbook.Worksheets.Add("USB");
            var headerRow = worksheet.Row(1);
            headerRow.Style.Alignment.WrapText = true;

            int currentCol = 1;
            int currentRow = 2;

            foreach (var building in buildings)
            {
                foreach (var floor in building.Floors ?? Enumerable.Empty<Floor>())
                {
                    foreach (var office in floor.Offices ?? Enumerable.Empty<Office>())
                    {
                        foreach (var wp in office.Workplaces ?? Enumerable.Empty<Workplace>())
                        {
                            var headerText = $"{building.Name}-{floor.FloorNum}-{office.Name} {Environment.NewLine}" +
                                $"{wp.Name}{Environment.NewLine}" +
                                $"{wp.User?.FullName ?? "Вакант"}";
                            worksheet.Cell(1, currentCol).Value = headerText;
                            var usb = wp.PC?.CurrentHardwareInfo?.UsbDevices;
                            currentRow = 2;

                            if (usb != null && usb.Count > 0)
                            {
                                var sortedUsb = usb.OrderBy(s => s).ToList();

                                foreach (var programName in sortedUsb)
                                {
                                    worksheet.Cell(currentRow, currentCol).Value = programName;
                                    currentRow++;
                                }
                            }
                            else
                            {
                                worksheet.Cell(currentRow, currentCol).Value = "Данные о ПО отсутствуют";
                                worksheet.Cell(currentRow, currentCol).Style.Font.Italic = true;
                            }
                            currentCol++;
                        }
                    }
                }
            }

            var usedHeaderRange = worksheet.Row(1).AsRange().RangeUsed();
            ApplyHeaderStyle(usedHeaderRange);
            usedHeaderRange.Style.Alignment.WrapText = true;
            worksheet.Columns().AdjustToContents();
            worksheet.Columns().Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

            worksheet.SheetView.FreezeRows(1);
            worksheet.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        }

        private void AddPrinterSheet(IXLWorkbook workbook, List<Building> buildings)
        {
            var worksheet = workbook.Worksheets.Add("Принтеры");
            var currentRow = 1;

            var headers = new[] { "Здание", "Этаж", "Кабинет", "РМ", "Пользователь", "Принтеры" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(currentRow, i + 1);
                cell.Value = headers[i];
                ApplyHeaderStyle(cell);
            }

            foreach (var building in buildings)
            {
                foreach (var floor in building.Floors ?? Enumerable.Empty<Floor>())
                {
                    foreach (var office in floor.Offices ?? Enumerable.Empty<Office>())
                    {
                        foreach (var wp in office.Workplaces ?? Enumerable.Empty<Workplace>())
                        {
                            currentRow++;

                            worksheet.Cell(currentRow, 1).Value = building.Name;
                            worksheet.Cell(currentRow, 2).Value = floor.FloorNum;
                            worksheet.Cell(currentRow, 3).Value = office.Name;
                            worksheet.Cell(currentRow, 4).Value = wp.Name;
                            worksheet.Cell(currentRow, 5).Value = wp?.User?.FullName ?? "Вакантно";

                            var printers = wp.PC?.CurrentHardwareInfo?.Printers;

                            if (printers != null && printers.Count > 0)
                            {
                                var sortedPrinters = printers.OrderBy(s => s).ToList();

                                worksheet.Cell(currentRow, 6).Value = sortedPrinters[0];

                                if (sortedPrinters.Count > 1)
                                {
                                    int printersStartRow = currentRow + 1;

                                    for (int i = 1; i < sortedPrinters.Count; i++)
                                    {
                                        currentRow++;
                                        worksheet.Cell(currentRow, 6).Value = sortedPrinters[i];
                                    }

                                    worksheet.Rows(printersStartRow, currentRow).Group();
                                    worksheet.Rows(printersStartRow, currentRow).Collapse();
                                }
                            }
                            else
                            {
                                worksheet.Cell(currentRow, 6).Value = "Данные о принтерах отсутствуют";
                                worksheet.Cell(currentRow, 6).Style.Font.Italic = true;
                            }

                            var rowRange = worksheet.Range(currentRow - (printers?.Count > 0 ? printers.Count - 1 : 0), 1, currentRow, 6);
                            if (currentRow % 2 == 0)
                                rowRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");
                        }
                    }
                }
            }

            worksheet.Columns().AdjustToContents();
            worksheet.Column(6).Width = 60;
            worksheet.Column(6).Style.Alignment.WrapText = true;

            worksheet.SheetView.FreezeRows(1);
            worksheet.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        }

        private void AddHardwareSheet(IXLWorkbook workbook, List<Building> buildings)
        {
            var worksheet = workbook.Worksheets.Add("Железо");
            var currentRow = 1;

            var headers = new[] { 
                "Здание", "Этаж", "Кабинет", "РМ", "Пользователь", 
                "Hostname", "IP-адрес", "Версия ОС", "Имя последнего пользователя", "Системная плата", 
                "S/N платы", "Процессор", "Видеодрайвер", "Объём ОЗУ", "Тип ОЗУ", 
                "Частота ОЗУ", "Слоты ОЗУ", "Производитель ОЗУ", "Тип диска", "Место на диске", 
                "SMART диска", "Антивирус", "Доступно обновлений", "UpTime"
            };
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
                            var info = wp.PC?.CurrentHardwareInfo ?? null;
                            if (info != null)
                            {
                                worksheet.Cell(currentRow, 6).Value = info.ComputerName;
                                worksheet.Cell(currentRow, 7).Value = info.IpAddress;
                                worksheet.Cell(currentRow, 8).Value = info.OSVersion;
                                worksheet.Cell(currentRow, 9).Value = info.CurrentUserName;
                                worksheet.Cell(currentRow, 10).Value = info.MotherboardModel;
                                worksheet.Cell(currentRow, 11).Value = info.SerialNumber;
                                worksheet.Cell(currentRow, 12).Value = info.ProcessorName;
                                worksheet.Cell(currentRow, 13).Value = info.VideoCard;
                                worksheet.Cell(currentRow, 14).Value = info.TotalMemoryGB;
                                worksheet.Cell(currentRow, 15).Value = info.RamType;
                                worksheet.Cell(currentRow, 16).Value = info.RamSpeed;
                                worksheet.Cell(currentRow, 17).Value = info.TotalRamSlots + "/" + info.UsedRamSlots;
                                worksheet.Cell(currentRow, 18).Value = info.RamManufacturer;
                                worksheet.Cell(currentRow, 19).Value = info.DiskType;
                                worksheet.Cell(currentRow, 20).Value = info.DiskInfo;
                                worksheet.Cell(currentRow, 21).Value = info.DiskHealth;
                                worksheet.Cell(currentRow, 22).Value = info.Antivirus;
                                worksheet.Cell(currentRow, 23).Value = info.PendingUpdatesCount;
                                worksheet.Cell(currentRow, 24).Value = info.Uptime;
                            }

                            if (currentRow % 2 == 0)
                                worksheet.Range(currentRow, 1, currentRow, headers.Length).Style.Fill.BackgroundColor = XLColor.FromHtml("#DEEBF7");
                        }
                    }

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

        private void ApplyHeaderStyle(IXLRange range)
        {
            range.Style.Font.Bold = true;
            range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            range.Style.Fill.BackgroundColor = XLColor.FromHtml("#D9D9D9");
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
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
