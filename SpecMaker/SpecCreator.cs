using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpecMaker
{
    public class SpecCreator
    {
        // GOST specification has exactly 9 columns
        private const int GOST_SPEC_NUMBER_OF_COLS = 9;
        private const string OUTPUT_FOLDER = "output";
        private const string PATTERN_FOR_QUANTITY_SEARCH = @"\s\d+\s+шт.$";
        private const string PATTERN_FOR_EXCEL_RANGE_ADDRESS = @"^[A-Z]+[1-9]\d*$";

        private string ExcelFormFilePath { get; init; }
        private HashSet<string> AllColumnNamesFromExcel { get; set; }
        private ProjectInfoDataSchema ProjectInfo { get; init; }
        private AppSettingsSchema AppSettings { get; init; }
        private SpecSettignsSchema SpecSettings { get; init; }
        private IProgress<ProgressHelper> Progress { get; init; }

        public SpecCreator(
            string excelFormPath, 
            ProjectInfoDataSchema projInfo, 
            AppSettingsSchema appSettings, 
            SpecSettignsSchema specSettings, 
            IProgress<ProgressHelper> progressReporter)
        {
            ExcelFormFilePath = excelFormPath;
            ProjectInfo = projInfo;
            AppSettings = appSettings;
            SpecSettings = specSettings;
            Progress = progressReporter;
        }
        public async Task MakeSpecificationAsync()
        {
            // required for EPPlus library
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            DataTable dt = ReadDataFromExcel(ExcelFormFilePath);
            ValidateData(in dt);

            var groupedDT = ProcessData(ref dt);
            string outputExcelPath = PrepareOutputExcel();
            await FillExcelSpec(outputExcelPath, groupedDT);

            Progress.Report(new ProgressHelper("Печать в PDF...", 100));

            PDFPrinters.PrintViaInterop(outputExcelPath);
        }
        private string PrepareOutputExcel()
        {
            DirectoryInfo outputFolder = Directory.Exists(OUTPUT_FOLDER) ?
                                         new DirectoryInfo(OUTPUT_FOLDER) :
                                         Directory.CreateDirectory(OUTPUT_FOLDER);

            string specExcelFullPath = Path.Join(outputFolder.FullName, $"{ProjectInfo.ProjectCode}.xlsx");
            File.Copy(sourceFileName: AppSettings.SpecExcelTemplatePath,
                destFileName: specExcelFullPath,
                overwrite: true);

            return specExcelFullPath;
        }

        private async Task FillExcelSpec(string specExcelFullPath, List<string[][]> grdt)
        {
            using ExcelPackage pck = new(specExcelFullPath);
            if ( ! pck.Workbook.Worksheets.Select(ws => ws.Name).Contains(SpecSettings.ExampleSheet2Name))
                throw new ArgumentException(
                    $"Лист \"{SpecSettings.ExampleSheet2Name}\" не содержится в шаблоне спецификации: \n\n" +
                    $"\"{AppSettings.SpecExcelTemplatePath}\"");

            if (pck.Workbook.Worksheets.Count != 2) throw new ArgumentException(
                     $"Шаблон спецификации должен содержать ровно два листа, он содержит " +
                     $"{pck.Workbook.Worksheets.Count}: \n\n" +
                     $"\"{AppSettings.SpecExcelTemplatePath}\"");

            ExcelWorksheet ws = pck.Workbook.Worksheets[0];

            FillStamp(ref ws);

            var rowNumber = -1;
            var total = (double) grdt.Count;
            var current = 0;
            foreach (var dt in grdt)
            {
                rowNumber = Fill(ref ws, in dt, rowNumber);
                ++current;
                Progress.Report( new ProgressHelper ($"{current}/{(int)total}", (int)(current / total * 100)) );
            }
            pck.Workbook.Worksheets.Delete(SpecSettings.ExampleSheet2Name);
            pck.Workbook.Calculate();  // update formulas after deleting a sheet
            await pck.SaveAsync();
        }

        private void FillStamp(ref ExcelWorksheet ws)
        {
            Type prjInfoType = typeof(ProjectInfoDataSchema);
            foreach (var (prop, xlRange) in SpecSettings.FirstPageStampMap)
            {
                string propValue = prjInfoType.GetProperty(prop)?.GetValue(ProjectInfo)?.ToString();
                ws.Cells[xlRange].Value = propValue;
            }
        }

        private int Fill(ref ExcelWorksheet ws, in string[][] dt, int currentAbsoluteRow)
        {
            int localMax = dt.Select(x => x.Length).Max();
            int r = currentAbsoluteRow == -1 ? 
                SpecSettings.FirstRowIndex :
                currentAbsoluteRow;

            for (int i = 0; i < localMax; ++i)
            {
                if (r > SpecSettings.MaxRowIndex[ws.Index == 0 ? 0 : 1])
                {
                    ws = CopySheetToTheEnd(ws.Workbook, SpecSettings.ExampleSheet2Name);
                    r = SpecSettings.FirstRowIndex;
                }
                for (int j = 0; j < GOST_SPEC_NUMBER_OF_COLS; ++j)
                {
                    string c = SpecSettings.ColumnLetterAddress[j];
                    ws.Cells[$"{c}{r}"].Value = i < dt[j].Length ? dt[j][i] : DBNull.Value;
                }
                r += SpecSettings.RowStep;
            }
            r += SpecSettings.RowStep;  // make empty row between groups
            return r;

            static ExcelWorksheet CopySheetToTheEnd(ExcelWorkbook wb, string exampleSheetName)
            {
                string newWorkSheetName = $"Лист{wb.Worksheets.Count}";
                
                return wb.Worksheets.Copy(exampleSheetName, newWorkSheetName);
            }
        }

        private List<string[][]> ProcessData(ref DataTable dt)
        {
            DeleteNullRows(ref dt);
            //dt.DefaultView.Sort = $"{SpecSettings.MandatoryOrderedColumnNames[0]} ASC";

            var groupedData = MakeGroups(in dt);
            //groupedData.Sort((l,r) => Utilities.SortTablesByPositions(l,r, SpecSettings.MandatoryOrderedColumnNames[0]));

            List<string[][]> separatedData = groupedData.Select(SeparateData).ToList();
            
            return separatedData;

            static void DeleteNullRows(ref DataTable datatable)
            {
                static bool IsEmpty(object x)
                {
                    return x switch
                    {
                    string y        => string.IsNullOrWhiteSpace(y),
                    DBNull or null  => true,
                    _               => false
                    };
                }

                for (int i = datatable.Rows.Count - 1; i >= 0; --i)
                {
                    DataRow row = datatable.Rows[i];
                    if (row.ItemArray.All(IsEmpty)) row.Delete();
                }
                datatable.AcceptChanges();
            }
        }
        private string[][] SeparateData(DataTable dt)
        {
            // make 9 clmns: extract from DataTable -> combine (i.e, for 1st and 2nd clmn)

            HashSet<string> secondColumnProps = new(AllColumnNamesFromExcel);
            secondColumnProps.ExceptWith(SpecSettings.MandatoryOrderedColumnNames.Values);

            string[] gr1 = ColumnSplitter.MakeFirstColumn(in dt, SpecSettings.MandatoryOrderedColumnNames[0]);

            string[] gr2 = ColumnSplitter.MakeSecondColumn(in dt, SpecSettings, secondColumnProps);

            string[] gr3 = ColumnSplitter.MakeThirdColumn(in dt, SpecSettings.MandatoryOrderedColumnNames[2]);
            string[] gr4 = ColumnSplitter.MakeFourthColumn(in dt, SpecSettings.MandatoryOrderedColumnNames[3]);
            string[] gr5 = ColumnSplitter.MakeFifthColumn(in dt, SpecSettings.MandatoryOrderedColumnNames[4]);
            
            (string[] gr6, string[] gr7) = ColumnSplitter.MakeSixthAndSeventhColumn(in dt, 
                SpecSettings.MandatoryOrderedColumnNames[5], 
                SpecSettings.MandatoryOrderedColumnNames[6], 
                ref gr2, 
                PATTERN_FOR_QUANTITY_SEARCH);

            string[] gr8 = ColumnSplitter.MakeEighthColumn(in dt, SpecSettings.MandatoryOrderedColumnNames[7]);
            string[] gr9 = ColumnSplitter.MakeNinthColumn(in dt, SpecSettings.MandatoryOrderedColumnNames[8]);

            return new[] { gr1, gr2, gr3, gr4, gr5, gr6, gr7, gr8, gr9 };
        }

        private void ValidateData(in DataTable dt)
        {
            CheckColumnNames();
            CheckSpecSettings();
            CheckAppSettings();
            CheckConvertability(in dt);
        }

        private void CheckColumnNames()
        {
            HashSet<string> clmnNames = SpecSettings.MandatoryOrderedColumnNames.Values.ToHashSet();
            if ( ! clmnNames.IsSubsetOf(AllColumnNamesFromExcel))
            {
                clmnNames.ExceptWith(AllColumnNamesFromExcel);
                throw new ArgumentException($"Excel-форма не содержит обязательных столбцов, " +
                    $"указанных в настройках спецификации:\n - \"" +
                    $"{string.Join("\",\n - \"", clmnNames)}\".");
            }
            
            if ( ! SpecSettings.ExcludedColumnNames.IsSubsetOf(AllColumnNamesFromExcel))
            {
                SpecSettings.ExcludedColumnNames.ExceptWith(AllColumnNamesFromExcel);
                throw new ArgumentException($"Excel-форма не содержит следующих столбцов, " +
                    $"указанных в настройках спецификации:\n - \"" +
                    $"{string.Join("\",\n - \"", SpecSettings.ExcludedColumnNames)}\".");
            }

            clmnNames = SpecSettings.NoDelimeterFormatColumnNames.ToHashSet();
            if ( ! clmnNames.IsSubsetOf(AllColumnNamesFromExcel))
                throw new ArgumentException($"Excel-форма не содержит следующих столбцов, " +
                                            $"указанных в настройках спецификации:\n - \"" +
                                            $"{string.Join("\",\n - \"", clmnNames)}\".");
        }
        private void CheckSpecSettings()
        {
            if (SpecSettings.ColumnLetterAddress.Length != GOST_SPEC_NUMBER_OF_COLS)
            {
                string ending = Utilities.GetProperEnding(GOST_SPEC_NUMBER_OF_COLS);
                
                throw new ArgumentException(
                    $"В настройках спецификации необходимо указать {GOST_SPEC_NUMBER_OF_COLS} значен{ending} адресов колонок.\n\n" +
                    $"Указано: {SpecSettings.ColumnLetterAddress.Length}"
                );
            }

            int countKeys = SpecSettings.MandatoryOrderedColumnNames.Keys.Count;
            int countValues = SpecSettings.MandatoryOrderedColumnNames.Values.Count;
            if (countKeys != GOST_SPEC_NUMBER_OF_COLS || countValues != GOST_SPEC_NUMBER_OF_COLS)
                throw new ArgumentException($"Количество обязательных колонок в настройках не соответствует заданному в " +
                                            $"программе ({GOST_SPEC_NUMBER_OF_COLS}).");

            Type prjInfoType = typeof(ProjectInfoDataSchema);
            foreach (string prop in SpecSettings.FirstPageStampMap.Keys)
            {
                var propProjInfo = prjInfoType.GetProperty(prop);
                if (propProjInfo is null) throw new ArgumentException(
                    $"Ошибка настройки штампа. Свойства \"{prop}\" не существует в исходном коде программы. " +
                    $"Измените имя свойства в конфиге или измените исходной код программы. Список доступных свойств:\n - \"" +
                    $"{string.Join<System.Reflection.PropertyInfo>("\",\n - \"", prjInfoType.GetProperties())}\"."
                    );
            }
            foreach (string prop in SpecSettings.FirstPageSignMap.Keys)
            {
                var propProjInfo = prjInfoType.GetProperty(prop);
                if (propProjInfo is null) throw new ArgumentException(
                    $"Ошибка настройки подписей в штампе. Свойства \"{prop}\" не существует в исходном коде программы. " +
                    $"Измените имя свойства в конфиге или измените исходной код программы. Список доступных свойств:\n - \"" +
                    $"{string.Join<System.Reflection.PropertyInfo>("\",\n - \"", prjInfoType.GetProperties())}\"."
                    );
            }

            foreach (string xlRange in SpecSettings.FirstPageStampMap.Values)
            {
                if ( ! Utilities.IsXLRangePatternSatisfied(xlRange, PATTERN_FOR_EXCEL_RANGE_ADDRESS)) throw new ArgumentException(
                  $"Адрес ячейки Excel должен удовлетворять паттерну \"{PATTERN_FOR_EXCEL_RANGE_ADDRESS}\"."
                  );
            }
            foreach (string xlRange in SpecSettings.FirstPageSignMap.Values)
            {
                if ( ! Utilities.IsXLRangePatternSatisfied(xlRange, PATTERN_FOR_EXCEL_RANGE_ADDRESS)) throw new ArgumentException(
                  $"Адрес ячейки Excel должен удовлетворять паттерну \"{PATTERN_FOR_EXCEL_RANGE_ADDRESS}\"."
                  );
            }

        }
        private void CheckAppSettings()
        {
            if ( ! File.Exists(AppSettings.SpecExcelTemplatePath)) throw new ArgumentException($"Укажите путь к шаблону спецификации.");
        }
        private void CheckConvertability(in DataTable dt)
        {
            var v = from DataRow r in dt.Rows select r.Field<string>(SpecSettings.MandatoryOrderedColumnNames[6]);
            int invalidValues = v
                .Where(s => ! string.IsNullOrEmpty(s))
                .Select(s => int.TryParse(s, out _))
                .Count(b => b == false);
            if (invalidValues != 0) throw new ArgumentException(
                $"Столбец \"{SpecSettings.MandatoryOrderedColumnNames[6]}\" должен содержать только целые числа.");
        }

        private List<DataTable> MakeGroups(in DataTable dtOriginal)
        {
            // think about effective enumerator ...
            HashSet<string> columnNamesForGrouping = new (AllColumnNamesFromExcel);  // ... to avoid a copy here
            columnNamesForGrouping.ExceptWith(SpecSettings.ExcludedColumnNames);

            List<DataTable> dtList = GroupByColumnValues(dtOriginal, columnNamesForGrouping.First());
            _ = columnNamesForGrouping.Remove(columnNamesForGrouping.First());

            // why so slow? unattended copies? apply DataView?
            foreach (var name in columnNamesForGrouping) dtList = GroupByColumnValues(dtList, name); 
            
            return dtList;
        }
        private static List<DataTable> GroupByColumnValues(DataTable dtOriginal, string columnNameToGroup)
        {
            // a bit of optimization...
            if (dtOriginal.Rows.Count == 1) return new List<DataTable> { dtOriginal };

            var groups = dtOriginal.AsEnumerable().GroupBy(
                row => row.Field<string>(columnNameToGroup),
                (_, rows) => rows
            ).ToArray();

            List<DataTable> dtList = new(groups.Length);
            foreach (var gr in groups)
            {
                DataTable dtTemp = dtOriginal.Clone();  // particularly, copy Columns, but not data

                foreach (DataRow r in gr) dtTemp.ImportRow(r);

                dtList.Add(dtTemp);
            }

            return dtList;
        }
        private static List<DataTable> GroupByColumnValues(List<DataTable> dtOriginal, string columnNameToGroup)
        {
            List<DataTable> res = new(dtOriginal.Count * 2);

            foreach (DataTable dt in dtOriginal)
            {
                List<DataTable> grouped = GroupByColumnValues(dt, columnNameToGroup);

                foreach (DataTable grdt in grouped) res.Add(grdt);
            }
            return res;
        }

        private DataTable ReadDataFromExcel(string excelFormFilePath)
        {
            using ExcelPackage pck = new(excelFormFilePath);

            // Check SpecWorksheet Name In ExcelFilledForm
            HashSet<string> allWorksheetNames = new( pck.Workbook.Worksheets.Select(x => x.Name) );
            if ( ! allWorksheetNames.Contains(SpecSettings.ExcelFilledFormSpecSheetName)) throw new ArgumentException(
                $"Лист \"{SpecSettings.ExcelFilledFormSpecSheetName}\" не найден в Excel-форме."
            );

            // Read data from worksheet to DataTable
            ExcelWorksheet ws = pck.Workbook.Worksheets[SpecSettings.ExcelFilledFormSpecSheetName];
            DataTable res = WorksheetToDataTable(in ws);

            var q = from DataColumn c in res.Columns select c.ColumnName;
            AllColumnNamesFromExcel = new(q);

            return res;

            static DataTable WorksheetToDataTable(in ExcelWorksheet ws, bool hasHeaderRow = true)
            {
                // https://stackoverflow.com/questions/13396604/excel-to-datatable-using-epplus-excel-locked-for-editing
                DataTable tbl = new() { MinimumCapacity = ws.Dimension.End.Row };

                // add column names to table from the first row of excel worksheet
                tbl.Columns.AddRange(ws.Cells[1, 1, 1, ws.Dimension.End.Column].Select(
                        c => new DataColumn($"{c.Value}")).ToArray()
                );

                var startRow = hasHeaderRow ? 2 : 1;
                for (var rowNum = startRow; rowNum <= ws.Dimension.End.Row; ++rowNum)
                {
                    var wsRow = ws.Cells[rowNum, 1, rowNum, tbl.Columns.Count];
                    var row = tbl.NewRow();
                    foreach (var cell in wsRow) row[cell.Start.Column - 1] = cell.Value;
                    tbl.Rows.Add(row);
                }
                return tbl;
            }
        }
    }
}
