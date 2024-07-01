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
using static System.Formats.Asn1.AsnWriter;

namespace DialControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int DIAL_ZINDEX = 5;
        private const int LINEPOINTER_ZINDEX = 6;
        private const int VINTAGEDIAL_ZINDEX = 7;
        private const double LINE_PART_TO_HIDE = 0.90;
        private const double LINE_PART_TO_HIDE_FOR_MARKER = 0.60;
        private const int INNER_DIAL_SIZE_RATIO = 3; // how much smaller will be vintage dial to the main dial
        private const int DIAL_MODE_MODERN = 0;
        private const int DIAL_MODE_FLAT = 1;
        private const int DIAL_MODE_VINTAGE = 2;

        private double mDialX = 0;
        private double mDialY = 0;
        private double mDialRadius = 0;
        private double mDialVintageRadius = 0;
        private double mMarkerAngle = 45; // 15, 40, 45,60,90,
        private int mMarkerCount = 0;
        private ArrayList mArrOrigPositions = new ArrayList(); // complete lines x,y pos for the marker dial to move through
        private ArrayList mArrPositions = new ArrayList(); // adjusted x,y pos for the marker dial to move through
     
        private int mCurrMarkerPos = 0;
        private bool mMousePosProcessing = false;   // becomes true when mouse pos is being calculated

        private ArrayList mArrArcs = new ArrayList(); //array of selection arcs

        private int mMode = DIAL_MODE_MODERN;      // dial mode

        public MainWindow()
        {
            InitializeComponent();
        }

        private void init()
        {
            Canvas.SetZIndex(dial, DIAL_ZINDEX);
            Canvas.SetZIndex(linePointer, LINEPOINTER_ZINDEX);
            Canvas.SetZIndex(dialVintage, VINTAGEDIAL_ZINDEX);

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

            if (mMode == DIAL_MODE_FLAT || mMode == DIAL_MODE_VINTAGE)
            {
                linePointer.Visibility = Visibility.Hidden;
                dial.Effect = null;
                dial.Stroke = new SolidColorBrush(Colors.DarkGray);
                dial.StrokeThickness = 1;
                LinearGradientBrush grad = new LinearGradientBrush();
                grad.StartPoint = new Point(0, 0);
                grad.EndPoint = new Point(1, 1);
                grad.GradientStops.Add(new GradientStop(Colors.White, 0.0));
                grad.GradientStops.Add(new GradientStop(Colors.White, 1.0));
                dial.Fill = grad;
            }
            if (mMode == DIAL_MODE_VINTAGE)
            {
                linePointer.Visibility = Visibility.Visible;
                linePointer.Stroke = new SolidColorBrush(Colors.Black);
                dialVintage.Width = dial.Width / INNER_DIAL_SIZE_RATIO;
                dialVintage.Height = dial.Height / INNER_DIAL_SIZE_RATIO;
                mDialVintageRadius = (dialVintage.Width) / 2;
                Canvas.SetLeft(dialVintage, Canvas.GetLeft(dial) + mDialRadius - mDialVintageRadius);
                Canvas.SetTop(dialVintage, Canvas.GetTop(dial) + mDialRadius - mDialVintageRadius);
                dialVintage.Visibility = Visibility.Visible;
            }
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

                if (mMode == DIAL_MODE_FLAT || mMode == DIAL_MODE_VINTAGE)
                {
                    mArrOrigPositions.Add(new Point(stopX, stopY));
                } 
                if (mMode == DIAL_MODE_VINTAGE)
                {
                    drawVintageArc(i);
                }

                //rotate 
                Point newPoint = rotatePoint(new Point(stopX, stopY), ptCenter, mMarkerAngle);
                stopX = newPoint.X; stopY = newPoint.Y;
                 

            }

            //for vintage arc,draw from last point to first point
            if (mMode == DIAL_MODE_VINTAGE)
            {
                drawVintageArc(0);
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
            bool toUpdate = true;
            if (e.RightButton == MouseButtonState.Pressed) // decrease
            {
                mCurrMarkerPos--;
                if (mCurrMarkerPos < 0)
                {
                    mCurrMarkerPos = 0;
                    toUpdate = false;
                }
            }
            if (e.LeftButton == MouseButtonState.Pressed) { // increase            {
                mCurrMarkerPos++;
                if (mCurrMarkerPos >= mMarkerCount)
                {
                    mCurrMarkerPos = mMarkerCount - 1;
                    toUpdate = false;
                }
            }
            if (toUpdate)
            {
                placeMarker(mCurrMarkerPos);
                if (mMode == DIAL_MODE_FLAT)
                    drawArc(mCurrMarkerPos, e.RightButton == MouseButtonState.Pressed);
            }
            mMousePosProcessing = false;
        }

        private void placeMarker(int elem)
        {
            Point pt = (Point) mArrPositions[elem];
            linePointer.X2 =  pt.X;
            linePointer.Y2 =  pt.Y;

        }

        private void drawArc(int elem, bool remove=false)
        {
            if (elem > 0 && !remove)
            {
                Point startPt = (Point)mArrOrigPositions[elem-1];
                Point endPt = (Point)mArrOrigPositions[elem];
                Line lnTemp = new Line();
                lnTemp.X1 = startPt.X; lnTemp.Y1 = startPt.Y; lnTemp.X2 = endPt.X; lnTemp.Y2 = endPt.Y;
                lnTemp.StrokeThickness = 4; lnTemp.Stroke = new SolidColorBrush(Colors.Red);
                //canvas.Children.Add(lnTemp);

                var g = new StreamGeometry();

                using (var gc = g.Open())
                {
                    gc.BeginFigure(
                    startPoint: new Point(startPt.X, startPt.Y),
                        isFilled: false,
                        isClosed: false);

                    gc.ArcTo(
                        point: new Point(endPt.X, endPt.Y),
                        size: new Size(100, 100),
                        rotationAngle: 0d,
                        isLargeArc: false,
                        sweepDirection: SweepDirection.Clockwise,
                        isStroked: true,
                        isSmoothJoin: false);
                }

               
                var arcPath = new Path
                {
                    Stroke = Brushes.DarkGray,
                    StrokeThickness = 6,
                    Data = g
                };
                if (mArrArcs.Count < elem)
                    mArrArcs.Add(arcPath);
                else
                    mArrArcs[elem-1] = arcPath;
                canvas.Children.Add(arcPath);
               
            } else if (remove)
            {
                Path thisPath = (Path) mArrArcs[elem];
                canvas.Children.Remove(thisPath);
                 

            }
        }
        
        private void drawVintageArc(int elem)
        {
            Point startPt = new Point(0,0);
            Point endPt = new Point(0,0);

            if (elem > 0)
            {
                startPt = (Point)mArrOrigPositions[elem - 1];
                endPt = (Point)mArrOrigPositions[elem];
            } else
            {
                startPt = (Point)mArrOrigPositions[mArrOrigPositions.Count - 1];
                endPt = (Point)mArrOrigPositions[elem];
            }
                Line lnTemp = new Line();
                lnTemp.X1 = startPt.X; lnTemp.Y1 = startPt.Y; lnTemp.X2 = endPt.X; lnTemp.Y2 = endPt.Y;
                lnTemp.StrokeThickness = 4; lnTemp.Stroke = new SolidColorBrush(Colors.Red);
                //canvas.Children.Add(lnTemp);
                

                var g = new StreamGeometry();

                using (var gc = g.Open())
                {
                    gc.BeginFigure(
                    startPoint: new Point(startPt.X, startPt.Y),
                        isFilled: false,
                        isClosed: false);

                    gc.ArcTo(
                        point: new Point(endPt.X, endPt.Y),
                        size: new Size(100, 100),
                        rotationAngle: 0d,
                        isLargeArc: false,
                        sweepDirection: SweepDirection.Counterclockwise,
                        isStroked: true,
                        isSmoothJoin: false);
                }


                var arcPath = new Path
                {
                    Stroke = Brushes.DarkGray,
                    StrokeThickness = 6,
                    Data = g
                };

                 
                mArrArcs.Add(arcPath);
               
                canvas.Children.Add(arcPath);

           
           
        }
        
        private void dial_MouseDown(object sender, MouseButtonEventArgs e)
        {
            processMarkerPos(e);
        }
    }
}