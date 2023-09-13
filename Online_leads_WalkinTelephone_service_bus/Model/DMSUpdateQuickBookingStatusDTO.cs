using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Online_leads_WalkinTelephone_service_bus.Model
{
    public class  DMSUpdateQuickBookingStatusDTO
    {
        public String UUID { get; set; }
        
        public Int64? BookingId { get; set; }
        
        public Int64 DMSBookingNo { get; set; }
        
        public Int64 DMSEnquiryNo { get; set; }
        
        public string DMSCreatedDate { get; set; }
        
        public USERDO User { get; set; }
    }
    public class USERDO
    {


        public Int32? UserId { get; set; }



    }
}
