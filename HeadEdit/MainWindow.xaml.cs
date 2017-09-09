using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        //public List<TextRange> HeadSelectRange;
        public List<TextRange> SplitWordsRange;
        public List<TextRange> HighLightRange;
        public int NowChoice = 0;//nowchoice of highlightrange
        public string NowChoiceString;


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
            //renew font property of last HighLightRange
            if (HighLightRange!=null && HighLightRange.Count > 0)
            {
                HighLightRange[NowChoice].Text = NowChoiceString;

                for (int i = 0; i < HighLightRange.Count; i++)
                {
                    SetFontColor(Config.HeadSelectColor, HighLightRange[i]);
                }
                

            }
            //if no input,return
            if (SplitWordsRange == null || input == null)
            {
                return;
            }
            if (input == "") return ;
            //string text = HeadSelectRange.Text;
            //find the match range
            HighLightRange = FindString(input);
            //highlight
            for(int i = 0; i < HighLightRange.Count; i++)
            {
                SetFontColor(Config.FontColor, HighLightRange[i]);
            }

            if (HighLightRange.Count != 0)
            {
                //default choice:0
                NowChoice = 0;
                //remember raw text
                NowChoiceString = HighLightRange[NowChoice].Text;
                //PreView of the input
                HighLightRange[NowChoice].Text = input;
                //SetDefaultBackGround
                SetBackGroundColor(Config.BackGroundColor, HighLightRange[NowChoice]);
            }
            
        }
        
        private List<TextRange> FindString(string keyword)  //return i th word in the text near to the keyword
        {
            //定义匹配阈值
            List<TextRange> HighLightRange = new List<TextRange>();
            int threshold = 5;
            if (keyword.Length <= 2) threshold = 3;
            //            else if(keyword.Length<=4) threshold = keyword.Length-1;
            //            else if (keyword.Length <= 6) threshold = keyword.Length - 2;
            //            else threshold = keyword.Length - 2;
            int i = 0;
            foreach (TextRange b in SplitWordsRange)
            {
                var a = b.Text;
                if (a == "") continue;
                if (a.Length == 0) continue;
                
                int distance = getEditDistance(a, keyword);
                if (distance <= threshold)
                {
                    if (distance == 0) continue;
                    HighLightRange.Add(SplitWordsRange[i]);

                }
                i++;
            }
            return HighLightRange;
        }
        private void updateInterface()
        {
            if (moveFlag == false && EditFlag == false)
            {
                //renew  last color
                if (SplitWordsRange != null && SplitWordsRange.Count > 0)
                {
                    foreach(TextRange a in SplitWordsRange)
                    {
                        SetFontColor(Config.DefaultFontColor, a);
                    }
                }

                SplitWordsRange = get_range(currentCursor,70,70);

                //setcolor
                foreach (TextRange a in SplitWordsRange)
                {
                    SetFontColor(Config.HeadSelectColor, a);
                }                
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
                //if(popupController.)
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

        public void SetFontColor(Brush color,TextRange range)
        {

            range.ApplyPropertyValue(TextElement.ForegroundProperty, color);
        }
        
        public void SetBackGroundColor(Brush color, TextRange range)
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

        public static IEnumerable<TextRange> GetAllWordRanges(FlowDocument document)
        {
            string pattern = @"[^\W\d](\w|[-']{1,2}(?=\w))*";
            TextPointer pointer = document.ContentStart;
            while (pointer != null)
            {
                if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string textRun = pointer.GetTextInRun(LogicalDirection.Forward);
                    MatchCollection matches = Regex.Matches(textRun, pattern);
                    foreach (Match match in matches)
                    {
                        int startIndex = match.Index;
                        int length = match.Length;
                        TextPointer start = pointer.GetPositionAtOffset(startIndex);
                        TextPointer end = start.GetPositionAtOffset(length);
                        yield return new TextRange(start, end);
                    }
                }

                pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
            }
        }
        public void SetBackGround(Color l, TextRange textRange)
        {
            textRange.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(l));
            //            textRange.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(CommonColorsHelp.SelectedKeyFontColor));
        }
        private bool in_circle(TextRange t, Point p, int w, int h)
        {

            Rect start = t.Start.GetCharacterRect(LogicalDirection.Forward);
            Rect end = t.End.GetCharacterRect(LogicalDirection.Backward);
            if (start.X > (p.X + w) || end.X < (p.X - w)) return false;
            if (start.Y > (p.Y + h) || start.Y < (p.Y - h)) return false;
            return true;
        }
        private List<TextRange> get_range(Point p, int w, int h) // 上下k行
        {
            List<TextRange> select = new List<TextRange>();
            var poz = richTextBox.GetPositionFromPoint(p, true);
            List<TextRange> allTextRanges = GetAllWordRanges(richTextBox.Document).ToList();
            foreach (var item in allTextRanges)
            {
                if (in_circle(item, p, w, h) == true)
                {
                    select.Add(item);
                }
            }
            return select;
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
                    for(int i=0;i< SplitWordsRange.Count; i++)
                    {
                        SetFontColor(Config.DefaultFontColor, SplitWordsRange[i]);
                    }
                    if (HighLightRange != null)
                    {
                        SetBackGroundColor(Config.DefaultBackGroundColor, HighLightRange[NowChoice]);
                    }                    
                }
                

                e.Handled = true;
            }
            {
                //do nothing
            }

        }

        public int getEditDistance(String s, String t)
        {
            s = s.ToLower(); // lower mode
            t = t.ToLower();
            ///编辑距离调整：1.长度差距很大，惩罚比较大。 2.字母之间的差距，和键盘相互结合
            int[,] d; // matrix
            int n = 0; // length of s
            int m = 0; // length of t
            int i; // iterates through s
            int j; // iterates through t
            char s_i; // ith character of s
            char t_j; // jth character of t
            int cost; // cost

            // Step 1

            n = s.Length;
            m = t.Length;
            if (n == 0)
            {
                return m;
            }
            if (m == 0)
            {
                return n;
            }
            d = new int[n + 1, m + 1];

            for (i = 0; i <= n; i++)
            {
                d[i, 0] = 1;
            }

            for (j = 0; j <= m; j++)
            {
                d[0, j] = j;
            }

            // Step 3

            for (i = 1; i <= n; i++)
            {
                s_i = s[i - 1];
                // Step 4
                for (j = 1; j <= m; j++)
                {
                    t_j = t[j - 1];
                    // Step 5
                    cost = chartocharx(s_i, t_j);
                    if (i > 1 && j > 1)
                    {
                        if (t[j - 1] == s[i - 2] && t[j - 2] == s[i - 1]) cost = 0;
                    }
                    d[i, j] = Minimum(d[i - 1, j] + 3, d[i, j - 1] + 3,
                            d[i - 1, j - 1] + cost);
                }
            }
            int charzhi = System.Math.Abs(m - n);
            if (charzhi == 2)
            {
                d[n, m] += 3;
            }
            else if (charzhi > 2)
            {
                d[n, m] += charzhi * 3;
            }
            // Step 7
            return d[n, m];

        }
        private int chartocharx(char a, char b)
        {
            int x1 = Char.ToLower(a) - 'a';
            int x2 = Char.ToLower(b) - 'a';
            if (x1 < 0 || x1 > 25 || x2 < 0 || x2 > 25)
            {
                return 3;
            }
            else
            {
                return chartochar[x1, x2];
            }
        }

        private static int Minimum(int a, int b, int c)
        {
            int mi;

            mi = a;
            if (b < mi)
            {
                mi = b;
            }
            if (c < mi)
            {
                mi = c;
            }
            return mi;
        }
        private static int[,] chartochar = new int[26, 26] {
             { 0, 3, 3, 3,3,3,3,3,3,3,3,3,3,3,3,3,1,3,1,3,3,3,3,3,3,1 },//a
            // a  b  c  d e f g h i j k l m n o p q r s t u v w x y z
             { 3, 0, 3, 3,3,3,1,1,3,3,3,3,3,1,3,3,3,3,3,3,3,1,3,3,3,3 },//b
            // a  b  c  d e f g h i j k l m n o p q r s t u v w x y z
             { 3, 3, 0, 1,3,1,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,1,3,1,3,3 },//c
            // a  b  c  d e f g h i j k l m n o p q r s t u v w x y z
             { 3, 3, 1, 0,1,1,3,3,3,3,3,3,3,3,3,3,3,3,1,3,3,3,3,3,3,3 },//d
            // a  b  c  d e f g h i j k l m n o p q r s t u v w x y z
             { 3, 3, 3, 1,0,3,3,3,3,3,3,3,3,3,3,3,3,1,3,3,3,3,1,3,3,3 },//e
            // a  b  c  d e 1 g h i j k l m n o p q r s t u v w x y z
             { 3, 3, 3, 1,3,0,1,3,3,3,3,3,3,3,3,3,3,1,3,3,3,1,3,3,3,3 },//f
            // a  b  c  d e f g h i j k l m n o p q r s t u v w x y z
             { 3, 1, 3, 3,3,1,0,1,3,3,3,3,3,3,3,3,3,3,3,1,3,3,3,3,3,3 },//g
            // a  b  c  d e f g h i j k l m n o p q r s t u v w x y z
             { 3, 3, 3, 3,3,3,1,0,3,1,3,3,3,1,3,3,3,3,3,3,3,3,3,3,1,3 },//h
            // a  b  c  d e f g h i j k l m n o p q r s t u v w x y z
             { 3, 3, 3, 3,3,3,3,3,0,3,1,3,3,3,1,3,3,3,3,3,1,3,3,3,3,3 },//i
            // a  b  c  d e f g h i j k l m n o p q r s t u v w x y z
             { 3, 3, 3, 3,3,3,3,1,1,0,1,3,1,1,3,3,3,3,3,3,1,3,3,3,3,3 },//j
            // a  b  c  d e f g h i j k l m n o p q r s t u v w x y z
             { 3, 3, 3, 3,3,3,3,3,1,1,0,1,1,3,3,3,3,3,3,3,3,3,3,3,3,3 },//k
            // a  b  c  d e f g h i j k l m n o p q r s t u v w x y z
             { 3, 3, 3, 3,3,3,3,3,3,3,1,0,3,3,1,3,3,3,3,3,3,3,3,3,3,3 },//l
            // a  b  c  d e f g h i j k l m n o p q r s t u v w x y z
             { 3, 3, 3, 3,3,3,3,3,3,1,1,3,0,1,3,3,3,3,3,3,3,3,3,3,3,3 },//m
            // a  b  c  d e f g h i j k l m n o p q r s t u v w x y z
             { 3, 1, 3, 3,3,3,3,1,3,1,3,3,1,0,3,3,3,3,3,3,3,3,3,3,3,3 },//n
            // a  b  c  d e f g h i j k l m n o p q r s t u v w x y z
             { 3, 3, 3, 3,3,3,3,3,1,3,1,1,3,3,0,1,3,3,3,3,3,3,3,3,3,3 },//o
            // a  b  c  d e f g h i j k l m n o p q r s t u v w x y z
             { 3, 3, 3, 3,3,3,3,3,3,3,3,3,3,3,1,0,3,3,3,3,3,3,3,3,3,3 },//p
            // a  b  c  d e f g h i j k l m n o p q r s t u v w x y z
             { 1, 3, 3, 3,3,3,3,3,3,3,3,3,3,3,3,3,0,3,3,3,3,3,1,3,3,3 },//q
            // a  b  c  d e f g h i j k l m n o p q r s t u v w x y z
             { 3, 3, 3, 3,1,1,3,3,3,3,3,3,3,3,3,3,3,0,3,1,3,3,3,3,3,3 },//r
            // a  b  c  d e f g h i j k l m n o p q r s t u v w x y z
             { 1, 3, 3, 1,3,3,3,3,3,3,3,3,3,3,3,3,3,3,0,3,3,3,1,1,3,3 },//s
            // a  b  c  d e f g h i j k l m n o p q r s t u v w x y z
             { 3, 3, 3, 3,3,3,1,3,3,3,3,3,3,3,3,3,3,1,3,0,3,3,3,3,1,3 },//t
            // a  b  c  d e f g h i j k l m n o p q r s t u v w x y z
             { 3, 3, 3, 3,3,3,3,3,1,1,3,3,3,3,3,3,3,3,3,3,0,3,3,3,1,3 },//u
            // a  b  c  d e f g h i j k l m n o p q r s t u v w x y z
             { 3, 1, 1, 3,3,1,3,3,3,1,3,3,3,3,3,3,3,3,3,3,3,0,3,3,3,3 },//v
            // a  b  c  d e f g h i j k l m n o p q r s t u v w x y z
             { 3, 3, 3, 3,1,3,3,3,3,3,3,3,3,3,3,3,1,3,1,3,3,3,0,3,3,3 },//w
            // a  b  c  d e f g h i j k l m n o p q r s t u v w x y z
             { 3, 3, 1, 1,3,3,3,3,3,3,3,3,3,3,3,3,3,3,1,3,3,3,3,0,3,1 },//x
            // a  b  c  d e f g h i j k l m n o p q r s t u v w x y z
             { 3, 3, 3, 3,3,3,3,1,3,3,3,3,3,3,3,3,3,3,3,1,1,3,3,3,0,3 },//y
            // a  b  c  d e f g h i j k l m n o p q r s t u v w x y z
             { 1, 3, 3, 3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,1,3,3,3,3,1,3,0 },//z
         };
    }
    
}
