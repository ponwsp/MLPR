using Bosch.VideoSDK;
using Bosch.VideoSDK.Device;
using Bosch.VideoSDK.MediaDatabase;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SCW_APP;

namespace MLPR
{
    public partial class formvdo : Form
    {
        //CultureInfo _cultureTHInfo = new CultureInfo("th-TH");
        //CultureInfo _cultureENInfo = new CultureInfo("en-US");


        private Bosch.VideoSDK.Device.DeviceConnectorClass deviceConnector = null;
        private Bosch.VideoSDK.Device.DeviceProxy proxy;

        private MediaDatabaseBrowser mediaDb;
        private int tracksCompletelyLoaded;
        private Queue<int> trackToBeLoaded = new Queue<int>();

        private MediaFileWriter writer;
        private PlaybackController playback;

        private Track exportTrack;
        //private string exportPath;
        private MediaFileFormatEnum exportFormat;
        private DateTime exportTimeStart;
        private DateTime exportTimeEnd;
        // New Develop Setting
        public String ConnectPath = "srvadmin:@10.43.11.8";
        public int Camara = 0;  //1 = cam1
        private int trackCamara = 0;
        //public String exportFilePath = @"" + System.AppDomain.CurrentDomain.BaseDirectory + "boschvdo_ex.mp4";
        public String exportFilePath = @"" + System.AppDomain.CurrentDomain.BaseDirectory + "file_vdo\\" + DateTime.Now.ToString("ddMMyyyyHHmmssfff") + ".mp4";

        public int timePeriod = 10;
        public DateTime dateStartSet = Convert.ToDateTime("28/04/2021 13:11:54", new CultureInfo("th-TH"));
        string date;
        string time;
        string IPnvr;
        string Camname;
        string Path;


        public event EventHandler Loadvideocomplete;


        public ConnectionState State { get; set; }

        public enum ConnectionState
        {
            Disconnected,
            Connecting,
            Connected,
            Disconnecting,
            Export
        };


