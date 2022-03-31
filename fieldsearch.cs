using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MLPR
{
    class fieldsearch
    {
        public string isadj { get; set; }

        public string datetime { get; set; }

        public string plaza { get; set; }

        public string lane { get; set; }

        public string  lane_mode { get; set; }

        public string  job { get; set; }

        public string staff_id { get; set; }

        public int @class { get; set; }

        public string avc { get; set; }




        public string color { get; set; }

        public string brand { get; set; }

        public string license { get; set; }

        public string province { get; set; }

        public string acc { get; set; }

    }

    public static class convertdata
    {
        public static string convertcolorid(string colorname,string[] datacolorid,string[] datacolorname)
        {
            try
            {
                int i = 0;
                foreach (string color in datacolorid)
                {
                    if (color.Trim() == colorname.Trim())
                    {

                        return datacolorname[i];
                    }
                    i++;
                }


                return "ไม่ระบุ";
            }
            catch
            {
                return "ไม่ระบุ";
            }
        }
        public static string convertbrandid(string brandname,string[] databrandid,string[] databrandname)
        {
            try
            {
                int i = 0;

                foreach (string brand in databrandid)
                {
                    if (brand.Trim() == brandname.Trim())
                    {
                        return databrandname[i];
                    }
                    i++;
                }

                return "ไม่สามารถระบุ";
            }
            catch { return "ไม่สามารถระบุ"; }
      
        }
        public static string convertprovinceid(string provincename,string[] dataprovinceid,string[] dataprovincename)
        {
            try
            {
                int i = 0;

                foreach (string provinceid in dataprovinceid)
                {
                    if (provinceid == provincename)
                    {
                        return dataprovincename[i];
                    }
                    i++;
                }

                return "ไม่สามารถระบุ";
            }
            catch { return "ไม่สามารถระบุ"; }


        }
    
    }
}
