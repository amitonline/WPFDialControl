using System.CodeDom;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfDial;
using static System.Formats.Asn1.AsnWriter;

namespace DialControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
        }

       
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           
        }

        private void dial1_DialClick(object sender, RoutedEventArgs e)
        {
            DialClickRoutedEventArgs args = (DialClickRoutedEventArgs)e;
            int markerPos = args.SelectedPos;
            int maxMakers = dial1.getMaxMarkers();

        }
    }
}