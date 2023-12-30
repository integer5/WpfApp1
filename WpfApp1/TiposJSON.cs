using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class TiposMessage
    {
        public string MessageID { get; set; }
        public string GeneratedDate { get; set; }
        public TiposEvent Event { get; set; }
    }

    public class TiposEvent
    {
        public int ProviderEventID { get; set; }
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public IList<TiposOdd> OddsList { get; set; }
    }

    public class TiposOdd
    {
        public int ProviderOddsID { get; set; }
        public string OddsName { get; set; }
        public float OddsRate { get; set; }
        public string Status { get; set; }
    }

}
