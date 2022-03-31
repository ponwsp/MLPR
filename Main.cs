using MatrixBus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Schema;
using System.Windows.Forms.VisualStyles;
using MLPR;


namespace MLPR
{
    public partial class Main : Form
    {
        private static string IpMB;
        private static string PortMB;

        private static string Ipimage;

        private static string Ipnvr;
        private static string Cam_T_name;
        private static string Cam_S_name;


        private static string plaza_name;
        private static string Layout_Width;

        private static string Layout_Height;
        private string Timeout_Time;

        private static string Alert_time;
        
        private static string Simulate;
        private static string Client_num;

        private static string timeserversync;

        private static string[] datacoloren = new string[50];
        private static string[] datacolorth = new string[50];
        private static string[] dataprovinceth = new string[100];
        private static string[] dataprovinceen = new string[100];

        MatrixBusClient mtx;

        System.Timers.Timer[] timeoutTimer = new System.Timers.Timer[100];
        int[] tTimeOutCount = new int[100];
        Linkfail linkfail = new Linkfail();
        searchlicense sc = new searchlicense();
        formvdo vdo;
        public event EventHandler Loadvideo;

        public delegate void videoCallBack();
        public Main()
        {
            InitializeComponent();    
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            loadconfig();
            label1.Text = "Manual License Plate Recognition System: MLPR" + " V." + ProductVersion.ToString() + "    " + plaza_name;
            Control.CheckForIllegalCrossThreadCalls = false;
            


            Thread _timer = new Thread(TimeOn);
            _timer.SetApartmentState(ApartmentState.STA);
            _timer.Start();

            try
            {
               // linkfail.Show();
            }
            catch { }

            if(Simulate == "1")
            {
               // txtimage.Visible = true;
               // btnfail.Visible = true;
                
            }

            Timeout_Time = "60";
            externalDisplay();

            sc.Cbbrand = this.txtbrand;
            sc.Cbcolor = this.txtcolor;
            sc.Cbprovince = this.txtprovince;

            datacoloren = sc.listcoloren;

            datacolorth = sc.listcolorth;

            dataprovinceen = sc.listprovinceen;

            dataprovinceth = sc.listprovinceth;


            Writestatelogfile("[LOAD MAIN] : Success" );

            sc.Show();
            sc.Hide();
            GC.Collect();

        }
        public void externalDisplay()
        {
            if (Screen.AllScreens.Length > 2)
            {

                this.Location = new Point(1920,0);

            }
            else if (Screen.AllScreens.Length == 1)
            {
                this.Location = new Point(0, 0);
            }
        }
        private void TimeOn()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(1);
                    lbtime.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");


                    if(lbtime.Text.Split(' ')[1].Trim() == "23:55:00")
                    {
                        Thread thclear = new Thread(new ThreadStart(clearlog));
                        thclear.Start();
                    }
                }
                catch (Exception ex) { Writeerrorlogfile("[TimeON]" + ex.Message); }

