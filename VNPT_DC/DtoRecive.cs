using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNPT_DC
{
   public class DtoRecive
    {
        public string NhietDo { get; set; }
        public string DienAp { get; set; }
        public string I1 { get; set; }
        public string I2 { get; set; }
        public string I3 { get; set; }
        public string P1Hour { get; set; }
        public string P2Hour { get; set; }
        public string P3Hour { get; set; }
        public string Year { get; set; }
        public string Month { get; set; }
        public string Day { get; set; }
        public string Hour { get; set; }
        public string Min { get; set; }
        public string P1 { get; set; }
        public string P2 { get; set; }
        public string P3 { get; set; }
        public DtoRecive(  string NhietDo ,
         string DienAp ,
         string I1 ,
         string I2 ,
         string I3 ,
         string P1Hour ,
         string P2Hour ,
         string P3Hour ,
         string Year ,
         string Month ,
         string Day ,
         string Hour ,
         string Min ,
         string P1 ,
         string P2 ,
         string P3 )
        {
            this.NhietDo = NhietDo;
            this.DienAp = DienAp;
         this.I1 = I1;
            this.I2 = I2;
            this.I3 = I3;
            this.P1Hour = P1Hour;
            this.P2Hour = P2Hour;
            this.P3Hour = P3Hour;
            this.Year = Year;
            this.Month = Month;
            this.Day = Day;
            this.Hour = Hour;
            this.Min = Min;
            this.P1 = P1;
            this.P2 = P2;
            this.P3 = P3;
        }

    }
}
