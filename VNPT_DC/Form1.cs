using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

namespace VNPT_DC
{
    public partial class VNPT_DC : DevExpress.XtraEditors.XtraForm
    {
        int baurate = 115200;
        private readonly SerialPortVNPT serialPort = SerialPortVNPT.Instance;
        const string SEND_ONE = "[,FFFFFFFF,001D,FE,050A,0028,3B04,]";
        const string SEND_OK = "[,FFFFFFFF,001D,FD,050A,0028,F8F9,]";
        const string SEND_FAIL = "[,FFFFFFFF,001D,FC,050A,0028,330C,]";
        const string FINISH = "[,FFFFFFFF,001D,FB,050A,0028,F0F1,]";
        void GetAllPort()
        {
            serialPort.PortReceived += recivePort;
            var ports = serialPort.GetPorts();
            if (ports != null && ports.Count() > 0)
            {
                if (ports.Count() > 1)
                {
                    this.comboBoxEdit1.Properties.Items.AddRange(ports);
                }
                else
                {
                    this.comboBoxEdit1.Properties.Items.AddRange(ports);
                    this.comboBoxEdit1.SelectedItem = ports[0];
                    try
                    {
                        serialPort.OpenPort(this.comboBoxEdit1.Text, baurate);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Port không kết nối: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Không có kết nối với thiết bị!");
            }
        }
        System.Windows.Forms.Timer timerApp = null;
        void createTimer()
        {
            if (timerApp == null)
            {
                timerApp = new System.Windows.Forms.Timer();
                timerApp.Interval = 1000;
                timerApp.Tick += new EventHandler(TimerEventProcessor);
            }

            timerApp.Start();
        }
        private void TimerEventProcessor(Object myObject,
                                            EventArgs myEventArgs)
        {

            if (!GetCycle())
            {
                return;
            }
            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate ()
                {
                    if (!GetCheckSum())
                    {
                        serialPort.SendText(SEND_FAIL);
                    }
                    else
                    {
                        serialPort.SendText(SEND_OK);
                    }
                });
            }
            else
            {
                if (!GetCheckSum())
                {
                    serialPort.SendText(SEND_FAIL);

                }
                else
                {
                    serialPort.SendText(SEND_OK);
                }
                isClickSend = true;
            }



        }
        void closeTimer()
        {
            timerApp.Stop();
            timerApp.Dispose();
            //timerApp = null;
        }
        List<DtoValue> lstRecive = new List<DtoValue>();
        private bool isCheck = false;
        int count = 0;
        bool cycleEnd = true;
        Mutex m = new Mutex();
        private bool GetCycle()
        {
            try
            {
                m.WaitOne();
                return cycleEnd;
            }
            finally
            {
                m.ReleaseMutex();
            }

        }
        private void SetCycle(bool isSet)
        {
            try
            {
                m.WaitOne();
                cycleEnd = isSet;
            }
            finally
            {
                m.ReleaseMutex();
            }

        }
        void recivePort(string value)
        {
            recivePortPrivate(value);

        }
        private bool SetCheckSum(bool isCheckNEw)
        {
            if (this.InvokeRequired)
            {
                return (bool)this.Invoke(new Func<bool>(() => SetCheckSum(isCheckNEw)));
            }
            else
            {
                this.isCheck = isCheckNEw;
                return isCheck;
            }
        }

