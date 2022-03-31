using Bosch.VideoSDK.GCALib;
using Bosch.Vms.SDK.Internal.ServerSdkContracts;
using Newtonsoft.Json;
using PTKSOFT190129.Lib;
using Sybase.Data.AseClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PresentationControls;
using System.Threading;

namespace MLPR
{
    public partial class searchlicense : Form
    {
        private static AseConnection conn = new AseConnection();
        private static AseCommand cmd;
        private static AseDataReader reader;

        private static string ipdb;
        private static string portdb;
        private static string userdb;
        private static string passdb;
        private static string namedb;

        private static string commanddb; //search
        private string commandcombo;

        private static string[] datacoloren = new string[50];
        private static string[] datacolorth = new string[50];
        private static string[] datacolorid = new string[50];

        private static string[] dataprovinceth = new string[100];
        private static string[] dataprovinceen = new string[100];
        private static string[] dataprovinceid = new string[100];

        private static string[] databrandid = new string[100];
        private static string[] databranden = new string[100];
        private ComboBox  _cbprovince;
        private ComboBox _cbcolor;
        private ComboBox _cbbrand;

        public searchlicense()
        {
            InitializeComponent();
        }

        private void searchlicense_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            startdate.Format = DateTimePickerFormat.Custom;
            enddate.Format = DateTimePickerFormat.Custom;
            startdate.CustomFormat = "yyyy-MM-dd";
            enddate.CustomFormat = "yyyy-MM-dd";
            loadconfig();
            externalDisplay();
            //loaddata();
            btnclear_Click(null,null);
            acc.Text = "100-90";
        }

        private void loadconfig()
        {
           try
           {

                var path = Application.StartupPath + "\\Config\\configDatabase.json";
                var file = File.OpenText(path);

                var readfile = file.ReadToEnd();

                ConfigDatabase cf = new ConfigDatabase();
                cf = JsonConvert.DeserializeObject<ConfigDatabase>(readfile);

                ipdb = cf.Database_ip;
                portdb = cf.Database_port;
                userdb = cf.Database_user;
                passdb = cf.Database_pass;
                namedb = cf.Database_name;






                loaddatacombobox();

            }
            catch (Exception ex)
           {

           }
        }
        private void loaddatacombobox()
        {

            try
            {

                if (Connectdb())
                {
                    string cmdbbrand = " SELECT vehicle_brand_name_en,vehicle_brand_id FROM BPW_DB.dbo.mas_vehicle_brand ";
                    cmd = new AseCommand(cmdbbrand, conn);
                    reader = cmd.ExecuteReader();


                    //  DataTable dt = new DataTable();
                    // dt.Load(reader);
                    cbbrand.Items.Add("เลือกทั้งหมด");
                    int i = 0;
                    while (reader.Read())
                    {
                     
                       cbbrand.Items.Add(reader[0].ToString());
                        _cbbrand.Items.Add(reader[0].ToString());

                        databranden[i] = reader[0].ToString();
                        databrandid[i] = reader[1].ToString();
                            i++;
                    }


                    string cmdprovince = " SELECT vehicle_province_name_th,vehicle_province_name_en,vehicle_province_id FROM BPW_DB.dbo.mas_vehicle_province ";

                    cmd = new AseCommand(cmdprovince, conn);
                    reader = cmd.ExecuteReader();


                    //  DataTable dt = new DataTable();
                    // dt.Load(reader);
                    cbprovince.Items.Add("เลือกทั้งหมด");
                     i = 0;
                    while (reader.Read())
                    {
                        if (reader[1].ToString().Trim() != "Shield" && reader[1].ToString().Trim() != "Badge")
                        {
                            cbprovince.Items.Add(reader[0].ToString());
                            _cbprovince.Items.Add(reader[0].ToString());
                            dataprovinceth[i] = reader[0].ToString();

                            dataprovinceen[i] = reader[1].ToString();

                            dataprovinceid[i] = reader[2].ToString();
                           // Writestatelogfile(dataprovinceth[i].ToString());
                        }

                        i++;
                    }

                    string cmdcolor = " SELECT vehicle_color_name_th,vehicle_color_name_en,vehicle_color_id FROM BPW_DB.dbo.mas_vehicle_color ";

                    cmd = new AseCommand(cmdcolor, conn);
                    reader = cmd.ExecuteReader();


                    //  DataTable dt = new DataTable();
                    // dt.Load(reader);
                    cbcolor.Items.Add("เลือกทั้งหมด");
                    i = 0;
                    while (reader.Read())
                    {

                        cbcolor.Items.Add(reader[0].ToString());
                        _cbcolor.Items.Add(reader[0].ToString());


                        datacolorth[i] = reader[0].ToString();

                        datacoloren[i] = reader[1].ToString();


                        datacolorid[i] = reader[2].ToString();




                        i++;
                    }

                   



                }
                else
                {

                }

                //   MyDetails m = new MyDetails();
                //  m.Id = (int)reader[0];
                //  m.Name = reader[1].ToString();
                //  m.Age = (int)reader[2];
                //  sequence.Add(m);

                reader.Close();
                cmd.Dispose();
                //  dt.Dispose();
            
                CloseConnectdb();



        



            }
            catch (Exception ex)
            {

              //  MessageBox.Show("search" + ex.Message);
            }
            GC.Collect();
        }
            

