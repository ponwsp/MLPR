using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLPR
{
    class Datamlpr
    {
        public string date { get; set; }  //id


        public int client_num { get; set; }


        public string timeout_time { get; set; }  //id
    }
    public class trxadjust
    {
        public class Header
        {
            public int sender { get; set; }
            public int receiver { get; set; }
            public string date { get; set; }
            public int msg { get; set; }
            public int seq { get; set; }
        }

        public class Toll
        {
            public int plaza { get; set; }
            public int lane { get; set; }
            public int station { get; set; }
        }

        public class Tariff
        {
            public int active { get; set; }
            public int download { get; set; }
        }

        public class Collector
        {
            public string no { get; set; }
            public int id { get; set; }
            public int position { get; set; }
            public string name { get; set; }
            public int jobno { get; set; }
            public string jobid { get; set; }
            public long onduty { get; set; }
            public string operation { get; set; }
            public int shift { get; set; }
            public string boj { get; set; }
            public object eoj { get; set; }
        }

        public class Route
        {
            public int no { get; set; }
            public int entry { get; set; }
            public int exit { get; set; }
            public double fare { get; set; }
            public double penalty { get; set; }
            public double vat { get; set; }
        }

        public class No
        {
            public int all { get; set; }
            public int manual { get; set; }
            public int automatic { get; set; }
        }

        public class Trx
        {
            public int type { get; set; }
            public int @class { get; set; }
            public int payment { get; set; }
            public List<Route> route { get; set; }
            public double paid { get; set; }
            public List<object> promotion { get; set; }
            public No no { get; set; }
            public string id { get; set; }
            public int transit { get; set; }
            public List<int> passing { get; set; }
        }

        public class Avc
        {
            public int @class { get; set; }
            public int axle { get; set; }
            public int dual { get; set; }
            public int wheel { get; set; }
            public int direction { get; set; }
        }

        public class Lpr
        {
            public string no { get; set; }
            public double confno { get; set; }
            public string nochar { get; set; }
            public double confnochar { get; set; }
            public string province { get; set; }
            public double confprovince { get; set; }
            public string color { get; set; }
            public double confcolor { get; set; }
            public string brand { get; set; }
            public double confbrand { get; set; }
            public double conf { get; set; }
            public string image { get; set; }
            public string imagelp { get; set; }
        }

        public class Root
        {
            public Header header { get; set; }
            public Toll toll { get; set; }
            public Tariff tariff { get; set; }
            public Collector collector { get; set; }
            public Trx trx { get; set; }
            public Avc avc { get; set; }
            public List<Lpr> lpr { get; set; }
        }
    }
}
