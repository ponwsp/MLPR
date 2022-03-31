using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MLPR
{
    class header
    {
       public int sender { get; set; }

       public int receiver { get; set; }
        
       public DateTime date { get; set; }

        public int msg { get; set; }


        public int seq { get; set; }


       
       
    }
    class toll
    {
        public int PLaza_ID { get; set;}

        public int Lane_No { get; set; }

        public int Lane_Station { get; set; }

    }
    class tariff
    {
        public int Tariff_Active_Version { get; set; }

        public int Tariff_Download_Version { get; set; }
    }
    class collector
    {
        public string Collector_No { get; set; }

        public string Collector_ID { get; set; }

        public string Collector_Position { get; set; }

        public string Collector_Name { get; set; }

        public string Job_No { get; set; }

        public string Job_ID { get; set; }

        public string On_duty_ID { get; set; }


        public string Operation_Date_and_Time { get; set; }


        public string Shift_ID { get; set; }

        public string BOJ_Date_and_Time { get; set; }

        public string EOJ_Date_and_Time { get; set; }
    }
    class trx
    {
        public int type { get; set; }

        public int Class { get; set; }

        public int payment { get; set; }

        public List<Array> route { get; set; }
    }
    class catridge
    {

    }
    class entry
    {

    }
    class avc
    {

    }
    class obu
    {

    }
    class credit
    {

    }
    class lpr
    {

    }

}
