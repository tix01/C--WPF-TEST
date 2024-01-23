using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Shapes;


namespace prot
{

    public partial class FilesWindow : Window
    {
        private string constr = "Server=localhost;Database=DB_Files_geretions;Uid=root;pwd=1111;charset=utf8";
        private string patternToRemove;
        private string selectedPath;
        public FilesWindow()
        {
            InitializeComponent();
            
        }

        private void Btn_Create_Files(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            DialogResult result = folderBrowserDialog.ShowDialog();

            
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string selectedPath = folderBrowserDialog.SelectedPath;

                for (int i = 1; i <= 100; i++)
                {
                    FileGenerator.GenerateFile(System.IO.Path.Combine(selectedPath), $"output_{i}.txt");
                }

                System.Windows.MessageBox.Show("Генерация файлов завершена!");
            }
        }

        private void Btn_Megre_Files(object sender, RoutedEventArgs e)
        {

            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            DialogResult result = folderBrowserDialog.ShowDialog();
            PatternTextBox.Visibility = Visibility.Visible;
            PaternText.Visibility = Visibility.Visible;
            System.Windows.MessageBox.Show("Введите паттерн для удаления строк (или оставьте поле пустым, чтобы объединить все строки).");
            
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                selectedPath = folderBrowserDialog.SelectedPath;
            }
        }

        private async void Btn_Upload_DB(object sender, RoutedEventArgs e)
        {
            try
            {
                MySqlConnection mycon = new MySqlConnection(constr);
                mycon.Open();
                System.Windows.MessageBox.Show("Connected to the database.");

                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

                if (openFileDialog.ShowDialog() == true)
                {
                    string filePath = openFileDialog.FileName;

                   
                    await LoadDataFromFile(mycon, filePath);

                    mycon.Close();
                }
                else
                {
                    
                    System.Windows.MessageBox.Show("Выбор файла отменен.");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Произошла ошибка: {ex.Message}");
            }
        }
        private async Task LoadDataFromFile(MySqlConnection connection, string filePath)
        {
            try
            {
                int totalRows = File.ReadLines(filePath).Count();
                int importedRows = 0;

                progressBar.Maximum = totalRows;

                using (StreamReader reader = new StreamReader(filePath))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        string[] data = line.Split(new string[] { "||" }, StringSplitOptions.None);

                        EntityModel entity = new EntityModel
                        {
                            Date = DateTime.ParseExact(data[0].Trim(), "dd.MM.yyyy", null),
                            LatinChars = data[1].Trim(),
                            RussianChars = data[2].Trim(),
                            IntegerNumber = int.Parse(data[3].Trim()),
                            DecimalNumber = decimal.Parse(data[4].Trim())
                        };


                        await InsertDataIntoDatabase(connection, entity);

                        importedRows++;
                        int remainingRows = totalRows - importedRows;
                        await UpdateUI(importedRows, totalRows, remainingRows);
                    }
                }

                System.Windows.MessageBox.Show("Data loaded successfully.");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An error occurred during data loading: {ex.Message}");
            }
        }

        private async Task InsertDataIntoDatabase(MySqlConnection connection, EntityModel entity)
        {
            try
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    cmd.Connection = connection;
                    cmd.CommandText = "INSERT INTO Files (Date, LatinChars, RussianChars, IntegerNumber, DecimalNumber) VALUES (@Date, @LatinChars, @RussianChars, @IntegerNumber, @DecimalNumber)";
                    cmd.Parameters.AddWithValue("@Date", entity.Date);
                    cmd.Parameters.AddWithValue("@LatinChars", entity.LatinChars);
                    cmd.Parameters.AddWithValue("@RussianChars", entity.RussianChars);
                    cmd.Parameters.AddWithValue("@IntegerNumber", entity.IntegerNumber);
                    cmd.Parameters.AddWithValue("@DecimalNumber", entity.DecimalNumber);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting data into the database: {ex.Message}");
            }
        }

        private async Task UpdateUI(int importedRows, int totalRows, int remainingRows)
        {
            
            await Dispatcher.InvokeAsync(() =>
            {
                progressBar.Value = importedRows;
                Console.WriteLine($"Imported: {importedRows}, Remaining: {remainingRows}");
            });
        }

        private void Btn_Calculate(object sender, RoutedEventArgs e)
        {
            try
            {
                
                using (MySqlConnection connection = new MySqlConnection(constr))
                {
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand("CalculateSumAndMedian", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                long totalSum = reader.GetInt64("TotalSum");
                                decimal decimalMedian = reader.GetDecimal("DecimalMedian");

                                System.Windows.MessageBox.Show($"Total Sum: {totalSum}\nDecimal Median: {decimalMedian}");
                            }
                            else
                            {
                                System.Windows.MessageBox.Show("No data returned.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private void Btn_Patern_Text(object sender, RoutedEventArgs e)
        {
            patternToRemove = PatternTextBox.Text;
            System.Windows.MessageBox.Show($"Patern {patternToRemove}");
            FileGenerator.MergeFiles("merged_output.txt", patternToRemove, System.IO.Path.Combine(selectedPath));
            PatternTextBox.Visibility = Visibility.Hidden;
            PatternTextBox.Text = string.Empty;
            PaternText.Visibility = Visibility.Hidden;
        }
    }

}

