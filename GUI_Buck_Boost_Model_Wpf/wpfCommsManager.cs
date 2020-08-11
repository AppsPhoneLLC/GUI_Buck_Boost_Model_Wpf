using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace GUI_Buck_Boost_Model_Wpf
{
    public class wpfCommsManager
    {

        private int[] echobyte = new int[4];
        private System.IO.Ports.SerialPort comPort;

        public bool comValid;
        public bool isReceiving;
        private int[] dataRxedBuffer;
        public bool dataRxedBufferFilled;

        public byte[] cmdNum;
        public byte[] itemNum;
        public int[] data;
        public int[] data2;
        public int[] amountToGet;
        public Object[] ctrl;

        private int _ptrWorkingAt;
        public int ptrWorkingAt
        {
            get
            { return _ptrWorkingAt; }
            set
            {
                if (value > 63) _ptrWorkingAt = 0;
                else _ptrWorkingAt = value;
            }
        }

        private int _ptrWriteAt;
        public int ptrWriteAt
        {
            get
            { return _ptrWriteAt; }
            set
            {
                if (value > 63) _ptrWriteAt = 0;
                else _ptrWriteAt = value;
            }
        }

        //xxx private public MainWindow mainForm;
        public MainWindow mainForm;
        private Thread RunCommsThread;
        private Thread TryNewCommsThread;

        #region Constructors

        //xxx public wpfCommsManager(MainWindow parentForm)
        public wpfCommsManager()

        {
            cmdNum = new byte[64];
            itemNum = new byte[64];
            data = new int[64];
            data2 = new int[64];
            amountToGet = new int[64];
            ctrl = new Object[64];
            isReceiving = false;
            //comPort = commsPort;
            _ptrWorkingAt = 0;
            _ptrWriteAt = 0;
            //xxx mainForm = parentForm;

            comPort = new System.IO.Ports.SerialPort();
            comPort.BaudRate = Convert.ToInt32(115200);
            comPort.ReceivedBytesThreshold = 600;
            comPort.ReadTimeout = 1000;
            comPort.WriteTimeout = 1000;
            comPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(commsPort_DataReceived);
            RunCommsThread = new Thread(new ThreadStart(RunCommsTask));
            TryNewCommsThread = new Thread(new ThreadStart(TryNewCommsTask));
            dataRxedBufferFilled = false;


        }
        #endregion

        //---Close the comport
        public void Close()
        {
            if (comPort.IsOpen == true)
            {
                comPort.Close();
            }
        }

        public bool IsOpen
        {
            get { return comPort.IsOpen; }
        }

        public string PortName
        {
            get { return comPort.PortName; }
            set { comPort.PortName = value; }
        }

        public int BaudRate
        {
            get { return comPort.BaudRate; }
            set { comPort.BaudRate = value; }
        }

        //---Try to open the comport.  Comport can fail if it is currently in use elsewhere
        public bool Open()
        {
            try
            {
                if (comPort.IsOpen == false)
                {
                    comPort.Open();
                    return true;
                }
                return false;
            }
            catch (Exception err)
            {
                return false;
            }
        }

        //---allow a different form to write data through the serialport
        public byte[] WriteBytes(byte[] text)
        {
            byte[] k = new byte[256];

            byte[] bytebuffer = new byte[text.Length];
            bytebuffer = text;

            for (int i = 0; i < bytebuffer.Length; i++)
            {
                comPort.Write(bytebuffer, i, 1);
                k[i] = ReadByte();
                if (text[i] != k[i])
                {
                    return null;
                }
            }
            return k;
        }

        //---allow a different form to read data through the serialport
        public byte ReadByte()
        {
            byte temp = 0;
            try
            {
                temp = (byte)comPort.ReadByte();
            }
            catch (Exception e)
            {
            }
            return temp;
        }

        //---allow a different form to write data through the serialport
        public void WriteString(string text)
        {
            char[] temp = new char[1];
            temp = (text.ToCharArray(0, 1));
            comPort.Write(temp, 0, 1);
        }

        //---Check if a communication is occurring through the serialport
        public void TryNewCommsTask()
        {

            // if serialport is currently not busy
            if (isReceiving == false && ptrWorkingAt != ptrWriteAt)
            {
                if (ptrWorkingAt == ptrWriteAt + 1)
                {
                    //xxx mainForm.Invoke(new EventHandler(delegate{mainForm.connectionLost();}));
                    this.mainForm.Dispatcher.Invoke((Action)(() => { mainForm.connectionLost(); }));

                }

                isReceiving = true;
                RunCommsThread = new Thread(new ThreadStart(RunCommsTask));
                RunCommsThread.Start();
            }
        }

        //---Sends data across the serialport
        private void RunCommsTask()
        {
            int ptr = ptrWorkingAt;
            if (cmdNum[ptr] >= 0 && cmdNum[ptr] <= 3)
            {
                byte[] byteBuffer = BitConverter.GetBytes(data[ptr]);
                DataWrite(cmdNum[ptr], itemNum[ptr], byteBuffer[1], byteBuffer[0]);
                if (comValid == true)
                {
                    TaskComplete();
                    while (TryNewCommsThread.IsAlive == true) { }
                    //xxx TryNewCommsThread = new Thread(new ThreadStart(TryNewCommsTask));
                    TryNewCommsThread = new Thread(new ThreadStart(TryNewCommsTask));

                    TryNewCommsThread.Start();
                }
                else
                {
                    //xxx mainForm.Invoke(new EventHandler(delegate{mainForm.connectionLost();}));
                    mainForm.Dispatcher.Invoke((Action)(() => { mainForm.connectionLost(); }));
                }
            }
            if (cmdNum[ptr] == 4 || cmdNum[ptr] == 5)
            {
                comPort.ReceivedBytesThreshold = amountToGet[ptr] * 2;
                byte[] byteBuffer = BitConverter.GetBytes(amountToGet[ptr]);
                DataWrite(cmdNum[ptr], itemNum[ptr], byteBuffer[1], byteBuffer[0]);
                if (comValid == false)
                {
                    //xxx mainForm.Invoke(new EventHandler(delegate{mainForm.connectionLost();}));
                    this.mainForm.Dispatcher.Invoke((Action)(() => { mainForm.connectionLost(); }));

                }
            }
            if (cmdNum[ptr] == 6)
            {
                if (amountToGet[ptr] > 0)
                {
                    comPort.ReceivedBytesThreshold = amountToGet[ptr];
                }
                byte[] byteBuffer = BitConverter.GetBytes(data[ptr]);
                DataWrite(cmdNum[ptr], itemNum[ptr], byteBuffer[1], byteBuffer[0]);
                byteBuffer = BitConverter.GetBytes(data2[ptr]);
                DataWrite(cmdNum[ptr], itemNum[ptr], byteBuffer[1], byteBuffer[0]);
                if (comValid == false)
                {
                    //xxx mainForm.Invoke(new EventHandler(delegate{mainForm.connectionLost();}));
                    this.mainForm.Dispatcher.Invoke((Action)(() => { mainForm.connectionLost(); }));


                }
            }
        }

        //---Writes 4 bytes of data across the SCI and echo checks---
        public void DataWrite(byte cmd, byte item, byte datHigh, byte datLow)
        {
            try
            {
                comPort.Write(new byte[] { cmd }, 0, 1);
                echobyte[3] = comPort.ReadByte();
                //label1.Text = echobyte3.ToString();          //Debug
                if (echobyte[3] != cmd)
                {
                    throw new InvalidProgramException();
                }

                comPort.Write(new byte[] { item }, 0, 1);
                echobyte[2] = comPort.ReadByte();
                //label1.Text = label1.Text + " " + echobyte2.ToString();     //Debug
                if (echobyte[2] != item)
                {
                    throw new InvalidProgramException();
                }

                comPort.Write(new byte[] { datLow }, 0, 1);  //host expects LSB then MSB
                echobyte[0] = comPort.ReadByte();
                //label1.Text = label1.Text + " " + echobyte1.ToString();     //Debug
                if (echobyte[0] != datLow)
                {
                    throw new InvalidProgramException();
                }

                comPort.Write(new byte[] { datHigh }, 0, 1);
                echobyte[1] = comPort.ReadByte();
                //label1.Text = label1.Text + " " + echobyte0.ToString();     //Debug
                if (echobyte[1] != datHigh)
                {
                    throw new InvalidProgramException();
                }
            }

            catch (Exception err)
            {
                // lblStatus.Text = "Unhandled Exception";
                comValid = false;
                //err.Message;
            }
        }


        //---Once task is complete allow a new task to begin
        public void TaskComplete()
        {
            ptrWorkingAt++;
            isReceiving = false;
        }

        //---Syncronizes Client and Target and open communications
        public bool SciConnect()
        {
            string[] s = System.IO.Ports.SerialPort.GetPortNames();
            for (int i = 0; i < s.Length; i++)
            {

                //xxx if (s[i] == Properties.Settings.Default.ComPortName)
                if (s[i] == PortName)
                {
                    try
                    {

                        comPort.Open();


                        while (comPort.IsOpen == false) { }

                        comPort.DiscardOutBuffer();
                        comPort.DiscardInBuffer();

                        //necessary for some SerialPorts
                        DataWrite(0x00, 0x00, 0x00, 0x00); //initialise data transfer
                        DataWrite(0x00, 0x00, 0x00, 0x00); //initialise data transfer                

                        comValid = true;
                        DataWrite(0x00, 0x00, 0x00, 0x02);  //try to blink LED and receive errors

                        if (comValid == false)
                        {
                            comPort.Close();
                        }

                        return comValid;
                    }
                    catch (Exception ex)
                    {
                        this.mainForm.Dispatcher.Invoke((Action)(() => { mainForm.exceptionMess(); }));
                    }
                }

            }
            return false;
        }

        //---Deletes all tasks
        public void ClearCommands()
        {
            for (int i = 0; i < 64; i++)
            {
                cmdNum[i] = 0x00;
            }
            ptrWorkingAt = 0;
            ptrWriteAt = 0;
        }

        //---Generic set task
        public void NewSetTask(Object _ctrl, byte _cmdNum, byte _itemNum, int _data)
        {
            cmdNum[ptrWriteAt] = _cmdNum;
            itemNum[ptrWriteAt] = _itemNum;
            data[ptrWriteAt] = _data;
            ctrl[ptrWriteAt] = _ctrl;

            ptrWriteAt++;

            TryNewCommsTask();
        }


        //---When data is received call the control who owns it to see what to do with the data
        private void commsPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            int numBytes = comPort.BytesToRead;
            if (numBytes == comPort.ReceivedBytesThreshold)
            {
                byte[] byteBuffer = new byte[numBytes];
                comPort.Read(byteBuffer, 0, numBytes);

                //xxx if ((ctrl[ptrWorkingAt]) is GUI_Template.GuiGetVar)
                if ((ctrl[ptrWorkingAt]) is GuiGetVar)
                    {
                        ((GuiGetVar)ctrl[ptrWorkingAt]).ReadBuffer(byteBuffer);
                }
                /*
                else if ((ctrl[ptrWorkingAt]) is MainWindow.GuiGetArray)
                {
                    ((GuiGetArray)ctrl[ptrWorkingAt]).ReadBuffer(byteBuffer);
                }
                else if ((ctrl[ptrWorkingAt]) is MainWindow.GuiGraphTSArray)
                {
                    ((GuiGraphTSArray)ctrl[ptrWorkingAt]).ReadBuffer(byteBuffer);
                }
                else if ((ctrl[ptrWorkingAt]) is MainWindow.GuiGetMemory)
                {
                    ((GuiGetMemory)ctrl[ptrWorkingAt]).ReadBuffer(byteBuffer);
                }
                */
                else if (dataRxedBufferFilled == false)
                {
                    dataRxedBuffer = new int[numBytes];
                    for (int i = 0; i < byteBuffer.Length / 2; i++)
                    {
                        dataRxedBuffer[i] = (byteBuffer[2 * i] + byteBuffer[2 * i + 1] * 256);
                    }
                    dataRxedBufferFilled = true;
                }
                while (TryNewCommsThread.IsAlive == true) { }
                TryNewCommsThread = new Thread(new ThreadStart(TryNewCommsTask));
                TryNewCommsThread.Start();
            }
        }


    }
}