        public void externalDisplay()
        {
            if (Screen.AllScreens.Length > 1)
            {

                this.Location = new Point(1920 + (960-693), 540 - 335);

            }
            else if (Screen.AllScreens.Length == 1)
            {
                this.Location = new Point(960 - 693 , 540 - 335);
            }
            GC.Collect();
        }
        private bool Connectdb()  //lane,plaza,
        {
            try
            {
                conn = new AseConnection("Data Source='" + ipdb + "';Port='" + portdb + "';UID='" + userdb + "';PWD='" + passdb + "';Database='" + namedb + "';");

                if (conn != null)
                {
                    

                    conn.Open();
                    GC.Collect();

                    return true;
                }
            }
            catch (Exception ex)
            {
                GC.Collect();
                return false;
            }
            return false;
        }
        private void CloseConnectdb()  //lane,plaza,
        {
            try
            {
                if (reader != null && !reader.IsClosed)
                    reader.Close();
                if (cmd != null)
                    cmd.Dispose();
                if (conn != null && conn.State != ConnectionState.Closed)
                    conn.Close();
                
                GC.Collect();
            }
            catch (Exception ex)
            {

            }
        }

        private void searchlicense_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Hide();
        }

        private void btnsearch_Click(object sender, EventArgs e)
        {
            try
            {
                //btnclear_Click(sender,null);
                Thread threadsearch = new Thread(new ThreadStart(searchdb));
                threadsearch.Start();
                lbstatesearch.Text = "......กำลังค้นหา.....";
                GC.Collect();
                lbnum.Text = "0";
            }
            catch(Exception ex)
            {

            }
        }


        private void searchdb()
        {
            try
            {
                lock (this)
                {
                    ManualResetEvent mre = new ManualResetEvent(false);
                    mre.Set();

                    if (Connectdb())
                    {

                        command(startdate.Value.Date.ToString("yyyy-MM-dd"), starttime.Text.Trim(), enddate.Value.Date.ToString("yyyy-MM-dd"), endtime.Text.Trim(), direction.Text.Trim(), acc.Text.Trim(), adjust.Text.Trim());
                        commandaddvehicle(license.Text.Trim(), cbprovince.Text.Trim(), cbcolor.Text.Trim(), cbbrand.Text.Trim());
                        //Writestatelogfile(commanddb);
                        dataGridView1.Rows.Clear();

                        cmd = new AseCommand(commanddb, conn);
                        reader = cmd.ExecuteReader();

                        //mre.WaitOne(,);

                        lbstatesearch.Text = "...ค้นหาข้อมูลสำเร็จ...";
                        //  DataTable dt = new DataTable();
                        // dt.Load(reader);
                        int num = 0;
                        while (reader.Read())
                        {
                            fieldsearch fc = new fieldsearch();

                            textBox1.Text = reader[9].ToString() + "," + reader[10].ToString() + "," + reader[11].ToString() + "," + reader[12].ToString();


                            if (reader[0].ToString() == "True")
                            {

                                fc.isadj = "Adj";
                            }
                            else
                            {
                                fc.isadj = "N/A";
                            }

                            fc.datetime = reader[1].ToString();
                            fc.plaza = reader[2].ToString();
                            fc.lane = reader[3].ToString();
                            fc.lane_mode = reader[4].ToString();
                            fc.job = reader[5].ToString();
                            fc.staff_id = reader[6].ToString();
                            fc.@class = Convert.ToInt16(reader[7]);
                            fc.avc = reader[8].ToString();

                            fc.color =   convertdata.convertcolorid(reader[9].ToString(),datacolorid,datacolorth);
                            fc.brand =  convertdata.convertbrandid(reader[10].ToString(),databrandid,databranden);
                            fc.license = reader[11].ToString();

                            //Writestatelogfile(reader[12].ToString() + "," + dataprovinceid[10] +","+ dataprovinceth[10]);
                            fc.province = convertdata.convertprovinceid(reader[12].ToString(),dataprovinceid,dataprovinceth);
                            //fc.province = reader[12].ToString();



                            fc.acc = ((Convert.ToDouble(reader[14].ToString()) + Convert.ToDouble(reader[15].ToString()) + Convert.ToDouble(reader[16].ToString())) / 3).ToString(("F2"));

                            // String.Format("{0:0.##}", fc.acc);

                            double db1 = Convert.ToDouble(fc.acc.ToString().Trim());

                            double max = Convert.ToDouble(acc.Text.Trim().Split('-')[0].ToString());

                            double min = Convert.ToDouble(acc.Text.Trim().Split('-')[1].ToString());


                            if (db1 > min && db1 <= max)
                            {

                                dataGridView1.Rows.Add(fc.isadj, fc.datetime, fc.plaza, fc.lane, fc.lane_mode, fc.job, fc.staff_id, fc.@class, fc.avc, fc.color, fc.brand, fc.license, fc.province, fc.acc);


                                if (fc.isadj == "N/A")
                                {
                                    dataGridView1.Rows[num].Cells[0].Style.BackColor = Color.Red;
                                    dataGridView1.Rows[num].Cells[0].Style.SelectionBackColor = Color.Red;
                                }
                                else
                                {
                                    dataGridView1.Rows[num].Cells[0].Style.BackColor = Color.Lime;
                                    dataGridView1.Rows[num].Cells[0].Style.SelectionBackColor = Color.Lime;
                                }


                                num++;
                            }
                            lbnum.Text = num.ToString();
                        }


                        //   MyDetails m = new MyDetails();
                        //  m.Id = (int)reader[0];
                        //  m.Name = reader[1].ToString();
                        //  m.Age = (int)reader[2];
                        //  sequence.Add(m);

                        reader.Close();
                        cmd.Dispose();

                        //  dt.Dispose();
                    }

                    CloseConnectdb();


                }




            }
            catch (Exception ex)
            {
                lbstatesearch.Text = "ข้อมูลมีจำนวนมากเกินไป กรุณาระบุเงื่อนไขใหม่"; 
                //MessageBox.Show("search" + ex.Message);
            }
            GC.Collect();
        }

       

        private void btnclear_Click(object sender, EventArgs e)
        {
            lbstatesearch.Text = "";
            lbnum.Text = "0";
            // dataGridView1.Rows.Clear();
            /*
                        DataTable DT = new DataTable("TEST TABLE FOR DEMO PURPOSES");
                        DT.Columns.AddRange(
                        new DataColumn[]
                            {
                    new DataColumn("Id", typeof(int)),
                    new DataColumn("SomePropertyOrColumnName", typeof(string)),
                    new DataColumn("Description", typeof(string)),
                            });
                        DT.Rows.Add(1, "AAAA", "AAAAA");
                        DT.Rows.Add(2, "BBBB", "BBBBB");
                        DT.Rows.Add(3, "CCCC", "CCCCC");
                        DT.Rows.Add(3, "DDDD", "DDDDD");

                        cbcolor.DataSource =
                        new ListSelectionWrapper<DataRow>(
                        DT.Rows,
                        // "SomePropertyOrColumnName" will populate the Name 
                        // on ObjectSelectionWrapper.
                        "SomePropertyOrColumnName"
                        );
                        cbcolor.DataSource = DT;
                        cbcolor.DisplayMember = "SomePropertyOrColumnName";

                        cbcolor.ValueMember = "Description";
                        */
            dataGridView1.Rows.Clear();

            CheckedAll(cbprovince);
            CheckedAll(cbcolor);
            CheckedAll(cbbrand);
            cbprovince.Text = "เลือกทั้งหมด";
            cbbrand.Text = "เลือกทั้งหมด";
            cbcolor.Text = "เลือกทั้งหมด";
            license.Text = "";
        }
        private void command(string stdate,string sttime,string enddate,string endtime,string direction,string acc,string is_adjust)
        {
            commanddb = " SELECT ";
           
                commanddb += " tlp.is_adjust AS Is_adj, ";
                             
            
            commanddb += " ext.trx_datetime AS date_time, " +
                         " (SELECT plaza_display_id " +
                         " FROM  BPW_DB.dbo.mas_plaza mp " +
                         " WHERE plaza_id = ext.plaza_id)                       AS Plaza, " +
                         " (SELECT lane_number FROM  BPW_DB.dbo.mas_lane ml " +
                        " WHERE  lane_id = ext.lane_id)                       AS Lane," +
                         " (SELECT lane_mode_abb" +
                        " FROM  BPW_DB.dbo.mas_lane_mode mlm" +

                        " WHERE  lane_mode_id = tj2.lane_mode_id)                       AS Lane_mode," +
                        " tj2.job_number AS Job," +
                        " tj2.staff_id AS staff_id," +
                        " ext.class_type_id AS Class,";
            if (direction == "Entry")
            {
                commanddb += " ext.class_type_id AS AVC, ";
            }
            else if (direction == "Exit")
            {
                commanddb += " ext.avc_class_type_id AS AVC, ";
            }

            //""

            //commanddb += " mvc2.vehicle_color_name_th AS Color,";

            commanddb += " tlp.vehicle_color_id AS Color,";

            //commanddb += " mvb2.vehicle_brand_name_th AS Brand, ";

            commanddb += " tlp.vehicle_brand_id AS Brand,";

            commanddb += " tlp.license_chr_id||tlp.license_num_id AS license,";

            //commanddb += " mvp.vehicle_province_name_th AS Province, ";


            commanddb += " tlp.license_province_id AS Province,";


            commanddb += " tlp.license_chr_conf AS acc , ";

            commanddb += " tlp.license_num_conf AS accnum , ";

            commanddb += " tlp.license_chr_conf AS accchr , ";

            commanddb += " tlp.license_province_conf AS accprovince";

                        if (direction == "Entry")
                        {
                            commanddb += " FROM BPW_DB.dbo.tbl_entry_transaction ext ";
                        }   
                        else if(direction == "Exit")
                        {
                            commanddb += " FROM BPW_DB.dbo.tbl_exit_transaction ext ";
                        }




            commanddb += " LEFT JOIN BPW_DB.dbo.tbl_job tj2" +
                  " ON ext.job_id = tj2.job_id" +
                  " RIGHT JOIN BPW_DB.dbo.tbl_license_plate tlp" +
                  " ON ext.trx_id = tlp.ref_trx_id" +
                 // " LEFt JOIN BPW_DB.dbo.mas_vehicle_color mvc2" +
                 // " ON mvc2.vehicle_color_id = tlp.vehicle_color_id " +
                 // " LEFt JOIN BPW_DB.dbo.mas_vehicle_brand mvb2" +
                 // " ON mvb2.vehicle_brand_id = tlp.vehicle_brand_id" +
                 // " LEFT JOIN BPW_DB.dbo.mas_vehicle_province mvp" +
                 // " ON mvp.vehicle_province_id  = tlp.license_province_id" +




                  " WHERE trx_datetime BETWEEN '" + stdate + " " + sttime + "' AND '" + enddate + " " + endtime + "' " + 

                  " AND ext.plaza_id = 104001001 ";
                         
                        //" AND tlp.license_chr_conf " + acc.ToString();

            if (is_adjust != "All" || is_adjust != "")
            {
                if (is_adjust == "Adj")
                {
                    commanddb += " AND tlp.is_adjust = 1 ";
                }
                else if (is_adjust == "N/A")
                {
                    commanddb += " AND tlp.is_adjust = 0 ";
                }
            }
            // " JOIN BPW_DB.dbo.mas_vehicle_color mc" +
            //" ON tlp.vehicle_color_id = mc.vehicle_color_id" +


            //    textBox3.Text = stdate + " " + enddate;
            GC.Collect();

        }
        private void commandaddvehicle(string license,string province,string color,string brand)
        {
            if (checkBox1.Checked == true)
            {
                if (license.Trim() != "")
                {
                    
                    commanddb += " AND tlp.license_chr_id||tlp.license_num_id LIKE '%" + license.Replace(" ","") + "%'";

                }

                if (province != "")
                {
                    if (cbprovince.GetItemChecked(0) == false)
                    {

                        if (cbprovince.Items.Count > 0)
                        {
                            commanddb += " AND (";
                        }
                        for (int i = 0; i < cbprovince.CheckedItems.Count; i++)
                        {

                            if (i >= 1)
                            {
                                commanddb += " OR ";
                            }

                            commanddb += "mvp.vehicle_province_name_th  =  '" + cbprovince.CheckedItems[i].ToString().Replace(" ", "").ToString() + "'";

                            // commanddb += " (mvc2.vehicle_color_name_th = 'เขียว')";





                            if (i == cbprovince.CheckedItems.Count - 1)
                            {
                                commanddb += ")";
                            }


                        }
                    }
                    
                }


                if (color != "")
                {
                    if (cbcolor.GetItemChecked(0) == false)
                    {
                        if (cbcolor.CheckedItems.Count > 0)
                        {
                            commanddb += " AND (";
                        }
                        for (int i = 0; i < cbcolor.CheckedItems.Count; i++)
                        {

                            if (i >= 1)
                            {
                                commanddb += " OR ";
                            }

                            commanddb += "mvc2.vehicle_color_name_th  =  '" + cbcolor.CheckedItems[i].ToString().Replace(" ", "").ToString() + "'";

                            // commanddb += " (mvc2.vehicle_color_name_th = 'เขียว')";





                            if (i == cbcolor.CheckedItems.Count - 1)
                            {
                                commanddb += ")";
                            }



                        }
                    }
                }


                if (brand != "")
                {
                    if (cbbrand.GetItemChecked(0) == false)
                    {
                        if (cbbrand.CheckedItems.Count > 0)
                        {
                            commanddb += " AND (";
                        }
                        for (int i = 0; i < cbbrand.CheckedItems.Count; i++)
                        {

                            if (i >= 1)
                            {
                                commanddb += " OR ";
                            }

                            commanddb += "mvb2.vehicle_brand_name_th  =  '" + cbbrand.CheckedItems[i].ToString().Replace(" ", "").ToString() + "'";

                            // commanddb += " (mvc2.vehicle_color_name_th = 'เขียว')";





                            if (i == cbbrand.CheckedItems.Count - 1)
                            {
                                commanddb += ")";
                            }


                        }


                        //  textBox1.Text = commanddb;
                    }
                }
            }
            GC.Collect();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                panelvehicle.Enabled = true;
            }
            else
            {
                panelvehicle.Enabled = false;
            }
        }

     

        private void cmdExit_Click(object sender, EventArgs e)
        {
            this.Hide();
        }



     

        private void cbprovince_CheckBoxCheckedChanged(object sender, EventArgs e)
        {
            /*
            try
            {
                string objsender = ((ListBox)(sender)).Text;

                if (objsender == "เลือกทั้งหมด")
                {
                    if (cbprovince.CheckBoxItems[1].Checked == true)
                    {
                        for (int i = 0; i < cbprovince.Items.Count; i++)
                        {
                            cbprovince.CheckBoxItems[i].Checked = true;
                        }
                        cbprovince.Text = "เลือกทั้งหมด";
                    }
                    else
                    {
                        for (int i = 0; i < cbprovince.Items.Count; i++)
                        {
                            cbprovince.CheckBoxItems[i].Checked = false;
                        }
                        cbprovince.Text = "";
                        // cbprovince.ClearSelection();
                    }
                }
                else
                {

                }
            }
            catch { }*/
        }

        private void cbbrand_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            string objsender = ((ListBox)(sender)).Text;
            int count = cbbrand.CheckedIndices.Count;
            if (e.NewValue == CheckState.Checked)
            {
                count = count + 1;
            }
            if (objsender.ToLower() == "เลือกทั้งหมด" && e.NewValue == CheckState.Unchecked)
            {
                cbbrand.ItemCheck -= new System.Windows.Forms.ItemCheckEventHandler(this.cbbrand_ItemCheck);
                UncheckedAll(cbbrand);
                cbbrand.Text = "";
                cbbrand.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.cbbrand_ItemCheck);
            }
            else if (objsender.ToLower() == "เลือกทั้งหมด" && e.NewValue == CheckState.Checked)
            {
                cbbrand.ItemCheck -= new System.Windows.Forms.ItemCheckEventHandler(this.cbbrand_ItemCheck);
               
                CheckedAll(cbbrand);

            

                cbbrand.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.cbbrand_ItemCheck);
            }
            else if ((objsender.ToLower() != "" && objsender.ToLower() != "เลือกทั้งหมด") && e.NewValue == CheckState.Unchecked)
            {
                this.cbbrand.ItemCheck -= new System.Windows.Forms.ItemCheckEventHandler(this.cbbrand_ItemCheck);
                cbbrand.SetItemChecked(0, false);
                this.cbbrand.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.cbbrand_ItemCheck);
            }
            else if ((objsender.ToLower() != "" && objsender.ToLower() != "เลือกทั้งหมด") && count == (cbbrand.Items.Count - 1))
            {
                cbbrand.ItemCheck -= new System.Windows.Forms.ItemCheckEventHandler(this.cbbrand_ItemCheck);
                cbbrand.SetItemChecked(0, true);
                cbbrand.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.cbbrand_ItemCheck);
            }
        }
        public void CheckedAll(CheckedComboBox objCombo)
        {
            for (int i = 0; i < objCombo.Items.Count; i++)
            {
                objCombo.SetItemChecked(i, true);
            }
       
        }

        public void UncheckedAll(CheckedComboBox objCombo)
        {
            for (int i = 0; i < objCombo.Items.Count; i++)
            {
                objCombo.SetItemChecked(i, false);
            }
       
        }
       
  

        private void cbbrand_Click(object sender, EventArgs e)
        {

            cbbrand.DroppedDown = true;
        }

        private void cbprovince_Click(object sender, EventArgs e)
        {
            cbprovince.DroppedDown = true;
        }

        private void cbcolor_Click(object sender, EventArgs e)
        {
            cbcolor.DroppedDown = true;
        }

        private void cbprovince_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            string objsender = ((ListBox)(sender)).Text;
            int count = cbprovince.CheckedIndices.Count;
            if (e.NewValue == CheckState.Checked)
            {
                count = count + 1;
            }
            if (objsender.ToLower() == "เลือกทั้งหมด" && e.NewValue == CheckState.Unchecked)
            {
                cbprovince.ItemCheck -= new System.Windows.Forms.ItemCheckEventHandler(this.cbprovince_ItemCheck);
                UncheckedAll(cbprovince);
                cbprovince.Text = "";
                cbprovince.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.cbprovince_ItemCheck);
            }
            else if (objsender.ToLower() == "เลือกทั้งหมด" && e.NewValue == CheckState.Checked)
            {
                cbprovince.ItemCheck -= new System.Windows.Forms.ItemCheckEventHandler(this.cbprovince_ItemCheck);

                CheckedAll(cbprovince);



                cbprovince.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.cbprovince_ItemCheck);
            }
            else if ((objsender.ToLower() != "" && objsender.ToLower() != "เลือกทั้งหมด") && e.NewValue == CheckState.Unchecked)
            {
                this.cbprovince.ItemCheck -= new System.Windows.Forms.ItemCheckEventHandler(this.cbprovince_ItemCheck);
                cbprovince.SetItemChecked(0, false);
                this.cbprovince.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.cbprovince_ItemCheck);
            }
            else if ((objsender.ToLower() != "" && objsender.ToLower() != "เลือกทั้งหมด") && count == (cbprovince.Items.Count - 1))
            {
                cbprovince.ItemCheck -= new System.Windows.Forms.ItemCheckEventHandler(this.cbprovince_ItemCheck);
                cbprovince.SetItemChecked(0, true);
                cbprovince.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.cbprovince_ItemCheck);
            }
        }

        private void cbcolor_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            string objsender = ((ListBox)(sender)).Text;
            int count = cbbrand.CheckedIndices.Count;
            if (e.NewValue == CheckState.Checked)
            {
                count = count + 1;
            }
            if (objsender.ToLower() == "เลือกทั้งหมด" && e.NewValue == CheckState.Unchecked)
            {
                cbcolor.ItemCheck -= new System.Windows.Forms.ItemCheckEventHandler(this.cbcolor_ItemCheck);
                UncheckedAll(cbcolor);
                cbcolor.Text = "";
                cbcolor.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.cbcolor_ItemCheck);
            }
            else if (objsender.ToLower() == "เลือกทั้งหมด" && e.NewValue == CheckState.Checked)
            {
                cbcolor.ItemCheck -= new System.Windows.Forms.ItemCheckEventHandler(this.cbcolor_ItemCheck);

                CheckedAll(cbcolor);



                cbcolor.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.cbcolor_ItemCheck);
            }
            else if ((objsender.ToLower() != "" && objsender.ToLower() != "เลือกทั้งหมด") && e.NewValue == CheckState.Unchecked)
            {
                this.cbcolor.ItemCheck -= new System.Windows.Forms.ItemCheckEventHandler(this.cbcolor_ItemCheck);
                cbcolor.SetItemChecked(0, false);
                this.cbcolor.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.cbcolor_ItemCheck);
            }
            else if ((objsender.ToLower() != "" && objsender.ToLower() != "เลือกทั้งหมด") && count == (cbcolor.Items.Count - 1))
            {
                cbcolor.ItemCheck -= new System.Windows.Forms.ItemCheckEventHandler(this.cbcolor_ItemCheck);
                cbcolor.SetItemChecked(0, true);
                cbcolor.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.cbcolor_ItemCheck);
            }
        }

        public ComboBox Cbprovince
        {
            get
            {
                return _cbprovince;
            }
            set
            {
                _cbprovince = value;
            }
        }
        public ComboBox Cbcolor
        {
            get
            {
                return _cbcolor;
            }
            set
            {
                _cbcolor = value;
            }
        }
        public ComboBox Cbbrand
        {
            get
            {
                return _cbbrand;
            }
            set
            {
                _cbbrand = value;
            }
        }

        public string[] listprovinceen
        {
            get
            {
                return dataprovinceen;
            }
           
        }
        public string[] listprovinceth
        {
            get
            {
                return dataprovinceth;
            }

        }
        public string[] listcoloren
        {
            get
            {
                return datacoloren;
            }

        }
        public string[] listcolorth
        {
            get
            {
                return datacoloren;
            }

        }
    }
}
