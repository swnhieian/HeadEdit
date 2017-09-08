using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HeadEdit
{
    public delegate void ThreadDelegate(Object para);
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    
    public partial class MainWindow : Window
    {
        private Calibration calibration;
        private Ellipse headEllipse;
        private PopupController popupController;
        private Point currentCursor;
        //flag
        private bool moveFlag = false;
        private bool editFlag = false;

        TextRange HeadSelectRange;

        public bool EditFlag
        {
            get
            {
                return editFlag;
            }

            set
            {
                editFlag = value;
            }
        }

        public Ellipse HeadEllipse
        {
            get
            {
                return headEllipse;
            }

            set
            {
                headEllipse = value;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            
            //start a background thread which can receive head position from server
            Thread headPositionThread = new Thread(() =>
            {
                Position position = new Position();
                position.start(this, new ThreadDelegate(handleHeadPosition));
            });
            headPositionThread.IsBackground = true;
            if (Config.useCamera)
            {
                headPositionThread.Start();
            }
            //initalize some variables
            currentCursor = new Point(0, 0);
            popupController = new PopupController(this);
            richTextBox.FontSize = Config.richTextBoxFontSize;
            calibration = new Calibration();
            HeadEllipse = new Ellipse()
            {
                Width = 200,
                Height = 200,
                Stroke = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)),
                StrokeThickness=2
            };
            HeadEllipse.Visibility = Config.debug ? Visibility.Visible : Visibility.Hidden;
            
            mainCanvas.Children.Add(HeadEllipse);
            if (Config.useCamera)
            {
                calibration.startCalibrate();    //first calibration before use
            }
            richTextBox.Focus();
            richTextBox.CaretPosition = richTextBox.Document.ContentEnd;
        }
        private void handleHeadPosition(Object para)
        {
            Point headPos = calibration.parsePoint((Point)para);
            currentCursor = headPosToCanvasPos(headPos);
            updateInterface();
        }
        public void matchString(String input)
        {

        }
        private void updateInterface()
        {
            if (moveFlag == false && EditFlag == false)
            {
                Canvas.SetLeft(HeadEllipse, currentCursor.X - HeadEllipse.Width / 2);
                Canvas.SetTop(HeadEllipse, currentCursor.Y - HeadEllipse.Height / 2);
            }
            else if (moveFlag == true)
            {
                //done
                var InsertPos = richTextBox.GetPositionFromPoint(currentCursor, true);
                richTextBox.CaretPosition = InsertPos;
                moveFlag = false;
            }
            else if (EditFlag == true)
            {
                popupController.pop(currentCursor);
                /*
                if (InputBox.Visibility!=Visibility.Visible)
                {
                    HeadSelectRange = HeadSelectRangeWay(headToCanvasPos);  // to do
                    //change the color of selectrange
                    SetFontColor(Config.FontColor,HeadSelectRange);
                    //match inputbox's positioh with headPos
                    SetBoxPos(headToCanvasPos);                               //to do
                    //Focus on the InputBox
                    InputBox.Focus();
                }*/
                //
                //TextRange SelectWordsRange= SearchKeyWord(HeadSelectRange, InputBox.Text);
            }
        }
        
        private Point headPosToCanvasPos(Point headPos)
        {
            Point point = new Point(headPos.X * richTextBox.ActualWidth - col0.ActualWidth, headPos.Y * richTextBox.ActualHeight /*- row1.ActualHeight*/);
            return point;
        }

        private TextRange HeadSelectRangeWay(Point headToCanvasPos)
        {
            //Modify
            TextRange range = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);    //*********Modify***************
            return range;
        }

        private void SetBoxPos(Point headToCanvasPos)
        {
            
            InputBox.Visibility = Visibility.Visible;
            //SOMETHING
            return;
        }

        private void SetFontColor(ConsoleColor color,TextRange range)
        {
            range.ApplyPropertyValue(TextElement.ForegroundProperty, color);
        }
        
        private void SetBackGroundColor(ConsoleColor color, TextRange range)
        {
            range.ApplyPropertyValue(TextElement.BackgroundProperty, color);
        }
        //private TextRange SearchKeyWord(TextRange HeadSelectRange,string keyword)
        //{
        //    return;
        //}
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (!Config.useCamera)
            {
                Point mousePos = e.GetPosition(richTextBox);
                //Console.WriteLine("{0},{1}", mousePos.X / richTextBox.ActualWidth, mousePos.Y / richTextBox.ActualHeight);
                handleHeadPosition(new Point(mousePos.X / richTextBox.ActualWidth, mousePos.Y / richTextBox.ActualHeight));
                //Canvas.SetLeft(headEllipse, mousePos.X - headEllipse.Width/2);
                //Canvas.SetTop(headEllipse, mousePos.Y - headEllipse.Height/2);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Config.calibrationKey)
            {
                calibration.startCalibrate();
                e.Handled = true;
            } else if (e.Key == Config.moveKey)
            {
                moveFlag = true;
                updateInterface();
                e.Handled = true;
            } else if (e.Key == Config.editKey)
            {
                EditFlag = true;
                updateInterface();
                e.Handled = true;
            } else if (e.Key == Config.cancelKey)
            {
                //return to raw pattern
                moveFlag = false;
                EditFlag = false;
                updateInterface();
                //return to the end of the richtextbox
                richTextBox.Focus();
                richTextBox.CaretPosition = richTextBox.Document.ContentEnd;

                //remove any property of selectrange
                if (InputBox.Visibility == Visibility.Visible)
                {
                    InputBox.Visibility = Visibility.Hidden;
                    SetFontColor(Config.DefaultFontColor, HeadSelectRange);
                    SetBackGroundColor(Config.DefaultBackGroundColor, HeadSelectRange);
                }
                

                e.Handled = true;
            }
            {
                //do nothing
            }

        }
    }
    
}
