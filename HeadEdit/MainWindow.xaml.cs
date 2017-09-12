using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
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
        public Log log;
        //flag
        private bool moveFlag = false;
        private bool editFlag = false;
        private bool changeModeFlag = true;
        private bool startFlag = true;
        private int TaskNumber = 1;  //当前执行第几个任务

        //public List<TextRange> HeadSelectRange;
        public List<TextRange> SplitWordsRange;
        public List<TextRange> HighLightRange;
        public int NowChoice = 0;//nowchoice of highlightrange
        public string NowChoiceString;
        List<Rectangle> rects = new List<Rectangle>();

        public void getWrongString(string rightString)
        {
            // richTextBox.Document.Blocks.Clear();
            string[] s1 = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            string[] sp = rightString.Split(' ');
            if (sp.Length == 0) { Console.WriteLine("no space"); return; }
            if (sp.Length == 1) { Console.WriteLine("only one word i dont want to modify"); return; }

            Random c = new Random();
            int i = c.Next(sp.Length); // change i
            int len = sp[i].Length;
            int methodKind = c.Next(3); // which method to use
            int index = c.Next(s1.Length); // the number of char
            int replace_i = c.Next(len);
            string wrong = "oooo";
            TargetWordBlock.Text = sp[i];
               Console.WriteLine(sp[i]);
              Console.WriteLine(s1[index]);
             Console.WriteLine(replace_i);
            if (len <= 1 && methodKind == 2)
            {
                methodKind = c.Next(2);
            }
            if (methodKind == 0)
            {
                Console.WriteLine("Add");
                if (len == replace_i + 1) wrong = sp[i].Substring(0, replace_i + 1) + s1[index];// is the last one
                else wrong = sp[i].Substring(0, replace_i + 1) + s1[index] + sp[i].Substring(replace_i + 1, sp[i].Length - replace_i - 1);
                Console.WriteLine(wrong);
            }
            if (methodKind == 1)
            {
                Console.WriteLine("Modify");
                if (len == replace_i + 1) wrong = sp[i].Substring(0, replace_i) + s1[index];// is the last one
                else wrong = sp[i].Substring(0, replace_i) + s1[index] + sp[i].Substring(replace_i + 1, sp[i].Length - replace_i - 1);
                Console.WriteLine(wrong);


            }
            if (methodKind == 2)
            {
                Console.WriteLine("Del");
                if (len == replace_i + 1) wrong = sp[i].Substring(0, replace_i) + s1[index];// is the last one
                wrong = sp[i].Substring(0, replace_i) + sp[i].Substring(replace_i + 1, sp[i].Length - replace_i - 1);
                Console.WriteLine(wrong);

            }
            sp[i] = wrong;
            string fault = "";
            Console.WriteLine(fault);
            Run zero = new Run(sp[0]);
            if (i == 0)
            {
                zero.Background = Brushes.AliceBlue;
                Paragraph par = new Paragraph(zero);

                for (int k = 1; k < sp.Length; k++)
                {
                    fault = string.Concat(fault, string.Concat(" ", sp[k]));
                }
                Run af = new Run(fault);
                par.Inlines.Add(af); return;
            }
            Console.WriteLine(fault);

            Paragraph para = new Paragraph(zero);

            for (int k = 1; k < i; k++)
            {
                fault = string.Concat(fault, string.Concat(" ", sp[k]));

            }
             Console.WriteLine(fault);
            string f2 = "";
            Run before = new Run(fault);



            para.Inlines.Add(before); // sentences before changed word

           
            Run mid = new Run(string.Concat(" ",sp[i]));
            //*********************
            mid.Background = Config.WrongWordTipColor;
            WrongWordBlock.Text = sp[i];
            //********************



            para.Inlines.Add(mid);
            if (i == sp.Length - 1) return; // is the last one of sp


            for (int k = i + 1; k < sp.Length; k++)
            {
                f2 = string.Concat(f2, string.Concat(" ", sp[k]));
            }
            // Console.WriteLine(f2);

            Run end = new Run(f2);
            para.Inlines.Add(end);

            richTextBox.Document.Blocks.Clear();
            richTextBox.Document.Blocks.Add(para);

        }

        public string RawString;  //Raw text from "text.txt"

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
            Console.WriteLine(getEditDistance("language", "languages"));
            InitializeComponent();
            LoadTextFile(this.richTextBox, "text.txt");
            this.WindowState = WindowState.Maximized;
            

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
            log = new Log(this,DateTime.Now);
            richTextBox.FontSize = Config.richTextBoxFontSize;
            calibration = new Calibration();
            HeadEllipse = new Ellipse()
            {
                Width = 400,
                Height = 150,
                Stroke = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)),
                StrokeThickness = 2
            };
            HeadEllipse.Visibility = Config.debug ? Visibility.Visible : Visibility.Hidden;

            mainCanvas.Children.Add(HeadEllipse);
            if (Config.useCamera)
            {
                calibration.startCalibrate();    //first calibration before use
            }
            richTextBox.Focus();
            richTextBox.CaretPosition = richTextBox.Document.ContentEnd;
            /*richTextBox.Selection.
            richTextBox.Selection.Start = richTextBox.Document.ContentStart;
            richTextBox.SelectionLength = richTextBox.Document.ContentStart.GetPositionAtOffset(10);*/
            for (int i = 0; i < 10; i++)
            {
                rects.Add(new Rectangle());
                mainCanvas.Children.Add(rects[i]);
                rects[i].Height = 35;
                rects[i].Fill = Brushes.Yellow;
                rects[i].Opacity = 0.3;
            }
            richTextBox.FontFamily = Config.fontFamily;
            double width = GetScreenSize("X", Config.fontFamily, Config.richTextBoxFontSize, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal).Width;
            Config.fontWidth = width;

            //System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            //dispatcherTimer.Tick += dispatcherTimer_Tick;
            //dispatcherTimer.Interval = new TimeSpan(0, 0, 5);
            //dispatcherTimer.Start();
        }
        /*private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if(moveFlag == false && EditFlag == false)
            {
                TextRange range = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
                string a = range.Text;
                //Run run = new Run(range.Text);
                range.Text = a;
            }
            // code goes here
        }
        */
        private void handleHeadPosition(Object para)
        {
            if (changeModeFlag == false) return;
            if (startFlag == false) return;
            Point headPos = calibration.parsePoint((Point)para);
            currentCursor = headPosToCanvasPos(headPos);
            updateInterface();
        }
        public void matchString(String input)
        {
            //renew font property of last HighLightRange
            if (HighLightRange != null && HighLightRange.Count > 0)
            {
                HighLightRange[NowChoice].Text = NowChoiceString;

                //for (int i = 0; i < HighLightRange.Count; i++)
                //{
                //    SetFontColor(Config.HeadSelectColor, HighLightRange[i]);
                //}


            }
            //if no input,return
            if (SplitWordsRange == null || input == null)
            {
                return;
            }
            if (input == "") return;
            //string text = HeadSelectRange.Text;
            //find the match range
            HighLightRange = FindString(input);
            //highlight
            for (int i = 0; i < HighLightRange.Count; i++)
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
            double threshold = 5;
            if (keyword.Length <= 4) threshold = 3;
            //            else if(keyword.Length<=4) threshold = keyword.Length-1;
            //            else if (keyword.Length <= 6) threshold = keyword.Length - 2;
            //            else threshold = keyword.Length - 2;
            int i = 0;
            foreach (TextRange b in SplitWordsRange)
            {
                var a = b.Text;
                if (a == "") continue;
                if (a.Length == 0) continue;

                double distance = getEditDistance(a, keyword);
                if (distance <= threshold)
                {
                    if (Math.Abs(distance) < Double.Epsilon) continue;
                    HighLightRange.Add(SplitWordsRange[i]);

                }
                i++;
            }
            return HighLightRange;
        }
        private void updateInterface()
        {
            //Console.WriteLine(richTextBox.ActualWidth);
            if (moveFlag == false && EditFlag == false)
            {
                //renew  last color
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (SplitWordsRange != null && SplitWordsRange.Count > 0)
                    {
                        foreach (TextRange a in SplitWordsRange)
                        {
                            //     SetFontColor(Config.DefaultFontColor, a);
                            //   a.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
                        }
                    }
                    //DateTime t1 = DateTime.Now;
                    var wordRangePos = get_range(currentCursor, headEllipse.Width / 2, headEllipse.Height / 2);
                    //Console.WriteLine(DateTime.Now.Subtract(t1).TotalMilliseconds);
                    SplitWordsRange = wordRangePos.Item1;

                    for (int i = 0; i < wordRangePos.Item2.Count; i++)
                    {
                        Canvas.SetLeft(rects[i], wordRangePos.Item2[i][1]);
                        Canvas.SetTop(rects[i], wordRangePos.Item2[i][0]);
                        rects[i].Visibility = Visibility.Visible;
                        rects[i].Width = wordRangePos.Item2[i][2] - wordRangePos.Item2[i][1];
                    }
                    for (int i = wordRangePos.Item2.Count; i < rects.Count; i++)
                    {
                        rects[i].Visibility = Visibility.Hidden;
                    }
                    foreach (TextRange a in SplitWordsRange)
                    {
                        //   SetFontColor(Config.HeadSelectColor, a);
                        //a.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                    }





                    //setcolor
                    /*if (SplitWordsRange.Count > 2)
                    {
                        SetFontColor(Config.HeadSelectColor, SplitWordsRange[0]);
                        SetFontColor(Config.HeadSelectColor, SplitWordsRange[1]);
                    }*/



                }));

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

        public void SetFontColor(Brush color, TextRange range)
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
                //TextRange range = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
                //string a = range.Text;
            }
        }

        public static IEnumerable<TextRange> GetAllWordRanges(TextPointer p, TextPointer pp)
        {
            //string pattern = @"[^\W\d](\w|[-']{1,2}(?=\w))*";
            string pattern = @"[^\s]+";
            TextPointer pointer = p;
            //
            //List<TextRange> res = new List<TextRange>();
            int no = 0;
            while (pointer != null)
            {
                no++;
                if (pp != null && pointer.CompareTo(pp) > -1)
                {
                    break;
                }
                if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string textRun = pointer.GetTextInRun(LogicalDirection.Forward);
                    string[] words = textRun.Split(' ');
                    int startIndex = 0;
                    foreach (var word in words)
                    {
                        if (word.Length > 0)
                        {
                            //res.Add(new TextRange(pointer.GetPositionAtOffset(startIndex), pointer.GetPositionAtOffset(startIndex + word.Length)));
                            yield return new TextRange(pointer.GetPositionAtOffset(startIndex), pointer.GetPositionAtOffset(startIndex + word.Length));
                        }
                        startIndex += (word.Length + 1);
                    }/*
                    MatchCollection matches = Regex.Matches(textRun, pattern);
                    foreach (Match match in matches)
                    {
                        int startIndex = match.Index;
                        int length = match.Length;
                        TextPointer start = pointer.GetPositionAtOffset(startIndex);
                        TextPointer end = start.GetPositionAtOffset(length);
                        yield return new TextRange(start, end);
                    }*/
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
        public static Size GetScreenSize(string text, FontFamily fontFamily, double fontSize, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch)
        {
            fontFamily = fontFamily ?? new TextBlock().FontFamily;
            fontSize = fontSize > 0 ? fontSize : new TextBlock().FontSize;
            var typeface = new Typeface(fontFamily, fontStyle, fontWeight, fontStretch);
            var ft = new FormattedText(text ?? string.Empty, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, fontSize, Brushes.Black);
            return new Size(ft.Width, ft.Height);
        }

        private Tuple<List<TextRange>, List<double[]>> get_range(Point p, double w, double h) // 上下k行
        {
            List<TextRange> select = new List<TextRange>();

            var pos = richTextBox.GetPositionFromPoint(p, true);
            if (pos == null) return new Tuple<List<TextRange>, List<double[]>>(select, new List<double[]>());
            TextPointer begin = pos.GetLineStartPosition(-1);
          //  TextPointer lastEnd = poz.DocumentEnd.GetLineStartPosition(-3);
            if (begin == null) begin = pos.DocumentStart;
            if (begin.CompareTo(pos.DocumentEnd) < -1)
            {
                begin = pos.DocumentStart;
            }
            /*if (begin != null && lastEnd != null && begin.CompareTo(lastEnd) > 0)
            {
                begin = lastEnd;
            }*/
            TextPointer end = pos.GetLineStartPosition(1);
            begin = begin.GetPositionAtOffset(2);  //?? Magic 2
            Rect beginR = begin.GetCharacterRect(LogicalDirection.Forward);
            List<double[]> arrayPos = new List<double[]>();
            TextPointer lineBegin = begin;
            //Console.WriteLine("========================");
            while (begin != null)
            {
                if (end != null && begin.CompareTo(end) > -1) { break; }
                if (begin.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string text = begin.GetTextInRun(LogicalDirection.Forward);
                    string[] words = text.Split(' ');
                    int startIndex = 0;

                    foreach (var word in words)
                    {
                        if (word.Length > 0)
                        {
                           // Console.WriteLine("startIndes:{0}, word.length:{1}", startIndex, word.Length);
                            TextPointer headP = lineBegin.GetPositionAtOffset(startIndex, LogicalDirection.Forward);
                            TextPointer tailP = lineBegin.GetPositionAtOffset(startIndex + word.Length, LogicalDirection.Forward);
                           // Console.WriteLine(new TextRange(headP, tailP).Text + "|");
                            //Console.WriteLine(new TextRange(headP, lineBegin.GetPositionAtOffset(startIndex+word.Length+1, LogicalDirection.Forward)).Text + "|");
                            //Console.WriteLine(new TextRange(headP, lineBegin.GetPositionAtOffset(startIndex + word.Length + 2, LogicalDirection.Forward)).Text + "|");
                            bool dealed = false;
                           // Console.WriteLine("{0}", headP.GetLineStartPosition(0).CompareTo(lineBegin.GetLineStartPosition(0)));
                            if (headP.GetLineStartPosition(0).CompareTo(lineBegin.GetLineStartPosition(0)) != 0 /*||
                                (headP.GetLineStartPosition(0).CompareTo(lineBegin.GetLineStartPosition(0)) == 0 &&
                                beginR.X + (startIndex + word.Length) * Config.fontWidth > richTextBox.ActualWidth)*/)
                            {
                                dealed = true;
                                //beginR = headP.GetCharacterRect(LogicalDirection.Forward);
                                beginR.Y += beginR.Height;
                                beginR.X = 0;
                                lineBegin = headP;
                                startIndex = 0;
                            }

                            //Rect head = headP.GetCharacterRect(LogicalDirection.Forward);
                            //Rect tail = tailP.GetCharacterRect(LogicalDirection.Backward);
                            Rect head = new Rect();
                            Rect tail = new Rect();
                            head.X = beginR.X + startIndex * Config.fontWidth;
                            head.Y = beginR.Y;
                            head.Height = beginR.Height;
                            tail.X = beginR.X + (startIndex + word.Length) * Config.fontWidth;
                            tail.Y = beginR.Y;
                            tail.Height = beginR.Height;
                            if (tailP.GetLineStartPosition(0).CompareTo(headP.GetLineStartPosition(0)) != 0 && !dealed) //如果换行
                            {
                                //Rect tp = tailP.GetCharacterRect(LogicalDirection.Backward);
                                //tail = tp;
                                head.X = 0;
                                head.Y += head.Height;
                                tail.X = (word.Length * Config.fontWidth);
                                tail.Y += head.Height;
                                lineBegin = tailP;
                                beginR.Y += head.Height;
                                beginR.X = tail.X;
                                startIndex = -(word.Length);
                                //if (tp.X > 0)
                                //{
                                //    head.Y = tp.Y;
                                //    head.X = 0;
                                //    head.Height = tp.Height;
                                //}
                            }
                            //Console.WriteLine("{0},{1},{2}, {3}", word, head.X, tail.X, tailP.GetLineStartPosition(0).CompareTo(headP.GetLineStartPosition(0)));
                            if (!(head.X > (p.X + w) || tail.X < (p.X - w)) && !(head.Y > (p.Y + h + head.Height + 5) || head.Y - head.Height < (p.Y - h - head.Height - 5)))
                            {
                                if ((arrayPos.Count == 0) || Math.Abs(arrayPos.Last()[0] - head.Y) > Double.Epsilon)
                                {
                                    arrayPos.Add(new double[3]);
                                    arrayPos.Last()[0] = head.Y;
                                    arrayPos.Last()[1] = head.X;
                                    arrayPos.Last()[2] = tail.X;
                                }
                                else
                                {
                                    arrayPos.Last()[1] = Math.Min(arrayPos.Last()[1], head.X);
                                    arrayPos.Last()[2] = Math.Max(arrayPos.Last()[2], tail.X);
                                }
                                select.Add(new TextRange(headP, tailP));
                                //Console.WriteLine("in:" + textRange.Text);
                            }
                        }
                        startIndex += (word.Length + 1);
                    }
                }
                begin = begin.GetNextContextPosition(LogicalDirection.Forward);
            }



            /*if (end != null)
            {
                end = poz.DocumentEnd;
            }*/
            //List<TextRange> allTextRanges = GetAllWordRanges(begin, end).ToList();

           // Console.WriteLine(arrayPos);
            return new Tuple<List<TextRange>, List<double[]>>(select, arrayPos);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (startFlag == false && e.Key != Config.TaskStart)
            {
                e.Handled = true;
                return;
            }
            if (e.Key == Config.changeMode)
            {
                if (changeModeFlag == false)
                {
                    LoadTextFile(this.richTextBox, "text.txt");
                    changeModeFlag = true;
                }
                else
                {
                    LoadTextFile(this.richTextBox, "text.txt");
                    changeModeFlag = false;
                    for (int i = 0; i < rects.Count; i++)
                    {
                        rects[i].Visibility = Visibility.Hidden;
                    }
                }
            }
            if (changeModeFlag == false) return;
            if (e.Key == Config.calibrationKey)
            {
                calibration.startCalibrate();
                e.Handled = true;
            }
            else if (e.Key == Config.moveKey)
            {
                moveFlag = true;
                updateInterface();
                e.Handled = true;
            }
            else if (e.Key == Config.editKey)
            {
                EditFlag = true;
                updateInterface();
                e.Handled = true;


            }
            else if (e.Key == Config.cancelKey)
            {
                //return to raw pattern
                moveFlag = false;
                EditFlag = false;
                updateInterface();
                //return to the end of the richtextbox
                richTextBox.Focus();
                richTextBox.CaretPosition = richTextBox.Document.ContentEnd;

                //remove any property of selectrange
                if (popup.IsOpen)
                {
                    popupController.exitPopup();
                    popup.IsOpen = false;
                    //InputBox.Visibility = Visibility.Hidden;
                    for (int i = 0; i < SplitWordsRange.Count; i++)
                    {
                        SetFontColor(Config.DefaultFontColor, SplitWordsRange[i]);
                    }
                    if (HighLightRange != null && HighLightRange.Count != 0)
                    {
                        SetBackGroundColor(Config.DefaultBackGroundColor, HighLightRange[NowChoice]);
                        HighLightRange[NowChoice].Text = NowChoiceString;
                    }

                }


                e.Handled = true;
                richTextBox.Focus();
                ClearRun();
                //range = null;

                //FlowDocument flowDoc = new FlowDocument();

                // Insert an initial paragraph at the beginning of the empty FlowDocument.
                //flowDoc.Blocks.Add(new Paragraph(new Run(
                //myText
                //)));
                //Run run = new Run(range.Text);
                //richTextBox.Document.Blocks.Clear();
                //richTextBox.Document.Blocks.Add(new Paragraph(new Run(myText)));

                richTextBox.CaretPosition = richTextBox.Document.ContentEnd;
                HighLightRange = null;
                SplitWordsRange = null;
                NowChoice = 0;
                NowChoiceString = null;




            }
            else if (e.Key == Config.NextTask)
            {
                Tips.Text = "Task " + TaskNumber.ToString();
                TaskNumber++;
                startFlag = false;
                for (int i = 0; i < rects.Count; i++)
                {
                    rects[i].Visibility = Visibility.Hidden;
                }

                getWrongString(RawString);
                //Run next task
            }
            else if (startFlag == false && e.Key == Config.TaskStart)
            {
                startFlag = true;
                e.Handled = true;
                ClearRun();
            }

        }

        public double getEditDistance(String s, String t)
        {
            s = s.ToLower(); // lower mode
            t = t.ToLower();
            int m = s.Length;
            int n = t.Length;
            double[,] d = new double[s.Length + 1, t.Length + 1];
            for (int i = 0; i < m + 1; i++)
            {
                for (int j = 0; j < n + 1; j++)
                {
                    d[i, j] = Double.MaxValue;
                }
            }
            d[0, 0] = 0;
            for (int i = 0; i < m + 1; i++)
            {
                d[i, 0] = i;
            }
            for (int j = 0; j < n + 1; j++)
            {
                d[0, j] = j;
            }
            for (int i = 1; i < m + 1; i++)
            {
                for (int j = 1; j < n + 1; j++)
                {
                    if (i - 1 >= 0 && d[i - 1, j] < Double.MaxValue)
                    {
                        d[i, j] = Math.Min(d[i - 1, j] + 2, d[i, j]);
                    }
                    if (j - 1 >= 0 && d[i, j - 1] < Double.MaxValue)
                    {
                        d[i, j] = Math.Min(d[i, j - 1] + 2, d[i, j]);
                    }
                    if (i - 1 >= 0 && j - 1 >= 0 && d[i - 1, j - 1] < Double.MaxValue)
                    {
                        d[i, j] = Math.Min(d[i - 1, j - 1] + chartocharx(s[i - 1], t[j - 1]), d[i, j]);
                    }
                }
            }
            return d[m, n];


            ///编辑距离调整：1.长度差距很大，惩罚比较大。 2.字母之间的差距，和键盘相互结合
         /*   int[,] d; // matrix
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
                    d[i, j] = Minimum(d[i - 1, j] + 2, d[i, j - 1] + 2,
                            d[i - 1, j - 1] + cost);
                }
            }
            /*int charzhi = System.Math.Abs(m - n);
            if (charzhi == 2)
            {
                d[n, m] += 3;
            }
            else if (charzhi > 2)
            {
                d[n, m] += charzhi * 3;
            }
            // Step 7
            return d[n, m];*/

        }
        private int chartocharx(char a, char b)
        {
            int x1 = Char.ToLower(a) - 'a';
            int x2 = Char.ToLower(b) - 'a';
            if (x1 < 0 || x1 > 25 || x2 < 0 || x2 > 25)
            {
                if (x1 == x2) return 0;
                return 2;
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

        private void LoadTextFile(RichTextBox richTextBox, string filename)
        {
            richTextBox.Document.Blocks.Clear();
            using (StreamReader streamReader = File.OpenText(filename))
            {
                RawString = streamReader.ReadToEnd();
                Paragraph paragraph = new Paragraph(new Run(RawString));
                richTextBox.Document.Blocks.Add(paragraph);
            }
        }

        private void ClearRun()
        {
            TextRange range = new TextRange(this.richTextBox.Document.ContentStart, this.richTextBox.Document.ContentEnd);
            //string b =range.ToString();
            string myText = range.Text;

            range.Text = myText;
        }
    }



}
