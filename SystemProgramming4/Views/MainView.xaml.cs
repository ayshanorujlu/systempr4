using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SystemProgramming4.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace SystemProgramming4.Views
{

    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
            DataContext = new MainViewModel();

        }
        private void AppClose_ButtonClicked(object sender, RoutedEventArgs e) => Close();

        private void DragWindow_MouseDown(object sender, MouseButtonEventArgs e) => DragMove();
    }
}

