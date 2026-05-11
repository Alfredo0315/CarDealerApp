using System;
using System.Collections.Generic;
using CarDealerApp.Models;
using ClosedXML.Excel;

namespace CarDealerApp.Services
{
    public static class ExcelExportService
    {
        public static void ExportCars(List<Car> cars, string filePath)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Автомобили");

            // Заголовок отчёта
            worksheet.Cell(1, 1).Value = "Автомобили автосалона CarDealer";
            worksheet.Range(1, 1, 1, 7).Merge().Style
                .Font.SetBold()
                .Font.SetFontSize(14)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            // Заголовки столбцов
            string[] headers = { "ID", "Марка", "Модель", "Цвет", "Год выпуска", "Цена (₽)", "Тех. характеристики" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(2, i + 1);
                cell.Value = headers[i];
                cell.Style
                    .Font.SetBold()
                    .Font.SetFontColor(XLColor.Black)
                    .Fill.SetBackgroundColor(XLColor.White)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            }

            // Данные
            int startRow = 3;
            for (int i = 0; i < cars.Count; i++)
            {
                int row = startRow + i;
                worksheet.Cell(row, 1).Value = cars[i].ID_Car;
                worksheet.Cell(row, 2).Value = cars[i].Mark;
                worksheet.Cell(row, 3).Value = cars[i].Model;
                worksheet.Cell(row, 4).Value = cars[i].Color;
                worksheet.Cell(row, 5).Value = cars[i].Year_of_release;
                worksheet.Cell(row, 6).Value = cars[i].Price;
                worksheet.Cell(row, 7).Value = cars[i].Technical_specifications;
            }

            // Границы таблицы
            if (cars.Count > 0)
            {
                var tableRange = worksheet.Range(2, 1, startRow + cars.Count - 1, headers.Length);
                tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }

            // Автоширина столбцов
            worksheet.Columns().AdjustToContents();

            workbook.SaveAs(filePath);
        }

        public static void ExportOrders(List<Order> orders, string filePath)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Заказы");

            // Заголовок отчёта
            worksheet.Cell(1, 1).Value = "Заказы автосалона CarDealer";
            worksheet.Range(1, 1, 1, 6).Merge().Style
                .Font.SetBold()
                .Font.SetFontSize(14)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            // Заголовки столбцов
            string[] headers = { "ID заказа", "Клиент", "Автомобиль", "Статус", "Дата исполнения", "Способ оплаты" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(2, i + 1);
                cell.Value = headers[i];
                cell.Style
                    .Font.SetBold()
                    .Font.SetFontColor(XLColor.Black)
                    .Fill.SetBackgroundColor(XLColor.White)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            }

            // Данные
            int startRow = 3;
            for (int i = 0; i < orders.Count; i++)
            {
                int row = startRow + i;
                worksheet.Cell(row, 1).Value = orders[i].ID_Order;
                worksheet.Cell(row, 2).Value = orders[i].ClientName ?? "";
                worksheet.Cell(row, 3).Value = orders[i].CarInfo ?? "";
                worksheet.Cell(row, 4).Value = orders[i].Order_status;
                worksheet.Cell(row, 5).Value = orders[i].Date_of_execution.ToString("dd.MM.yyyy");
                worksheet.Cell(row, 6).Value = orders[i].Payment_method;
            }

            // Границы
            if (orders.Count > 0)
            {
                var tableRange = worksheet.Range(2, 1, startRow + orders.Count - 1, headers.Length);
                tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }

            worksheet.Columns().AdjustToContents();

            workbook.SaveAs(filePath);
        }
    }
}