        public formvdo(string Date, string Time,string ipnvr,string camname)
        {
            //"C_401E1_5T"
            try
            {
                InitializeComponent();
                //LogClass.writeLogDetail("configINI.strCameraServer.Trim();=" + configINI.strCameraServer.Trim());
                // Create the Video SDK's device connector
                deviceConnector = new DeviceConnectorClass();
                deviceConnector.ConnectResult += OnConnectResult;
                deviceConnector.DefaultTimeout = 2000;

                // create media database class
                mediaDb = new MediaDatabaseBrowser();
                mediaDb.Tracks.CollectionChanged += Tracks_CollectionChanged;
                mediaDb.OnStateChanged += mediaDb_OnStateChanged;
                mediaDb.OnProgress += mediaDb_OnProgress;

                // subscribe for log events
                var logTraceListener = new LogTraceListener();
                //logTraceListener.LogWrite += OnTraceListenerLogWrite;
                Trace.Listeners.Add(logTraceListener);

                date = Date;
                time = Time;
                IPnvr = ipnvr;
                Camname = camname;
            }
            catch (Exception ex)
            {
                LogClass.writeLogDetail("export vdo" + ex.Message);
                Writeerrorlogfile("export vdo" + ex.Message);
            }

        }
        private static void Writeerrorlogfile(string message)
        { 



            //find log is exist or not
            string curfile = Application.StartupPath + @"\log\" + DateTime.Now.ToString("dd_MM_yyyy") + ".txt";
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
        private void MainWindow_Load(object sender, EventArgs e)
        {
            try
            {
               // this.Visible = false;
                //this.Location = new Point(313,105);
                 axwmp.close();


                //DateTime dateStartSet1, dateStartSet2;
                //string dateT, date1, date2 ,dateTrandaction = "";
                //string[] arrDateT;
                //dateT= publicValue.trx_Date.Trim();

                //arrDateT= dateT.Split('/');
                //dateTrandaction= arrDateT[1] + "/" + arrDateT[0] + "/"+ arrDateT[2];//swap date , month

                //dateStartSet1 = Convert.ToDateTime(dateTrandaction + " " + publicValue.trx_Time.Trim());
                //dateStartSet2 = Convert.ToDateTime("29/04/2564 15:30:19", new CultureInfo("th-TH"));

                //date1 = dateStartSet1.ToString();
                //date2 = dateStartSet2.ToString();

                //LogClass.writeLogDetail("dateTrandaction="+ date1+"---"+"dateFix="+ date2+ "---dateT=" + dateT);

                //  frmMain formMain = (frmMain)Application.OpenForms["frmMain"];

                //MessageBox.Show("TrxID="+publicValue.trx_id+"/nTrxDate"+publicValue.trx_Date+ "/nTrxTime"+ publicValue.trx_Time);//Transaction ID
                //axwmp.URL = "D:\\ExportVDOcode260364\\BoschExport\\BoschExport\\MediaDbExport\\bin\\Debug\\file_vdo\\25032021131800.mp4";

                //formMain.winMediaReplay.URL = System.Environment.CurrentDirectory+ "\\file_vdo\\25032021131800.mp4";
                // axwmp.URL= System.Environment.CurrentDirectory + "\\file_vdo\\08042021100034.mp4";
                // axwmp.Ctlcontrols.play();
                //axwmp.settings.autoStart = true;

                //string path0 = @"D:\vdos\02042021152702.mp3";// Test VDO 100664
                //axwmp.URL = path0;
                //axwmp.Ctlcontrols.play();
                //axwmp.settings.autoStart = true;


                LoadConfigfile(date,time);

                SetConnectionState(ConnectionState.Disconnected, "Disconnected");
                Clear(true);

                dateStartSet = dateStartSet.AddHours(-7);

                if (File.Exists(exportFilePath))
                {
                    File.Delete(exportFilePath);
                }

                ConnectVdo(); // connect server


                Thread trackThread = new Thread(SelectTrackThread);
                trackThread.Start();
            }
            catch (Exception exp)
            {
                LogClass.writeLogDetail("connect VDO Failed " + exp.ToString());
                //textBoxLog.Text = exp.ToString();
                Writeerrorlogfile("Connect VDO Failed " + exp.ToString());
            }

        }

       
        private void SelectTrackThread()
        {
            int i = 0;
            while (true)
            {
                Thread.Sleep(500);
                if (mediaDb.Tracks.Count > 0)
                {
                    foreach (Bosch.VideoSDK.MediaDatabase.Track track in mediaDb.Tracks)
                    {

                        //track.Name == "camera_" + Camara
                        //track.Name == "C_413E1_12T"
                        //C_401E1_5V
                        //C_413X1_2V

                        if (track.Name == Camname)
                        {
                            trackCamara = i;
                            break;
                        }
                        i++;
                    }

                    SelectTrack(trackCamara); // select camara
                    break;
                }
            }

            ExportStart();
        }


        #region Connect methods and handlers

        private void OnConnectResult(ConnectResultEnum connectresult, string url, DeviceProxy deviceProxy)
        {
            proxy = deviceProxy;
           
            switch (connectresult)
            {

                case ConnectResultEnum.creConnected:
                    break;

                case ConnectResultEnum.creInitialized:
                    // The connection was successfully established.
                    SetConnectionState(ConnectionState.Connected, "Connected. Searching tracks...");

                    // start searching tracks
                    mediaDb.SearchTracks(proxy.MediaDatabase);
                    break;

                default:
                    proxy.Disconnect();
                    Trace.TraceError("Connect to {0} failed. Error: {1}", url, connectresult);
                    SetConnectionState(ConnectionState.Disconnected, "Connection failed");
                    break;
            }
        }

        private void SetConnectionState(ConnectionState state, string statusDescription)
        {
            Trace.TraceInformation(statusDescription);

            State = state;

            // update Connect button caption
            Action updateButtonAction = (() =>
            {
                //switch (State)
                //{
                //    case ConnectionState.Disconnected:
                //        buttonConnect.Text = @"Connect";
                //        buttonConnect.Enabled = true;
                //        break;
                //    case ConnectionState.Connected:
                //        buttonConnect.Text = @"Disconnect";
                //        buttonConnect.Enabled = true;
                //        break;
                //    default:
                //        buttonConnect.Text = state.ToString();
                //        buttonConnect.Enabled = false;
                //        break;
                //}
            });

            // update status 
            Action updateStatusAction = (() =>
            {
                //labelStatusInfo.Text = statusDescription;
            });

            //SafeInvoke(buttonConnect, updateButtonAction);
            //SafeInvoke(labelStatusInfo, updateStatusAction);
        }
        #endregion

        #region Load media database event handlers

        private void mediaDb_OnProgress(int progress)
        {
            float totalProgress = 0;
            if (mediaDb.State == MediaDatabaseBrowserState.TracksLoading || mediaDb.State == MediaDatabaseBrowserState.TracksLoaded)
            {
                // estimate loading tracks as 10%
                totalProgress = progress / 10;
            }
            else
            {
                // "divide" rest of progress bar (90%) between all tracks
                int tracksCount = mediaDb.Tracks.Count;

                if (tracksCount > 0)
                    totalProgress = 10 + 0.9f * (progress + tracksCompletelyLoaded * 100) / tracksCount;
                else
                    totalProgress = progress;
            }

            SetConnectionState(ConnectionState.Connected, "Connected. Loading tracks info " + (int)totalProgress + "%");
            //Trace.TraceInformation("Total progress: {0}%", (int)totalProgress);
        }

        private void mediaDb_OnStateChanged(MediaDatabaseBrowserState state, string description)
        {
            //labelStatusInfo.Text = description;
            //labelStatusInfo.Update();

            if (state == MediaDatabaseBrowserState.TracksLoaded)
            {
                // prepare list of tracks for loading records
                trackToBeLoaded.Clear();
                foreach (var track in mediaDb.Tracks)
                {
                    trackToBeLoaded.Enqueue(track.TrackID);
                }

                // start load first
                LoadNextTrackRecords();
            }

            if (state == MediaDatabaseBrowserState.RecordsLoaded)
            {
                tracksCompletelyLoaded++;
                LoadNextTrackRecords();
            }
        }

        private void Tracks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // add track nodes
                foreach (Bosch.VideoSDK.MediaDatabase.Track track in e.NewItems)
                {
                    string displayName = string.Format("Track #{0} {1}", track.TrackID, track.Name);

                    //comboBoxTracks.Items.Add(displayName);
                }
            }
        }

