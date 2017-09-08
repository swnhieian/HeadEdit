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
            window.popupTextBox.KeyDown += PopupTextBox_KeyDown;
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
                exitPopup();
            }
            else if (e.Key == Config.selectNext)
            {
                //to do: change highlight

            }
            else  // normal input
            {
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
