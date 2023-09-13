using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Online_leads_WalkinTelephone_service_bus.Model
{
   public class BookingInfoObjDO
    {
        
            public string dealerId { get; set; }
            public string branchId { get; set; }
            public string bookingNumber { get; set; }
            public string bookingId { get; set; }
            public string internetEnquiryId { get; set; }

            public string UUID { get; set; }
            public string bookingDate { get; set; }
        
    }
}
