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

namespace IdlingMiner
{
    /// <summary>
    /// Логика взаимодействия для PassWindows.xaml
    /// </summary>
    public partial class PassWindows : Window
    {
        private bool _Access = false;

        public bool Access
        {
        get { return _Access; }
        }
        public PassWindows()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            _Access = false;
            if (textBoxLogin.Text.ToString() == "admin" && passwordBox.Password.ToString() == "Access")
                _Access = true;
            this.Close();
        }
    }
}
