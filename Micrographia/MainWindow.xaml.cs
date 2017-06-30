using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;





namespace Micrographia
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MediaPage current;
        private StudyForm sf;
        
        public MainWindow()
        {
            InitializeComponent();
            current = new MediaPage();
            _mainFrame.Navigate(current);
            sf = new StudyForm();
            sf.Show();

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            current?.Dispose();
        }
    }



}
