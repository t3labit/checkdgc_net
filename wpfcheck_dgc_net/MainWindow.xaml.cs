using System;
using System.Net;
using System.Windows;
using checkgreenpass;


namespace wpfcheck_dgc_net
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        public Core C;
        public MainWindow()
        {
            InitializeComponent();
            C = new Core();
        }
    }
}
