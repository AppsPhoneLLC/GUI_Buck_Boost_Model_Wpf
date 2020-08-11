using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.Windows.Media; // Brushes

namespace GUI_Buck_Boost_Model_Wpf
{
    public class GuiSetSldr : Object
    {
        private System.Windows.Controls.TextBox _txt;
        private System.Windows.Controls.Slider  _sldr;
        private System.Windows.Controls.Label _lblMinVal;
        private System.Windows.Controls.Label _lblMaxVal;
        private byte _cmdNum;
        private byte _itemNum;
        private double _fsvVal;
        private double _value;
        private int _guiTypePtr;        //1 - raw integer, 2 - float w/ fsv, 3 - resistive fsv
        //private double _Kv;
        private int _qValue;

        public wpfCommsManager commsMngr;
        
        
        #region Constructors
        //no min-max labels
        public GuiSetSldr(System.Windows.Controls.Slider slider, 
			System.Windows.Controls.TextBox textBox, byte itemNumber)
        {
            _sldr = slider;
            _txt = textBox;

            _itemNum = itemNumber;
            if (_txt.Text == "")
            {
                _txt.Text = "0";
            }

            _fsvVal = 0;
            _value = (double)Convert.ToInt32(_txt.Text);

            _txt.Tag = (int)_itemNum;
            _sldr.Tag = (int)_itemNum; 

            _cmdNum = 0x03;
            _guiTypePtr = 1;
        }

        //keeps _value at 0-FSV while using full range of an Int16
        public GuiSetSldr(System.Windows.Controls.Slider slider, System.Windows.Controls.TextBox textBox, 
            System.Windows.Controls.Label lblMinimumValue, System.Windows.Controls.Label lblMaximumValue, 
            byte itemNumber)
        {
            _txt = textBox;
            _sldr = slider;
            _lblMaxVal = lblMaximumValue;
            _lblMinVal = lblMinimumValue;

            _itemNum = itemNumber;
            if (_txt.Text == "")
            {
                _txt.Text = "0";
            }

            _fsvVal = Convert.ToDouble(_lblMaxVal.Content);
            _value = Convert.ToDouble(_txt.Text);

            _txt.Tag = (int)_itemNum;
            _sldr.Tag = (int)_itemNum;


            _cmdNum = 0x03;
            _guiTypePtr = 2;
        }

        //full scale value via resistive divider
        public GuiSetSldr(System.Windows.Controls.Slider slider, System.Windows.Controls.TextBox textBox, 
            System.Windows.Controls.Label lblMinimumValue, System.Windows.Controls.Label lblMaximumValue, 
            byte itemNumber, int R1, int R2)
        {
            _txt = textBox;
            _sldr = slider;
            _lblMaxVal = lblMaximumValue;
            _lblMinVal = lblMinimumValue;


            _itemNum = itemNumber;
            if (_txt.Text == "")
            {
                _txt.Text = "0";
            }

            _value = Convert.ToDouble(_txt.Text);

            _txt.Tag = (int)_itemNum;
            _sldr.Tag = (int)_itemNum;

            _cmdNum = 0x03;

            double Gfb = (double)R2 / (double)(R1 + R2);
            _fsvVal = 3 / Gfb;
            _qValue = (int)(Math.Log(Math.Pow(2, 15) / _fsvVal) / Math.Log(2));
            //_Kv = _fsvVal / Math.Pow(2, 15 - qValue);
            _lblMinVal.Content = ((int)0).ToString("0.0");
            _lblMaxVal.Content = _fsvVal.ToString("0.0");
            _guiTypePtr = 3;
        }

        //min-max lbls with Q value scaling/limiting
        public GuiSetSldr(System.Windows.Controls.Slider slider, System.Windows.Controls.TextBox textBox, 
            System.Windows.Controls.Label lblMinimumValue, System.Windows.Controls.Label lblMaximumValue, 
            byte itemNumber, int q_Value)
        {
            _txt = textBox;
            _sldr = slider;
            _lblMaxVal = lblMaximumValue;
            _lblMinVal = lblMinimumValue;
            _qValue = q_Value;

            _cmdNum = 0x03;
            _itemNum = itemNumber;


            if (_txt.Text == "")
            {
                _txt.Text = "0";
            }

            _value = Convert.ToDouble(_txt.Text);
            _txt.Tag = (int)_itemNum;
            _sldr.Tag = (int)_itemNum;

            int temp = ((int)Math.Pow(2, 15 - _qValue));
            _lblMinVal.Content = Convert.ToDouble(_lblMinVal.Content).ToString("0.0"); ;
            _lblMaxVal.Content = Convert.ToDouble(_lblMaxVal.Content).ToString("0.0");

            _sldr.Minimum = (int)(Convert.ToDouble(_lblMinVal.Content) * (Math.Pow(2, _qValue)));
            _sldr.Maximum = (int)(Convert.ToDouble(_lblMaxVal.Content) * (Math.Pow(2, _qValue)));

            double diff;

            diff = (Convert.ToDouble(_lblMaxVal.Content) - Convert.ToDouble(_lblMinVal.Content));
            for (int i = 0; i < 1000; i++)
            {
                if (diff > 1.5 * (Math.Pow(10, 4 - i)))
                {
                    if (_qValue < 9 && i > 4)
                    {
                        i--;
                    }
                    _sldr.TickFrequency = (int)((Math.Pow(10, 4 - i)) * Math.Pow(2, _qValue));
                    i = 1000;
                }
            }

            _guiTypePtr = 4;
        }