        private void LoadNextTrackRecords()
        {
            // Start loading records for found tracks from queue
            if (trackToBeLoaded.Count > 0)
            {
                int trackId = trackToBeLoaded.Dequeue();
                mediaDb.SearchTrackRecords(trackId, true);
            }
            else
            {
                // all tracks loaded
                //labelStatusInfo.Text = string.Format("Complete. {0} tracks found.", mediaDb.Tracks.Count);
                SelectTrack(-1);
            }
        }

        #endregion


        #region Select track methods and handlers

        private void SelectTrack(int selectedTrackId)
        {
            //buttonExport.Enabled = selectedTrackId >= 0;
            //textBoxOutPath.Enabled = selectedTrackId >= 0;
            //buttonOutFile.Enabled = selectedTrackId >= 0;
            //comboBoxTracks.Enabled = State == ConnectionState.Connected;

            if (selectedTrackId >= 0)
            {
                if (selectedTrackId < mediaDb.Tracks.Count)
                {
                    exportTrack = mediaDb.Tracks[selectedTrackId];

                }
                else
                {
                    Trace.TraceWarning("Track not found in media database");
                    SelectTrack(-1);
                    return;
                }

                // update file name, but keep folder and extension
                //string path = textBoxOutPath.Text.Contains("\\")
                //                  ? Path.GetDirectoryName(textBoxOutPath.Text)
                //                  : Directory.GetCurrentDirectory();

                //string ext = Path.GetExtension(textBoxOutPath.Text);

                // set defualt format if not specified
                //if (string.IsNullOrEmpty(ext))
                //    ext = ".asf";

                //string fileName = string.Format("{0}-Track{1}{2}", textBoxDeviceIP.Text, exportTrack.TrackID, ext);

                //textBoxOutPath.Text = Path.Combine(path, fileName);


                // get track time bounds 
                // Note: In current version Track.StartTime and Track.EndTime throws NotImlementedException
                //      So, we gets time bounds from first and last events

                var trackRecords = mediaDb.Records.Where(x => x.TrackID == exportTrack.TrackID);

                if (trackRecords.Count() == 0)
                {
                    // disable export
                    SelectTrack(-1);
                    //labelTrackInfo.Text = "Error: Unable to determinate track time bounds.";
                    return;
                }
                else
                {
                    exportTimeStart = trackRecords.Min(x => x.StartTime.UTC);
                    exportTimeEnd = trackRecords.Max(x => x.EndTime.UTC);

                    string info = string.Format("Track time bounds: {0} - {1}", exportTimeStart, exportTimeEnd);
                   
                    //labelTrackInfo.Text = info;
                }
            }
            else
            {
                // reset info
                //textBoxOutPath.Text = null;
                //labelTrackInfo.Text = mediaDb.Tracks.Count == 0 ? "Tracks are not loaded" : "Track is not selected";
                //labelFormatInfo.Text = null;
                //labelExportInfo.Text = null;
            }
        }
        #endregion

