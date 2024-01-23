using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
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
using MySql.Data.MySqlClient;
using OfficeOpenXml;
using System.IO;

using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace prot
{
    public partial class ExcelParsing : Window
    {
        private AllDataViewModel _allDataViewModel;
        private DatabaseHelper databaseHelper;
        private ExcelHelper excelHelper;
        public ExcelParsing()
        {
            InitializeComponent();
            string constr = "Server=localhost;Database=DB_Exel;Uid=root;pwd=1111;charset=utf8";
            databaseHelper = new DatabaseHelper(constr);
            excelHelper = new ExcelHelper();
            _allDataViewModel = new AllDataViewModel();

            DataContext = _allDataViewModel;
        }
        public void Upload_DB(object sender, RoutedEventArgs e)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            try
            {
                string filePath = GetExcelFilePath();

                if (string.IsNullOrEmpty(filePath))
                    return;

                using (MySqlConnection mycon = databaseHelper.OpenConnection())
                {
                    
                    try
                    {
                        using (var package = excelHelper.OpenExcelPackage(filePath))
                        {

                            ExcelWorksheet worksheet = excelHelper.GetWorksheet(package, 0);
                            
                            ProcessExcelData(mycon, worksheet);
                            
                        }
                    }
                    catch (Exception ex)
                    {
                        HandleException(ex);
                    }

                    databaseHelper.CloseConnection(mycon);
                    MessageBox.Show("Data uploaded to the database.");
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private string GetExcelFilePath()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Выберите файл Excel",
                Filter = "Файлы Excel|*.xlsx;*.xls"
            };

            return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : null;
        }

        private void ProcessExcelData(MySqlConnection mycon, ExcelWorksheet worksheet)
        {
            int bankId = InsertBankRecord(mycon, worksheet.Cells[1, 1].Text);
            int infoId = InsertReportsInfoRecord(mycon, worksheet.Cells[2, 1].Text, worksheet.Cells[3, 1].Text, worksheet.Cells[6, 7].Text, worksheet.Cells[6, 1].Text);
            int reportId = InsertReportsRecord(mycon, bankId, infoId);

            int indexColumn = 1;
            int indexRow = 9;
            int classNumber = 0;
            do
            {
                string classDescription = worksheet.Cells[indexRow, indexColumn].Text;

                if (classDescription == "БАЛАНС")
                {
                    ProcessBalanceRecord(mycon, worksheet, reportId, ref classNumber, indexRow, indexColumn);
                    break;
                }
                classNumber = InsertClassRecord(mycon, classNumber, classDescription);



                indexRow++;
                indexColumn = 1;
                do
                {
                    ProcessAccountEntryRecord(mycon, worksheet, reportId, classNumber, indexRow, ref indexColumn);

                    indexRow++;
                    indexColumn = 1;
                } while (worksheet.Cells[indexRow, 1].Text != "ПО КЛАССУ");
                indexRow++;

                InsertReportClassRecord(mycon, reportId, classNumber);

            } while (true);
        }

        private int InsertBankRecord(MySqlConnection mycon, string bankName)
        {
            try
            {
                databaseHelper.ExecuteNonQuery(mycon, "INSERT INTO banks (bank_name) VALUES (@bank_name)",
                    new MySqlParameter("@bank_name", bankName));

                return databaseHelper.GetLastBankId(mycon) + 1;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return 1;
        }

        private int InsertReportsInfoRecord(MySqlConnection mycon, string reportName, string dateString, string currency, string creationDateStr)
        {
            try
            {
                string startDateStr = ParseStartDate(dateString);
                string endDateStr = ParseEndDate(dateString);

                databaseHelper.ExecuteNonQuery(mycon, "INSERT INTO reports_info (creation_date, currency, report_name, start_date, end_date) VALUES (@creation_date, @currency, @report_name, @start_date, @end_date)",
                    new MySqlParameter("@creation_date", DateTime.Parse(creationDateStr)),
                    new MySqlParameter("@currency", currency),
                    new MySqlParameter("@report_name", reportName),
                    new MySqlParameter("@start_date", startDateStr),
                    new MySqlParameter("@end_date", endDateStr));

                return databaseHelper.GetLastInfoId(mycon) + 1;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return 1;
        }

        private int InsertReportsRecord(MySqlConnection mycon, int bankId, int infoId)
        {
            try
            {
                databaseHelper.ExecuteNonQuery(mycon, "INSERT INTO reports (bank_id, info_id) VALUES (@bank_id, @info_id)",
                    new MySqlParameter("@bank_id", bankId),
                    new MySqlParameter("@info_id", infoId));

                return databaseHelper.GetLastReportId(mycon);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return 1;
        }

        private int InsertClassRecord(MySqlConnection mycon, int classNumber, string classDescription)
        {
            try
            {
                databaseHelper.ExecuteNonQuery(mycon, "INSERT INTO classes (class_number, class_name) VALUES (@class_number, @class_name)",
                    new MySqlParameter("@class_number", classNumber),
                    new MySqlParameter("@class_name", classDescription));

                return classNumber + 1;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return 1;
        }

        private void ProcessBalanceRecord(MySqlConnection mycon, ExcelWorksheet worksheet, int reportId, ref int classNumber, int indexRow, int indexColumn)
        {
            try
            {
                string b_sch = worksheet.Cells[indexRow, indexColumn].Text;
                string vhodyaschee_saldo_aktiv = worksheet.Cells[indexRow, indexColumn + 1].Text;
                string vhodyaschee_saldo_passiv = worksheet.Cells[indexRow, indexColumn + 2].Text;
                string oborot_debet = worksheet.Cells[indexRow, indexColumn + 3].Text;
                string oborot_kredit = worksheet.Cells[indexRow, indexColumn + 4].Text;
                string ishodyaschee_saldo_aktiv = worksheet.Cells[indexRow, indexColumn + 5].Text;
                string ishodyaschee_saldo_passiv = worksheet.Cells[indexRow, indexColumn + 6].Text;

                databaseHelper.ExecuteNonQuery(mycon, "INSERT INTO account_entries (report_id, class_id, b_sch, vhodyaschee_saldo_aktiv, vhodyaschee_saldo_passiv, oborot_debet, oborot_kredit, ishodyaschee_saldo_aktiv, ishodyaschee_saldo_passiv) VALUES (@report_id, @class_id, @b_sch, @vhodyaschee_saldo_aktiv, @vhodyaschee_saldo_passiv, @oborot_debet, @oborot_kredit, @ishodyaschee_saldo_aktiv, @ishodyaschee_saldo_passiv)",
                    new MySqlParameter("@report_id", reportId),
                    new MySqlParameter("@class_id", classNumber),
                    new MySqlParameter("@b_sch", b_sch),
                    new MySqlParameter("@vhodyaschee_saldo_aktiv", vhodyaschee_saldo_aktiv),
                    new MySqlParameter("@vhodyaschee_saldo_passiv", vhodyaschee_saldo_passiv),
                    new MySqlParameter("@oborot_debet", oborot_debet),
                    new MySqlParameter("@oborot_kredit", oborot_kredit),
                    new MySqlParameter("@ishodyaschee_saldo_aktiv", ishodyaschee_saldo_aktiv),
                    new MySqlParameter("@ishodyaschee_saldo_passiv", ishodyaschee_saldo_passiv));
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void ProcessAccountEntryRecord(MySqlConnection mycon, ExcelWorksheet worksheet, int reportId, int classNumber, int indexRow, ref int indexColumn)
        {
            try
            {
                string b_sch = worksheet.Cells[indexRow, indexColumn].Text;
                string vhodyaschee_saldo_aktiv = worksheet.Cells[indexRow, indexColumn + 1].Text;
                string vhodyaschee_saldo_passiv = worksheet.Cells[indexRow, indexColumn + 2].Text;
                string oborot_debet = worksheet.Cells[indexRow, indexColumn + 3].Text;
                string oborot_kredit = worksheet.Cells[indexRow, indexColumn + 4].Text;
                string ishodyaschee_saldo_aktiv = worksheet.Cells[indexRow, indexColumn + 5].Text;
                string ishodyaschee_saldo_passiv = worksheet.Cells[indexRow, indexColumn + 6].Text;

                databaseHelper.ExecuteNonQuery(mycon, "INSERT INTO account_entries (report_id, class_id, b_sch, vhodyaschee_saldo_aktiv, vhodyaschee_saldo_passiv, oborot_debet, oborot_kredit, ishodyaschee_saldo_aktiv, ishodyaschee_saldo_passiv) VALUES (@report_id, @class_id, @b_sch, @vhodyaschee_saldo_aktiv, @vhodyaschee_saldo_passiv, @oborot_debet, @oborot_kredit, @ishodyaschee_saldo_aktiv, @ishodyaschee_saldo_passiv)",
                    new MySqlParameter("@report_id", reportId),
                    new MySqlParameter("@class_id", classNumber),
                    new MySqlParameter("@b_sch", b_sch),
                    new MySqlParameter("@vhodyaschee_saldo_aktiv", vhodyaschee_saldo_aktiv),
                    new MySqlParameter("@vhodyaschee_saldo_passiv", vhodyaschee_saldo_passiv),
                    new MySqlParameter("@oborot_debet", oborot_debet),
                    new MySqlParameter("@oborot_kredit", oborot_kredit),
                    new MySqlParameter("@ishodyaschee_saldo_aktiv", ishodyaschee_saldo_aktiv),
                    new MySqlParameter("@ishodyaschee_saldo_passiv", ishodyaschee_saldo_passiv));

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

        }

        private void InsertReportClassRecord(MySqlConnection mycon, int reportId, int classNumber)
        {
            try
            {
                databaseHelper.ExecuteNonQuery(mycon, "INSERT INTO report_class (report_id, class_id) VALUES (@report_id, @class_id)",
                    new MySqlParameter("@report_id", reportId),
                    new MySqlParameter("@class_id", classNumber));
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }



        private void HandleException(Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}");
            Console.WriteLine($"Error: {ex.Message}");
        }
        static string ParseStartDate(string dateString)
        {
            
            int startIndex = dateString.IndexOf("с ") + 2; 
            int endIndex = dateString.IndexOf(" по ");     
            string startDateStr = dateString.Substring(startIndex, endIndex - startIndex);
            
            return startDateStr;
        }

        static string ParseEndDate(string dateString)
        {

            int startIndex = dateString.IndexOf("по ") + 3;
            string endDateStr = dateString.Substring(startIndex);
            return endDateStr;
        }

        private void Load_DB(object sender, RoutedEventArgs e)
        {

            
            string bankName = Microsoft.VisualBasic.Interaction.InputBox("Введите название банка:", "Введите название банка", "");

            if (!string.IsNullOrEmpty(bankName))
            {
                LoadData(bankName);
            }
        }

        private void LoadData(string bank)
        {


            try
            {
                using (MySqlConnection connection = new MySqlConnection("Server=localhost;Database=DB_Exel;Uid=root;pwd=1111;charset=utf8;"))
                {
                    connection.Open();

                    string query = "SELECT * FROM AllDataView WHERE bank_name = @bank_name;";

                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        try
                        {
                            cmd.Connection = connection;
                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@bank_name", bank);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"ERORRRRR{ex.Message}");
                        }
                        DataTable dt = new DataTable();

                        using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }

                        _allDataViewModel.AllDataItems = new ObservableCollection<AllDataItem>(
                            dt.AsEnumerable().Select(row => new AllDataItem
                            {
                                BankId = row.Field<int>("bank_id"),
                                BankName = row.Field<string>("bank_name"),
                                ClassId = row.Field<int>("class_id"),
                                ClassNumber = row.Field<int>("class_number"),
                                ClassName = row.Field<string>("class_name"),
                                InfoId = row.Field<int>("info_id"),
                                CreationDate = row.Field<DateTime>("creation_date"),
                                Currency = row.Field<string>("currency"),
                                ReportName = row.Field<string>("report_name"),
                                StartDate = row.Field<string>("start_date"),
                                EndDate = row.Field<string>("end_date"),
                                ReportId = row.Field<int>("report_id"),
                                ReportBankId = row.Field<int>("report_bank_id"),
                                ReportInfoId = row.Field<int>("report_info_id"),
                                RcReportId = row.Field<int>("rc_report_id"),
                                RcClassId = row.Field<int>("rc_class_id"),
                                EntryId = row.Field<int>("entry_id"),
                                AeReportId = row.Field<int>("ae_report_id"),
                                AeClassId = row.Field<int>("ae_class_id"),
                                BSch = row.Field<string>("b_sch"),
                                VhodyascheeSaldoAktiv = row.Field<string>("vhodyaschee_saldo_aktiv"),
                                VhodyascheeSaldoPassiv = row.Field<string>("vhodyaschee_saldo_passiv"),
                                OborotDebet = row.Field<string>("oborot_debet"),
                                OborotKredit = row.Field<string>("oborot_kredit"),
                                IshodyascheeSaldoAktiv = row.Field<string>("ishodyaschee_saldo_aktiv"),
                                IshodyascheeSaldoPassiv = row.Field<string>("ishodyaschee_saldo_passiv"),
                            })
                        );
                    }
                    MessageBox.Show("load");
                    dataGrid.Visibility = Visibility.Visible;
                    closeTable.Visibility = Visibility.Visible;
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        private void Btn_Close(object sender, RoutedEventArgs e)
        {
            dataGrid.Visibility = Visibility.Collapsed;
            closeTable.Visibility = Visibility.Collapsed;

        }
    }
}

public class AllDataViewModel : INotifyPropertyChanged
{
    private ObservableCollection<AllDataItem> _allDataItems;

    public ObservableCollection<AllDataItem> AllDataItems
    {
        get { return _allDataItems; }
        set
        {
            _allDataItems = value;
            OnPropertyChanged(nameof(AllDataItems));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class AllDataItem
{
    public int BankId { get; set; }
    public string BankName { get; set; }
    public int ClassId { get; set; }
    public int ClassNumber { get; set; }
    public string ClassName { get; set; }
    public int InfoId { get; set; }
    public DateTime CreationDate { get; set; }
    public string Currency { get; set; }
    public string ReportName { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public int ReportId { get; set; }
    public int ReportBankId { get; set; }
    public int ReportInfoId { get; set; }
    public int RcReportId { get; set; }
    public int RcClassId { get; set; }
    public int EntryId { get; set; }
    public int AeReportId { get; set; }
    public int AeClassId { get; set; }
    public string BSch { get; set; }
    public string VhodyascheeSaldoAktiv { get; set; }
    public string VhodyascheeSaldoPassiv { get; set; }
    public string OborotDebet { get; set; }
    public string OborotKredit { get; set; }
    public string IshodyascheeSaldoAktiv { get; set; }
    public string IshodyascheeSaldoPassiv { get; set; }
}

public class ExcelHelper
{
    public ExcelPackage OpenExcelPackage(string filePath)
    {
        return new ExcelPackage(new FileInfo(filePath));
    }

    public ExcelWorksheet GetWorksheet(ExcelPackage package, int index)
    {
        return package.Workbook.Worksheets[index];
    }
}
public class DatabaseHelper
{
    private string constr;

    public DatabaseHelper(string connectionString)
    {
        constr = connectionString;
    }

    public MySqlConnection OpenConnection()
    {
        MySqlConnection mycon = new MySqlConnection(constr);
        mycon.Open();
        return mycon;
    }

    public void CloseConnection(MySqlConnection connection)
    {
        if (connection.State == ConnectionState.Open)
        {
            connection.Close();
        }
    }

    public int ExecuteNonQuery(MySqlConnection connection, string query, params MySqlParameter[] parameters)
    {
        using (var cmd = new MySqlCommand())
        {
            cmd.Connection = connection;
            cmd.CommandText = query;

            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }

            return cmd.ExecuteNonQuery();
        }
    }


    public int GetLastReportId(MySqlConnection connection)
    {
        string query = "SELECT * FROM reports ORDER BY report_id DESC LIMIT 1;";
        int reportId = 0;

        using (MySqlCommand cmd = new MySqlCommand(query, connection))
        {
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    reportId = reader.GetInt32("report_id");
                }
            }
        }

        return reportId;
    }
    public int GetLastBankId(MySqlConnection connection)
    {
        string query = "SELECT * FROM reports ORDER BY bank_id DESC LIMIT 1;";
        int reportId = 0;

        using (MySqlCommand cmd = new MySqlCommand(query, connection))
        {
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    reportId = reader.GetInt32("bank_id");
                }
            }
        }

        return reportId;
    }
    public int GetLastInfoId(MySqlConnection connection)
    {
        string query = "SELECT * FROM reports ORDER BY info_id DESC LIMIT 1;";
        int reportId = 0;

        using (MySqlCommand cmd = new MySqlCommand(query, connection))
        {
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    reportId = reader.GetInt32("info_id");
                }
            }
        }

        return reportId;
    }
}



