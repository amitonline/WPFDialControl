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
        
        private const int MIN_VOLUME = 0;
        private const int MAX_VOLUME = 85;

        private const double MIN_REVERB = 0; 
        private const double MAX_REVERB = 1.5;

        private const int MIN_TREBLE = 0; //4 is the starting point
        private const int MAX_TREBLE = 20;

        private const int MIN_BASS = 0; //20 is the starting point
        private const int MAX_BASS = 250;


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
            calcVolume(markerPos, maxMakers);
        }

        private void dial2_DialClick(object sender, RoutedEventArgs e)
        {
            DialClickRoutedEventArgs args = (DialClickRoutedEventArgs)e;
            int markerPos = args.SelectedPos;
            int maxMakers = dial2.getMaxMarkers();
            calcReverb(markerPos, maxMakers);
        }
        private void dial3_DialClick(object sender, RoutedEventArgs e)
        {
            DialClickRoutedEventArgs args = (DialClickRoutedEventArgs)e;
            int markerPos = args.SelectedPos;
            int maxMakers = dial3.getMaxMarkers();
            calcTreble(markerPos, maxMakers);
        }

        private void dial4_DialClick(object sender, RoutedEventArgs e)
        {
            DialClickRoutedEventArgs args = (DialClickRoutedEventArgs)e;
            int markerPos = args.SelectedPos;
            int maxMakers = dial4.getMaxMarkers();
            calcBass(markerPos, maxMakers);
        }

        private void calcVolume(int currPos, int maxPos)
        {
            double valuePerMarker = (double) (MAX_VOLUME - MIN_VOLUME) / (maxPos-1);
            double finalVal = valuePerMarker * (currPos);
            string strVal = string.Format("{0:N2}", finalVal);
            lblVolume.Text = strVal + "dB";
        }

        private void calcReverb(int currPos, int maxPos)
        {
            double valuePerMarker = (double)(MAX_REVERB - MIN_REVERB) / (maxPos-1);
            double finalVal = valuePerMarker * (currPos);
            string strVal = string.Format("{0:N2}", finalVal);
            lblReverb.Text = strVal + " seconds";
        }

        private void calcTreble(int currPos, int maxPos)
        {
            double valuePerMarker = (double)(MAX_TREBLE - MIN_TREBLE) / (maxPos - 1);
            double finalVal = valuePerMarker * (currPos);
            string strVal = string.Format("{0:N2}", finalVal);
            lblTreble.Text = strVal + "Khz";
        }

        private void calcBass(int currPos, int maxPos)
        {
            double valuePerMarker = (double)(MAX_BASS - MIN_BASS) / (maxPos - 1);
            double finalVal = valuePerMarker * (currPos);
            string strVal = string.Format("{0:N2}", finalVal);
            lblBass.Text = strVal + "Hz";
        }

       
    }
}