        private void Export(Track track, string path, MediaFileFormatEnum format, DateTime firstEventStart, DateTime lastEventEnd)
        {
            try
            {

                Trace.TraceInformation("Export track #{0} to file {1}", track.TrackID, path);
                Trace.TraceInformation("Set export file format to {0}", format);
                Trace.TraceInformation("Set export time bounds {0} - {1}", firstEventStart, lastEventEnd);

               // txtstatus.Text = track.TrackID + "," + path.ToString() + "," + format.ToString() + "," + firstEventStart.ToString() + "," + lastEventEnd.ToString();



                playback = new PlaybackController();

                // init writer
                writer = new MediaFileWriter();
                writer.FileFormat = format;
                writer.FileSizeLimitKB = 0; // no limit
                writer.MaximumNumberOfFiles = 0;
                writer.RecordingStartTime = new Time64() { UTC = firstEventStart };
                writer.RecordingEndTime = new Time64() { UTC = lastEventEnd };

              //  txtstatus.Text += "\r\n" + writer.RecordingStartTime.ToString() + "," + writer.RecordingEndTime.ToString();

                // set handlers
              //  writer.Progress += writer_Progress;
                writer.RecordingStopped += writer_RecordingStopped;
                writer.NewFileCreated += writer_NewFileCreated;

                var session = track.GetMediaSession(playback);




                // set stream
                writer.AddStream(session.GetVideoStream(),
                                 MediaTypeEnum.mteVideo,
                                 track.TrackID,
                                 "Track " + track.TrackID,
                                 track.SourceURL,
                                 // "rtsp://10.4.1.9",
                                 track.SourceID);

               // txtcon.Text += "addstream" + path;

                // start export
                //writer.StartRecording(textBoxOutPath.Text, String.Empty);
                writer.StartRecording(path, String.Empty);

                //frmMain formMain = (frmMain)Application.OpenForms["frmMain"];

                //Path = path;

                axwmp.URL = path;

                //create trigger finish

        
                    


                ///////////////////////////////show vdo only 270364////////////////////////////
                //path = "D:\\ExportVDOcode260364\\BoschExport\\BoschExport\\MediaDbExport\\bin\\Debug\\file_vdo\\25032021131800.mp4";
                //axwmp.URL = path;
                //////////////////////////////////////////////////////////////////
                ///

                SetConnectionState(ConnectionState.Export, "Export in progress");

                //textBoxOutPath.Enabled = false;
                //buttonOutFile.Enabled = false;
                //comboBoxTracks.Enabled = false;
                //textBoxDeviceIP.Enabled = false; 
               /* 
                downloadvideo dwn = new downloadvideo();

                dwn.Date = date;

                dwn.Time = time;

                dwn.Path = path;



                if (Loadvideocomplete != null)
                {
                    Loadvideocomplete(dwn, null);
                }
                */

            }
            catch (Exception ex)
            {

                LogClass.writeLogDetail("export VDO Faild " + ex.ToString());

            }



        }

