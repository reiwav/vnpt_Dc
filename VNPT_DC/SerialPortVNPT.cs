
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace VNPT_DC
{
    public class SerialPortVNPT
    {
        public static readonly SerialPortVNPT Instance = new SerialPortVNPT();
        public delegate void HandlerSendProgram(string value);
        public event HandlerSendProgram PortReceived;
        private  SerialPort serialPort = null;

        private Queue<byte[]> sendQueue = null;
        private BackgroundWorker comSendWorker;
        private int bauRate;
        private const string DEVICENAME = "USB-SERIAL CH340";
        public SerialPortVNPT()
        {
            init();
        }
        public SerialPortVNPT(string comName, int bauRate)
        {
            init();
            initPort(comName, bauRate);
        }
        public void OpenPort(string comName, int bauRate)
        {
          
            initPort(comName, bauRate);
        }

        void initPort(string comName, int bauRate)
        {
            this.bauRate = bauRate;
            serialPort = new System.IO.Ports.SerialPort(comName.ToUpper(), bauRate);
            serialPort.ReadBufferSize = 2000000;
            serialPort.DataReceived += serialPortDataReceived;
            try
            {
                openPort(bauRate);
            }
            catch(Exception ex) {
                throw ex;
            }
        }
        public void init()
        {
            sendQueue = new Queue<byte[]>();

            comSendWorker = new BackgroundWorker();
            comSendWorker.WorkerSupportsCancellation = true;
            comSendWorker.DoWork += comSendWorker_DoWork;
            comSendWorker.RunWorkerAsync();

        }

        public void DiscardInBuffer()
        {
            serialPort.DiscardInBuffer();
        }

        private void readRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Console.WriteLine("Woker read com run completed");
        }
        public void serialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //isSend = true;
            Thread.Sleep(1000);
           
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadLine();
            PortReceived(indata);
            sp.DiscardInBuffer();
            //while (indata != "")
            //{

            //    Thread.Sleep(100);
            //    indata = sp.ReadLine();
            //}

        }
        private byte[] ReadByteInPort()
        {
            //Thread.Sleep(1000);

            byte[] r = new byte[serialPort.BytesToRead];
            try
            {
                serialPort.Read(r, 0, serialPort.BytesToRead);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception read byte in port: " + ex.ToString());
            }
            finally
            {
                if (serialPort.BytesToRead != 0) serialPort.DiscardInBuffer();
            }

            return r;
        }

        public void SendData(byte[] data)
        {
            sendQueue.Enqueue(data);
            if (!comSendWorker.IsBusy)
            {
                comSendWorker.RunWorkerAsync();
            }
        }

        private void comSendWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!comSendWorker.CancellationPending && sendQueue.Count != 0)
            {
                SendDataToKeyBoard();
                Thread.Sleep(100);
            }
        }

        //private bool isSend = false;
        public void SendText(string value)
        {
            Thread.Sleep(800);
            try
            {
                if (openPort(this.bauRate))
                {
                    serialPort.Write(value);
                    //isSend = true;
                    //while (isSend)
                    //{
                    //    Thread.Sleep(2000);
                    //    if (isSend)
                    //    {
                    //        serialPort.Write(value);
                    //    }
                    //    else
                    //    {
                    //        isSend = false;
                    //        PortReceived("");
                    //    }

                    //}
                }
                else
                {
                    PortReceived("ERROR");
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void SendDataToKeyBoard()
        {
            try
            {
                if (openPort(this.bauRate))
                {
                    var data = sendQueue.Dequeue();
                    serialPort.Write(data, 0, data.Length);
                }
            }
            catch
            { }
        }
        public string[] GetPorts()
        {
            return SerialPort.GetPortNames();
        }
        public string getPortCommonWindows()
        {
            var coms = SerialPort.GetPortNames();
            var lsCom = new List<string>();
            ManagementObjectCollection collection = null;
            using (var searcher = new ManagementObjectSearcher("Select * From Win32_PnPEntity"))
                collection = searcher.Get();
            foreach (var device in collection)
            {
                var des = (string)device.GetPropertyValue("Description");
                var caption = (string)device.GetPropertyValue("Caption");
                if (!string.IsNullOrWhiteSpace(des) && !string.IsNullOrWhiteSpace(caption))
                {
                    foreach (var com in coms)
                    {
                        if (caption.Contains(com) && des.Contains(DEVICENAME))
                        {
                            return com;
                        }
                    }
                }
            }
            return "";
        }
        public bool openPort(int bauRate)
        {
            if (!serialPort.IsOpen)
            {
                try
                {
                    serialPort.Open();
                }
                catch
                {
                    Thread.Sleep(200);
                    var comName = getPortCommonWindows();
                    if (comName.Length > 0)
                    {
                        initPort(comName, bauRate);
                    }

                }
            }
            return true;
        }
    }
}