        #endregion


        public int CheckValidity()
        {
            Regex HasRptingDecimals = new Regex("[0-9]*[.][0-9]*[.]");
            Regex HasInvalidChars = new Regex("[^0-9.]");
            Regex HasInvalidChars_Neg = new Regex("[^0-9.-]");
            Regex IsNotEmpty = new Regex("[0-9]");
            Regex HasInt = new Regex("int-[0-9]+");

            string checkText = _txt.Text;

            //xxx _txt.Foreground = System.Drawing.Color.Crimson;
            _txt.Foreground = Brushes.Crimson;


            if (_guiTypePtr == 1)
            {
                if (HasInvalidChars.IsMatch(checkText) == false &&
                       IsNotEmpty.IsMatch(checkText) == true &&
                       Convert.ToInt32(_txt.Text) < _sldr.Maximum)
                {
                    //xxx _txt.ForeColor = System.Drawing.Color.SeaGreen;
                    _txt.Foreground = Brushes.SeaGreen;
                    _value = Convert.ToDouble(_txt.Text);
                    return 0;
                }
            }

            if (_guiTypePtr == 2)
            {

                if (HasInvalidChars.IsMatch(checkText) == false &&
	                    HasRptingDecimals.IsMatch(checkText) == false && 
		                IsNotEmpty.IsMatch(checkText) == true)
                {
                    double textFloat = Convert.ToDouble(_txt.Text);
                    double fsvFloat = Convert.ToDouble(_lblMaxVal.Content);

                    if (textFloat <= fsvFloat)
                    {
                        //xxx _txt.ForeColor = System.Drawing.Color.SeaGreen;
                        _txt.Foreground = Brushes.SeaGreen;
                        _value = textFloat;
                        return 0;
                    }
                }
            }

            if (_guiTypePtr == 3)
            {
                if (HasInvalidChars.IsMatch(checkText) == false && 
		                HasRptingDecimals.IsMatch(checkText) == false && 
		                IsNotEmpty.IsMatch(checkText) == true)
                {
                    double textFloat = Convert.ToDouble(_txt.Text);
                    double fsvFloat = Convert.ToDouble(_lblMaxVal.Content);

                    if (textFloat <= fsvFloat)
                    {
                        //xxx txt.ForeColor = System.Drawing.Color.SeaGreen;
                        _txt.Foreground = Brushes.SeaGreen;

                        _value = textFloat;
                        return 0;
                    }
                }
            }

            if (_guiTypePtr == 4)
            {
                if (HasInvalidChars_Neg.IsMatch(checkText) == false && 
		                HasRptingDecimals.IsMatch(checkText) == false && 
		                IsNotEmpty.IsMatch(checkText) == true)
                {
                    double textFloat = Convert.ToDouble(_txt.Text);
                    double fsvFloatMax = Convert.ToDouble(_lblMaxVal.Content);
                    double fsvFloatMin = Convert.ToDouble(_lblMinVal.Content);

                    if ((textFloat <= fsvFloatMax) && (textFloat >= fsvFloatMin) &&
                        (textFloat <= Convert.ToDouble(_lblMaxVal.Content)) &&
                        (textFloat >= Convert.ToDouble(_lblMinVal.Content)))
                    {
                        //xxx _txt.ForeColor = System.Drawing.Color.SeaGreen;
                        _txt.Foreground = Brushes.SeaGreen;
                        _value = textFloat;
                        return 0;
                    }
                }
            }
            return -1;
        }


