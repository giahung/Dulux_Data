using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Excel = Microsoft.Office.Interop.Excel;

namespace Dulux
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static List<Info> Infos = new List<Info>();
        private SolidColorBrush ColorWhite = new SolidColorBrush(System.Windows.Media.Colors.White);

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ResizeWindow();
            //ReadData();
            ReadDataOpenXML();
            BindData();
        }

        private void BindData()
        {
            foreach (var info in Infos)
            {
                DataGridContainer.RowDefinitions.Add( new RowDefinition() { Height = new GridLength(50) });
                var lblNo = new Label();
                lblNo.Content = info.No;
                lblNo.SetValue(Grid.ColumnProperty, 0);
                lblNo.SetValue(Grid.RowProperty, DataGridContainer.RowDefinitions.Count - 1);
                DataGridContainer.Children.Add(lblNo);

                var lblName = new Label();
                lblName.Content = info.Name;
                lblName.SetValue(Grid.ColumnProperty, 1);
                lblName.SetValue(Grid.RowProperty, DataGridContainer.RowDefinitions.Count - 1);
                DataGridContainer.Children.Add(lblName);

                var lblAddress = new Label();
                lblAddress.Content = info.Address;
                lblAddress.SetValue(Grid.ColumnProperty, 2);
                lblAddress.SetValue(Grid.RowProperty, DataGridContainer.RowDefinitions.Count - 1);
                DataGridContainer.Children.Add(lblAddress);

                var lblAmount = new Label();
                lblAmount.Content = info.Amount;
                lblAmount.SetValue(Grid.ColumnProperty, 3);
                lblAmount.SetValue(Grid.RowProperty, DataGridContainer.RowDefinitions.Count - 1);
                DataGridContainer.Children.Add(lblAmount);
            }
        }

        private void ResizeWindow()
        {
            if (this.ActualWidth >= 2000 && this.ActualHeight >= 418)
            {
                Container.Width = 2000;
                Container.Height = 418;
                return;
            }
            
            double ratio = 418 / 2000.0;
            double currentRatio = this.ActualHeight / this.ActualWidth;

            if (currentRatio > ratio)
            {
                Container.Width = this.ActualWidth;
                Container.Height = Container.Width * ratio;
            }
            else
            {
                Container.Height = this.ActualHeight;
                Container.Width = Container.Height / ratio;
            }
        }

        private static void ReadData()
        {
            var path = Directory.GetCurrentDirectory();
            //Create COM Objects. Create a COM object for everything that is referenced
            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(path + @"\Dulux_data.xlsx");
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;

            int rowCount = xlRange.Rows.Count;
            int colCount = xlRange.Columns.Count;

            //iterate over the rows and columns and print to the console as it appears in the file
            //excel is not zero based!!
            for (int i = 2; i <= rowCount; i++)
            {
                try
                {
                    var info = new Info();
                    info.No = xlRange.Cells[i, 1].Value2.ToString();
                    info.Name = xlRange.Cells[i, 2].Value2.ToString();
                    info.Address = xlRange.Cells[i, 3].Value2.ToString();
                    info.Amount = xlRange.Cells[i, 4].Value2.ToString();
                    Infos.Add(info);
                }
                catch (Exception)
                {
                }
                //for (int j = 1; j <= colCount; j++)
                //{
                //    //new line
                //    //if (j == 1)
                //    //    Console.Write("\r\n");

                //    //write the value to the console
                //    if (xlRange.Cells[i, j] != null && xlRange.Cells[i, j].Value2 != null)
                //    {
                //        //Console.Write(xlRange.Cells[i, j].Value2.ToString() + "\t");
                //        switch (j)
                //        {
                //            case 1:
                //                info.No = xlRange.Cells[i, j].Value2.ToString();
                //                break;
                //            case 2:

                //                break;
                //            case 3:
                //                break;
                //            case 4:
                //                break;
                //            default:
                //                break;
                //        }
                //    }
                //}
            }

            //cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();

            //rule of thumb for releasing com objects:
            //  never use two dots, all COM objects must be referenced and released individually
            //  ex: [somthing].[something].[something] is bad

            //release com objects to fully kill excel process from running in the background
            Marshal.ReleaseComObject(xlRange);
            Marshal.ReleaseComObject(xlWorksheet);

            //close and release
            xlWorkbook.Close();
            Marshal.ReleaseComObject(xlWorkbook);

            //quit and release
            xlApp.Quit();
            Marshal.ReleaseComObject(xlApp);
        }


        private static void ReadDataOpenXML()
        {
            try
            {
                var path = Directory.GetCurrentDirectory();
                var filePath = path + @"\Dulux_data.xlsx";

                using (SpreadsheetDocument doc = SpreadsheetDocument.Open(filePath, false))
                {
                    WorkbookPart workbookPart = doc.WorkbookPart;
                    IEnumerable<Sheet> sheets = doc.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();
                    string relationshipId = sheets.First().Id.Value;
                    WorksheetPart worksheetPart = (WorksheetPart)doc.WorkbookPart.GetPartById(relationshipId);
                    Worksheet workSheet = worksheetPart.Worksheet;
                    SheetData sheetData = workSheet.GetFirstChild<SheetData>();
                    List<Row> rows = sheetData.Descendants<Row>().ToList();

                    for (int i = 1; i < rows.Count(); i++)
                    {
                        var info = new Info();
                        info.No = GetCellValue(doc, rows[i].Descendants<Cell>().ElementAt(0));
                        info.Name = GetCellValue(doc, rows[i].Descendants<Cell>().ElementAt(1));
                        info.Address = GetCellValue(doc, rows[i].Descendants<Cell>().ElementAt(2));
                        info.Amount = GetCellValue(doc, rows[i].Descendants<Cell>().ElementAt(3));
                        Infos.Add(info);                
                    }
                }
            }
            catch (Exception)
            {
                
            }
        }

        private static string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
            string value = cell.CellValue.InnerXml;

            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
            }
            else
            {
                return value;
            }
        }
    }
}