        private void writer_NewFileCreated(string Filename)
        {
            Trace.TraceInformation("Export file created {0}", Filename);
            LogClass.writeLogDetail(Filename);

        }

        private void writer_RecordingStopped(RecordingStoppedEnum reason)
        {
            Trace.TraceInformation("Export recording stoped with reason {0}", reason);

            //if (reason == RecordingStoppedEnum.rseEndTimeReached)
            //    labelExportInfo.Text = "Completed";
            //else if (reason == RecordingStoppedEnum.rseClientStoppedRecording)
            //    labelExportInfo.Text = "Aborted";
            //else if (reason == RecordingStoppedEnum.rseEndTimeReached)
            //    labelExportInfo.Text = "Finished with errors";
            //else
            //    labelExportInfo.Text = "Failed due to " + reason;

            writer = null;
            //buttonExport.Text = "Start";

            SetConnectionState(ConnectionState.Connected, "");

            LogClass.writeLogDetail("stop");

            //textBoxOutPath.Enabled = true;
            //buttonOutFile.Enabled = true;
            //comboBoxTracks.Enabled = true;
            //textBoxDeviceIP.Enabled = true;
            /*
            downloadvideo dwn = new downloadvideo();

            dwn.Date = date;

            dwn.Time = time;

            dwn.Path = Path;



            if (Loadvideocomplete != null)
            {
                Loadvideocomplete(dwn, null);
            }
            this.Close();
            */

        }

        private void writer_Progress(int TotalEstSec, int ElapsedSec, int KBytesWritten, Time64 pCurrentRecTime, int ErrorCount)
        {
            try
            {

                int progress = ElapsedSec * 100 / TotalEstSec;
                //progressBarLoading.Value = progress;

                string errorsInfo = ErrorCount == 0 ? String.Empty : ErrorCount.ToString("{0} errors.");
                string remainingTimeInfo = new TimeSpan(0, 0, TotalEstSec - ElapsedSec).ToString();

                string info = string.Format("Export in progress {0}%... {1} {2} Kb written. Remaining time : {3}",
                    progress,
                    errorsInfo,
                    KBytesWritten,
                    remainingTimeInfo);

                //labelExportInfo.Text = info;
                Trace.TraceInformation(info);

            }
            catch (Exception ex)
            {
                LogClass.writeLogDetail("write vdo failed " + ex.ToString());

            }

        }



        #region UI aux methods

        private void Clear(bool clearLog)
        {
            if (clearLog)
                //textBoxLog.Clear();

                //comboBoxTracks.Items.Clear();

                trackToBeLoaded.Clear();
            tracksCompletelyLoaded = 0;

            //progressBarLoading.Value = 0;

            // deselect track item
            SelectTrack(-1);
        }

        private static void SafeInvoke(Control uiElement, Action updateAction)
        {
            if (uiElement == null)
            {
                throw new ArgumentNullException("uiElement");
            }

            if (uiElement.InvokeRequired)
            {
                uiElement.BeginInvoke((Action)(() => SafeInvoke(uiElement, updateAction)));
            }
            else
            {
                updateAction();
            }
        }
        #endregion
        private void ConnectVdo()
        {
            try
            {
                // connect


                if (State == ConnectionState.Disconnected)
                {

                    Clear(true);

                    string info = string.Format("Connecting to {0}...", ConnectPath);
                    SetConnectionState(ConnectionState.Connecting, info);
                   // txtstatus.Text = "disconnectset";
                    // it is recommended to use a ProgID string in the connect call
                    deviceConnector.ConnectAsync("srvadmin:@" + IPnvr.Trim(), "GCA.VIP.DeviceProxy");
                    // deviceConnector.ConnectAsync("rtsp://user:srvadmin@10.43.56.8", "GCA.VIP.DeviceProxy");
                    //"GCA.VIP.DeviceProxy"
                    // txtstatus.Text = "disconnect";
                }

                // disconect
                if (State == ConnectionState.Connected)
                {
                   // txtstatus.Text = "connect";
                    SetConnectionState(ConnectionState.Disconnecting, "Disconnecting...");

                    proxy.Disconnect();

                    SetConnectionState(ConnectionState.Disconnected, "Disconnected");

                    Clear(false);
                }
            }
            catch (Exception exp)
            {
                LogClass.writeLogDetail("connect vdo failed " + exp.ToString());
                //textBoxLog.Text = exp.ToString();
            }
        }



