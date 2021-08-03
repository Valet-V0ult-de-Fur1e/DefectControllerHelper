using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

namespace DefectControllerHelper
{
    public class ExselWorker
    {
        public static Excel.Application ExcelApp = new Excel.Application();
        private static Excel.Workbook workbook;
        private static Excel.Sheets worksheets;
        internal static void MakeMaket(Dictionary<string, Dictionary<string, List<string>>> parameters, string name, string city)
        {
            ExcelApp.Visible = false;
            workbook = ExcelApp.Workbooks.Add(Type.Missing);
            for (int sheet_id = 0; sheet_id < parameters.Keys.Count; sheet_id++)
            {
                Excel.Worksheet worksheet = (Excel.Worksheet)ExcelApp.Worksheets.get_Item(sheet_id+1);
                worksheet.Name = parameters.ElementAt(sheet_id).Key;
                for(int id_parametr = 1; id_parametr < parameters.ElementAt(sheet_id).Value.Count + 2; id_parametr ++)
                {
                    switch (id_parametr)
                    {
                        case 1:
                            worksheet.Cells[1, id_parametr] = "Серийный номер в кассе";
                            break;
                        case 2:
                            worksheet.Cells[1, id_parametr] = "Серийный номер на кассе";
                            break;
                        default:
                            worksheet.Cells[1, id_parametr] = parameters.ElementAt(sheet_id).Value.ElementAt(id_parametr - 3).Key;
                            break;
                    }
                    worksheet.Cells[1, parameters.ElementAt(sheet_id).Value.Count + 2] = "стоимость ремонта";
                    worksheet.Cells[1, parameters.ElementAt(sheet_id).Value.Count + 3] = "состояние";
                    worksheet.Cells[1, parameters.ElementAt(sheet_id).Value.Count + 4] = "комментарий";
                    worksheet.Cells.EntireColumn.AutoFit();
                    worksheet.Cells.EntireRow.AutoFit();
                }
                Excel.Sheets sheets = workbook.Sheets;
                Excel.Worksheet sheetPivot = (Excel.Worksheet)sheets.Add(Type.Missing, sheets[sheet_id+1], Type.Missing, Type.Missing);
                sheetPivot.Name = sheet_id.ToString();
            }
            string filedate = DateTime.Now.ToShortDateString();
            string basepath = AppDomain.CurrentDomain.BaseDirectory;
            ExcelApp.Application.ActiveWorkbook.SaveAs(string.Format("Дефектовка {0} {1} {2}.xlsx", name, city, filedate));
        }
        internal static void Add_New_Item(int id_model, List<List<string>> list_parameters_rusult, int id_item, string status)
        {
            worksheets = workbook.Worksheets;
            Excel.Worksheet worksheet = worksheets.Item[id_model + 1];
            for (int id_item_top = 0; id_item_top < list_parameters_rusult.Count; id_item_top++)
            {
                worksheet.Cells[id_item - 1, id_item_top + 1].Value = list_parameters_rusult[id_item_top][0];
            }
            worksheet.Cells[id_item - 1, list_parameters_rusult.Count + 1].Value = status;
            ExcelApp.Application.ActiveWorkbook.Save();
        }
        public static void Clear_Data_Full()
        {
            ExcelApp.Quit();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            Marshal.ReleaseComObject(ExcelApp);
        }
    }
    public partial class MainWindow : Window
    {
        private static Dictionary<string, Dictionary<string, List<string>>> param = new Dictionary<string, Dictionary<string, List<string>>>();
        private static Dictionary<string, int> kkt_prise_dictionary = new Dictionary<string, int>();
        private string PARAMETERS_PATH = "../../price_json.json";
        private string MAIN_PARAMETERS_PATH = "../../price_models_json.json";
        private ExselWorker exsel_worker = new ExselWorker();
        public MainWindow()
        {
            InitializeComponent();
            Parser();
            NewTableDialogWindow();
        }
        private void NewTableDialogWindow()
        {
            NewTableWindow NewTableWindow = new NewTableWindow();
            NewTableWindow.Show();
        }
        internal static void NewTable(List<string> NewTableParameters)
        {
            ExselWorker.MakeMaket(param, NewTableParameters[0], NewTableParameters[1]);
        }
        private void Parser()
        {
            string json = File.ReadAllText(PARAMETERS_PATH);
            string main_json = File.ReadAllText(MAIN_PARAMETERS_PATH);
            param = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<string>>>>(json);
            kkt_prise_dictionary = JsonConvert.DeserializeObject<Dictionary<string, int>>(main_json);
            SepareteParams();
            foreach (var parametr in param)
            {
                parametr.Value.Add("object_num", new List<string> { "2" });
            }
        }
        private void SepareteParams()
        {
            foreach (var model_name in param.Keys)
            {
                KKT_Model.Items.Add(new TextBlock { Text = model_name });
            }
        }
        private void Update_Params()
        {
            string model = KKT_Model.Text.ToString();
            string number = KKT_id.Text.ToString();
            if (model == "")
            {
                model = param.ElementAt(0).Key;
                KKT_Model.Text = model;
            }
            params_list.Items.Clear();
            foreach (var param_item in param[model])
            {
                if (param_item.Key != "object_num")
                {
                    params_list.Items.Add(new CheckBox { Content = param_item.Key, IsChecked = false });
                }
            }
        }
        private void Get_Model_and_ID_Button_Click(object sender, RoutedEventArgs e)
        {
            Update_Params();
            KKT_id.Clear();
            KKT_inside_id.Clear();
            Problem_Status_Box.Clear();
        }
        private void Add_Button_Click(object sender, RoutedEventArgs e)
        {
            int max_price = 1;
            int update_price = 1;
            int sum = 0;
            List<List<string>> parameters_item = new List<List<string>>();
            parameters_item.Add(new List<string> { KKT_id.Text });
            parameters_item.Add(new List<string> { KKT_inside_id.Text });
            param[KKT_Model.Text.ToString()]["object_num"][0] = (int.Parse(param[KKT_Model.Text.ToString()]["object_num"][0]) + 1).ToString();
            foreach (CheckBox parametr in params_list.Items)
            {
                if (parametr.IsChecked == true)
                {
                    if (int.Parse(param[KKT_Model.Text.ToString()][parametr.Content.ToString()][0]) > max_price)
                    {
                        max_price = int.Parse(param[KKT_Model.Text.ToString()][parametr.Content.ToString()][0]);
                        update_price = int.Parse(param[KKT_Model.Text.ToString()][parametr.Content.ToString()][1]);
                    }
                    parameters_item.Add(param[KKT_Model.Text.ToString()][parametr.Content.ToString()]);
                    sum += int.Parse(param[KKT_Model.Text.ToString()][parametr.Content.ToString()][0]);
                }
                else
                {
                    parameters_item.Add(new List<string> { "1" });
                }
            }
            sum += update_price - max_price;
            parameters_item.Add(new List<string> { sum.ToString() });
            if (kkt_prise_dictionary[KKT_Model.Text.ToString()] > sum && sum != 0) parameters_item.Add(new List<string> { "ремонт" });
            else if (kkt_prise_dictionary[KKT_Model.Text.ToString()] < sum) parameters_item.Add(new List<string> { "утилизация" });
            else if (sum == 0) parameters_item.Add(new List<string> { "рабочий" });
            ExselWorker.Add_New_Item(Array.IndexOf(param.Keys.ToArray(), KKT_Model.Text.ToString()), parameters_item, int.Parse(param[KKT_Model.Text.ToString()]["object_num"][0]), Problem_Status_Box.Text.ToString());
        }
        protected override void OnClosed(EventArgs e)
        {
            ExselWorker.Clear_Data_Full();
            ExselWorker.ExcelApp.Quit();
            base.OnClosed(e);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}
