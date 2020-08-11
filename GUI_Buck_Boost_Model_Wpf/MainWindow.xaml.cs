using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Windows.Threading;   // added to override DoEvents 
using System.Timers;
using System.Text.RegularExpressions;


namespace GUI_Buck_Boost_Model_Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow
    {
        ~MainWindow()
        {
            Disconnect();
        }

        // Another DoEvents overload
        /*
        public static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { } ));
        }
        */

        // another DoEvents overload with helper function
        public void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
        }

        public DispatcherTimer MainTimer = new System.Windows.Threading.DispatcherTimer();
        public DispatcherTimer GraphTimer = new System.Windows.Threading.DispatcherTimer();

        public object ExitFrame(object f)
        {
            ((DispatcherFrame)f).Continue = false;
            return null;
        }


        // ---global variables ---

        GuiSetBtn[] btnGroup;
        GuiSetTxt[] txtGroup;

        GuiGetVar[] varGroup;
        GuiGetArray[] arrayGroup;
        GuiSetSldr[] sldrGroup;

        bool autoUpdateOn = false;

        bool graphOn = false;
        bool powerCycled = false;

        wpfCommsManager commsMngr = new wpfCommsManager();

        //xxx int guiGetVarSize   = 12;
        int guiGetVarSize   = 7;
        int guiGetArraySize = 1;
        int guiSetSldrSize  = 7;
        //xxx int guiSetBtnSize   = 2;
        int guiSetTxtSize   = 20;


        int tickCount = 0;
        bool myInitialized = false;

        double doubleVout     = 0;
        double doubleVin      = 0;
        double doubleIout     = 0;
        double doubleIin      = 0;
        double faultnum       = 0;

        Setup_Connection setupConnection;
        public MainWindow()
        {

            InitializeComponent();
            //grphForm = newGraphForm(this);
            setupConnection = new Setup_Connection(this, commsMngr);
            commsMngr.Close();

            //xxx MainTimer.Enabled = true;
            MainTimer.Stop();
            MainTimer.Tick += new EventHandler(MainTimer_Tick);

            //xxx GraphTimer.Enabled = true; 
            //GraphTimer.Stop;

            graphOn = true;

            //xxx btnGroup = new GuiSetBtn[guiSetBtnSize];
            txtGroup = new GuiSetTxt[guiSetTxtSize];
            sldrGroup = new GuiSetSldr[guiSetSldrSize];

            varGroup = new GuiGetVar[guiGetVarSize];
            arrayGroup = new GuiGetArray[guiGetArraySize];
            //xxx TSGraphGroup = new GuiGraphTSArray[guiGetTSGraphSize];
            //xxx memoryGroup = new GuiGetMemory[guiGetMemorySize];

            //---Set Controls---
            //btns
            //***(btnGroup[0] used to ClearFaultFlag)
            //btnGroup[0] = new GuiSetBtn(btnClearFaultFlg, 0x00);      //do not use
            //xxx btnGroup[0] = new GuiSetBtn(btnDBauto, 0x00);
            //xxx btnGroup[1] = new GuiSetBtn(btn2P2Zselect, 0x01);
            //xxx btnGroup[2] = new GuiSetBtn(btnCC_Enable, 0x02);
            //xxx btnGroup[3] = new GuiSetBtn(btnCP_Enable, 0x03);

            //txts
            //xxx txtGroup[0] = new GuiSetTxt(txtSlope, btnSlope, 0, 0x00);
            //xxx txtGroup[1] = new GuiSetTxt(txtIpri_trip, btnIpri_trip, 15, 0x01);
            //xxx txtGroup[2] = new GuiSetTxt(txtDBAtoP, btnDBAtoP, 0, 0x02);
            //xxx txtGroup[3] = new GuiSetTxt(txtDBPtoA, btnDBPtoA, 0, 0x03);
            //xxx txtGroup[4] = new GuiSetTxt(txtsub_adj, btnsub_adj, 0, 0x04);
            //zzz txtGroup[11] = new GuiSetTxt(txtSetSRmode, btnSetSRmode, 0, 0x0c);  // Crashes


            //sldrs
            //xxx sldrGroup[0] = new GuiSetSldr(sldrVout, txtVoutSet, lblVoutMin, lblVoutMax, 0x00, 10);  //

            //xxx sldrGroup[1] = new GuiSetSldr(sldrPgain_V, txtPgain_V, lblPgain_VMin, lblPgain_VMax, 0x01, 0); //Dlog Trigger
            //xxx sldrGroup[2] = new GuiSetSldr(sldrIgain_V, txtIgain_V, lblIgain_VMin, lblIgain_VMax, 0x02, 0); //Shoulder
            //xxx sldrGroup[3] = new GuiSetSldr(sldrDgain_V, txtDgain_V, lblDgain_VMin, lblDgain_VMax, 0x03, 0); //Shoulder

            //---Get Controls---
            //vars
            varGroup[0] = new GuiGetVar(txtVout, 0x00, 7);      // 
            varGroup[1] = new GuiGetVar(txtVin, 0x01, 5);
            varGroup[2] = new GuiGetVar(txtIpri, 0x02, 7);      // 
            varGroup[3] = new GuiGetVar(txtIout, 0x03, 7);      // 
            varGroup[4] = new GuiGetVar(txtFaultFlg, 0x04, 6);
            varGroup[5] = new GuiGetVar(txtModelFreq, 0x05, 0);
            varGroup[6] = new GuiGetVar(txtDutyCycle, 0x06, 8);

            // varGroup[4] = new GuiGetVar(txtFaultFlg, 0x04);  // no worky
            // varGroup[5] = new GuiGetVar(txtModelFreq, 0x05);
            //xxx varGroup[6] = new GuiGetVar(txtDBPtoA_read, 0x06, 0);
            //xxx varGroup[7] = new GuiGetVar(txtSRmode, 0x07, 0);
            //xxx varGroup[8] = new GuiGetVar(txtPout, 0x08, 6);
            //xxx varGroup[9] = new GuiGetVar(txtCC_flag, 0x09, 0);
            //xxx varGroup[10] = new GuiGetVar(txtCP_flag, 0x0a, 0);
            //xxx varGroup[11] = new GuiGetVar(txtUV_Shutdown, 0x0b, 0);

            //arrays
            //arrayGroup[0] = new GuiGetArray(new TextBox[2] {txtVGetCH1, txtVGetCH2}, 0x00, 12);

            //Time-sequenced graphs (block of data from target)
            //TSGraphGroup[0] = new GuiGraphTSArray(128, 0x00, pnlIpcTotal, lblIpfcTotalASMin, lblIpfcTotalASMax, 11, chbAutoScaleIpfc);
            //TSGraphGroup[1] = new GuiGraphTSArray(128, 0x01, pnlVacRect, lblVacTotalASMin, lblVacTotalASMax, 9, chbAutoScaleVac);

            //memory gets
            //memoryGroup[0] = new GuiGetMemory(textBox1, textBox2, 0x00);

            #region Initialize common components among the ctrl groups (ex. commsMngr reference)

            
            //xxx for (int i = 0; i < guiSetBtnSize; i++)
            //xxx {Set
            //xxx     if (btnGroup[i] != null)
            //xxx     {
            //xxx        btnGroup[i].commsMngr = commsMngr;
            //xxx    }
            //xxx }
                        
            for (int i = 0; i < guiSetTxtSize; i++)
            {
                if (txtGroup[i] != null)
                {
                    txtGroup[i].commsMngr = commsMngr;
                }
            }
                      
            for (int i = 0; i < guiSetSldrSize; i++)
            {
                if (sldrGroup[i] != null)
                {
                    sldrGroup[i].commsMngr = commsMngr;
                }
            }
            


            for (int i = 0; i < guiGetVarSize; i++)
            {
                if (varGroup[i] != null)
                {
                    varGroup[i].commsMngr = commsMngr;
                }
            }
            
            for (int i = 0; i < guiGetArraySize; i++)
            {
                if (arrayGroup[i] != null)
                {
                    arrayGroup[i].commsMngr = commsMngr;
                }
            }

            /*
            for (int i = 0; i < guiGetTSGraphSize; i++)
            {
                if (TSGraphGroup[i] != null)
                {
                    TSGraphGroup[i].commsMngr = commsMngr;
                }
            }
            */

            /*
            for (int i = 0; i < guiGetMemorySize; i++)
            {
                if (memoryGroup[i] != null)
                {
                    memoryGroup[i].commsMngr = commsMngr;
                }
            }
            */

            #endregion

            lblStatus.Content = "NOT CONNECTED";
            lblStatus.Foreground = Brushes.Red;

            cmbMainUpdateRate.Items.Add("1000");
            cmbMainUpdateRate.Items.Add("1.00");
            cmbMainUpdateRate.Items.Add("1.50");
            cmbMainUpdateRate.Items.Add("2.00");
            autoUpdateOn = true;

            // default port info
            //xxx commsMngr.PortName = "COM4";
            //xxx commsMngr.BaudRate = Convert.ToInt32("57600");

            setupConnection.getAllPorts();
            cmbMainUpdateRate.SelectedIndex = 2;
            commsMngr.mainForm = this;
            myInitialized = true;

            //xxx  updateSliders();

       }



        private void btnSetupConnection_Click(object sender, RoutedEventArgs e)
        {
            Disconnect();
            setupConnection.Show();
        }

        //---Save new Settings to Internal Storage
        private void btnSaveDefaults_Click(object sender, RoutedEventArgs e)
        {
            if (cmbMainUpdateRate.SelectedItem != null)
            {
              Properties.Settings.Default.VariableUpdateInterval = cmbMainUpdateRate.SelectedItem.ToString();
            }
            if (setupConnection.cmbComms.SelectedItem != null)
            {
              Properties.Settings.Default.cmbComportSelect = setupConnection.cmbComms.SelectedItem.ToString();
            }

            Properties.Settings.Default.txtSetPwmSwFreq  = txtSetPwmSwFreq.Text;
            Properties.Settings.Default.txtSetVin        = txtSetVin.Text;
            Properties.Settings.Default.txtInductor_L    = txtInductor_L.Text;
            Properties.Settings.Default.txtDCR           = txtDCR.Text;
            Properties.Settings.Default.txtCelec         = txtCelec.Text;
            Properties.Settings.Default.txtESRelec       = txtESRelec.Text;
            Properties.Settings.Default.txtCcer          = txtCcer.Text;
            Properties.Settings.Default.txtESRcer        = txtESRcer.Text;
            Properties.Settings.Default.txtVoutMS        = txtVoutMS.Text;
            Properties.Settings.Default.txtVoutCOF       = txtVoutCOF.Text;
            Properties.Settings.Default.txtVinMS         = txtVinMS.Text;
            Properties.Settings.Default.txtIindMS        = txtIindMS.Text;
            Properties.Settings.Default.txtIlCOF         = txtIlCOF.Text;
            Properties.Settings.Default.txtRdsQ1         = txtRdsQ1.Text;
            Properties.Settings.Default.txtRdsQ2         = txtRdsQ2.Text;
            Properties.Settings.Default.txtIsRCS         = txtIsRCS.Text;
            Properties.Settings.Default.txtIloadLo       = txtIloadLo.Text;
            Properties.Settings.Default.txtIloadHi       = txtIloadHi.Text;
            Properties.Settings.Default.txtIloadCC       = txtIloadCC.Text;
            Properties.Settings.Default.txtIloadMS       = txtIloadMS.Text;
            Properties.Settings.Default.txtbaudrate      = setupConnection.txtBaudRate.Text;
            Properties.Settings.Default.txtcomport       = commsMngr.PortName;

            Properties.Settings.Default.Save(); 
        }

        //---Sets all 'Set' Controls to their default state
        private void SetDefault()
        {      
          //xxx int defaultValIndex = cmbMainUpdateRate.Items.IndexOf(Properties.Settings.Default.VariableUpdateInterval);
          //xxx if (defaultValIndex != -1)
          //xxx {
          //xxx   cmbMainUpdateRate.SelectedIndex = defaultValIndex;
          //xxx }
          //xxx else cmbMainUpdateRate.SelectedIndex = 0;

          //xxx defaultValIndex = cmbGraphUpdateRate.Items.IndexOf(Properties.Settings.Default.GraphUpdateInterval);
          //xxx if (defaultValIndex != -1)
          //xxx {
          //xxx   cmbGraphUpdateRate.SelectedIndex = defaultValIndex;
          //xxx }
          //xxx else cmbGraphUpdateRate.SelectedIndex = 0;

           
          //xxx for (int i = 0; i < guiSetBtnSize; i++)
          //xxx   {
          //xxx       if (btnGroup[i] != null)
          //xxx       {
          //xxx           btnGroup[i].SetDefault();
          //xxx       }
          //xxx   }
            

          
          //xxx for (int i = 0; i < guiSetTxtSize; i++)
          //xxx {
          //xxx   if (txtGroup[i] != null)
          //xxx   {
          //xxx    txtGroup[i].SetDefault();
          //xxx  }
          //xxx}
          
            

           /* 
           for (int i = 0; i < guiSetSldrSize; i++)
           {
             if (sldrGroup[i] != null)
             {
               sldrGroup[i].SetDefault();
             }
           }
           */

        }

    private void btnResetDefaults_Click(object sender, RoutedEventArgs e)
    {
       int defaultValIndex = cmbMainUpdateRate.Items.IndexOf(Properties.Settings.Default.VariableUpdateInterval);
       if (defaultValIndex != -1)
       {
          cmbMainUpdateRate.SelectedIndex = defaultValIndex;
       }
       else cmbMainUpdateRate.SelectedIndex = 0;

       defaultValIndex = setupConnection.cmbComms.Items.IndexOf(Properties.Settings.Default.cmbComportSelect);
       if (defaultValIndex != -1)
       {
          setupConnection.cmbComms.SelectedIndex = defaultValIndex;
       }
       else setupConnection.cmbComms.SelectedIndex = 0;

       txtSetPwmSwFreq.Text             = Properties.Settings.Default.txtSetPwmSwFreq;
       txtSetVin.Text                   = Properties.Settings.Default.txtSetVin;
       txtInductor_L.Text               = Properties.Settings.Default.txtInductor_L;
       txtDCR.Text                      = Properties.Settings.Default.txtDCR;
       txtCelec.Text                    = Properties.Settings.Default.txtCelec;
       txtESRelec.Text                  = Properties.Settings.Default.txtESRelec;
       txtCcer.Text                     = Properties.Settings.Default.txtCcer;
       txtESRcer.Text                   = Properties.Settings.Default.txtESRcer;
       txtVoutMS.Text                   = Properties.Settings.Default.txtVoutMS;
       txtVoutCOF.Text                  = Properties.Settings.Default.txtVoutCOF;
       txtVinMS.Text                    = Properties.Settings.Default.txtVinMS;
       txtIindMS.Text                   = Properties.Settings.Default.txtIindMS;
       txtIlCOF.Text                    = Properties.Settings.Default.txtIlCOF;
       txtRdsQ1.Text                    = Properties.Settings.Default.txtRdsQ1;
       txtRdsQ2.Text                    = Properties.Settings.Default.txtRdsQ2;
       txtIsRCS.Text                    = Properties.Settings.Default.txtIsRCS;
       txtIloadLo.Text                  = Properties.Settings.Default.txtIloadLo;
       txtIloadHi.Text                  = Properties.Settings.Default.txtIloadHi;
       txtIloadCC.Text                  = Properties.Settings.Default.txtIloadCC;
       txtIloadMS.Text                  = Properties.Settings.Default.txtIloadMS;
       setupConnection.txtBaudRate.Text = Properties.Settings.Default.txtbaudrate;

       //xxx colorTextOrange();

       //xxx Properties.Settings.Default.Reload();

       if (commsMngr.IsOpen == true)
       {
         //xxx SetDefault();
       }
       else
        {
          //xxx commsMngr.PortName               = Properties.Settings.Default.txtcomport;
          //xxx setupConnection.SetDefault();
       }
    }

    private void btnConnect_Click(object sender, RoutedEventArgs e)
    {
        lblStatus.Foreground = Brushes.DarkRed;            
        if (commsMngr.IsOpen == true)
        {
            Disconnect();
        }
        else
        {
            Connect();
            autoUpdateOn = true;
            tickCount    = 0;
        }
     }

        //---Disconnect from the target and change displays to show this---
        public void Disconnect()
        {
            if (commsMngr.comValid == true && commsMngr.IsOpen == true)
            {
                // txtVoutSet.Text = "0.00";
                // txtSlope.Text = "290";
                //xxx sldrGroup[0].SetDefault();  // cause exception
                // txtGroup[0].SetDefault();

                autoUpdateOn = false;       //stop automatically getting variables from target
                MainTimer.Stop();
                GraphTimer.Stop();

                //Allow SerialPort to finish backlogged commands
                for (int i = 0; commsMngr.ptrWorkingAt != commsMngr.ptrWriteAt; i++)
                {
                    DoEvents();                 //allow main thread to update GUI items (textboxes, etc)
                    System.Threading.Thread.Sleep(20);      // 50 ms
                    if (i > 100)
                    {
                        commsMngr.isReceiving = false;
                        commsMngr.TryNewCommsTask();
                    }
                }
                DoEvents();

                commsMngr.Close();
                lblStatus.Foreground = Brushes.Red;

                colorTextOrange();

                btnConnect.Content = "Connect";

                lblStatus.Content = "Disconnected";
                //xxx EnableCtrls(false);
            }
        }

        //---Lost Connection so Disconnect and warn user---
        public void connectionLost()
        {
            MainTimer.Stop();
            commsMngr.Close();
            commsMngr.ptrWorkingAt = commsMngr.ptrWriteAt;
            btnConnect.Content = "Connect";
            lblStatus.Foreground = Brushes.DarkRed;
            lblStatus.Content = "Connection Lost : Board May Not Be Properly Turned On";

            colorTextOrange();

            //xxx pnlConnect.Background = Brushes.DarkRed;
            //xxx EnableCtrls(false);
        }

        //---Exception and warn user---
        public void exceptionMess()
        {
            MainTimer.Stop();
            commsMngr.Close();
            commsMngr.ptrWorkingAt = commsMngr.ptrWriteAt;
            lblStatus.Content = "Exception: Try reloading FPGA";
            //xxx pnlConnect.Background = Brushes.DarkRed;
            //xxx EnableCtrls(false);
        }

        //---Connect to the target via SCI and change displays to show this---
        public void Connect()
        {
            if (commsMngr.SciConnect() == false)
            {
                //xxx pnlConnect.Background = Brushes.Red;
                lblStatus.Content = "Could Not Connect:  Please Check Setup or Connections";
                btnConnect.Content = "Connect";
            }
            else
            {
                #region Connected Successfully
                commsMngr.ClearCommands();
                //xxx EnableCtrls(true);
                commsMngr.ptrWriteAt = 0;
                commsMngr.ptrWorkingAt = 0;
                commsMngr.isReceiving = false;

                //Set Gui Controls to default settings
                Properties.Settings.Default.Reload();
                //SetDefault();

                lblStatus.Content = "Connected";
                lblStatus.Foreground = Brushes.Green;

                //xxx MainTimer.myInterval = 1000;
                MainTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000);  // 1000 ms

                MainTimer.Start();
                btnConnect.Content = "Disconnect";
                //xxx cmbMainUpdateRate_SelectedIndexChanged(this, new EventArgs());
                // cmbMainUpdateRate_SelectionChanged(this, new SelectionChangedEventArgs());
                #endregion
            }
        }

        private void cmbMainUpdateRate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            {
                if (cmbMainUpdateRate.SelectedIndex == -1)
                {
                    autoUpdateOn = true;  //xxx
                    MainTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000);  // 1000 ms
                }
                else if (cmbMainUpdateRate.SelectedIndex == 0)
                {
                    autoUpdateOn = true;
                    //xxx autoUpdateOn = false;
                    MainTimer.Interval = new TimeSpan(0, 0, 0, 0,
                        (int)(Convert.ToDouble(cmbMainUpdateRate.SelectedItem.ToString()) * 1000));

                }
                else
                { 
                    MainTimer.Interval = new TimeSpan(0, 0, 0, 0,
                        (int)(Convert.ToDouble(cmbMainUpdateRate.SelectedItem.ToString()) * 1000));
                    autoUpdateOn = true;
                }
            }
        } 
        
        //---Get variables/arrays/data from host---
        private void GetData()
        {
            if (varGroup[0] != null)
            {
                for (int i = 0; i < varGroup.Length; i++)
                {
                    varGroup[i].RequestBuffer();
                }
            }

            
            if (arrayGroup[0] != null)
            {
               for (int i = 0; i < arrayGroup.Length; i++)
               {
                   arrayGroup[i].RequestBuffer();
               }
            }
            
        }
       

        
         //---(After [interval] ms this event triggers)---
         //---Blink LED and update the Get Group if autoUpdateOn is enabled---
         public void MainTimer_Tick(object sender, EventArgs e)
         {             
             //toggle an LED
             //NewSetTask(ctrl who sent cmd, cmd#, item#, data)
             txtFaultFlg.Text = ".000";
             commsMngr.NewSetTask(null, 0x00, 0x00, 2);
             commsMngr.TryNewCommsTask();
             lblTicks.Content = tickCount;
             tickCount++;

             if (autoUpdateOn == true)
              {
                 GetData();
                 double doublePowerOut;
                 string toWrite;
                 double doublePowerIn;
                 double efficiency;

                 doublePowerOut        = doubleVout * doubleIout;                
                 toWrite               = doublePowerOut.ToString("#.000");
                 lblPowerOut.Content   = toWrite;

                 doublePowerIn         = doubleVin * doubleIin;
                 toWrite               = doublePowerIn.ToString("#.000");
                 lblPowerIn.Content    = toWrite;

                 efficiency            = (doublePowerOut/doublePowerIn) * 100;
                 if(efficiency < 100)
                 {
                   toWrite               = efficiency.ToString("#.000");
                   lblEfficiency.Content = toWrite;
                 }
                 else
                 { 
                   lblEfficiency.Content = "-------";
                 }
              
              }
          }




        private void txtSetPwmSwFreq_TextChanged(object sender, TextChangedEventArgs e)
        {
            Regex HasRptingDecimals = new Regex("[.][0-9]*[.]");
            Regex HasRptingNegSign = new Regex("[-][0-9.]*[-]");
            Regex HasBadNegSign = new Regex("[0-9.]+[-]");
            Regex HasInvalidChars = new Regex("[^0-9.-]");
            Regex HasInvalidChars_NoNeg = new Regex("[^0-9.]");
            Regex IsNotEmpty = new Regex("[0-9]");

            string checkText = txtSetPwmSwFreq.Text;
            txtSetPwmSwFreq.Foreground = Brushes.Crimson;

            if (HasInvalidChars.IsMatch(checkText) == false && HasBadNegSign.IsMatch(checkText) == false && 
                HasRptingNegSign.IsMatch(checkText) == false && HasRptingDecimals.IsMatch(checkText) == false && 
                IsNotEmpty.IsMatch(checkText) == true)
            {
                double test = Convert.ToDouble(txtSetPwmSwFreq.Text);
                if (test >= 20 && test < 500)
                {
                    txtSetPwmSwFreq.Foreground = Brushes.Orange;
                    //_value = test;
                }
            }

        }


        private void txtInductor_L_TextChanged(object sender, TextChangedEventArgs e)
        {
            Regex HasRptingDecimals = new Regex("[.][0-9]*[.]");
            Regex HasRptingNegSign = new Regex("[-][0-9.]*[-]");
            Regex HasBadNegSign = new Regex("[0-9.]+[-]");
            Regex HasInvalidChars = new Regex("[^0-9.-]");
            Regex HasInvalidChars_NoNeg = new Regex("[^0-9.]");
            Regex IsNotEmpty = new Regex("[0-9]");

            string checkText = txtInductor_L.Text;
            txtInductor_L.Foreground = Brushes.Crimson;

            if (HasInvalidChars.IsMatch(checkText) == false && HasBadNegSign.IsMatch(checkText) == false && 
                HasRptingNegSign.IsMatch(checkText) == false && HasRptingDecimals.IsMatch(checkText) == false && 
                IsNotEmpty.IsMatch(checkText) == true)
            {
                double test = Convert.ToDouble(txtInductor_L.Text);
                if (test >= 0.1 && test < 1024)
                {
                    txtInductor_L.Foreground = Brushes.Orange;
                    //_value = test;
                }
            }
        }

        private void txtDCR_TextChanged(object sender, TextChangedEventArgs e)
        {
            Regex HasRptingDecimals = new Regex("[.][0-9]*[.]");
            Regex HasRptingNegSign = new Regex("[-][0-9.]*[-]");
            Regex HasBadNegSign = new Regex("[0-9.]+[-]");
            Regex HasInvalidChars = new Regex("[^0-9.-]");
            Regex HasInvalidChars_NoNeg = new Regex("[^0-9.]");
            Regex IsNotEmpty = new Regex("[0-9]");

            string checkText = txtDCR.Text;
            txtDCR.Foreground = Brushes.Crimson;

            if (HasInvalidChars.IsMatch(checkText) == false && HasBadNegSign.IsMatch(checkText) == false && 
                HasRptingNegSign.IsMatch(checkText) == false && HasRptingDecimals.IsMatch(checkText) == false && 
                IsNotEmpty.IsMatch(checkText) == true)
            {
                double test = Convert.ToDouble(txtDCR.Text);
                if (test >= 0.000 && test < 3.991)
                {
                    txtDCR.Foreground = Brushes.Orange;
                    //_value = test;
                }
            }

        }

        private void txtSetVin_TextChanged(object sender, TextChangedEventArgs e)
        {
            Regex HasRptingDecimals = new Regex("[.][0-9]*[.]");
            Regex HasRptingNegSign = new Regex("[-][0-9.]*[-]");
            Regex HasBadNegSign = new Regex("[0-9.]+[-]");
            Regex HasInvalidChars = new Regex("[^0-9.-]");
            Regex HasInvalidChars_NoNeg = new Regex("[^0-9.]");
            Regex IsNotEmpty = new Regex("[0-9]");

            string checkText = txtSetVin.Text;
            //xxx txtSetVin.ForeColor = System.Drawing.Color.Crimson;
            txtSetVin.Foreground = Brushes.Crimson;

            if (HasInvalidChars.IsMatch(checkText) == false && HasBadNegSign.IsMatch(checkText) == false && 
                HasRptingNegSign.IsMatch(checkText) == false && HasRptingDecimals.IsMatch(checkText) == false && 
                IsNotEmpty.IsMatch(checkText) == true)
            {
                double test = Convert.ToDouble(txtSetVin.Text);
                if (test > 2 && test < 1024)
                {
                    txtSetVin.Foreground = Brushes.Orange;
                    //_value = test;
                }
            }

        }

        private void txtCelec_TextChanged(object sender, TextChangedEventArgs e)
        {
            Regex HasRptingDecimals = new Regex("[.][0-9]*[.]");
            Regex HasRptingNegSign = new Regex("[-][0-9.]*[-]");
            Regex HasBadNegSign = new Regex("[0-9.]+[-]");
            Regex HasInvalidChars = new Regex("[^0-9.-]");
            Regex HasInvalidChars_NoNeg = new Regex("[^0-9.]");
            Regex IsNotEmpty = new Regex("[0-9]");

            string checkText = txtCelec.Text;
            txtCelec.Foreground = Brushes.Crimson;

            if (HasInvalidChars.IsMatch(checkText) == false && HasBadNegSign.IsMatch(checkText) == false &&
                HasRptingNegSign.IsMatch(checkText) == false && HasRptingDecimals.IsMatch(checkText) == false && 
                IsNotEmpty.IsMatch(checkText) == true)
            {
                double test = Convert.ToDouble(txtCelec.Text);
                if (test > 0.01 && test < 10000)
                {
                    txtCelec.Foreground = Brushes.Orange;
                    //_value = test;
                }
            }
        }


        private void btnCntl_Calc_Click(object sender, RoutedEventArgs e)
        {
            double pwm_sw_freq;
            double vin;
            double inductor_l;
            double dcr;
            double celec;
            double esrelec;

            double ccer;
            double esrcer;
            double voutms;
            double voutcof;
            double vinms;
            double iindms;
            double ilcof;
            double rdsq1;
            double rdsq2;
            double isrcs;
            double iloadlo;
            double iloadhi;
            double iloadcc;
            double illoadms;
          
            if (commsMngr.IsOpen == true)
            {
              btnCntl_Calc.Background = Brushes.Green;
              btnCntl_Calc.Content = "Busy";
 
              if(txtSetPwmSwFreq.Foreground == Brushes.Orange)
              { 
                pwm_sw_freq = Convert.ToDouble(txtSetPwmSwFreq.Text);
                commsMngr.NewSetTask(null, 0x01, 0x00, (Int16)(pwm_sw_freq * Math.Pow(2, 0)));
                txtSetPwmSwFreq.Foreground = Brushes.Green;
              }
             
              if(txtSetVin.Foreground == Brushes.Orange)
              { 
                vin = Convert.ToDouble(txtSetVin.Text);
                commsMngr.NewSetTask(null, 0x01, 0x01, (Int16)(vin * Math.Pow(2, 6)));
                txtSetVin.Foreground = Brushes.Green;
              }
  
              if(txtInductor_L.Foreground == Brushes.Orange)
              { 
                inductor_l = Convert.ToDouble(txtInductor_L.Text);
                commsMngr.NewSetTask(null, 0x01, 0x02, (UInt16)(inductor_l * Math.Pow(2, 6)));
                txtInductor_L.Foreground = Brushes.Green;
              }
  
              if(txtDCR.Foreground == Brushes.Orange)
              { 
                dcr = Convert.ToDouble(txtDCR.Text);
                commsMngr.NewSetTask(null, 0x01, 0x03, (UInt16)(dcr * Math.Pow(2, 14)));
                txtDCR.Foreground = Brushes.Green;
              }

              if(txtCelec.Foreground == Brushes.Orange)
              { 
                celec = Convert.ToDouble(txtCelec.Text);
                commsMngr.NewSetTask(null, 0x01, 0x04, (UInt16)(celec * Math.Pow(2, 0)));
                txtCelec.Foreground = Brushes.Green;
              }

              if(txtESRelec.Foreground == Brushes.Orange)
              { 
                esrelec = Convert.ToDouble(txtESRelec.Text);
                commsMngr.NewSetTask(null, 0x01, 0x05, (UInt16)(esrelec * Math.Pow(2, 15)));
                txtESRelec.Foreground = Brushes.Green;
              }

              if(txtCcer.Foreground == Brushes.Orange)
              { 
                ccer    = Convert.ToDouble(txtCcer.Text);
                commsMngr.NewSetTask(null, 0x01, 0x06, (UInt16)(ccer * Math.Pow(2, 0)));
                txtCcer.Foreground = Brushes.Green;
              }

              if(txtESRcer.Foreground == Brushes.Orange)
              { 
                esrcer  = Convert.ToDouble(txtESRcer.Text);
                commsMngr.NewSetTask(null, 0x01, 0x07, (UInt16)(esrcer * Math.Pow(2, 15)));
                txtESRcer.Foreground = Brushes.Green;
              }

              if(txtVoutMS.Foreground == Brushes.Orange)
              { 
                voutms = Convert.ToDouble(txtVoutMS.Text);
                commsMngr.NewSetTask(null, 0x01, 0x08, (UInt16)(voutms * Math.Pow(2, 6)));
                txtVoutMS.Foreground = Brushes.Green;
              }

              if(txtVoutCOF.Foreground == Brushes.Orange)
              { 
                voutcof = Convert.ToDouble(txtVoutCOF.Text);
                commsMngr.NewSetTask(null, 0x01, 0x09, (UInt16)(voutcof * Math.Pow(2, 08)));
                txtVoutCOF.Foreground = Brushes.Green;
              }

              if(txtVinMS.Foreground == Brushes.Orange)
              { 
                vinms   = Convert.ToDouble(txtVinMS.Text);
                commsMngr.NewSetTask(null, 0x01, 0x0a, (UInt16)(vinms * Math.Pow(2, 6)));
                txtVinMS.Foreground = Brushes.Green;
              }

              if(txtIindMS.Foreground == Brushes.Orange)
              { 
                iindms = Convert.ToDouble(txtIindMS.Text);
                commsMngr.NewSetTask(null, 0x01, 0x0c, (UInt16)(iindms * Math.Pow(2, 6)));
                txtIindMS.Foreground = Brushes.Green;
              }

              if(txtIlCOF.Foreground == Brushes.Orange)
              { 
                ilcof = Convert.ToDouble(txtIlCOF.Text);
                commsMngr.NewSetTask(null, 0x01, 0x0d, (UInt16)(ilcof * Math.Pow(2, 10)));
                txtIlCOF.Foreground = Brushes.Green;
              }

              if(txtRdsQ1.Foreground == Brushes.Orange)
              { 
                rdsq1 = Convert.ToDouble(txtRdsQ1.Text);
                commsMngr.NewSetTask(null, 0x01, 0x0e, (UInt16)(rdsq1 * Math.Pow(2, 15)));
                txtRdsQ1.Foreground = Brushes.Green;
              }

              if(txtRdsQ2.Foreground == Brushes.Orange)
              { 
                rdsq2 = Convert.ToDouble(txtRdsQ2.Text);
                commsMngr.NewSetTask(null, 0x01, 0x0f, (UInt16)(rdsq2 * Math.Pow(2, 15)));
                txtRdsQ2.Foreground = Brushes.Green;
              }

              if(txtIsRCS.Foreground == Brushes.Orange)
              { 
                isrcs = Convert.ToDouble(txtIsRCS.Text);
                commsMngr.NewSetTask(null, 0x01, 0x10, (UInt16)(isrcs * Math.Pow(2, 14)));
                txtIsRCS.Foreground = Brushes.Green;
              }

              if(txtIloadLo.Foreground == Brushes.Orange)
              { 
                iloadlo = Convert.ToDouble(txtIloadLo.Text);
                commsMngr.NewSetTask(null, 0x01, 0x11, (UInt16)(iloadlo * Math.Pow(2, 12)));
                txtIloadLo.Foreground = Brushes.Green;
              }

              if(txtIloadHi.Foreground == Brushes.Orange)
              { 
                iloadhi = Convert.ToDouble(txtIloadHi.Text);
                commsMngr.NewSetTask(null, 0x01, 0x12, (UInt16)(iloadhi * Math.Pow(2, 12)));
                txtIloadHi.Foreground = Brushes.Green;
              }

              if(txtIloadCC.Foreground == Brushes.Orange)
              { 
                iloadcc = Convert.ToDouble(txtIloadCC.Text);
                commsMngr.NewSetTask(null, 0x01, 0x13, (UInt16)(iloadcc * Math.Pow(2, 9)));
                txtIloadCC.Foreground = Brushes.Green;
              }

              if(txtIloadMS.Foreground == Brushes.Orange)
              { 
                illoadms = Convert.ToDouble(txtIloadMS.Text);
                commsMngr.NewSetTask(null, 0x01, 0x14, (UInt16)(illoadms * Math.Pow(2, 6)));
                txtIloadMS.Foreground = Brushes.Green;
              }

              commsMngr.NewSetTask(null, 0x01, 0x0b, (Int16)(1)); //coeff_change
  

              DoEvents();
              System.Threading.Thread.Sleep(1000);
              DoEvents();
              btnCntl_Calc.Background = Brushes.LightGray;
              // xxx checkFault();
              btnCntl_Calc.Content = "Updated";
              // xxx txtFaultFlg.Text = ".000";
            }
        }

        private void txtESRelec_TextChanged(object sender, TextChangedEventArgs e)
        
        {
            Regex HasRptingDecimals = new Regex("[.][0-9]*[.]");
            Regex HasRptingNegSign = new Regex("[-][0-9.]*[-]");
            Regex HasBadNegSign = new Regex("[0-9.]+[-]");
            Regex HasInvalidChars = new Regex("[^0-9.-]");
            Regex HasInvalidChars_NoNeg = new Regex("[^0-9.]");
            Regex IsNotEmpty = new Regex("[0-9]");

            string checkText = txtESRelec.Text;
            txtESRelec.Foreground = Brushes.Crimson;

            if (HasInvalidChars.IsMatch(checkText) == false && HasBadNegSign.IsMatch(checkText) == false &&
                HasRptingNegSign.IsMatch(checkText) == false && HasRptingDecimals.IsMatch(checkText) == false &&
                IsNotEmpty.IsMatch(checkText) == true)
            {
                double test = Convert.ToDouble(txtESRelec.Text);
                if (test >= 0.010 && test < 1)
                {
                    txtESRelec.Foreground = Brushes.Orange;
                    //_value = test;
                }
            }
        }

        private void txtModelFreq_TextChanged(object sender, TextChangedEventArgs e)
        {
          double   doublenum;
          uint     uintnum, uintnumMod;
          doublenum = Convert.ToDouble(txtModelFreq.Text);
          uintnum   = (uint)doublenum;
          uintnumMod   = 0x00FF & uintnum;
          lblModelFreq.Content = uintnumMod.ToString();
          lblModel.Foreground = Brushes.Green;
          if((uintnum - uintnumMod) > 0)
          {
            lblModel.Content = "BUCK ";
            txtVoutMS.Text   = "6.09";
            txtVoutMS.Foreground = Brushes.Orange;
            txtIindMS.Text   = "6.83";
            txtIindMS.Foreground = Brushes.Orange;
            txtSetVin.Text = "9.0";
            txtSetVin.Foreground = Brushes.Orange;
            txtIloadMS.Text   = "6.83";
            txtIloadMS.Foreground = Brushes.Orange;
            txtVinMS.Text   = "12.09";
            txtVinMS.Foreground = Brushes.Orange;
            txtIsRCS.Text   = "0.03";
            txtIsRCS.Foreground = Brushes.Orange;
          }
          else
          {
            lblModel.Content = "BOOST";
            txtVoutMS.Text   = "85.27";
            txtVoutMS.Foreground = Brushes.Orange;
            txtIindMS.Text   = "68.00";
            txtIindMS.Foreground = Brushes.Orange;
            txtSetVin.Text = "15.0";
            txtSetVin.Foreground = Brushes.Orange;
            txtIloadMS.Text   = "20.00";
            txtIloadMS.Foreground = Brushes.Orange;
            txtVinMS.Text   = "18.0";
            txtVinMS.Foreground = Brushes.Orange;
            txtIsRCS.Text   = "0.006";
            txtIsRCS.Foreground = Brushes.Orange;
          }
        }

        private void txtCcer_TextChanged(object sender, TextChangedEventArgs e)
        {
            Regex HasRptingDecimals = new Regex("[.][0-9]*[.]");
            Regex HasRptingNegSign = new Regex("[-][0-9.]*[-]");
            Regex HasBadNegSign = new Regex("[0-9.]+[-]");
            Regex HasInvalidChars = new Regex("[^0-9.-]");
            Regex HasInvalidChars_NoNeg = new Regex("[^0-9.]");
            Regex IsNotEmpty = new Regex("[0-9]");

            string checkText = txtCcer.Text;
            txtCcer.Foreground = Brushes.Crimson;

            if (HasInvalidChars.IsMatch(checkText) == false && HasBadNegSign.IsMatch(checkText) == false &&
                HasRptingNegSign.IsMatch(checkText) == false && HasRptingDecimals.IsMatch(checkText) == false &&
                IsNotEmpty.IsMatch(checkText) == true)
            {
                double test = Convert.ToDouble(txtCcer.Text);
                if (test > 1 && test < 64000)
                {
                    txtCcer.Foreground = Brushes.Orange;
                    //_value = test;
                }
            }

        }

        private void txtESRcer_TextChanged(object sender, TextChangedEventArgs e)
        {
            Regex HasRptingDecimals = new Regex("[.][0-9]*[.]");
            Regex HasRptingNegSign = new Regex("[-][0-9.]*[-]");
            Regex HasBadNegSign = new Regex("[0-9.]+[-]");
            Regex HasInvalidChars = new Regex("[^0-9.-]");
            Regex HasInvalidChars_NoNeg = new Regex("[^0-9.]");
            Regex IsNotEmpty = new Regex("[0-9]");

            string checkText = txtESRcer.Text;
            txtESRcer.Foreground = Brushes.Crimson;

            if (HasInvalidChars.IsMatch(checkText) == false && HasBadNegSign.IsMatch(checkText) == false &&
                HasRptingNegSign.IsMatch(checkText) == false && HasRptingDecimals.IsMatch(checkText) == false &&
                IsNotEmpty.IsMatch(checkText) == true)
            {
                double test = Convert.ToDouble(txtESRcer.Text);
                if (test >= 0.002 && test < 1)
                {
                    txtESRcer.Foreground = Brushes.Orange;
                    //_value = test;
                }
            }

        }

        private void txtVoutMS_TextChanged(object sender, TextChangedEventArgs e)
        {
            Regex HasRptingDecimals = new Regex("[.][0-9]*[.]");
            Regex HasRptingNegSign = new Regex("[-][0-9.]*[-]");
            Regex HasBadNegSign = new Regex("[0-9.]+[-]");
            Regex HasInvalidChars = new Regex("[^0-9.-]");
            Regex HasInvalidChars_NoNeg = new Regex("[^0-9.]");
            Regex IsNotEmpty = new Regex("[0-9]");

            string checkText = txtVoutMS.Text;
            txtVoutMS.Foreground = Brushes.Crimson;

            if (HasInvalidChars.IsMatch(checkText) == false && HasBadNegSign.IsMatch(checkText) == false &&
                HasRptingNegSign.IsMatch(checkText) == false && HasRptingDecimals.IsMatch(checkText) == false &&
                IsNotEmpty.IsMatch(checkText) == true)
            {
                double test = Convert.ToDouble(txtVoutMS.Text);
                if (test >= 3 && test < 1024)  
                {
                    txtVoutMS.Foreground = Brushes.Orange;
                    //_value = test;
                }
            }

        }

        private void txtVoutCOF_TextChanged(object sender, TextChangedEventArgs e)
        {
            Regex HasRptingDecimals = new Regex("[.][0-9]*[.]");
            Regex HasRptingNegSign = new Regex("[-][0-9.]*[-]");
            Regex HasBadNegSign = new Regex("[0-9.]+[-]");
            Regex HasInvalidChars = new Regex("[^0-9.-]");
            Regex HasInvalidChars_NoNeg = new Regex("[^0-9.]");
            Regex IsNotEmpty = new Regex("[0-9]");

            string checkText = txtVoutCOF.Text;
            txtVoutCOF.Foreground = Brushes.Crimson;

            if (HasInvalidChars.IsMatch(checkText) == false && HasBadNegSign.IsMatch(checkText) == false &&
                HasRptingNegSign.IsMatch(checkText) == false && HasRptingDecimals.IsMatch(checkText) == false &&
                IsNotEmpty.IsMatch(checkText) == true)
            {
                double test = Convert.ToDouble(txtVoutCOF.Text);
                if (test >= 2 && test < 256)
                {
                    txtVoutCOF.Foreground = Brushes.Orange;
                    //_value = test;
                }
            }

        }

        private void txtVinMS_TextChanged(object sender, TextChangedEventArgs e)
        {
            Regex HasRptingDecimals = new Regex("[.][0-9]*[.]");
            Regex HasRptingNegSign = new Regex("[-][0-9.]*[-]");
            Regex HasBadNegSign = new Regex("[0-9.]+[-]");
            Regex HasInvalidChars = new Regex("[^0-9.-]");
            Regex HasInvalidChars_NoNeg = new Regex("[^0-9.]");
            Regex IsNotEmpty = new Regex("[0-9]");

            string checkText = txtVinMS.Text;
            txtVinMS.Foreground = Brushes.Crimson;

            if (HasInvalidChars.IsMatch(checkText) == false && HasBadNegSign.IsMatch(checkText) == false &&
                HasRptingNegSign.IsMatch(checkText) == false && HasRptingDecimals.IsMatch(checkText) == false &&
                IsNotEmpty.IsMatch(checkText) == true)
            {
                double test = Convert.ToDouble(txtVinMS.Text);
                if (test >= 4 && test <= 1023)
                {
                    txtVinMS.Foreground = Brushes.Orange;
                    //_value = test;
                }
            }

        }

        private void txtIindMS_TextChanged(object sender, TextChangedEventArgs e)
        {
            Regex HasRptingDecimals = new Regex("[.][0-9]*[.]");
            Regex HasRptingNegSign = new Regex("[-][0-9.]*[-]");
            Regex HasBadNegSign = new Regex("[0-9.]+[-]");
            Regex HasInvalidChars = new Regex("[^0-9.-]");
            Regex HasInvalidChars_NoNeg = new Regex("[^0-9.]");
            Regex IsNotEmpty = new Regex("[0-9]");

            string checkText = txtIindMS.Text;
            txtIindMS.Foreground = Brushes.Crimson;

            if (HasInvalidChars.IsMatch(checkText) == false && HasBadNegSign.IsMatch(checkText) == false &&
                HasRptingNegSign.IsMatch(checkText) == false && HasRptingDecimals.IsMatch(checkText) == false &&
                IsNotEmpty.IsMatch(checkText) == true)
            {
                double test = Convert.ToDouble(txtIindMS.Text);
                if (test >= 4 && test < 1024)
                {
                    txtIindMS.Foreground = Brushes.Orange;
                    //_value = test;
                }
            }

        }

        private void txtIlCOF_TextChanged(object sender, TextChangedEventArgs e)
        {
            Regex HasRptingDecimals = new Regex("[.][0-9]*[.]");
            Regex HasRptingNegSign = new Regex("[-][0-9.]*[-]");
            Regex HasBadNegSign = new Regex("[0-9.]+[-]");
            Regex HasInvalidChars = new Regex("[^0-9.-]");
            Regex HasInvalidChars_NoNeg = new Regex("[^0-9.]");
            Regex IsNotEmpty = new Regex("[0-9]");

            string checkText = txtIlCOF.Text;
            txtIlCOF.Foreground = Brushes.Crimson;

            if (HasInvalidChars.IsMatch(checkText) == false && HasBadNegSign.IsMatch(checkText) == false &&
                HasRptingNegSign.IsMatch(checkText) == false && HasRptingDecimals.IsMatch(checkText) == false &&
                IsNotEmpty.IsMatch(checkText) == true)
            {
                double test = Convert.ToDouble(txtIlCOF.Text);
                if (test >= 2 && test < 64)
                {
                    txtIlCOF.Foreground = Brushes.Orange;
                    //_value = test;
                }
            }

        }

        private void txtRdsQ1_TextChanged(object sender, TextChangedEventArgs e)
        {
            Regex HasRptingDecimals = new Regex("[.][0-9]*[.]");
            Regex HasRptingNegSign = new Regex("[-][0-9.]*[-]");
            Regex HasBadNegSign = new Regex("[0-9.]+[-]");
            Regex HasInvalidChars = new Regex("[^0-9.-]");
            Regex HasInvalidChars_NoNeg = new Regex("[^0-9.]");
            Regex IsNotEmpty = new Regex("[0-9]");

            string checkText = txtRdsQ1.Text;
            txtRdsQ1.Foreground = Brushes.Crimson;

            if (HasInvalidChars.IsMatch(checkText) == false && HasBadNegSign.IsMatch(checkText) == false &&
                HasRptingNegSign.IsMatch(checkText) == false && HasRptingDecimals.IsMatch(checkText) == false &&
                IsNotEmpty.IsMatch(checkText) == true)
            {
                double test = Convert.ToDouble(txtRdsQ1.Text);
                if (test >= 0.000 && test < 1)
                {
                    txtRdsQ1.Foreground = Brushes.Orange;
                    //_value = test;
                }
            }

        }

        private void txtRdsQ2_TextChanged(object sender, TextChangedEventArgs e)
        {
            Regex HasRptingDecimals = new Regex("[.][0-9]*[.]");
            Regex HasRptingNegSign = new Regex("[-][0-9.]*[-]");
            Regex HasBadNegSign = new Regex("[0-9.]+[-]");
            Regex HasInvalidChars = new Regex("[^0-9.-]");
            Regex HasInvalidChars_NoNeg = new Regex("[^0-9.]");
            Regex IsNotEmpty = new Regex("[0-9]");

            string checkText = txtRdsQ2.Text;
            txtRdsQ2.Foreground = Brushes.Crimson;

            if (HasInvalidChars.IsMatch(checkText) == false && HasBadNegSign.IsMatch(checkText) == false &&
                HasRptingNegSign.IsMatch(checkText) == false && HasRptingDecimals.IsMatch(checkText) == false &&
                IsNotEmpty.IsMatch(checkText) == true)
            {
                double test = Convert.ToDouble(txtRdsQ2.Text);
                if (test >= 0.000 && test < 1)
                {
                    txtRdsQ2.Foreground = Brushes.Orange;
                    //_value = test;
                }
            }

        }

        private void txtIsRCS_TextChanged(object sender, TextChangedEventArgs e)
        {
            Regex HasRptingDecimals = new Regex("[.][0-9]*[.]");
            Regex HasRptingNegSign = new Regex("[-][0-9.]*[-]");
            Regex HasBadNegSign = new Regex("[0-9.]+[-]");
            Regex HasInvalidChars = new Regex("[^0-9.-]");
            Regex HasInvalidChars_NoNeg = new Regex("[^0-9.]");
            Regex IsNotEmpty = new Regex("[0-9]");

            string checkText = txtIsRCS.Text;
            txtIsRCS.Foreground = Brushes.Crimson;
            // xxx txtFaultFlg.Text = "0000";

            if (HasInvalidChars.IsMatch(checkText) == false && HasBadNegSign.IsMatch(checkText) == false &&
                HasRptingNegSign.IsMatch(checkText) == false && HasRptingDecimals.IsMatch(checkText) == false &&
                IsNotEmpty.IsMatch(checkText) == true)
            {
                double test = Convert.ToDouble(txtIsRCS.Text);
                if (test >= 0.000 && test < 3.991)
                {
                    txtIsRCS.Foreground = Brushes.Orange;
                    //_value = test;
                }
            }

        }

        private void txtIloadLo_TextChanged(object sender, TextChangedEventArgs e)
        {
            Regex HasRptingDecimals = new Regex("[.][0-9]*[.]");
            Regex HasRptingNegSign = new Regex("[-][0-9.]*[-]");
            Regex HasBadNegSign = new Regex("[0-9.]+[-]");
            Regex HasInvalidChars = new Regex("[^0-9.-]");
            Regex HasInvalidChars_NoNeg = new Regex("[^0-9.]");
            Regex IsNotEmpty = new Regex("[0-9]");

            string checkText = txtIloadLo.Text;
            txtIloadLo.Foreground = Brushes.Crimson;

            if (HasInvalidChars.IsMatch(checkText) == false && HasBadNegSign.IsMatch(checkText) == false &&
                HasRptingNegSign.IsMatch(checkText) == false && HasRptingDecimals.IsMatch(checkText) == false &&
                IsNotEmpty.IsMatch(checkText) == true)
            {
                double test = Convert.ToDouble(txtIloadLo.Text);
                if (test >= 0.002 && test < 16)
                {
                    txtIloadLo.Foreground = Brushes.Orange;
                    //_value = test;
                }
            }
        }

        private void txtIloadHi_TextChanged(object sender, TextChangedEventArgs e)
        {
            Regex HasRptingDecimals = new Regex("[.][0-9]*[.]");
            Regex HasRptingNegSign = new Regex("[-][0-9.]*[-]");
            Regex HasBadNegSign = new Regex("[0-9.]+[-]");
            Regex HasInvalidChars = new Regex("[^0-9.-]");
            Regex HasInvalidChars_NoNeg = new Regex("[^0-9.]");
            Regex IsNotEmpty = new Regex("[0-9]");

            string checkText = txtIloadHi.Text;
            txtIloadHi.Foreground = Brushes.Crimson;

            if (HasInvalidChars.IsMatch(checkText) == false && HasBadNegSign.IsMatch(checkText) == false &&
                HasRptingNegSign.IsMatch(checkText) == false && HasRptingDecimals.IsMatch(checkText) == false &&
                IsNotEmpty.IsMatch(checkText) == true)
            {
                double test = Convert.ToDouble(txtIloadHi.Text);
                if (test >= 0.002 && test < 16)
                {
                    txtIloadHi.Foreground = Brushes.Orange;
                    //_value = test;
                }
            }
        }

        private void txtIloadCC_TextChanged(object sender, TextChangedEventArgs e)
        {
            Regex HasRptingDecimals = new Regex("[.][0-9]*[.]");
            Regex HasRptingNegSign = new Regex("[-][0-9.]*[-]");
            Regex HasBadNegSign = new Regex("[0-9.]+[-]");
            Regex HasInvalidChars = new Regex("[^0-9.-]");
            Regex HasInvalidChars_NoNeg = new Regex("[^0-9.]");
            Regex IsNotEmpty = new Regex("[0-9]");

            string checkText = txtIloadCC.Text;
            txtIloadCC.Foreground = Brushes.Crimson;

            if (HasInvalidChars.IsMatch(checkText) == false && HasBadNegSign.IsMatch(checkText) == false &&
                HasRptingNegSign.IsMatch(checkText) == false && HasRptingDecimals.IsMatch(checkText) == false &&
                IsNotEmpty.IsMatch(checkText) == true)
            {
                double test = Convert.ToDouble(txtIloadCC.Text);
                if (test >= 0.000 && test < 128)
                {
                    txtIloadCC.Foreground = Brushes.Orange;
                    //_value = test;
                }
            }
        }

        public void colorTextOrange()
        {
          txtSetPwmSwFreq.Foreground = Brushes.Orange;
          txtSetVin.Foreground = Brushes.Orange;
          txtInductor_L.Foreground = Brushes.Orange;
          txtDCR.Foreground = Brushes.Orange;
          txtCelec.Foreground = Brushes.Orange;
          txtESRelec.Foreground = Brushes.Orange;
          txtCcer.Foreground = Brushes.Orange;
          txtESRcer.Foreground = Brushes.Orange;
          txtVoutMS.Foreground = Brushes.Orange;
          txtVoutCOF.Foreground = Brushes.Orange;
          txtVinMS.Foreground = Brushes.Orange;
          txtIindMS.Foreground = Brushes.Orange;
          txtIlCOF.Foreground = Brushes.Orange;
          txtRdsQ1.Foreground = Brushes.Orange;
          txtRdsQ2.Foreground = Brushes.Orange;
          txtIsRCS.Foreground = Brushes.Orange;
          txtIloadLo.Foreground = Brushes.Orange;
          txtIloadHi.Foreground = Brushes.Orange;
          txtIloadCC.Foreground = Brushes.Orange;
          txtIloadMS.Foreground = Brushes.Orange;
        }

        private void txtIloadMS_TextChanged(object sender, TextChangedEventArgs e)
        {
            Regex HasRptingDecimals = new Regex("[.][0-9]*[.]");
            Regex HasRptingNegSign = new Regex("[-][0-9.]*[-]");
            Regex HasBadNegSign = new Regex("[0-9.]+[-]");
            Regex HasInvalidChars = new Regex("[^0-9.-]");
            Regex HasInvalidChars_NoNeg = new Regex("[^0-9.]");
            Regex IsNotEmpty = new Regex("[0-9]");

            string checkText = txtIloadMS.Text;
            txtIloadMS.Foreground = Brushes.Crimson;

            if (HasInvalidChars.IsMatch(checkText) == false && HasBadNegSign.IsMatch(checkText) == false &&
                HasRptingNegSign.IsMatch(checkText) == false && HasRptingDecimals.IsMatch(checkText) == false &&
                IsNotEmpty.IsMatch(checkText) == true)
            {
                double test = Convert.ToDouble(txtIloadMS.Text);
                if (test >= 0.002 && test < 1024)
                {
                    txtIloadMS.Foreground = Brushes.Orange;
                    //_value = test;
                }
            }
        }

        private void txtFaultFlg_TextChanged(object sender, TextChangedEventArgs e)
        {
          faultnum = Convert.ToDouble(txtFaultFlg.Text);
          checkFault();
        }


        private void txtVout_TextChanged(object sender, TextChangedEventArgs e)
        {
            doubleVout          = Convert.ToDouble(txtVout.Text);
            // doublePowerOut      = doubleVout * doubleIout;
            // string toWrite      = doublePowerOut.ToString("#.000");
            // lblPowerOut.Content = toWrite;

            // double efficiency = (doublePowerOut / doublePowerIn) * 100;
            // toWrite = efficiency.ToString("#.000");
            // lblEfficiency.Content = toWrite;

        }

        private void txtIout_TextChanged(object sender, TextChangedEventArgs e)
        {
            doubleIout          = Convert.ToDouble(txtIout.Text);
            // doublePowerOut      = doubleVout * doubleIout;
            // string toWrite      = doublePowerOut.ToString("#.000");
            // lblPowerOut.Content = toWrite;

            // double efficiency = (doublePowerOut / doublePowerIn) * 100;
            // toWrite = efficiency.ToString("#.000");
            // lblEfficiency.Content = toWrite;

        }

        private void txtVin_TextChanged(object sender, TextChangedEventArgs e)
        {
            doubleVin          = Convert.ToDouble(txtVin.Text);
            // doublePowerIn      = doubleVin * doubleIin;
            // string toWrite     = doublePowerIn.ToString("#.000");
            // lblPowerIn.Content = toWrite;

            // double efficiency     = (doublePowerOut/doublePowerIn) * 100;
            // toWrite               = efficiency.ToString("#.000");
            // lblEfficiency.Content = toWrite;
 
        }

        private void txtIpri_TextChanged(object sender, TextChangedEventArgs e)
        {
            doubleIin          = Convert.ToDouble(txtIpri.Text);
            // doublePowerIn      = doubleVin * doubleIin;
            // string toWrite     = doublePowerIn.ToString("#.000");
            // lblPowerIn.Content = toWrite;

            // double efficiency     = (doublePowerOut/doublePowerIn) * 100;
            // toWrite               = efficiency.ToString("#.000");
            // lblEfficiency.Content = toWrite;

        }

        public void checkFault()
        {
          uint     faultenum;
          uint     checkrange;

          checkrange = (uint)faultnum;
          faultenum  = (uint)(Math.Round((faultnum - (float)checkrange) * Math.Pow(2, 6)));

          if(faultnum > 0)
          {
            txtFaultFlg.Foreground = Brushes.Red;

            switch(faultenum)
            {
              case 8:
              txtSetPwmSwFreq.Foreground = Brushes.Orange;
              break;

              case 10:
              txtSetVin.Foreground = Brushes.Orange;
              break;

              case 11:
              txtInductor_L.Foreground = Brushes.Orange;
              break;

              case 12:
              txtDCR.Foreground = Brushes.Orange;
              break;

              case 6:
              txtCelec.Foreground = Brushes.Orange;
              break;

              case 0:
              txtESRelec.Foreground = Brushes.Orange;
              break;

              case 7:
              txtCcer.Foreground = Brushes.Orange;
              break;

              case 1:
              txtESRcer.Foreground = Brushes.Orange;
              break;

              case 13:
              txtVoutMS.Foreground = Brushes.Orange;
              break;

              case 14:
              txtVoutCOF.Foreground = Brushes.Orange;
              break;

              case 15:
              txtVinMS.Foreground = Brushes.Orange;
              break;

              // case b:
              // coeff_change
              // break;

              case 17:
              txtIindMS.Foreground = Brushes.Orange;
              break;

              case 18:
              txtIlCOF.Foreground = Brushes.Orange;
              break;

              case 19:
              txtRdsQ1.Foreground = Brushes.Orange;
              break;

              case 20:
              txtRdsQ2.Foreground = Brushes.Orange;
              break;
                        
              case 22:
              txtIsRCS.Foreground = Brushes.Orange;
              break;

              case 23:
              txtIloadLo.Foreground = Brushes.Orange;
              break;

              case 24:
              txtIloadHi.Foreground = Brushes.Orange;
              break;

              case 25:
              txtIloadCC.Foreground = Brushes.Orange;
              break;

              case 26:
              txtIloadMS.Foreground = Brushes.Orange;
              break;

              default:
              break;

            }

          }
          else
          {
            txtFaultFlg.Foreground = Brushes.Black;
          }
        }

        private void txtDutyCycle_TextChanged(object sender, TextChangedEventArgs e)
        {
          
        }
        //-----------------------------------------------------------
    }
}