        private bool recivePortPrivate(string value)
        {
            if (this.InvokeRequired)
            {
                return (bool)this.Invoke(new Func<bool>(() => recivePortPrivate(value)));
            }
            else
            {
                SetCycle(false);
                isClickSend = false;

                try
                {
                    if (value.Contains(FINISH))
                    {
                        closeTimer();
                        SetCheckSum(false);
                        return true;
                    }
                    else if (value == "ERROR")
                    {
                        MessageBox.Show("Mất kết nối với cổng COM");
                    }
                    SetCheckSum(false);
                    if (value.Contains("]"))
                    {
                        try
                        {
                            value = value.Replace("\r", "");
                            SetCheckSum(checkSum(value));
                            if (GetCheckSum())
                            {
                                count++;
                                var rows = value.Split(new string[] { ",," }, StringSplitOptions.None);
                                foreach (var row in rows)
                                {
                                    var rowVal = row.Replace("[,", "");
                                    rowVal = rowVal.Replace(",]", "");
                                    var rowData = rowVal.Split(new string[] { "," }, StringSplitOptions.None);
                                    if (rowData != null && rowData.Length > 0)
                                    {
                                        var dtoVal = new DtoValue();
                                        var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                                        var myField = dtoVal.GetType().GetFields(bindingFlags);
                                        var fieldLng = myField.Length;
                                        var rowLen = rowData.Length;
                                        for (int i = 0; i < fieldLng; i++)
                                        {
                                            // Determine whether or not each field is a special name.
                                            if (i >= rowLen)
                                            {
                                                continue;
                                            }
                                            myField[i].SetValue(dtoVal, rowData[i]);
                                        }
                                        lstRecive.Add(dtoVal);
                                        gridControl1.DataSource = lstRecive;
                                        gridControl1.Invalidate();
                                    }
                                }

                            }

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                    this.textBox1.Text = "COUNT: " + count + " " + GetCheckSum() + " :" + value + "\n";
                    this.textBox1.Refresh();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    SetCycle(true);
                }
                return true;
            }
        }
        private bool GetCheckSum()
        {
            if (this.InvokeRequired)
            {
                return (bool)this.Invoke(new Func<bool>(() => GetCheckSum()));
            }
            else
            {
                return isCheck;
            }
        }

        public VNPT_DC()
        {
            InitializeComponent();
            GetAllPort();
        }

        private DtoRecive setDtoRecive()
        {
            //DtoRecive d = new DtoRecive();
            return null;
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            serialPort.OpenPort(this.comboBoxEdit1.Text, baurate);
        }

        bool isClickSend = false;
        private void simpleButton3_Click(object sender, EventArgs e)
        {
            this.lstRecive = new List<DtoValue>();
            serialPort.DiscardInBuffer();
            serialPort.SendText(SEND_ONE);
            cycleEnd = false;
            Thread.Sleep(1000);
            this.count = 0;
            createTimer();
        }

        private bool checkSum(string value)
        {
            var arrValue = value.Split(new char[] { ',' });
            //if arrValue != nil && len(arrValue) >= 24 {
            if ((arrValue != null) && arrValue.Count() >= 0)
            {
                var valueCrc = getCrcConvert(arrValue);
                var crc = ConvertCrc16Modbus(valueCrc);
                var lenVAlue = arrValue.Count();
                var crcDevice = ConvertHexStrToDec(arrValue[lenVAlue - 2]);
                //if (this.InvokeRequired)
                //{
                //    this.BeginInvoke((MethodInvoker)delegate ()
                //    {
                //        this.textBox2.Text = crc + "---" + crcDevice;
                //    });
                //}

                if (crc != crcDevice)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        private int ConvertCrc16Modbus(string data)
        {
            var datac = Encoding.ASCII.GetBytes(data);
            var c = 0;
            var flag = 0;
            var i = 0;
            var crc16 = 0xffff;
            var len = datac.Count();
            while (i < len)
            {
                crc16 = crc16 ^ (int)(datac[i]);
                for (c = 0; c < 8; c++)
                {
                    flag = (crc16 & 0x01);
                    crc16 = (crc16 >> 1);
                    if (flag != 0)
                    {
                        crc16 = (crc16 ^ 0xa001);
                    }
                }
                i++;
            }
            return crc16;
        }
        private int ConvertHexStrToDec(string hex)
        {
            return Convert.ToInt32(hex, 16);
        }

        private string getCrcConvert(string[] arrValue)
        {
            var lenArrStop = arrValue.Count() - 2;

            var valueConvertCrc = "";
            int i = 0;
            foreach (var val in arrValue)
            {
                if (i == lenArrStop)
                {
                    break;
                }
                valueConvertCrc += val + ",";
                i++;
            }
            return valueConvertCrc;
        }

        private void simpleButton4_Click(object sender, EventArgs e)
        {
            if(lstRecive != null && lstRecive.Count() == 0)
            {
                MessageBox.Show("Không có dữ liệu");
                return;
            }
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();
                var fileName = "Du_lieu_"+ DateTime.Now.ToString("ddMMyyyy hhmmss") + ".csv";
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    var path = Path.Combine(fbd.SelectedPath, fileName);
                    this.textEdit1.Text = path;
                    File.Create(path).Dispose();
                    if (File.Exists(path))
                    {
                        Csv.SaveToCsv(lstRecive,path);
                    }
                }
            }
        }
    }
}
