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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DialControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int DIAL_ZINDEX = 5;
        private const int LINEPOINTER_ZINDEX = 6;
        private const double LINE_PART_TO_HIDE = 0.90;
        private const double LINE_PART_TO_HIDE_FOR_MARKER = 0.60;

        private double mDialX = 0;
        private double mDialY = 0;
        private double mDialRadius = 0;
        private double mMarkerAngle = 45; // 15, 40, 45,60,90,
        private int mMarkerCount = 0;
        private ArrayList mArrPositions = new ArrayList(); // x,y pos for the marker dial to move through
     
        private int mCurrMarkerPos = 0;
        private bool mMousePosProcessing = false;   // becomes true when mouse pos is being calculated


        public MainWindow()
        {
            InitializeComponent();
        }

        private void init()
        {
            //Canvas.SetZIndex(dial, DIAL_ZINDEX);
            //Canvas.SetZIndex(linePointer, LINEPOINTER_ZINDEX);

            //calc center point of main ellipse
            mDialRadius = (dial.Width) / 2;
            mDialX = Canvas.GetLeft(dial) + mDialRadius;
            mDialY = Canvas.GetTop(dial) + mDialRadius;
 
            linePointer.X1 = mDialX; linePointer.Y1 = mDialY;

            double startX = 0; double startY = 0; double stopX = 0; double stopY = 0;
            startX = mDialX + mDialRadius  + 5;
            startY = mDialY;
            stopX = startX+15;
            stopY = startY;
            Point ptCenter = new Point(mDialX, mDialY);

            mMarkerCount = Convert.ToInt16(360 / mMarkerAngle);

            //mMarkerCount = 4;
            double targetX = 0;
            double slope = 0;
            double targetY = 0;
            double yintercept = 0;
            double diffX = 0; double diffY = 0;

            double markerX = 0.0; double markerY = 0.0;

            for (int i = 0; i < mMarkerCount; i++)
            {
                if (true)
                {
                    diffY = stopY - mDialY;
                    diffX = stopX - mDialX;
                    if (diffX == 0)
                        slope = 0;
                    else
                        slope = diffY / diffX;
                    yintercept = mDialY - (slope * mDialX);
                    if (slope > 0.01)
                    {
                        double adjustX = ((stopX - mDialX) * LINE_PART_TO_HIDE);
                        targetX = mDialX + adjustX;
                        targetY = (slope * targetX) + yintercept;

                        double adjustX2 = ((stopX - mDialX) * LINE_PART_TO_HIDE_FOR_MARKER);
                        markerX = mDialX + adjustX2;
                        markerY = (slope * markerX) + yintercept;
                        
                    }
                    else if (slope == -90)
                    {
                        double adjustX = ((stopX - mDialX) * LINE_PART_TO_HIDE);
                        targetX = mDialX + adjustX;
                        targetY = (slope * targetX) + yintercept;

                        double adjustX2 = ((stopX - mDialX) * LINE_PART_TO_HIDE_FOR_MARKER);
                        markerX = mDialX + adjustX2;
                        markerY = (slope * markerX) + yintercept;

                    }
                    else if (slope == -0 && diffX < 0 && diffY >= 0)
                    {
                        double adjustX = ((stopX - mDialX) * LINE_PART_TO_HIDE);
                        targetX = mDialX + adjustX;
                        targetY = stopY;

                        double adjustX2 = ((stopX - mDialX) * LINE_PART_TO_HIDE_FOR_MARKER);
                        markerX = mDialX + adjustX2;
                        markerY = stopY;


                    }
                    else if (slope < 0.0)
                    {
                        double adjustX = ((stopX - mDialX) * LINE_PART_TO_HIDE);
                        targetX = mDialX + adjustX;
                        targetY = (slope * targetX) + yintercept;

                        double adjustX2 = ((stopX - mDialX) * LINE_PART_TO_HIDE_FOR_MARKER);
                        markerX = mDialX + adjustX2;
                        markerY = (slope * markerX) + yintercept;

                    }
                    else
                    {
                        if (diffX > 0.0)
                        {
                            double adjustX = ((stopX - mDialX) * LINE_PART_TO_HIDE);
                            targetX = mDialX + adjustX;
                            targetY = stopY;

                            double adjustX2 = ((stopX - mDialX) * LINE_PART_TO_HIDE_FOR_MARKER);
                            markerX = mDialX + adjustX2;
                            markerY = stopY;

                        }
                        else if (diffX == 0) //                             //6 pm
                        {
                            targetX = stopX;
                            double adjustY = ((stopY - mDialY) * LINE_PART_TO_HIDE);
                            targetY = mDialY + adjustY;

                            markerX = stopX;
                            double adjustY2 = ((stopY - mDialY) * LINE_PART_TO_HIDE_FOR_MARKER);
                            markerY = mDialY + adjustY2;
                            
                        }
                    }

                    Debug.WriteLine("slope=" + slope +", yintercept=" + yintercept + " x1,y1=" + targetX + "," + 
                        targetY +", x2,y2=" + stopX + "," + stopY +", diffx=" + diffX +", diffy=" + diffY);
                }
                else
                {
                    targetX = mDialX; targetY = mDialY;
                }
              
                Line ln = new Line();
                ln.X1 = targetX; ln.Y1 = targetY; ln.X2 = stopX; ln.Y2 = stopY;
                ln.StrokeThickness = 1; ln.Stroke = System.Windows.Media.Brushes.Gray;
                canvas.Children.Add(ln);

                mArrPositions.Add(new Point(markerX, markerY));
              
                //rotate 
                Point newPoint = rotatePoint(new Point(stopX, stopY), ptCenter, mMarkerAngle);
                stopX = newPoint.X; stopY = newPoint.Y;

                
            }
            placeMarker(mCurrMarkerPos);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            init();
        }

        /// <summary>
        /// Rotates one point around another
        /// </summary>
        /// <param name="pointToRotate">The point to rotate.</param>
        /// <param name="centerPoint">The center point of rotation.</param>
        /// <param name="angleInDegrees">The rotation angle in degrees.</param>
        /// <returns>Rotated point</returns>
        private  Point rotatePoint(Point pointToRotate, Point centerPoint, double angleInDegrees)
        {
            double angleInRadians = angleInDegrees * (Math.PI / 180);
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);
            return new Point
            {
                X =
                    (int)
                    (cosTheta * (pointToRotate.X - centerPoint.X) -
                    sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                Y =
                    (int)
                    (sinTheta * (pointToRotate.X - centerPoint.X) +
                    cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
            };
        }

      
        private void processMarkerPos(MouseEventArgs e)
        {
                if (mMousePosProcessing)
                    return;
                mMousePosProcessing = true;
                Point ptThis = e.GetPosition(this);
                double thisX = ptThis.X; double thisY = ptThis.Y;
            if (e.RightButton == MouseButtonState.Pressed) // decrease
            {
                mCurrMarkerPos--;
                if (mCurrMarkerPos < 0)
                    mCurrMarkerPos = mMarkerCount - 1;
            }
            if (e.LeftButton == MouseButtonState.Pressed) { // increase            {
                mCurrMarkerPos++;
                if (mCurrMarkerPos >= mMarkerCount)
                    mCurrMarkerPos = 0;
            }

            placeMarker(mCurrMarkerPos);
            mMousePosProcessing = false;
        }

        private void placeMarker(int elem)
        {
            Point pt = (Point) mArrPositions[elem];
            linePointer.X2 =  pt.X;
            linePointer.Y2 =  pt.Y;

        }

        private void dial_MouseDown(object sender, MouseButtonEventArgs e)
        {
            processMarkerPos(e);
        }
    }
}