using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Shapes;


namespace prot
{

    public partial class MainWindow : Window
    {
        public static FilesWindow filseWin;
        public static ExcelParsing excelPars;
        public MainWindow()
        {
            InitializeComponent();

        }

        private void Btn_Filese_Generation(object sender, RoutedEventArgs e)
        {
            if (filseWin == null)
            {
                filseWin = new FilesWindow();
                filseWin.Show();
            }
            else
            {
                filseWin.Activate();
            }
        }

        private void Btn_Exsel_Parsing(object sender, RoutedEventArgs e)
        {
            if (excelPars == null)
            {
                excelPars = new ExcelParsing();
                excelPars.Show();
            }
            else
            {
                excelPars.Activate();
            }
        }
    }

}

