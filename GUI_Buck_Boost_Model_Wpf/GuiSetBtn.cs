using System;
using System.Collections.Generic;
using System.Text;
//xxx using System.Drawing;
using System.Windows.Media; // Brushes

namespace GUI_Buck_Boost_Model_Wpf
{
    public class GuiSetBtn : Object
    {
        private System.Windows.Controls.Button _btn;
        private byte _cmdNum;
        private byte _itemNum;
        private string _btnLabelWhenOn;
        private string _btnLabelWhenOff;
        //xxx private System.Drawing.Color _colorWhenOn;
        //xxx private System.Drawing.Color _colorWhenOff;
        System.Windows.Media.SolidColorBrush _colorWhenOn;
        System.Windows.Media.SolidColorBrush _colorWhenOff;


        private int _value;
        public int value
        {
            get { return _value; }
            set { if(value == 0 || value == 1) _value = value; }
        }

        public wpfCommsManager commsMngr;


        #region Constructors
        public GuiSetBtn(System.Windows.Controls.Button button, byte itemNumber)
        {
            _btn = button;
            _itemNum = itemNumber;
            _btnLabelWhenOn = "On";
            _btnLabelWhenOff = "Off";
            //xxx _colorWhenOn = System.Drawing.Color.SeaGreen;
            //xxx _colorWhenOff = System.Drawing.Color.Crimson;

            _colorWhenOn = Brushes.SeaGreen;
            _colorWhenOff = Brushes.Crimson;
                 
            if (_btn.Content == _btnLabelWhenOn)
            {
                _value = 1;
            }
            else
            {
                _value = 0;
                _btn.Content = _btnLabelWhenOff;
            }
            
            _btn.Tag = (int)_itemNum;

            _cmdNum = 0x02;
        }

        public GuiSetBtn(System.Windows.Controls.Button button, string btnLabelTrue, string btnLabelFalse, byte itemNumber)
        {
            _btn = button;
            _itemNum = itemNumber;
            _btnLabelWhenOn = btnLabelTrue;
            _btnLabelWhenOff = btnLabelFalse;
            _colorWhenOn = Brushes.Black;
            _colorWhenOff = Brushes.Black;


            if (_btn.Content == _btnLabelWhenOn)
            {
                _value = 1;
            }
            else
            {
                _value = 0;
                _btn.Content = _btnLabelWhenOff;
            }

            _btn.Tag = (int)_itemNum;

            _cmdNum = 0x02;
        }
        #endregion


        public void SetButton()
        {
            if (_value == 0)
            {
                _value = 1;
                _btn.Content = _btnLabelWhenOn;
                _btn.Foreground = _colorWhenOn; 
            }
            else
            {
                _value = 0;
                _btn.Content = _btnLabelWhenOff;
                _btn.Foreground = _colorWhenOff;
            }
            int ptr = commsMngr.ptrWriteAt;
            commsMngr.cmdNum[ptr] = _cmdNum;
            commsMngr.itemNum[ptr] = _itemNum;
            commsMngr.data[ptr] = _value;
            commsMngr.ctrl[ptr] = this;
            commsMngr.ptrWriteAt++;
            commsMngr.TryNewCommsTask();
        }


        public void SetDefault()
        {
            if (_btn.Content == _btnLabelWhenOn)
            {
                _value = 1;            
                _btn.Foreground = _colorWhenOn;
            }
            else
            {
                _btn.Content = _btnLabelWhenOff;
                _value = 0;
                _btn.Foreground = _colorWhenOff;
            }
            int ptr = commsMngr.ptrWriteAt;
            commsMngr.cmdNum[ptr] = _cmdNum;
            commsMngr.itemNum[ptr] = _itemNum;
            commsMngr.data[ptr] = _value;
            commsMngr.ctrl[ptr] = this;
            commsMngr.ptrWriteAt++;
            commsMngr.TryNewCommsTask();
        }
    }
}


