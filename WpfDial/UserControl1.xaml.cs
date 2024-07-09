using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
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


namespace WpfDial
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        private const int DIAL_ZINDEX = 5;
        private const int LINEPOINTER_ZINDEX = 6;
        private const int VINTAGEDIAL_ZINDEX = 7;
        private const double LINE_PART_TO_HIDE = 0.90;
        private const double LINE_PART_TO_HIDE_FOR_MARKER = 0.60;
        private const int INNER_DIAL_SIZE_RATIO = 3; // how much smaller will be vintage dial to the main dial
        private const int MIN_SIZE = 150;     // min width and height of canvas
        private const int MAX_SIZE = 450;    // max width and height of canvas
        private const int DIAL_DIFF_IN_SIZE = -100; // how much smaller will dial be in relation to component size

        private const int DIAL_MODE_MODERN = 0;
        private const int DIAL_MODE_FLAT = 1;
        private const int DIAL_MODE_VINTAGE = 2;
        public enum MODE_ENUM { MODERN , FLAT, VINTAGE};

        public enum ANGLE_ENUM { ANGLE_15, ANGLE_40, ANGLE_45, ANGLE_60, ANGLE_90 };

        private double mDialX = 0;
        private double mDialY = 0;
        private double mDialRadius = 0;
        private double mDialVintageRadius = 0;
        private double mMarkerAngle = 40; // 15, 40, 45,60,90,
        private int mMarkerCount = 0;
        private ArrayList mArrOrigPositions = new ArrayList(); // complete lines x,y pos for the marker dial to move through
        private ArrayList mArrPositions = new ArrayList(); // adjusted x,y pos for the marker dial to move through

        private int mCurrMarkerPos = 0;
        private bool mMousePosProcessing = false;   // becomes true when mouse pos is being calculated

        private ArrayList mArrArcs = new ArrayList(); //array of selection arcs

        private int mMode = DIAL_MODE_FLAT;      // dial mode

        private int mSize = 250;                  // current size for user control

        private bool mReady = false;            // becomes true when component has rendered

        public static readonly RoutedEvent DialClickEvent =
        EventManager.RegisterRoutedEvent("DialClick",
                     RoutingStrategy.Bubble, typeof(RoutedEventHandler),
                     typeof(UserControl1));

     
        public UserControl1()
        {
            InitializeComponent();
            SetCurrentValue(SetModeProperty, MODE_ENUM.MODERN);
        }

        public int getMaxMarkers()
        {
            return mMarkerCount;
        }


        public event RoutedEventHandler DialClick
        {
            add { AddHandler(DialClickEvent, value); }
            remove { RemoveHandler(DialClickEvent, value); }
        }

        void RaiseDialClickEvent(int val)
        {
            DialClickRoutedEventArgs newEventArgs =
                    new DialClickRoutedEventArgs(UserControl1.DialClickEvent, val);
            RaiseEvent(newEventArgs);
        }

        #region SET PROPERTIES-----------------------------------------------------------------

        //DIAL MODE
        public static readonly DependencyProperty SetModeProperty =
        DependencyProperty.Register("Mode", typeof(MODE_ENUM), typeof(UserControl1),
                                     new FrameworkPropertyMetadata(onSetModeChanged)
                                     {
                                         BindsTwoWayByDefault = true
                                     }  
        );

        public MODE_ENUM Mode
        {
            get { return (MODE_ENUM) GetValue(SetModeProperty); }
            set { SetValue(SetModeProperty, value); }
        }
        private static void onSetModeChanged(DependencyObject d,DependencyPropertyChangedEventArgs e)
        {
            
            UserControl1 UserControl1Control = d as UserControl1;
            UserControl1Control.onSetModeChanged(e);
        }

        private void onSetModeChanged(DependencyPropertyChangedEventArgs e)
        {
            mMode = (int) e.NewValue;
                
        }

        //ANGLE
        public static readonly DependencyProperty SetAngleProperty =
          DependencyProperty.Register("Angle", typeof(ANGLE_ENUM), typeof(UserControl1),
                                       new FrameworkPropertyMetadata(onSetAngleChanged)
                                       {
                                           BindsTwoWayByDefault = true
                                       }
          );

        public ANGLE_ENUM  Angle
        {
            get { return (ANGLE_ENUM)GetValue(SetAngleProperty); }
            set { SetValue(SetAngleProperty, value); }
        }
        private static void onSetAngleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            UserControl1 UserControl1Control = d as UserControl1;
            UserControl1Control.onSetAngleChanged(e);
        }

        private void onSetAngleChanged(DependencyPropertyChangedEventArgs e)
        {
            mMarkerAngle = angleEnumToAngle((ANGLE_ENUM) e.NewValue);

        }


        //DIAL SIZE
        public static readonly DependencyProperty SetDialSizeProperty =
        DependencyProperty.Register("DialSize", typeof(int), typeof(UserControl1),
                                     new FrameworkPropertyMetadata(onSetDialSizeChanged)
                                     {
                                         BindsTwoWayByDefault = true
                                     }
        );

        public int DialSize
        {
            get { return (int )GetValue(SetDialSizeProperty); }
            set { SetValue(SetDialSizeProperty, value); }
        }
        private static void onSetDialSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            UserControl1 UserControl1Control = d as UserControl1;
            UserControl1Control.onSetDialSizeChanged(e);
        }

        private void onSetDialSizeChanged(DependencyPropertyChangedEventArgs e)
        {
            int newVal = (int)e.NewValue;
            if (newVal < MIN_SIZE)
                newVal = MIN_SIZE;
            if (newVal > MAX_SIZE)
                newVal = MAX_SIZE;
            mSize = (int)e.NewValue;
            init();


        }


        #endregion ------------------------------------------------------------------------

        private double angleEnumToAngle(ANGLE_ENUM a)
        {
            double retVal = 0.0;
            switch (a)
            {
                case ANGLE_ENUM.ANGLE_15:
                    retVal = 15;
                    break;
                case ANGLE_ENUM.ANGLE_40:
                    retVal = 40;
                    break;
                case ANGLE_ENUM.ANGLE_45:
                    retVal = 45;
                    break;
                case ANGLE_ENUM.ANGLE_60:
                    retVal = 60;
                    break;
                case ANGLE_ENUM.ANGLE_90:
                    retVal = 90;
                    break;
            }
            return retVal;
        }


        private void init()
        {
            if (!mReady)
                return;
            Canvas.SetZIndex(dial, DIAL_ZINDEX);
            Canvas.SetZIndex(linePointer, LINEPOINTER_ZINDEX);
            Canvas.SetZIndex(dialVintage, VINTAGEDIAL_ZINDEX);

            // calc.sizes of dial and canvas 
            this.Width = mSize; this.Height = mSize;

            canvas.Width = mSize;  canvas.Height = mSize;
            dial.Width = canvas.ActualWidth + DIAL_DIFF_IN_SIZE; dial.Height = canvas.ActualHeight + DIAL_DIFF_IN_SIZE;

            //calc center point of main ellipse
            mDialRadius = (dial.ActualWidth) / 2;
            mDialX = Canvas.GetLeft(dial) + mDialRadius;
            mDialY = Canvas.GetTop(dial) + mDialRadius;

            linePointer.X1 = mDialX; linePointer.Y1 = mDialY;

            double startX = 0; double startY = 0; double stopX = 0; double stopY = 0;
            startX = mDialX + mDialRadius + 5;
            startY = mDialY;
            stopX = startX + 15;
            stopY = startY;
            Point ptCenter = new Point(mDialX, mDialY);

            mMarkerAngle = angleEnumToAngle(Angle);
            mMarkerCount = Convert.ToInt16(360 / mMarkerAngle);
            //mMarkerCount = 5;
            double targetX = 0;
            double slope = 0;
            double targetY = 0;
            double yintercept = 0;
            double diffX = 0; double diffY = 0;

            double markerX = 0.0; double markerY = 0.0;

            mMode = (int) Mode;
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

                        } else  if (diffX < 0.0) {
                                double adjustX = ((stopX - mDialX) * LINE_PART_TO_HIDE);
                                targetX = mDialX + adjustX;
                                targetY = stopY;

                                double adjustX2 = ((stopX - mDialX) * LINE_PART_TO_HIDE_FOR_MARKER);
                                markerX = mDialX + adjustX2;
                                markerY = stopY;

                        }
                    }

                    Debug.WriteLine("slope=" + slope + ", yintercept=" + yintercept + " x1,y1=" + targetX + "," +
                        targetY + ", x2,y2=" + stopX + "," + stopY + ", diffx=" + diffX + ", diffy=" + diffY);
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

        /// <summary>
        /// Rotates one point around another
        /// </summary>
        /// <param name="pointToRotate">The point to rotate.</param>
        /// <param name="centerPoint">The center point of rotation.</param>
        /// <param name="angleInDegrees">The rotation angle in degrees.</param>
        /// <returns>Rotated point</returns>
        private Point rotatePoint(Point pointToRotate, Point centerPoint, double angleInDegrees)
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
            if (e.LeftButton == MouseButtonState.Pressed)
            { // increase            
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
            Point pt = (Point)mArrPositions[elem];
            linePointer.X2 = pt.X;
            linePointer.Y2 = pt.Y;

        }

        private void drawArc(int elem, bool remove = false)
        {
            if (elem > 0 && !remove)
            {
                Point startPt = (Point)mArrOrigPositions[elem - 1];
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
                    mArrArcs[elem - 1] = arcPath;
                canvas.Children.Add(arcPath);

            }
            else if (remove)
            {
                Path thisPath = (Path)mArrArcs[elem];
                canvas.Children.Remove(thisPath);


            }
        }

        private void drawVintageArc(int elem)
        {
            Point startPt = new Point(0, 0);
            Point endPt = new Point(0, 0);

            if (elem > 0)
            {
                startPt = (Point)mArrOrigPositions[elem - 1];
                endPt = (Point)mArrOrigPositions[elem];
            }
            else
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            mReady = true;
            init();
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            processMarkerPos(e);
            RaiseDialClickEvent(mCurrMarkerPos);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            init();
        }
    }


    #region  DIALCLICK ROUTEDEVENTARGS--------------------------------------------------------

    public class DialClickRoutedEventArgs : RoutedEventArgs
    {
        private readonly int selectedPos;

        public DialClickRoutedEventArgs(RoutedEvent routedEvent,
                                          int selectedItem)
            : base(routedEvent)
        {
            this.selectedPos = selectedItem;
        }

        public int SelectedPos
        {
            get
            {
                return selectedPos;
            }
        }
    }

    #endregion
}