        public void SetSlider()
        { 
            if (_guiTypePtr == 1)
            {
                _value = _sldr.Value;

                //xxx _txt.ForeColor = SystemColors.WindowText;
                _txt.Foreground = Brushes.Black;

                _txt.Text = _sldr.Value.ToString();
                _value = _sldr.Value;

                int ptr = commsMngr.ptrWriteAt;
                commsMngr.cmdNum[ptr] = _cmdNum;
                commsMngr.itemNum[ptr] = _itemNum;
                //xxx commsMngr.data[ptr] = _sldr.Value;
                commsMngr.data[ptr] = (int)_sldr.Value;
                commsMngr.ctrl[ptr] = this;
                commsMngr.ptrWriteAt++;
                commsMngr.TryNewCommsTask();
            }

            if (_guiTypePtr == 2)
            {
                _value = _sldr.Value * _fsvVal / 32767;
                _txt.Text = _value.ToString("0.000");
                //xxx _txt.ForeColor = SystemColors.WindowText;
                _txt.Foreground = Brushes.Black;


                int ptr = commsMngr.ptrWriteAt;
                commsMngr.cmdNum[ptr] = _cmdNum;
                commsMngr.itemNum[ptr] = _itemNum;
                commsMngr.data[ptr] = (int) _sldr.Value;
                commsMngr.ctrl[ptr] = this;
                commsMngr.ptrWriteAt++;
                commsMngr.TryNewCommsTask();
            }

            if (_guiTypePtr == 3)
            {
                _value = _sldr.Value * _fsvVal / 32767;
                _txt.Text = _value.ToString("0.000");
                //xxx _txt.ForeColor = SystemColors.WindowText;
                _txt.Foreground = Brushes.Black;


                int ptr = commsMngr.ptrWriteAt;
                commsMngr.cmdNum[ptr] = _cmdNum;
                commsMngr.itemNum[ptr] = _itemNum;
                commsMngr.data[ptr] = (int)_sldr.Value;
                commsMngr.ctrl[ptr] = this;
                commsMngr.ptrWriteAt++;
                commsMngr.TryNewCommsTask();
            }

            if (_guiTypePtr == 4)
            {
                int temp = (int)_sldr.Value;
                if (temp > 32767)
                {
                    temp = 32767;
                    _sldr.Value = temp;
                }
                _value = _sldr.Value / Math.Pow(2, _qValue);
                if (_qValue > 2)
                {
                    _txt.Text = _value.ToString("0.000");
                }
                else if (_qValue == 1)
                {
                    _txt.Text = _value.ToString("0.0");
                }
                else if (_qValue == 0)
                {
                    _txt.Text = _value.ToString("0");
                }
                //xxx _txt.ForeColor = SystemColors.WindowText;
                _txt.Foreground = Brushes.Black;

                int ptr = commsMngr.ptrWriteAt;
                commsMngr.cmdNum[ptr] = _cmdNum;
                commsMngr.itemNum[ptr] = _itemNum;
                commsMngr.data[ptr] = (int) _sldr.Value;
                commsMngr.ctrl[ptr] = this;
                commsMngr.ptrWriteAt++;
                commsMngr.TryNewCommsTask();
            }
        }


        public void SetText()
        {
            if (_guiTypePtr == 1 && _txt.Foreground == Brushes.SeaGreen)  //raw integer
            {
                _sldr.Value = (Int32)_value;
                //xxx _txt.ForeColor = SystemColors.WindowText;
                _txt.Foreground = Brushes.Black;


                int ptr = commsMngr.ptrWriteAt;
                commsMngr.cmdNum[ptr] = _cmdNum;
                commsMngr.itemNum[ptr] = _itemNum;
                commsMngr.data[ptr] = (int)_sldr.Value;
                commsMngr.ctrl[ptr] = this;
                commsMngr.ptrWriteAt++;
                commsMngr.TryNewCommsTask();
            }

            if (_guiTypePtr == 2 && _txt.Foreground == Brushes.Black)   //fsv
            {
                _sldr.Value = (int)(_value * _fsvVal / Math.Pow(2, 16));
                //xxx _txt.ForeColor = SystemColors.WindowText;
                _txt.Foreground = Brushes.SeaGreen;


                int ptr = commsMngr.ptrWriteAt;
                commsMngr.cmdNum[ptr] = _cmdNum;
                commsMngr.itemNum[ptr] = _itemNum;
                commsMngr.data[ptr] = (Int32)((_value * 32767) / Convert.ToDouble(_fsvVal));
                commsMngr.ctrl[ptr] = this;
                commsMngr.ptrWriteAt++;
                commsMngr.TryNewCommsTask();
            }

            if (_guiTypePtr == 3 && _txt.Foreground == Brushes.SeaGreen)  //for easy resistor entry...
            {
                _sldr.Value = (int)(_value * 32768 / _fsvVal);
                //xxx _txt.ForeColor = SystemColors.WindowText;
                _txt.Foreground = Brushes.Black;

                int ptr = commsMngr.ptrWriteAt;
                commsMngr.cmdNum[ptr] = _cmdNum;
                commsMngr.itemNum[ptr] = _itemNum;
                commsMngr.data[ptr] = (Int32)((_value * 32767) / Convert.ToDouble(_fsvVal));
                commsMngr.ctrl[ptr] = this;
                commsMngr.ptrWriteAt++;
                commsMngr.TryNewCommsTask();
            }

            if (_guiTypePtr == 4 && _txt.Foreground == Brushes.SeaGreen)
            {
                int temp = (int)(_value * (Math.Pow(2, _qValue)));
                if (temp > 32767)
                {
                    temp = 32767;
                }
                _sldr.Value = temp;
                //xxx _txt.ForeColor = SystemColors.WindowText;
                _txt.Foreground = Brushes.Black;

                int ptr = commsMngr.ptrWriteAt;
                commsMngr.cmdNum[ptr] = _cmdNum;
                commsMngr.itemNum[ptr] = _itemNum;
                commsMngr.data[ptr] = (Int32)(_value * Math.Pow(2, _qValue));
                commsMngr.ctrl[ptr] = this;
                commsMngr.ptrWriteAt++;
                commsMngr.TryNewCommsTask();
            }
        }


        public void SetDefault()
        {
            CheckValidity();
            SetText();
        }
    }
}