        private void ExportStart()
        {
            try
            {


                //File.Delete(exportFilePath);

                if (writer != null)
                {
                    writer.StopRecording();
                    writer = null;

                    //buttonExport.Text = "Start";
                    return;
                }

                //buttonExport.Text = "Stop";

                //labelExportInfo.Text = "Start Export track #" + exportTrack.TrackID + "...";

                //exportFormat = string.Equals(".asf", ".asf", StringComparison.CurrentCultureIgnoreCase)
                //               ? MediaFileFormatEnum.mffASF
                //               : MediaFileFormatEnum.mffMPEGActiveX;

                exportFormat = MediaFileFormatEnum.mffISOMP4;

                exportTimeStart = dateStartSet.AddSeconds(-(timePeriod / 2));
                exportTimeEnd = dateStartSet.AddSeconds(timePeriod/2);

                Export(exportTrack, exportFilePath, exportFormat, exportTimeStart, exportTimeEnd);

                axwmp.settings.autoStart = true;

                //Thread playThread = new Thread(AutoPlayThread);
                //playThread.Start();

            }
            catch (Exception ex)
            {
                LogClass.writeLogDetail(" export failed " + ex.ToString());

            }

        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            axwmp.close();
            axwmp.Dispose();
        }

        private void LoadConfigfile(string date,string time)
        {
            // dateStartSet = Convert.ToDateTime("01/04/2564 17:30:10", new CultureInfo("th-TH"));
            try
            {
                string dateT, dateTrandaction = "";
                string[] arrDateT;
                // dateT = publicValue.trx_Date.Trim();

                //arrDateT = dateT.Split('/');
                //  dateTrandaction = arrDateT[0] + "/" + arrDateT[1] + "/" + arrDateT[2];//swap date , month
                dateStartSet = Convert.ToDateTime(date + " " + time, new CultureInfo("en-US"));


                // dateStartSet = Convert.ToDateTime("29/04/2564 15:30:19", new CultureInfo("th-TH"));

                //MessageBox.Show(DateTime.ParseExact(publicValue.trx_Date.Trim() + " " + publicValue.trx_Time.Trim(), "dd/MM/yyyy HH:mm:ss", new CultureInfo("en-US")).ToString());
            }
            catch (Exception ex)
            {

                LogClass.writeLogDetail("Load config failed " + ex.ToString());

            }

            ////MessageBox.Show("====");
            //string[] args = Environment.GetCommandLineArgs();
            //if (args.Length == 0)
            //{
            //    Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location, "arg1 arg2");
            //    Close();
            //}
            //else
            //{
            //    //MessageBox.Show(  "====" +  args[1] + "===="); 
            //    dateStartSet = Convert.ToDateTime(args[1].Replace("-", " "));
            //    Camara = Convert.ToInt32(args[2]);
            //    timePeriod = Convert.ToInt32(args[3]);
            //    ConnectPath = args[4];

            //}
            //string xmlString = System.AppDomain.CurrentDomain.BaseDirectory + "VdoDataConfig.xml";

            //DataSet ds = new DataSet();
            //System.IO.FileStream fsReadXml = new System.IO.FileStream(xmlString, System.IO.FileMode.Open);
            //try
            //{
            //    ds.ReadXml(fsReadXml);
            //    dateStartSet = Convert.ToDateTime(ds.Tables["Config"].Rows[0]["TranDatetime"]);
            //    ConnectPath = ds.Tables["Config"].Rows[0]["Username_NVR"] + ":" + ds.Tables["Config"].Rows[0]["Password_NVR"] + "@" + ds.Tables["Config"].Rows[0]["IP_NVR"];
            //    timePeriod = Convert.ToInt32(ds.Tables["Config"].Rows[0]["TimePeriod"]);
            //    Camara = Convert.ToInt32(ds.Tables["Config"].Rows[0]["LaneNumber"]);
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.ToString());
            //}
            //finally
            //{
            //    fsReadXml.Close();
            //}
        }

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            //if (radioButtonMp4.Checked)
            //{
            //    //preparing MOV exporter
            //    //exporter = Logger.LogOnError(() => cameoSdk.ExporterFactory.CreateMp4Exporter());
            //}
            //else if (radioButtonMov.Checked)
            //{
            //    //preparing MOV exporter
            //    //exporter = Logger.LogOnError(() => cameoSdk.ExporterFactory.CreateMovExporter());
            //}
            //else
            //{
            //    //preparing ASF exporter
            //   // exporter = Logger.LogOnError(() => cameoSdk.ExporterFactory.CreateAsfExporter());
            //}

            //if (exporter != null)
            //{
            //    //exporter.ProgressChanged += OnExporterProgressChanged;
            //    //exporter.ExportComplete += OnExporterExportComplete;

            //    try
            //    {
            //        //enumerating cameras and add them to exporter
            //        foreach (var camera in cameras)
            //            exporter.AddCamera(camera);

            //        if (radioButtonMov.Checked || radioButtonMp4.Checked)
            //            //start async export
            //            exporter.StartExportAsync(dateTimePickerStartTime.Value.ToUniversalTime(), dateTimePickerEndTime.Value.ToUniversalTime(),
            //                textBoxExportPath.Text, textBoxName.Text);
            //        else
            //            exporter.StartExportAsync(dateTimePickerStartTime.Value.ToUniversalTime(), dateTimePickerEndTime.Value.ToUniversalTime(),
            //                                      textBoxExportPath.Text, textBoxName.Text,
            //                                      (ExportQuality)comboBoxFormatQuality.SelectedIndex);


            //        progressBarExport.Value = 0;
            //        progressBarExport.Visible = true;
            //        buttonExport.Enabled = false;
            //    }
            //    catch (Exception ex)
            //    {
            //        //Logger.Log(ex);
            //    }
            //}
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            //folderBrowserDialog.SelectedPath = textBoxExportPath.Text;
            //if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
            //    textBoxExportPath.Text = folderBrowserDialog.SelectedPath;
        }

        private void btnExportImage_Click(object sender, EventArgs e)
        {
            axwmp.URL = System.Environment.CurrentDirectory + "\\file_vdo\\08042021100034.mp4";

            axwmp.Ctlcontrols.play();
            axwmp.settings.autoStart = true;

            //string path0 = @"D:\vdos\02042021152702.mp3";// Test VDO 100664
            //axwmp.URL = path0;
            axwmp.Ctlcontrols.play();
            //axwmp.settings.autoStart = true;
        }


        //private Image takeImage(int width, int height)
        //{

        //    Bitmap img = new Bitmap(width, height);
        //    Graphics gp = Graphics.FromImage(img);
        //   // var size = new Size(Screen.PrimaryScreen.Bounds.Size.Height - 100,
        //             // Screen.PrimaryScreen.Bounds.Size.Width - 100);
        //   // gp.CopyFromScreen(100, 100, 0, 0, size);

        //    //gp.CopyFromScreen(expVDO.axwmp.PointToScreen(expVDO.axwmp.ClientRectangle.Location), Point.Empty, expVDO.axwmp.ClientSize);
        //    gp.Dispose();
        //    return img;
        //}




        //private void Upload(string fileName)
        //{
        //    try {

        //        var client = new WebClient();
        //        var uri = new Uri("http://" + configINI.strImageServer.Trim() + "/");
        //        {
        //            client.Headers.Add("fileName", System.IO.Path.GetFileName(fileName));
        //            client.UploadFileAsync(uri, fileName);
        //        }

        //    } catch (Exception ex)
        //    {
        //        LogClass.writeLogDetail(" upload image faileld "+ex.ToString());

        //    }

        //}
    }
}
