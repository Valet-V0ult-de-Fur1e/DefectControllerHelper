using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace DefectControllerHelper
{
    public partial class NewTableWindow : Window
    {
        List<string> user_parameters = new List<string>();
        public NewTableWindow()
        {
            InitializeComponent();
        }

        private void Make_New_Table_Button_Click(object sender, RoutedEventArgs e)
        {
            user_parameters.Add(User_Name.Text);
            user_parameters.Add(User_City.Text);
            MainWindow.NewTable(user_parameters);
            Close();
        }
    }
}