                GC.Collect();
            }
        }
        private void clearlog()
        {
            try
            {
                string[] files = Directory.GetFiles(Application.StartupPath + "//Images//");
                if (files.Length > 5)
                {
                    foreach (string file in files)
                    {
                        File.Delete(file);
                        Writestatelogfile("[Delete Image] : " + file);
                        // Console.WriteLine($"{file} is deleted.");
                    }
                }
                string[] filess = Directory.GetFiles(Application.StartupPath + "//file_vdo//");
                if (filess.Length > 5)
                {
                    foreach (string file in filess)
                    {
                        File.Delete(file);
                        Writestatelogfile("[Delete vdo] : " + file);
                        // Console.WriteLine($"{file} is deleted.");
                    }
                }
            }
            catch { }
        }
        private void loadconfig()
        {
            try
            {
                var path = Application.StartupPath + "\\Config\\config.json";
                var file = File.OpenText(path);

                var readfile = file.ReadToEnd();

                Config cf = new Config();
                cf = JsonConvert.DeserializeObject<Config>(readfile);

                IpMB = cf.IpMB;

                PortMB = cf.PortMB;

                Ipimage = cf.Ipimage;

                Ipnvr = cf.Ipnvr;

            //    Cam_T_name = cf.Cam_T_name;  //"C_401E1_5T"

              //  Cam_S_name = cf.Cam_S_name;  //"C_401E1_5T"


                Layout_Width = cf.Layout_Width;

                Layout_Height = cf.Layout_Height;

                //Timeout_Time = cf.Timeout_Time;

                Alert_time = cf.Alert_Time;


                Simulate = cf.Sim_Test;

                Client_num = cf.Client_Num;

                plaza_name = cf.plazaname;


                //lbclient.Text = Client_num;

       




                dataGridView1.AutoGenerateColumns = false;




                mtx = new MatrixBusClient(IpMB);
                mtx.OnPacketArrival += new PacketMatrixBusArrivalEvent(packetarrival);
               // mtx.OnMatrixBusOffline += new MatrixBusOfflineEvent(Event_MbOffline);
               // mtx.OnMatrixBusOnline += new MatrixBusOnlineEvent(Event_MbOnline);

                string[] sendtopic = new string[5];
                sendtopic[0] = "MLPRTRX";
                sendtopic[1] = "RECEIVEBOOKING";
                sendtopic[2] = "RECEIVEREJECT";
                sendtopic[3] = "RECEIVECANCEL";
                sendtopic[4] = "RECEIVEPOLLING";
                // sendtopic[2] = "PASSTTRX";
                //sendtopic[3] = "PASSTSUM";
                mtx.SubscribeTopic(sendtopic);
                mtx.Start();

                if(Simulate == "1")
                {
                    button1.Visible = true;
                }
                Writestatelogfile("[LOAD CONFIG] : Success");

            }
            catch (Exception ex)
            {
                Writeerrorlogfile("[LOAD CONFIG]" + ex.Message);
            }
            GC.Collect();

        }

        private void Event_MbOnline()
        {
            try
            {
                lblink.ForeColor = Color.Lime;
                lblink.Text = "LINK";


                Writestatelogfile("[MB] : Online");

                closeform(linkfail, "Linkfail");
            }
            catch(Exception ex)
            {
                Writeerrorlogfile("[MB ONLINE]" + ex.Message);
            }
            GC.Collect();
        }

        private void Event_MbOffline()
        {
            try
            {
                lblink.ForeColor = Color.Red;
                lblink.Text = "FAIL";


                if (this.ShowInTaskbar == true)
                {
                    Writestatelogfile("[MB] : OFFline");
                    showform(linkfail, "Linkfail");
                }
            }
            catch (Exception ex)
            {
                Writeerrorlogfile("[MB OFFLINE]" + ex.Message);
            }
            GC.Collect();

        }

        private void packetarrival(PacketMatrixBus pk)
        {
            try
            {

                Thread thread = new Thread(delegate () { processpacket(pk); });
                thread.SetApartmentState(ApartmentState.STA); 
                thread.Start();

                // downloadvideo("02/08/2021", "07:00:01");
                // processpacket(pk);
                GC.Collect();
            }
            catch (Exception ex)
            {
                Writeerrorlogfile("[ packet arrival : ] " + ex.Message);
            }
        }
        private void Writeerrorlogfile(string message)
        {

            lock (this)
            {
                try
                {
                    //find log is exist or not
                    string curfile = Application.StartupPath + @"\LOG\" + DateTime.Now.ToString("dd_MM_yyyy") + ".txt";
                    if (!File.Exists(curfile))
                    {
                        using (TextWriter tw = new StreamWriter(curfile))
                        {

                            tw.WriteLine(DateTime.Now.ToString("HH:mm:ss:ffff") + " : " + message);
                        };
                    }
                    else
                    {
                        using (StreamWriter sw = File.AppendText(curfile))
                        {
                            sw.WriteLine(DateTime.Now.ToString("HH:mm:ss:ffff") + " : " + message);
                        }
                    }
                }
                catch { }

            }
        }
        private void Writestatelogfile(string message)
        {

            lock (this)
            {
                try
                {
                    //find log is exist or not
                    string curfile = Application.StartupPath + @"\LOG\STATE\" + DateTime.Now.ToString("dd_MM_yyyy") + ".txt";
                    if (!File.Exists(curfile))
                    {
                        using (TextWriter tw = new StreamWriter(curfile))
                        {

                            tw.WriteLine(DateTime.Now.ToString("HH:mm:ss:ffff") + " : " + message);
                        };
                    }
                    else
                    {
                        using (StreamWriter sw = File.AppendText(curfile))
                        {
                            sw.WriteLine(DateTime.Now.ToString("HH:mm:ss:ffff") + " : " + message);
                        }
                    }
                }
                catch { }
                GC.Collect();
            }
        }
        private void processpacket(PacketMatrixBus pck)
        {

            try
            {
               // Writestatelogfile("[PCK INCOME] :" + pck.topic.ToString() + ":::" +pck.jdata);
                if (pck.topic == "MLPRTRX")   //client receive transaction 
                {
                    // mtx.ConfirmTransaction(pck);
                    //txtlife.Text = pck.jdata;
                    // txtbx.Text = pck.jdata;
                    
                    if (pck.name == "MLPR_S")
                    {

                        adddatarow(pck.jdata);

                    }
                    else if (pck.name == "MLPR_SB")
                    {
                        adddatarowbooking(pck.jdata);
                    }



                }

                else if (pck.topic == "RECEIVEBOOKING")  //client receive booking 
                {
                    Datamlpr data = new Datamlpr();

                    data = JsonConvert.DeserializeObject<Datamlpr>(pck.jdata);

                    int num = data.client_num;


                    if (Convert.ToInt32(Client_num) != num)
                    {
                        lock (dataGridView1)
                        {
                            foreach (DataGridViewRow row in dataGridView1.Rows)
                            {
                                if (dataGridView1.Rows[row.Index].Cells[9].Value.ToString() == data.date.ToString())
                                {

                                    DataGridViewButtonCell dgbtn = null;
                                    DataGridViewButtonCell dgbtn1 = null;
                                    dgbtn1 = (DataGridViewButtonCell)(dataGridView1.Rows[row.Index].Cells[8]);
                                    dgbtn = (DataGridViewButtonCell)(dataGridView1.Rows[row.Index].Cells[7]);

                                    dgbtn.UseColumnTextForButtonValue = false;
                                    //  dataGridView1.CurrentCell = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];
                                    // dataGridView1.CurrentCell.ReadOnly = false;
                                    //Change this to dgbtn.Text = "Save";
                                    //dgbtn.UseColumnTextForButtonValue = true;


                                    dgbtn.Style.BackColor = Color.DarkGray;
                                    dgbtn.Style.SelectionBackColor = Color.DarkGray;
                                    dgbtn.Value = "Adj....";
                                    dgbtn.Style.ForeColor = Color.Black;
                                    dgbtn.Style.SelectionForeColor = Color.Black;
                                    dataGridView1.Rows[row.Index].Cells[0].Value = Properties.Resources.edit;


                                    dataGridView1.Rows[row.Index].DefaultCellStyle.BackColor = Color.LightGoldenrodYellow;
                                    dataGridView1.Rows[row.Index].DefaultCellStyle.SelectionBackColor = Color.LightGoldenrodYellow;

                                    dgbtn1.Style.BackColor = Color.DarkGray;
                                    dgbtn1.Style.SelectionBackColor = Color.DarkGray;
                                    dgbtn.FlatStyle = FlatStyle.Flat;
                                    dgbtn1.FlatStyle = FlatStyle.Flat;

                                    dataGridView1.Rows[row.Index].Cells[11].Value = "1";
                                    // dataGridView1.Rows.RemoveAt(row.Index);
                                    // data.client_num = Convert.ToInt32(Client_num);
                                    // sendmb(2, data, null);  //Reject
                                }



                            }
                        }



                    }



                }
                else if (pck.topic == "RECEIVEREJECT")
                {
                    Datamlpr data = new Datamlpr();

                    data = JsonConvert.DeserializeObject<Datamlpr>(pck.jdata);

                    int num = data.client_num;


                    lock (dataGridView1)
                    {
                        foreach (DataGridViewRow row in dataGridView1.Rows)
                        {
                            if (dataGridView1.Rows[row.Index].Cells[9].Value.ToString() == data.date.ToString())
                            {
                                if (dataGridView1.Rows[row.Index].Cells[5].Value.ToString() != "-")
                                {
                                    dataGridView1.Rows.RemoveAt(row.Index);
                                    data.client_num = Convert.ToInt32(Client_num);
                                    sendmb(2, data, null);
                                }
                            }
                        }
                    }

                }
                else if (pck.topic == "RECEIVECANCEL")
                {

                    Datamlpr data = new Datamlpr();

                    data = JsonConvert.DeserializeObject<Datamlpr>(pck.jdata);

                    int num = data.client_num;


                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (dataGridView1.Rows[row.Index].Cells[9].Value.ToString() == data.date.ToString())
                        {

                            if (dataGridView1.Rows[row.Index].Cells[11].Value.ToString() == "1")  //waitbooking
                            {

                                DataGridViewButtonCell dgbtn = null;
                                DataGridViewButtonCell dgbtn1 = null;

                                dgbtn = (DataGridViewButtonCell)(dataGridView1.Rows[row.Index].Cells[7]);
                                //dgbtn1 = (DataGridViewButtonCell)(dataGridView1.Rows[row.Index].Cells[8]);
                                dgbtn.UseColumnTextForButtonValue = false;
                                //  dataGridView1.CurrentCell = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];
                                // dataGridView1.CurrentCell.ReadOnly = false;
                                //Change this to dgbtn.Text = "Save";
                                //dgbtn.UseColumnTextForButtonValue = true;
                                dataGridView1.Rows[row.Index].DefaultCellStyle.BackColor = Color.White;

                                dataGridView1.Rows[row.Index].DefaultCellStyle.SelectionBackColor = Color.Gainsboro;

                                dgbtn.Style.BackColor = Color.DodgerBlue;
                                dgbtn.Style.SelectionBackColor = Color.DodgerBlue;
                                dgbtn.Value = "Acc";
                                dgbtn.Style.ForeColor = Color.White;
                                dgbtn.Style.SelectionForeColor = Color.White;
                                dataGridView1.Rows[row.Index].Cells[0].Value = Properties.Resources.warning;
                                dgbtn.FlatStyle = FlatStyle.Popup;
                                dgbtn1.FlatStyle = FlatStyle.Popup;

                                // dgbtn1.Style.BackColor = Color.Pink;
                                //dgbtn1.Style.SelectionBackColor = Color.Pink;

                                dataGridView1.Rows[row.Index].Cells[11].Value = "0";


                            }
                        }

                    }



                }
                else if (pck.topic == "RECEIVEPOLLING")
                {

                    Datamlpr data = new Datamlpr();

                    data = JsonConvert.DeserializeObject<Datamlpr>(pck.jdata);

                    int num = data.client_num;




                    if (data.timeout_time != null)
                    {
                        Timeout_Time = data.timeout_time;
                       
                    }


                    if (num == Convert.ToInt32(Client_num))
                    {
                        timeserversync = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    }


             

                   // txtimage.Text = timeserversync;




                }
                else { return; }



            }
            catch (Exception ex)
            {
                Writeerrorlogfile("[ process packet : ] " + ex.Message);
            }
            GC.Collect();

        }
        private void adddatarow(string idata)
        {
            lock(this)
            {
                try
                {
                    trxadjust.Root data = new trxadjust.Root();

                    data = JsonConvert.DeserializeObject<trxadjust.Root>(idata);

                    int num = 0;
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {

                        if(dataGridView1.Rows[row.Index].Cells[9].Value.ToString() == data.trx.id.ToString())
                        {
                            return;
                        }



                        num++;

                        //num++;
                    }
                
                    

                    string[]  dt = reformatdate(data.header.date.ToString()).Split(' ');
                    double[] conf = new double[data.lpr.Count];  //conf all
                    int i = 0;
                    foreach(var Conf in data.lpr)
                    {
                        if (Convert.ToDouble(Conf.conf.ToString("f2")) != 0.00)
                        {
                            conf[i] = Convert.ToDouble(Conf.conf.ToString("f2"));

                            if (conf[i] <= 1.00)
                            {
                                conf[i] = conf[i] * 100;
                            }

                            i++;
                        }
                    }

                    double conf_max = Max(conf);

                    dataGridView1.Rows.Insert(0,Properties.Resources.warning, (data.toll.station == 2) ? "EXIT" : "ENTRY", data.toll.lane,dt[0], dt[1], Timeout_Time,conf_max + "%");
                    dataGridView1.Rows[0].Cells[9].Value = data.trx.id.ToString();  // trx id
                    dataGridView1.Rows[0].Cells[10].Value = idata.ToString();  //data all
                    dataGridView1.Rows[0].Cells[11].Value = "0"; //normal



                    DataGridViewButtonCell dgbtn = null;
                    dgbtn = (DataGridViewButtonCell)(dataGridView1.Rows[0].Cells[7]);

                    dgbtn.UseColumnTextForButtonValue = false;
                    dgbtn.Value = "Acc";


                  //  dataGridView1_SelectionChanged(null,null);
                  //  dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.SelectedRows[0].Index;

                    if(num == 0)
                    {
                        Thread.Sleep(3000);
                        dataGridView1.Rows[0].Selected = true;
                        dataGridView1_SelectionChanged(null, null);
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
    

                    GC.Collect();
                }
                catch(Exception ex) 
                {
                    Writeerrorlogfile("[ add datarow : ] " + ex.Message);
                }

            }
        }

        private void adddatarowbooking(string idata)
        {
            lock (this)
            {
                try
                {
                    trxadjust.Root data = new trxadjust.Root();

                    data = JsonConvert.DeserializeObject<trxadjust.Root>(idata);

                    int num = 0;
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {

                        if (dataGridView1.Rows[row.Index].Cells[9].Value.ToString() == data.trx.id.ToString())
                        {
                            return;
                        }



                        num++;

                        //num++;
                    }

                    num = 0;

                    string[] dt = reformatdate(data.header.date.ToString()).Split(' ');
                    double[] conf = new double[data.lpr.Count];  //conf all
                    int i = 0;
                    foreach (var Conf in data.lpr)
                    {
                        if (Convert.ToDouble(Conf.conf.ToString("f2")) != 0.00)
                        {
                            conf[i] = Convert.ToDouble(Conf.conf.ToString("f2"));

                            if (conf[i] <= 1.00)
                            {
                                conf[i] = conf[i] * 100;
                            }

                            i++;
                        }
                    }

                    double conf_max = Max(conf);

                 
                    dataGridView1.Rows.Insert(0,Properties.Resources.warning, (data.toll.station == 2) ? "EXIT" : "ENTRY", data.toll.lane, dt[0], dt[1], Timeout_Time, conf_max + "%");
                    dataGridView1.Rows[num].Cells[9].Value = data.trx.id.ToString();  // trx id
                    dataGridView1.Rows[num].Cells[10].Value = idata.ToString();  //data all
                    dataGridView1.Rows[num].Cells[11].Value = "0"; //normal



                    DataGridViewButtonCell dgbtn = null;
                    DataGridViewButtonCell dgbtn1 = null;
                    dgbtn = (DataGridViewButtonCell)(dataGridView1.Rows[num].Cells[7]);
                    dgbtn1 = (DataGridViewButtonCell)(dataGridView1.Rows[num].Cells[8]);
                    dgbtn.UseColumnTextForButtonValue = false;
                    dgbtn.Value = "Adj....";




                    

                
                    dataGridView1.Rows[num].Cells[0].Value = Properties.Resources.edit;


                    dataGridView1.Rows[num].DefaultCellStyle.BackColor = Color.LightGoldenrodYellow;
                    dataGridView1.Rows[num].DefaultCellStyle.SelectionBackColor = Color.LightGoldenrodYellow;

                    dgbtn1.Style.BackColor = Color.DarkGray;
                    dgbtn1.Style.SelectionBackColor = Color.DarkGray;

                    dgbtn.Style.BackColor = Color.DarkGray;
                    dgbtn.Style.SelectionBackColor = Color.DarkGray;

                    dgbtn.Style.ForeColor = Color.White;
                    dgbtn.Style.SelectionForeColor = Color.White;


                    dgbtn1.Style.ForeColor = Color.White;
                    dgbtn1.Style.SelectionForeColor = Color.White;


                    dataGridView1.Rows[num].Cells[11].Value = "1";


                    dgbtn.FlatStyle = FlatStyle.Flat;
                    dgbtn1.FlatStyle = FlatStyle.Flat;


                    //   dataGridView1_SelectionChanged(null, null);
                    // dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.SelectedRows[0].Index;




                    if (num == 0)
                    {
                        dataGridView1.Rows[0].Selected = true;
                    }




                    GC.Collect();
                }
                catch (Exception ex)
                {
                    Writeerrorlogfile("[ add datarowbooking : ] " + ex.Message);
                }

            }
        }
        public double Min(double[] input)
        {
            double min = input[0];

            for (int i = 0; i < input.Length; i++)
            {
                double number = input[i];

                if ((number < min) && (number != 0.00))
                {
                    min = number;
                }
            }

            return min;
        }
        public double Max(double[] input)
        {
            double max = input[0];

            for (int i = 0; i < input.Length; i++)
            {
                double number = input[i];

                if ((number > max) && (number != 0.00))
                {
                    max = number;
                }
            }

            return max;
        }
        
        
        private string reformatdate(string date)
        {
            
            string dateout = date[6].ToString() + date[7].ToString() + "/" + date[4].ToString() + date[5].ToString() + "/" + date[0].ToString() + date[1].ToString() + date[2].ToString() + date[3].ToString() + " " + date[8].ToString() + date[9].ToString() + ":" + date[10].ToString() + date[11].ToString() + ":" + date[12].ToString() + date[13].ToString();
            
            

            return dateout;
        }
        private void sendmb( int type, Datamlpr mlpr, trxadjust.Root trx)
        {
            try
            {
                string[] sendtopic = new string[5];
                sendtopic[0] = "MLPRBOOKING";
                sendtopic[1] = "MLPRADJ"; //trx
                sendtopic[2] = "LPRREJECT";
                sendtopic[3] = "POLLING";
                sendtopic[4] = "LPRCANCEL";

                PacketMatrixBus pck = new PacketMatrixBus("MLPR_C", mlpr, sendtopic[0]);

                if (type == 0)
                {
                    pck = new PacketMatrixBus("MLPR_C", mlpr, sendtopic[0]);
                }
                else if (type == 1) // trxadjust
                {
                    pck = new PacketMatrixBus("MLPR_C", trx, sendtopic[1]);
                  
                }
                else if (type == 2)
                {
                    pck = new PacketMatrixBus("MLPR_C", mlpr, sendtopic[2]);
                   
                }
                else if (type == 3)
                {
                    pck = new PacketMatrixBus("MLPR_C", mlpr, sendtopic[3]);
                }
                else if (type == 4)
                {
                    pck = new PacketMatrixBus("MLPR_C", mlpr, sendtopic[4]);
                }

                //  mtx.SendTransaction(pck);
                mtx.SendRealTime(pck);
                Writestatelogfile("[send mb] : " + sendtopic[type].ToString());
               
            }
            catch(Exception ex)
            {
                Writeerrorlogfile("[ send mb : ] " + ex.Message);
            }
            GC.Collect();

        }
        private void cmdExit_Click(object sender, EventArgs e)
        {
            Writestatelogfile("[Main Close]");
            linkfail.Close();
            Environment.Exit(0);
            this.Close();
        }

        private void btnminimize_Click(object sender, EventArgs e)
        {
           
            this.WindowState = FormWindowState.Minimized;
        }

        private void button6_Click(object sender, EventArgs e)  //btnadd
        {
            int num = 0;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                num++;

                //num++;
            }
            dataGridView1.Rows.Insert(0,Properties.Resources.warning, "Entry", "1", DateTime.Now.ToString("dd/MM/yyyy"), DateTime.Now.ToString("HH:mm:ss"), Timeout_Time, "90%");
            dataGridView1.Rows[num].Cells[9].Value = DateTime.Now.ToString("yyyyMMddHHmmssfff");
           // dataGridView1.Rows[num].Cells[10].Value = idata.ToString();  //data all

            // start_timer(num);
            // dataGridView1.Rows[num].Selected = true;
            dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.SelectedRows[0].Index;



        }

      
       

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
              

                if (dataGridView1.Columns[e.ColumnIndex].Name == "Column9"  && dataGridView1.Rows[e.RowIndex].Cells[11].Value != "1")
                {
                    DataGridViewButtonCell dgbtn = null;
               
                    dgbtn = (DataGridViewButtonCell)(dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex]);

                    if(dgbtn.FlatStyle == FlatStyle.Flat)
                    {
                        return;
                    }

                    if (MessageBox.Show("ต้องการลบรายการดังกล่าวหรือไม่ ?", "คำถาม", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {

                        Datamlpr data = new Datamlpr();
                        data.date = dataGridView1.Rows[e.RowIndex].Cells[9].Value.ToString();
                        data.client_num = Convert.ToInt32(Client_num);


                        dataGridView1.Rows.RemoveAt(e.RowIndex);
                        
                        sendmb( 2, data,null);

                     
                    }
                }
                else if (dataGridView1.Columns[e.ColumnIndex].Name == "Column8" && dataGridView1.Rows[e.RowIndex].Cells[11].Value != "1")
                {
                    lock (dataGridView1)
                    {
                       
                        DataGridViewButtonCell dgbtn = null;
                        DataGridViewButtonCell dgbtn1 = null;

                        dgbtn = (DataGridViewButtonCell)(dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex]);
                        dgbtn1 = (DataGridViewButtonCell)(dataGridView1.Rows[e.RowIndex].Cells["Column9"]);

                        dgbtn.UseColumnTextForButtonValue = false;
                        dataGridView1.CurrentCell = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];
                        dataGridView1.CurrentCell.ReadOnly = false;
                        //Change this to dgbtn.Text = "Save";
                        
                        if (dgbtn.Value == "Acc")
                        {

                            dgbtn.Style.BackColor = Color.DarkGray;
                            dgbtn.Style.SelectionBackColor = Color.DarkGray;

                            dgbtn.Value = "Adj";
                            dgbtn.Style.ForeColor = Color.White;
                            dgbtn.Style.SelectionForeColor = Color.White;
                            dataGridView1.Rows[e.RowIndex].Cells[0].Value = Properties.Resources.edit;


                            dgbtn1.Style.BackColor = Color.DarkGray;
                            dgbtn1.Style.SelectionBackColor = Color.DarkGray;


                            dgbtn1.Style.ForeColor = Color.White;
                            dgbtn1.Style.SelectionForeColor = Color.White;

                            dgbtn.FlatStyle = FlatStyle.Flat;
                            dgbtn1.FlatStyle = FlatStyle.Flat;

                            //    dgbtn.

                            //    DataGridViewDisableButtonCell buttonCell = (DataGridViewDisableButtonCell)dataGridView1.Rows[e.RowIndex].Cells["Column8"];
                            //   DataGridViewDisableButtonCell buttonCell1 = (DataGridViewDisableButtonCell)dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];
                            //    buttonCell.Enabled = false;
                            //    buttonCell1.Enabled = false;
                            //  dgbtn.ReadOnly = true;

                            paneleditlicense.Visible = true;

                            //booking 
                            // dataGridView1.Rows[e.RowIndex].Cells[0].Value = Properties.Resources.warning;
                            // dgbtn.Style.BackColor = Color.SkyBlue;
                            // dgbtn.Style.SelectionBackColor = Color.SkyBlue;
                            // dgbtn.Value = "Adj";
                            //dataGridView1.Rows[e.RowIndex].Cells[5].Value = "-";





                            Datamlpr data = new Datamlpr();
                            data.date = dataGridView1.Rows[e.RowIndex].Cells[9].Value.ToString();
                            data.client_num = Convert.ToInt32(Client_num);

                            sendmb(0, data, null);  //acc  MLPRBOOKING



                            //  else if(dgbtn.Style.BackColor == Color.Lime && dgbtn.Style.SelectionBackColor == Color.Lime)
                            //  {
                            //  dgbtn.Value = "Adj";
                        }
                        // }

                    }

                    //dgbtn.UseColumnTextForButtonValue = true;
                    dataGridView1.CurrentCell.ReadOnly = true;
                }
                else
                {
                    dataGridView1_SelectionChanged(sender, e);
                }
               
            }
            catch(Exception ex) 
            {
                Writeerrorlogfile("[ CellContentClick : ] " + ex.Message);
            }
            GC.Collect();
        }

        private void downloadpic(string namepicl,string namepics)
        {
            // if (vdo != null)
            // {
            //   vdo.Close();
            // }
            string curfile = Application.StartupPath + @"\Images\"; //+ "\\" + DateTime.Now.ToString("dd_MM_yyyy_HHmmssfff") + ".txt";

            try
            {

                if (!string.IsNullOrEmpty(namepicl))
                {
                    Image imgl;



                        WebRequest req = WebRequest.Create("http://" + Ipimage.Trim() + ":8080/?" + namepicl + ".jpg");
                        //req.Credentials = new NetworkCredential("service", "WSS4Bosch!");

                        WebResponse resp = req.GetResponse();
                        imgl = Image.FromStream(resp.GetResponseStream());

                        imgl.Save(curfile + namepicl.Remove(21, 2) + "31" + ".jpg");


                    


                    lock (picl)
                        {
                            picl.Size = new Size(609, 396);
                            Resize(imgl, picl.Width, picl.Height, picl);
                        Writestatelogfile("[Show image picl] : Success");
                    }
                    
                }
                else { picl.Image = null; }

            }
            catch(Exception ex)
            {
                Writeerrorlogfile("[ downloadpic : ] " + ex.Message);
            }
            GC.Collect();

            try
            {
                //curfile = Application.StartupPath + @"\Imagestop\" + DateTime.Now.ToString("dd_MM") + @"\" + comboBox1.Text + "_" + comboBox2.Text; //+ "\\" + DateTime.Now.ToString("dd_MM_yyyy_HHmmssfff") + ".txt";

                if (!string.IsNullOrEmpty(namepics))
                {
                    Image imgs;

               
                    
                        WebRequest req1 = WebRequest.Create("http://" + Ipimage.Trim() + ":8080/?" + namepics + ".jpg");
                        //req.Credentials = new NetworkCredential("service", "WSS4Bosch!");

                        WebResponse resp1 = req1.GetResponse();
                        imgs = Image.FromStream(resp1.GetResponseStream());

                        imgs.Save(curfile + namepics.Remove(21, 2) + "41" + ".jpg");

                    

                    lock (pics)
                    {
                        Resize(imgs, pics.Width, pics.Height, pics);
                        Writestatelogfile("[Show image picS] : Success");
                    }
                }
                else { pics.Image = null; }
            }
            catch(Exception ex)
            {
                Writeerrorlogfile("[ downloadpic : ] " + ex.Message);
            }
            
            GC.Collect();
            
          


        }

        private void downloadpicves(string namepicl, string namepics)
        {
            // if (vdo != null)
            // {
            //   vdo.Close();
            // }
            string curfile = Application.StartupPath + @"\Images\"; //+ "\\" + DateTime.Now.ToString("dd_MM_yyyy_HHmmssfff") + ".txt";

            try
            {

                if (!string.IsNullOrEmpty(namepicl))
                {
                    Image imgl;



                    WebRequest req = WebRequest.Create("http://" + Ipimage.Trim() + ":80/?" + namepicl + ".jpg");
                    //req.Credentials = new NetworkCredential("service", "WSS4Bosch!");

                    WebResponse resp = req.GetResponse();
                    imgl = Image.FromStream(resp.GetResponseStream());

                    imgl.Save(curfile + namepicl.Remove(21, 2) + "31" + ".jpg");





                    lock (picl)
                    {
                        picl.Size = new Size(609, 396);
                        Resize(imgl, picl.Width, picl.Height, picl);
                        Writestatelogfile("[Show image piclves] : Success");
                    }

                }
                else { picl.Image = null; }

            }
            catch (Exception ex)
            {
                Writeerrorlogfile("[ downloadpic : ] " + ex.Message);
            }


            try
            {
                //curfile = Application.StartupPath + @"\Imagestop\" + DateTime.Now.ToString("dd_MM") + @"\" + comboBox1.Text + "_" + comboBox2.Text; //+ "\\" + DateTime.Now.ToString("dd_MM_yyyy_HHmmssfff") + ".txt";

                if (!string.IsNullOrEmpty(namepics))
                {
                    Image imgs;



                    WebRequest req1 = WebRequest.Create("http://" + Ipimage.Trim() + ":80/?" + namepics + ".jpg");
                    //req.Credentials = new NetworkCredential("service", "WSS4Bosch!");

                    WebResponse resp1 = req1.GetResponse();
                    imgs = Image.FromStream(resp1.GetResponseStream());

                    imgs.Save(curfile + namepics.Remove(21, 2) + "41" + ".jpg");



                    lock (pics)
                    {
                        Resize(imgs, pics.Width, pics.Height, pics);
                        Writestatelogfile("[Show image picSves] : Success");
                    }
                }
                else { pics.Image = null; }
            }
            catch (Exception ex)
            {
                Writeerrorlogfile("[ downloadpic : ] " + ex.Message);
            }

            GC.Collect();




        }
        public void Resize(Image image, int width, int height, PictureBox pic)
        {
            lock (this)
            {
                var destRect = new Rectangle(0, 0, width, height);
                var destImage = new Bitmap(width, height);

                destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                using (var graphics = Graphics.FromImage(destImage))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    using (var wrapMode = new ImageAttributes())
                    {
                        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                        graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                    }
                }



                pic.Image = destImage;


                GC.Collect();

            }


        }
        

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                lock (dataGridView1)
                {
                    if (e.ColumnIndex == 5)
                    {


                        foreach (DataGridViewRow row in dataGridView1.Rows)
                        {
                            if (dataGridView1[5, row.Index].Value != null && dataGridView1.Rows[e.RowIndex].Cells[9].Value != null)
                            {

                                    if (Convert.ToInt32(dataGridView1[5, row.Index].Value.ToString()) <= 0 || (string.IsNullOrEmpty(dataGridView1[5, row.Index].Value.ToString()) == true))
                                    {

                                        Datamlpr data = new Datamlpr();
                                        data.date = dataGridView1.Rows[e.RowIndex].Cells[9].Value.ToString();
                                        data.client_num = Convert.ToInt32(Client_num);
                                        sendmb(2, data, null);
                                        dataGridView1.Rows.RemoveAt(row.Index);
                                    }
                                    else if (Convert.ToInt32(dataGridView1[5, row.Index].Value.ToString()) <= Convert.ToInt32(Alert_time))
                                    {
                                        if (!CompareImages((Bitmap)((Image)(dataGridView1.Rows[row.Index].Cells[0].Value)), Properties.Resources.edit))
                                        {
                                        dataGridView1[0, row.Index].Value = Properties.Resources.alert;
                                        }

                                        
                                        dataGridView1[5, row.Index].Style.SelectionForeColor = Color.Red;
                                        dataGridView1[5, row.Index].Style.ForeColor = Color.Red;
                                    }
                                

                            }



                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Writeerrorlogfile("[ Cellvaluechange : ] " + ex.Message);
            }
            GC.Collect();
        }

        private void clearlabel()
        {
            try
            {
                lbdateselect.Text = "-";
                lbtimeselect.Text = "-";
                lblicense.Text = "-";
                lbprovince.Text = "-";
                lbcolor.Text = "-";
                lbbrand.Text = "-";
                lbacc.Text = "";
                txtlane_direction.Text = "-";
            }
            catch { }
        }
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            lock (dataGridView1)
            {
                try
                {
                    if (dataGridView1.Rows[0].Cells[1].Value != null)
                    {
                        int row = dataGridView1.CurrentRow.Index;



                        txtlane_direction.Text = dataGridView1.Rows[row].Cells[1].Value.ToString();
                        lbdateselect.Text = dataGridView1.Rows[row].Cells[3].Value.ToString();
                        lbtimeselect.Text = dataGridView1.Rows[row].Cells[4].Value.ToString();

                        if (dataGridView1.Rows[row].Cells[10].Value != null)
                        {
                            trxadjust.Root data = new trxadjust.Root();

                            data = JsonConvert.DeserializeObject<trxadjust.Root>(dataGridView1.Rows[row].Cells[10].Value.ToString());


                            txt3digit.Text = "";
                            txt4digit.Text = "";
                            txtprovince.Text = "Unknown";
                            txtcolor.Text = "Unknown";
                            txtbrand.Text = "UNKNOWN";





                            if (data.lpr[0].no != null && data.lpr[0].nochar != null)
                            {


                                

                                
                                lblicense.Text = data.lpr[0].nochar.ToString() + "-" + checkstringlicense(data.lpr[0].no.ToString());
                                txt3digit.Text = data.lpr[0].nochar.ToString();
                                txt4digit.Text = checkstringlicense(data.lpr[0].no.ToString());
                            }
                            if (data.lpr[0].province != null)
                            {
                                lbprovince.Text =  convertprovinceentoth(data.lpr[0].province.ToString());
                                txtprovince.Text = convertprovinceentoth(data.lpr[0].province.ToString());
                            }
                            if(data.lpr[0].color != null)
                            {
                                lbcolor.Text = convertcolorentoth(data.lpr[0].color.ToString());
                                txtcolor.Text = convertcolorentoth(data.lpr[0].color.ToString());
                            }
                            if(data.lpr[0].brand != null)
                            {
                                lbbrand.Text = data.lpr[0].brand.ToString();
                                txtbrand.Text = data.lpr[0].brand.ToString();
                            }


                            lbacc.Text = "";
                            if(data.lpr[0].conf <= 1 && data.lpr[0].conf != 0)
                            {
                                lbacc.Text = (data.lpr[0].conf*100).ToString("f2") + "  %";
                            }
                            else if(data.lpr[0].conf > 1 && data.lpr[0].conf != 0)
                            {
                                lbacc.Text = data.lpr[0].conf.ToString("f2") + "  %";
                            }
                            
                            numpicall.Text = data.lpr.Count.ToString();
                           
                            btnimage_Click(sender, e);

                            //downloadpic(data.lpr[0].image.ToString(),data.lpr[0].imagelp.ToString());


                        }


                        if(dataGridView1.Rows[row].Cells[11].Value == "1")
                        {
                            paneleditlicense.Visible = false;
                            return;
                        }
                      //  lbacc.Text = dataGridView1.Rows[row].Cells[6].Value.ToString();


                        if (CompareImages((Bitmap)((Image)(dataGridView1.Rows[row].Cells[0].Value)), Properties.Resources.edit))
                        {
                            paneleditlicense.Visible = true;
                        }
                        else
                        {
                            paneleditlicense.Visible = false;
                        }
                    }


                }
                catch (Exception ex)
                {
                    Writeerrorlogfile("[ Selection changed : ] " + ex.Message);
                }
                GC.Collect();
            }
        }
        private string checkstringlicense(string inputlicense)
        {
            try
            {
                if(Convert.ToInt32(inputlicense) == 0)
                {
                    return "-";
                }
                else { return inputlicense; }
            }
            catch(Exception ex)
            {
                return "-";
            }


        }
        private static bool CompareImages(Bitmap image1, Bitmap image2)
        {
            try
            {
                if (image1.Width == image2.Width && image1.Height == image2.Height)
                {
                    for (int i = 0; i < image1.Width; i++)
                    {
                        for (int j = 0; j < image1.Height; j++)
                        {
                            if (image1.GetPixel(i, j) != image2.GetPixel(i, j))
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch { return false; }
        }

        private void btnclear_Click(object sender, EventArgs e) //cancel
        {
            try
            {
                Datamlpr data = new Datamlpr();

                int row = dataGridView1.CurrentRow.Index;

                data.date = dataGridView1.Rows[row].Cells[9].Value.ToString();
                data.client_num = Convert.ToInt32(Client_num);
                

                DataGridViewButtonCell dgbtn = null;
                DataGridViewButtonCell dgbtn1 = null;
                dgbtn = (DataGridViewButtonCell)(dataGridView1.Rows[row].Cells[7]);
                dgbtn1 = (DataGridViewButtonCell)(dataGridView1.Rows[row].Cells[8]);
                dgbtn.UseColumnTextForButtonValue = false;
                //  dataGridView1.CurrentCell = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];
                // dataGridView1.CurrentCell.ReadOnly = false;
                //Change this to dgbtn.Text = "Save";
                //dgbtn.UseColumnTextForButtonValue = true;


                dgbtn.Style.BackColor = Color.DodgerBlue;
                dgbtn.Style.SelectionBackColor = Color.DodgerBlue;
                dgbtn.Value = "Acc";
                dgbtn.Style.ForeColor = Color.White;
                dgbtn.Style.SelectionForeColor = Color.White;
                dataGridView1.Rows[row].Cells[0].Value = Properties.Resources.warning;

                dgbtn1.FlatStyle = FlatStyle.Popup;
                dgbtn.FlatStyle = FlatStyle.Popup;

                dgbtn1.Style.BackColor = Color.Pink;
                dgbtn1.Style.SelectionBackColor = Color.Pink;
                
                dgbtn1.Style.ForeColor = Color.Black;
                dgbtn1.Style.SelectionForeColor = Color.Black;


                sendmb(4, data, null);

                paneleditlicense.Visible = false;

            }
            catch (Exception ex)
            {
                Writeerrorlogfile("[ btnclear : ] " + ex.Message);
            }
            GC.Collect();
        }

       
        private void paneleditlicense_VisibleChanged(object sender, EventArgs e)
        {
            try
            {
                if (lblicense.Text.Contains('-') == true)
                {
                    txt3digit.Text = lblicense.Text.Split('-')[0].Trim();
                    txt4digit.Text = checkstringlicense(lblicense.Text.Split('-')[1].Trim());
                }

                txtprovince.Text = lbprovince.Text;
                txtcolor.Text = lbcolor.Text;
                txtbrand.Text = lbbrand.Text;
            }
            catch (Exception ex)
            {

            }
            GC.Collect();
        }
        private void showform(Form _form,string name)
        {
            try
            {
                FormCollection fc = Application.OpenForms;




                foreach (Form frm in fc)
                {
                    //iterate through



                    if (frm.Text == name)
                    {
                        _form.Show();

                        // this.Close();
                        return;
                    }

                }
                _form.Show();
            }
            catch { }

            //  Menu menu = new Menu();
            // menu._MainForm = this;
            //  menu.Show();
        }
        private void closeform(Form _form, string name)
        {
            try
            {
                FormCollection fc = Application.OpenForms;

                foreach (Form frm in fc)
                {
                    //iterate through
                    if (frm.Text == name)
                    {
                        _form.Hide();

                        // this.Close();
                        return;
                    }
                }
            }
            catch { }


            //  Menu menu = new Menu();
            // menu._MainForm = this;
            //  menu.Show();
        }
     

        private void timer2_Tick(object sender, EventArgs e)  //client polling to  server
        {
            try
            {
                Thread.Sleep(1);
                Datamlpr data = new Datamlpr();
                data.date = DateTime.Now.ToString();
                data.client_num = Convert.ToInt32(Client_num);

                sendmb(3,data,null);

                DateTime DateTime1970 = new DateTime(1970, 1, 1).ToLocalTime();

                if (timeserversync != null)
                {
                    long dt = (long)(DateTime.Now.ToLocalTime() - Convert.ToDateTime(timeserversync)).TotalSeconds;

                    if (dt > 15)
                    {
                        if (Simulate != "1")
                        {
                            Event_MbOffline();
                        }
                    }
                    else
                    {
                        if (Simulate != "1")
                        {
                            Event_MbOnline();
                        }
                    }
                }



            }
            catch(Exception ex) 
            {
                Writeerrorlogfile("client polling to  server timer 2tick" + ex.Message);
            }
            GC.Collect();
        }

        private void btnsave_Click(object sender, EventArgs e)
        {
            lock (dataGridView1)
            {
                try
                {

                    int row = dataGridView1.CurrentCell.RowIndex;
                    int cell = dataGridView1.CurrentCell.ColumnIndex;

                    int num = Convert.ToInt32(numpicshow.Text) -1 ;

                    trxadjust.Root data = new trxadjust.Root();

                    data = JsonConvert.DeserializeObject<trxadjust.Root>(dataGridView1.Rows[row].Cells[10].Value.ToString());

                    
                        data.lpr[num].no = txt4digit.Text.Trim();

                    data.lpr[num].conf = 100;
                    data.lpr[num].confbrand = 100;
                    data.lpr[num].confno = 100;
                    data.lpr[num].confcolor  = 100;
                    data.lpr[num].confprovince = 100;
                    data.lpr[num].confnochar = 100;

                    data.lpr[num].nochar = txt3digit.Text.Trim();
                    
                 
                        data.lpr[num].province = txtprovince.Text.Trim();
                  
                        data.lpr[num].color = txtcolor.Text.Trim();
                    
                   
                        data.lpr[num].brand = txtbrand.Text.Trim() + "_" + Client_num;

                    if (data.lpr[num].image != null)
                    {
                        if (data.lpr[num].image.Length > 21)
                        {
                            data.lpr[num].image = data.lpr[num].image.ToString().Remove(21, 2) + "31".ToString();
                        }
                    }
                    else { data.lpr[num].image = ""; }
                    if (data.lpr[num].imagelp != null)
                    {
                        if (data.lpr[num].imagelp.Length > 21)
                        {
                            data.lpr[num].imagelp = data.lpr[num].imagelp.ToString().Remove(21, 2) + "41".ToString();
                        }
                    }
                    else { data.lpr[num].imagelp = ""; }


                    uploadpic(data.lpr[num].image.ToString(),data.lpr[num].imagelp.ToString());






                    sendmb(1, null, data);
                    //Thread.Sleep(500);

               

               

                    Datamlpr datareject = new Datamlpr();
                    datareject.date = dataGridView1.Rows[row].Cells[9].Value.ToString();
                    datareject.client_num = Convert.ToInt32(Client_num);


                    dataGridView1.Rows.RemoveAt(row);
                    sendmb(2, datareject, null);

              

                }
                catch (Exception ex)
                {
                    Writeerrorlogfile("btnsave" + ex.Message);
                    //pon test git 
                }
                GC.Collect();
            }
        }

        private void uploadpic(string namel,string names)
        {
            lock (this)
            {
                try
                {
                    string curfile = Application.StartupPath + @"\Images\"; //+ "\\" + DateTime.Now.ToString("dd_MM_yyyy_HHmmssfff") + ".txt";


                    if (!string.IsNullOrEmpty(namel))
                    {
                        Upload(curfile + namel + ".jpg");
                        Writestatelogfile("[Upload] : Success" + curfile + namel + ".jpg");
                    }
                    if (!string.IsNullOrEmpty(names))
                    {
                        Upload(curfile + names + ".jpg");
                        Writestatelogfile("[Upload] : Success" + curfile + names + ".jpg");
                    }

                    
                }
                catch (Exception ex)
                {
                    Writeerrorlogfile("uploadpic" +  ex.Message);
                    // txtdebug.Text = ex.Message;
                }
                GC.Collect();
            }
        }
        private void Upload(string fileName)
        {
            try
            {

                var client = new WebClient();
                var uri = new Uri("http://" + Ipimage + ":8080/");
                {
                    client.Headers.Add("fileName", System.IO.Path.GetFileName(fileName));
                    client.UploadFileAsync(uri, fileName);
                }

            }
            catch (Exception ex)
            {
                Writeerrorlogfile("upload" + ex.Message);
            }
            GC.Collect();
        }
 

        private void dataGridView1_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            try
            {

                if (dataGridView1.Rows.Count == 0)
                {
                    clearlabel();
                    paneleditlicense.Visible = false;

                    if(vdo != null)
                    {
                        vdo.Close();
                    }
                    //picl.Hide();
                    picl.Image = null;
                    pics.Image = null;
                    picl.Show();
                    pics.Show();
                    numpicall.Text = "1";
                    numpicshow.Text = "1";

                }
            }


            catch (Exception ex)
            {
                Writeerrorlogfile("Row removed" + ex.Message);
            }
            GC.Collect();
        }

        private void btnimage_Click(object sender, EventArgs e)
        {
            Thread btnimageclick = new Thread(new ThreadStart(btnimagec));

            btnimageclick.Start();


        
        }
        private void btnimagec()
        {
            try
            {

                if (dataGridView1.Rows.Count == 0)
                {
                    return;
                }

                panelselectimage.Visible = true;
                btnimage.BackColor = System.Drawing.SystemColors.ActiveCaption;
                btnimage.ForeColor = Color.White;
                btnvdo.BackColor = Color.FromArgb(76, 136, 197);
                btnvdo.ForeColor = Color.Black;

                if (vdo != null)
                {
                    vdo.Close();
                }
                picl.Image = null;
                pics.Image = null;

                picl.Show();
                picl.BringToFront();
                pics.Show();
                pics.BringToFront();

                if (Simulate == "2")
                {

                    downloadpic("21062414522110204130211", "21062414522110204130221");
                }
                else
                {
                    if (dataGridView1.Rows.Count > 0)
                    {
                        int row = dataGridView1.CurrentRow.Index;
                        int num = 1;

                        numpicshow.Text = "1";

                        if (dataGridView1.Rows[row].Cells[10].Value != null)
                        {
                            trxadjust.Root data = new trxadjust.Root();

                            data = JsonConvert.DeserializeObject<trxadjust.Root>(dataGridView1.Rows[row].Cells[10].Value.ToString());



                            string lprl = "";
                            string lprs = "";
                            if (data.lpr[num - 1].image != null)
                            {
                                lprl = data.lpr[num - 1].image.ToString();
                            }
                            if (data.lpr[num - 1].imagelp != null)
                            {
                                lprs = data.lpr[num - 1].imagelp.ToString();
                            }



                            var down = new Thread(() => downloadpic(lprl, lprs));
                            down.Start();

                            Writestatelogfile("[btnimagec] + click");
                            //  downloadpic(data.lpr[num-1].image.ToString(), data.lpr[num-1].imagelp.ToString());

                            //downloadpic(data.lpr[0].image.ToString(), null);

                        }
                    }
                }




            }
            catch (Exception ex)
            {
                Writeerrorlogfile("btnimage" + ex.Message);
            }
            GC.Collect();
        }
       
        private void btnvdo_Click(object sender, EventArgs e)
        {

            btnvideoc();

            //Thread btnvideoclick = new Thread(new ThreadStart(btnvideoc));

            //btnvideoclick.Start();
        }
        private void btnvideoc()
        {
            try
            {
                if (dataGridView1.CurrentRow.Index == -1)
                {
                    return;
                }


                panelselectimage.Visible = false;
                btnvdo.BackColor = System.Drawing.SystemColors.ActiveCaption;
                btnvdo.ForeColor = Color.White;
                btnimage.BackColor = Color.FromArgb(76, 136, 197);
                btnimage.ForeColor = Color.Black;


                picl.Hide();

                if (vdo != null)
                {
                    vdo.Close();
                }

                if (Simulate == "2")
                {
                    // vdo = new formvdo("27/7/2021", "08:06:05", Ipnvr, Cam_T_name);
                    // vdo = new formvdo(datetest.Text.Trim(), timetest.Text.Trim(), Ipnvr, Cam_T_name);
                    // vdo.TopLevel = false;


                    //  panelpic.Controls.Add(vdo);
                    //  vdo.Dock = DockStyle.Fill;
                    // vdo.Show();
                }
                else
                {
                  

                    int row = dataGridView1.CurrentRow.Index;

                    string date = dataGridView1.Rows[row].Cells[3].Value.ToString();

                    string time = dataGridView1.Rows[row].Cells[4].Value.ToString();

                    trxadjust.Root data = new trxadjust.Root();

                    data = JsonConvert.DeserializeObject<trxadjust.Root>(dataGridView1.Rows[row].Cells[10].Value.ToString());


                    string lane = data.toll.lane.ToString();

                    string plaza = data.toll.plaza.ToString();

                    string cam = "C_" + plaza.Substring(2, 1) + plaza.Substring(7, 2) + "E1_" + lane + "T";


                    //  vdo = new formvdo(datetest.Text.Trim(), timetest.Text.Trim(), Ipnvr, Cam_T_name);
                    vdo = new formvdo(date, time, Ipnvr, cam);
                    vdo.TopLevel = false;


                    panelpic.Controls.Add(vdo);
                    vdo.Dock = DockStyle.Fill;
                    vdo.Show();
                    Writestatelogfile("[btnvideo] + success");
                }
            }
            catch (Exception ex)
            {
                Writeerrorlogfile("btnvdo" + ex.Message);
            }
            //GC.Collect();
        }
         

        private void btndown_Click(object sender, EventArgs e)
        {
            try
            {
                int numshow = Convert.ToInt32(numpicshow.Text.ToString());
                int numall = Convert.ToInt32(numpicall.Text.ToString());
                int row = dataGridView1.CurrentRow.Index;


                trxadjust.Root data = new trxadjust.Root();

                data = JsonConvert.DeserializeObject<trxadjust.Root>(dataGridView1.Rows[row].Cells[10].Value.ToString());

                if (numshow == 1)
                {
                    return;
                }
              
                else
                {
                    numshow--;
                    numpicshow.Text = numshow.ToString();

                 
                    lblicense.Text = "-";
                    lbprovince.Text = "-";
                    lbcolor.Text = "-";

                    lbbrand.Text = "-";
                    lbacc.Text = "-";


                    txt3digit.Text = "";
                    txt4digit.Text = "";
                    txtprovince.Text = "Unknown";
                    txtcolor.Text = "Unknown";
                    txtbrand.Text = "UNKNOWN";

                    string lprl = "";
                    string lprs = "";
                    if (data.lpr[numshow - 1].image != null)
                    {
                        lprl = data.lpr[numshow - 1].image.ToString();
                    }
                    if (data.lpr[numshow - 1].imagelp != null)
                    {
                        lprs = data.lpr[numshow - 1].imagelp.ToString();
                    }



                    var down = new Thread(() =>  downloadpic(lprl, lprs));
                    down.Start();



                    if (data.lpr[numshow - 1].no != null && data.lpr[numshow - 1].nochar != null)
                    {
                        lblicense.Text = data.lpr[numshow - 1].nochar.ToString() + "-" + checkstringlicense(data.lpr[numshow - 1].no.ToString());
                        txt3digit.Text = data.lpr[numshow - 1].nochar.ToString();
                        txt4digit.Text = checkstringlicense(data.lpr[numshow - 1].no.ToString());
                    }

                    if (data.lpr[numshow - 1].province != null)
                    {
                        lbprovince.Text = convertprovinceentoth(data.lpr[numshow - 1].province.ToString());
                        txtprovince.Text = convertprovinceentoth(data.lpr[numshow - 1].province.ToString());
                    }
                    if (data.lpr[numshow - 1].color != null)
                    {
                        lbcolor.Text = convertcolorentoth(data.lpr[numshow - 1].color.ToString());
                        txtcolor.Text = convertcolorentoth(data.lpr[numshow - 1].color.ToString());
                    }
                    if (data.lpr[numshow - 1].brand != null)
                    {
                        lbbrand.Text = data.lpr[numshow - 1].brand.ToString();
                        txtbrand.Text = data.lpr[numshow - 1].brand.ToString();
                    }

                    lbacc.Text = "";

                    if (data.lpr[numshow -1].conf <= 1 && data.lpr[numshow - 1].conf != 0)
                    {
                        lbacc.Text = (data.lpr[numshow - 1].conf *100).ToString("f2") + "  %";
                    }
                    else if (data.lpr[numshow - 1].conf > 1 && data.lpr[numshow - 1].conf != 0)
                    {
                       lbacc.Text = data.lpr[numshow - 1].conf.ToString("f2") + "  %";
                    }







                    //     var ld = new Thread(() => downloadpic(data.lpr[numshow-1].image.ToString(), data.lpr[numshow - 1].imagelp.ToString()));
                    //  ld.SetApartmentState(ApartmentState.STA);
                    //  ld.Start();







                }


            }
            catch(Exception ex)
            {
                Writeerrorlogfile("btndown" + ex.Message);
            
            }
            GC.Collect();
        }

        private void btnup_Click(object sender, EventArgs e)
        {
            try
            {
                int numshow = Convert.ToInt32(numpicshow.Text.ToString());
                int numall = Convert.ToInt32(numpicall.Text.ToString());
                int row = dataGridView1.CurrentRow.Index;


                trxadjust.Root data = new trxadjust.Root();

                data = JsonConvert.DeserializeObject<trxadjust.Root>(dataGridView1.Rows[row].Cells[10].Value.ToString());

                if (numshow == numall)
                {
                    return;
                }
                else
                {
                    numshow++;
                    numpicshow.Text = numshow.ToString();


                    lblicense.Text = "-";
                    lbprovince.Text = "-";
                    lbcolor.Text = "-";

                    lbbrand.Text = "-";
                    lbacc.Text = "";


                    txt3digit.Text = "";
                    txt4digit.Text = "";
                    txtprovince.Text = "ไม่ทราบจังหวัด";
                    txtcolor.Text = "ไม่ระบุ";
                    txtbrand.Text = "UNKNOWN";




                    string lprl = "";
                    string lprs = "";
                    if (data.lpr[numshow - 1].image != null)
                    {
                        lprl = data.lpr[numshow - 1].image.ToString();
                    }
                    if (data.lpr[numshow - 1].imagelp != null)
                    {
                        lprs = data.lpr[numshow - 1].imagelp.ToString();
                    }



                    var down = new Thread(() => downloadpic(lprl, lprs));
                    down.Start();
                    
                    if (numshow == numall)
                    {
                        var downloadves = new Thread(() => downloadpicves(lprl, lprs));
                        downloadves.Start();

                    }



                    if (data.lpr[numshow - 1].no != null && data.lpr[numshow - 1].nochar != null)
                    {
                        lblicense.Text = data.lpr[numshow - 1].nochar.ToString() + "-" + checkstringlicense(data.lpr[numshow - 1].no.ToString());
                        txt3digit.Text = data.lpr[numshow - 1].nochar.ToString();
                        txt4digit.Text = checkstringlicense(data.lpr[numshow - 1].no.ToString());
                    }
                    
                    if (data.lpr[numshow - 1].province != null)
                    {
                        lbprovince.Text = convertprovinceentoth(data.lpr[numshow - 1].province.ToString());
                        txtprovince.Text = convertprovinceentoth(data.lpr[numshow - 1].province.ToString());
                    }
                    if (data.lpr[numshow - 1].color != null)
                    {
                        lbcolor.Text = convertcolorentoth(data.lpr[numshow - 1].color.ToString());
                        txtcolor.Text = convertcolorentoth(data.lpr[numshow - 1].color.ToString());
                    }
                    if (data.lpr[numshow - 1].brand != null)
                    {
                        lbbrand.Text = data.lpr[numshow - 1].brand.ToString();
                        txtbrand.Text = data.lpr[numshow - 1].brand.ToString();
                    }
                    lbacc.Text = "";

                    if (data.lpr[numshow - 1].conf <= 1 && data.lpr[numshow - 1].conf != 0)
                    {
                        lbacc.Text = (data.lpr[numshow - 1].conf * 100).ToString("f2") + "  %";
                    }
                    else if (data.lpr[numshow - 1].conf > 1 && data.lpr[numshow - 1].conf != 0)
                    {
                        lbacc.Text = data.lpr[numshow - 1].conf.ToString("f2") + "  %";
                    }


                    // var ld = new Thread(() => downloadpic(data.lpr[numshow - 1].image.ToString(), data.lpr[numshow - 1].imagelp.ToString()));
                    // ld.SetApartmentState(ApartmentState.STA);
                    // ld.Start();

                    //downloadpic(data.lpr[numshow - 1].image.ToString(), data.lpr[numshow - 1].imagelp.ToString());




                }


            }
            catch(Exception ex)
            {
                Writeerrorlogfile("btnup" + ex.Message);
            }
            GC.Collect();
        }
        private string convertprovinceentoth(string provinceinputen)
        {
            try
            {
                string pvoutput = "ไม่ทราบจังหวัด";
                int i = 0;
                foreach(var pv in dataprovinceen)
                {
                    if(pv == provinceinputen)
                    {
                        pvoutput = dataprovinceth[i];
                        return pvoutput;
                    }
                    i++;
                }


                return pvoutput;

            }
            catch(Exception ex)
            {
                return "ไม่ทราบจังหวัด";
            }
        }
        private string convertcolorentoth(string colorinputen)
        {
            try
            {
                string coloroutput = "ไม่ระบุ";
                int i = 0;
                foreach (var co in datacoloren)
                {
                    if (co == colorinputen)
                    {
                        coloroutput = datacolorth[i];
                        return coloroutput;
                    }
                    i++;
                }


                return coloroutput;

            }
            catch (Exception ex)
            {
                return "-";
            }
        }
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Writestatelogfile("[Main Closing]");
            linkfail.Close();
            Environment.Exit(0);
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
           
            showform(sc,"searchlicense");
            GC.Collect();

        }

        private void btnreject_Click(object sender, EventArgs e)
        {
            try
            {

                int row = dataGridView1.CurrentRow.Index;
                    if (MessageBox.Show("ต้องการลบรายการดังกล่าวหรือไม่ ?", "คำถาม", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {

                        Datamlpr data = new Datamlpr();
                        data.date = dataGridView1.Rows[row].Cells[9].Value.ToString();
                        data.client_num = Convert.ToInt32(Client_num);


                        dataGridView1.Rows.RemoveAt(row);

                        sendmb(2, data, null);


                    }
                
            }
            catch(Exception ex)
            {

            }
            GC.Collect();
        }

        private void txt3digit_TextChanged(object sender, EventArgs e)
        {
            if(txt3digit.Text == "")
            {
              ///  btnsave.Visible = false;
                btnsave.Enabled = false;
            }
            else if (txt4digit.Text != "" && txt3digit.Text != "")
            {
                btnsave.Visible = true;
                btnsave.Enabled = true;
            }
        }

        private void txt4digit_TextChanged(object sender, EventArgs e)
        {
            if (txt4digit.Text == "")
            { 
                //btnsave.Visible = false;
                btnsave.Enabled = false;

            }
            else if (txt4digit.Text != "" && txt3digit.Text != "")
            {
                 btnsave.Visible = true;
                btnsave.Enabled = true;
            }


        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            int num = 0;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                num++;

                //num++;
            }
            //  DataGridViewDisableButtonColumn addRow = new DataGridViewDisableButtonColumn();
            //  addRow.Width = 50;
            // addRow.Name = "AddButton";
            num = 0;
           
            dataGridView1.Rows.Insert(0,Properties.Resources.warning, "Entry", "1", DateTime.Now.ToString("dd/MM/yyyy"), DateTime.Now.ToString("HH:mm:ss"), Timeout_Time, "90%");
            dataGridView1.Rows[num].Cells[9].Value = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            // dataGridView1.Rows[num].Cells[10].Value = idata.ToString();  //data all
            dataGridView1.Rows[0].Cells[10].Value = "0";  //data all
            dataGridView1.Rows[0].Cells[11].Value = "0"; //normal

            // start_timer(num);
            // dataGridView1.Rows[num].Selected = true;
            dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.SelectedRows[0].Index;

            DataGridViewButtonCell dgbtn = null;
            dgbtn = (DataGridViewButtonCell)(dataGridView1.Rows[num].Cells[7]);

           dgbtn.UseColumnTextForButtonValue = false;
            dgbtn.Value = "Acc";

           // DataGridViewDisableButtonColumn addRow = new DataGridViewDisableButtonColumn();
          //  addRow.Width = 50;
          //  addRow.Name = "AddButton";
          
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                lock (dataGridView1)
                {
                    Thread.Sleep(1);

                    int num = 0;
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (row.Index < 20)
                        {
                            if (dataGridView1[5, row.Index].Value != null)
                            {
                                DateTime DateTime1970 = new DateTime(1970, 1, 1).ToLocalTime();

                                long dt = (long)(DateTime.Now.ToLocalTime() - Convert.ToDateTime(dataGridView1[3, row.Index].Value.ToString() + " " + dataGridView1[4, row.Index].Value.ToString())).TotalSeconds;

                                if (dataGridView1[5, row.Index].Value.ToString() != "-" && string.IsNullOrEmpty(dataGridView1[5, row.Index].Value.ToString()) == false)
                                {
                                    if (Convert.ToInt64(Timeout_Time) - dt > 0)
                                    {
                                        dataGridView1[5, row.Index].Value = Convert.ToInt64(Timeout_Time) - dt;
                                    }
                                    else
                                    {
                                        Datamlpr data = new Datamlpr();
                                        data.date = dataGridView1.Rows[row.Index].Cells[9].Value.ToString();
                                        data.client_num = Convert.ToInt32(Client_num);
                                        sendmb(2, data, null);
                                        dataGridView1.Rows.RemoveAt(row.Index);
                                    }
                                    if (Convert.ToInt32(dataGridView1[5, row.Index].Value.ToString()) <= Convert.ToInt32(Alert_time))
                                    {
                                        if (!CompareImages((Bitmap)((Image)(dataGridView1.Rows[row.Index].Cells[0].Value)), Properties.Resources.edit))
                                        {
                                            dataGridView1[0, row.Index].Value = Properties.Resources.alert;
                                        }


                                        dataGridView1[5, row.Index].Style.SelectionForeColor = Color.Red;
                                        dataGridView1[5, row.Index].Style.ForeColor = Color.Red;
                                    }
                                }
                                else
                                {
                                    Datamlpr data = new Datamlpr();
                                    data.date = dataGridView1.Rows[row.Index].Cells[9].Value.ToString();
                                    data.client_num = Convert.ToInt32(Client_num);
                                    sendmb(2, data, null);
                                    dataGridView1.Rows.RemoveAt(row.Index);
                                }
                                        


                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Writeerrorlogfile("[timer1_Tick]" + ex.Message);
            }
        }
        private void timer3_Tick(object sender, EventArgs e)
        {
            try
            {
                lock (dataGridView1)
                {
                    Thread.Sleep(1);

                    int num = 0;
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (row.Index >= 20 && row.Index < 40)
                        {
                            if (dataGridView1[5, row.Index].Value != null)
                            {
                                DateTime DateTime1970 = new DateTime(1970, 1, 1).ToLocalTime();

                                long dt = (long)(DateTime.Now.ToLocalTime() - Convert.ToDateTime(dataGridView1[3, row.Index].Value.ToString() + " " + dataGridView1[4, row.Index].Value.ToString())).TotalSeconds;

                                if (dataGridView1[5, row.Index].Value.ToString() != "-" && string.IsNullOrEmpty(dataGridView1[5, row.Index].Value.ToString()) == false)
                                {
                                    if (Convert.ToInt64(Timeout_Time) - dt > 0)
                                    {
                                        dataGridView1[5, row.Index].Value = Convert.ToInt64(Timeout_Time) - dt;
                                    }
                                    else
                                    {
                                        Datamlpr data = new Datamlpr();
                                        data.date = dataGridView1.Rows[row.Index].Cells[9].Value.ToString();
                                        data.client_num = Convert.ToInt32(Client_num);
                                        sendmb(2, data, null);
                                        dataGridView1.Rows.RemoveAt(row.Index);
                                    }
                                    if (Convert.ToInt32(dataGridView1[5, row.Index].Value.ToString()) <= Convert.ToInt32(Alert_time))
                                    {
                                        if (!CompareImages((Bitmap)((Image)(dataGridView1.Rows[row.Index].Cells[0].Value)), Properties.Resources.edit))
                                        {
                                            dataGridView1[0, row.Index].Value = Properties.Resources.alert;
                                        }


                                        dataGridView1[5, row.Index].Style.SelectionForeColor = Color.Red;
                                        dataGridView1[5, row.Index].Style.ForeColor = Color.Red;
                                    }
                                }
                                else
                                {
                                    Datamlpr data = new Datamlpr();
                                    data.date = dataGridView1.Rows[row.Index].Cells[9].Value.ToString();
                                    data.client_num = Convert.ToInt32(Client_num);
                                    sendmb(2, data, null);
                                    dataGridView1.Rows.RemoveAt(row.Index);
                                }



                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Writeerrorlogfile("[timer1_Tick]" + ex.Message);
            }
        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            try
            {
                lock (dataGridView1)
                {
                    Thread.Sleep(1);

                    int num = 0;
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (row.Index >= 40)
                        {
                            if (dataGridView1[5, row.Index].Value != null)
                            {
                                DateTime DateTime1970 = new DateTime(1970, 1, 1).ToLocalTime();

                                long dt = (long)(DateTime.Now.ToLocalTime() - Convert.ToDateTime(dataGridView1[3, row.Index].Value.ToString() + " " + dataGridView1[4, row.Index].Value.ToString())).TotalSeconds;

                                if (dataGridView1[5, row.Index].Value.ToString() != "-" && string.IsNullOrEmpty(dataGridView1[5, row.Index].Value.ToString()) == false)
                                {
                                    if (Convert.ToInt64(Timeout_Time) - dt > 0)
                                    {
                                        dataGridView1[5, row.Index].Value = Convert.ToInt64(Timeout_Time) - dt;
                                    }
                                    else
                                    {
                                        Datamlpr data = new Datamlpr();
                                        data.date = dataGridView1.Rows[row.Index].Cells[9].Value.ToString();
                                        data.client_num = Convert.ToInt32(Client_num);
                                        sendmb(2, data, null);
                                        dataGridView1.Rows.RemoveAt(row.Index);
                                    }
                                    if (Convert.ToInt32(dataGridView1[5, row.Index].Value.ToString()) <= Convert.ToInt32(Alert_time))
                                    {
                                        if (!CompareImages((Bitmap)((Image)(dataGridView1.Rows[row.Index].Cells[0].Value)), Properties.Resources.edit))
                                        {
                                            dataGridView1[0, row.Index].Value = Properties.Resources.alert;
                                        }


                                        dataGridView1[5, row.Index].Style.SelectionForeColor = Color.Red;
                                        dataGridView1[5, row.Index].Style.ForeColor = Color.Red;
                                    }
                                }
                                else
                                {
                                    Datamlpr data = new Datamlpr();
                                    data.date = dataGridView1.Rows[row.Index].Cells[9].Value.ToString();
                                    data.client_num = Convert.ToInt32(Client_num);
                                    sendmb(2, data, null);
                                    dataGridView1.Rows.RemoveAt(row.Index);
                                }



                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Writeerrorlogfile("[timer1_Tick]" + ex.Message);
            }
        }
    }
}
    
