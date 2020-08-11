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
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Windows.Threading;   // added to override DoEvents 

using System.IO.Ports;
using System.IO;


namespace GUI_Buck_Boost_Model_Wpf
{
    /// <summary>
    /// Interaction logic for Setup_Connection.xaml
    /// </summary>


    public partial class Setup_Connection : Window
    {
 
        public object ExitFrame(object f)
        {
            ((DispatcherFrame)f).Continue = false;
            return null;
        }

        System.Collections.Specialized.StringCollection comportEchoWhenOff;

        int findComportState = 0;
        public int numPorts = 0;
        wpfCommsManager commsMngr;
        MainWindow parentForm;

        public Setup_Connection(MainWindow parent, wpfCommsManager comMngr)
        {
            InitializeComponent();

            comportEchoWhenOff = new System.Collections.Specialized.StringCollection();
            commsMngr = comMngr;  // needed to get visibility to other methods
            parentForm = parent;

            
            txtBaudRate.Text = comMngr.BaudRate.ToString(); //zzz
            cmbComms.Items.Clear();                         //zzz
            numPorts = 0;                                   //zzz


            getAllPorts();
            cmbComms.SelectedIndex = 0;      // default to 1st item in selection list
       }

        private void txtBaudRate_TextChanged(object sender, TextChangedEventArgs e)
        {
            Regex IsNotEmpty = new Regex("[0-9]");
            Regex HasInvalidChars = new Regex("[^0-9]");

            if (IsNotEmpty.IsMatch(txtBaudRate.Text) == true && 
                HasInvalidChars.IsMatch(txtBaudRate.Text) == false)
            {
                txtBaudRate.Foreground = Brushes.Green;
                txtBaudRate.Background = Brushes.White;

            }
            else
            {
                txtBaudRate.Foreground = Brushes.White;
                txtBaudRate.Background = Brushes.Red;
            }
        }

        private void cmbComms_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void btnRefreshComports_Click(object sender, RoutedEventArgs e)
        {
            getAllPorts();
        }

        public void getAllPorts()
        {
            cmbComms.Items.Clear();
            numPorts = 0;
            foreach (string s in System.IO.Ports.SerialPort.GetPortNames())
            {
                cmbComms.Items.Add(s);
                if (s == commsMngr.PortName)
                {
                    cmbComms.SelectedItem = s;
                }
                numPorts++;
            }

            if (numPorts < 1)
            {
                cmbComms.Items.Add("NoOpenPorts");
            }
        }

        private void btnFindComport_Click(object sender, RoutedEventArgs e)
        {
            switch (findComportState)
            {
                case 0:
                    btnFindComport.Content = "Continue";
                    findComportState++;
                    //xx btnRefreshComports_Click(this, new EventArgs());
                    lblStatus.Content = "Please Turn EVM Board Off then Press \"Continue\"";
                    comportEchoWhenOff.Clear();
                    break;

                case 1:
                    lblStatus.Content = "Testing...";
                    btnFindComport.IsEnabled = false;
                    // The Application DoEvents method has been deprecated in WPF in favor
                    // of using Dispatcher or a Background Worker Thread
                    parentForm.DoEvents();
                    string tempPortName = commsMngr.PortName;
                    int tempBaudRate = commsMngr.BaudRate;
                    for (int i = 0; i < cmbComms.Items.Count; i++)
                    {
                        commsMngr.Close();

                        commsMngr.PortName = cmbComms.Items[i].ToString();
                        commsMngr.BaudRate = Convert.ToInt32(txtBaudRate.Text);

                        lblStatus.Content = "Trying Communications on: " + cmbComms.Items[i].ToString();
                        parentForm.DoEvents();

                        bool openSucceeded = commsMngr.Open();

                        if (openSucceeded == true)
                        {
                             commsMngr.WriteString("A");
                             byte readByte = commsMngr.ReadByte();

                             if (readByte != 0)
                            {
                                comportEchoWhenOff.Add(commsMngr.PortName);
                            }
                            commsMngr.Close();
                        }
                    }

                    commsMngr.PortName = tempPortName;
                    commsMngr.BaudRate = tempBaudRate;
                    findComportState++;
                    btnFindComport.IsEnabled = true;
                    lblStatus.Content = "Please Turn EVM Board On then Press \"Continue\"";

                    break;

                case 2:
                    lblStatus.Content = "Testing...";
                    btnFindComport.IsEnabled = false;
                    parentForm.DoEvents();
                    tempPortName = commsMngr.PortName;
                    tempBaudRate = commsMngr.BaudRate;
                    System.Collections.Specialized.StringCollection comportProper = new System.Collections.Specialized.StringCollection();

                    for (int i = 0; i < cmbComms.Items.Count; i++)
                    {


                        commsMngr.PortName = cmbComms.Items[i].ToString();
                        commsMngr.BaudRate = Convert.ToInt32(txtBaudRate.Text);

                        lblStatus.Content = "Trying Communications on: " + cmbComms.Items[i].ToString();
                        parentForm.DoEvents();

                        bool openSucceeded = false;
                        openSucceeded = commsMngr.Open();

                        if (openSucceeded == true)
                        {
                            commsMngr.WriteString("A");
                            byte readByte = commsMngr.ReadByte();

                            if (readByte != 0)
                            {
                                if (comportEchoWhenOff.Contains(commsMngr.PortName) == false)
                                {
                                    comportProper.Add(commsMngr.PortName);
                                }
                            }
                            commsMngr.Close();
                        }
                    }

                    if (comportProper.Count != 1)
                    {
                        lblStatus.Content = "Please Ensure SCI Boot Jumper is Placed or that the Target is Flashed Then Try Again";
                    }
                    else
                    {
                        parentForm.DoEvents();
                        cmbComms.SelectedItem = comportProper[0];
                        lblStatus.Content = "Comport found: " + comportProper[0];
                        parentForm.DoEvents();
                    }

                    commsMngr.PortName = tempPortName;
                    commsMngr.BaudRate = tempBaudRate;
                    findComportState = 0;
                    btnFindComport.IsEnabled = true;
                    btnFindComport.Content = "Find Comport";
                    break;
            }

        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Regex IsNotEmpty = new Regex("[0-9]");
            Regex HasInvalidChars = new Regex("[^0-9]");

            if (IsNotEmpty.IsMatch(txtBaudRate.Text) == true && HasInvalidChars.IsMatch(txtBaudRate.Text) == false)
            {
                //xxx txtBaudRate.ForeColor = SystemColors.ControlText;
                if (cmbComms.SelectedIndex != -1)
                {
                    Hide();
                    commsMngr.BaudRate = Convert.ToInt32(txtBaudRate.Text);
                    commsMngr.PortName = cmbComms.SelectedItem.ToString();
                }
                else
                {
                    cmbComms.Foreground = Brushes.White;
                    cmbComms.Background = Brushes.Red;
                    //xxx cmbComms.ForeColor = System.Drawing.Color.Red;
                }
            }
            //lblStatus.Text = "";
            //xxx Properties.FileLocationSettings.Default.Save();
            //xxx Properties.Settings.Default.Save();

            //parentForm.btnConnect_Click(this, new EventArgs());
            //cmbComms.SelectedIndex = cmbComms.Items.IndexOf(commsMngr.PortName);          
        }

        public void SetDefault()
        {
            commsMngr.BaudRate = Convert.ToInt32(txtBaudRate.Text);
            commsMngr.PortName = cmbComms.SelectedItem.ToString();
        }

        //public override void Refresh()
        //{
        //    findComportState = 0;
        //    lblStatus.Content = "";
        //    btnFindComport.Content = "Find Comport";
        //}

    }
}
