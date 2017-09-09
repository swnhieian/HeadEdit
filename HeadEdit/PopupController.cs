using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace HeadEdit
{
    class PopupController
    {
        MainWindow window;
        public PopupController(MainWindow window)
        {
            this.window = window;
            //set styles of popup window
            window.popup.IsOpen = false;
            window.popupTextBox.KeyUp += PopupTextBox_KeyDown;
            window.popupTextBox.FontSize = Config.richTextBoxFontSize;
        }

        private void PopupTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Config.cancelKey)
            {
                exitPopup();
            }
            else if (e.Key == Config.enterSelect)
            {
                //to do: replace text
                if (window.popupTextBox.Text!=null&&window.popupTextBox.Text != "")
                {
                    window.popupTextBox.Text = "";

                    //remove all property of headselectrange
                    for (int i = 0; i < window.SplitWordsRange.Count; i++)
                    {
                        window.SetFontColor(Config.HeadSelectColor, window.SplitWordsRange[i]);
                    }
                    if (window.HighLightRange != null)
                    {
                        window.SetBackGroundColor(Config.DefaultBackGroundColor, window.HighLightRange[window.NowChoice]);
                    }
                    
                }
                
                //exitPopup();
            }
            else if (e.Key == Config.selectNext)
            {
                if(window.HighLightRange==null||window.HighLightRange.Count <= 1)
                {
                    return;
                }
                else
                {
                    //remove background property of lastchoice
                    window.SetBackGroundColor(Config.DefaultBackGroundColor, window.HighLightRange[window.NowChoice]);
                    //remove preview
                    window.HighLightRange[window.NowChoice].Text = window.NowChoiceString;
                    //
                    window.SetFontColor(Config.HeadSelectColor, window.HighLightRange[window.NowChoice]);
                    //change now choice
                    window.NowChoice = (window.NowChoice + 1) % window.HighLightRange.Count;

                    window.NowChoiceString = window.HighLightRange[window.NowChoice].Text;
                    //PreView of the input
                    window.HighLightRange[window.NowChoice].Text = window.popupTextBox.Text;
                    //SetBackGround
                    window.SetBackGroundColor(Config.BackGroundColor, window.HighLightRange[window.NowChoice]);
                }

            }
            else  // normal input
            {
                //Console.WriteLine(window.popupTextBox.Text);
                //remove background property of lastchoice
                if(window.HighLightRange != null&& window.HighLightRange.Count > 0)
                {
                    window.SetBackGroundColor(Config.DefaultBackGroundColor, window.HighLightRange[window.NowChoice]);
                }
                //window.HighLightRange = null;
                window.matchString(window.popupTextBox.Text);
                
            }
        }
        private void exitPopup()
        {
            window.popup.IsOpen = false;
            window.richTextBox.Focusable = true;
            window.popupTextBox.Text = "";
            window.richTextBox.Focus();
            window.EditFlag = false;
        }

        public void pop(Point position)
        {
            window.popup.PlacementTarget = window.HeadEllipse;
            window.popup.Placement = PlacementMode.Right;
            window.popup.VerticalOffset = 100;
            window.popup.IsOpen = true;
            window.richTextBox.Focusable = false;
            window.popupTextBox.Focus();
        }
    }
}
