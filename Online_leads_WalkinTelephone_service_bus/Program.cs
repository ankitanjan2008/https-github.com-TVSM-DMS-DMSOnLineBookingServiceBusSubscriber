using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using System.Reflection;
using System.Net.Mail;
using System.Configuration;
using System.Diagnostics;
//using System.Net;
using System.Text.RegularExpressions;
using RestSharp;
using Online_leads_WalkinTelephone_service_bus.Model;

namespace Online_leads_WalkinTelephone_service_bus
{
    class Program
    {

        static SqlConnection OpenDMSLiveConnection()
        {
            string strDMSConn = System.Configuration.ConfigurationManager.ConnectionStrings["DMSConnection"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection();
            sqlConn.ConnectionString = strDMSConn;
            if (sqlConn.State == ConnectionState.Closed)
            {
                sqlConn.Open();
            }
            return sqlConn;
        }
        static SqlConnection OpenTravelConnection()
        {
            string strDMSConn = System.Configuration.ConfigurationManager.ConnectionStrings["TravelConn"].ConnectionString;
            SqlConnection sqlConn = new SqlConnection();
            sqlConn.ConnectionString = strDMSConn;
            if (sqlConn.State == ConnectionState.Closed)
            {
                sqlConn.Open();
            }
            return sqlConn;
        }
        static void CloseLiveConnection(SqlConnection sqlConn, SqlCommand cmd, SqlDataReader Reader)
        {
            if (Reader != null)
            {
                if (!Reader.IsClosed)
                {
                    Reader.Close();
                }
            }
            if (sqlConn.State == ConnectionState.Open)
            {
                sqlConn.Close();
            }
            if (cmd != null)
            {
                cmd.Dispose();
                cmd.Parameters.Clear();
            }
        }
        

        static void Main(string[] args)
        {
            


            //******************************************************************************************

            string Msg = null;
            //DMS 1124 Start


            Stopwatch Sw = new Stopwatch(); Sw.Start();
            Msg = string.Empty;
            Msg += "------------------------------------------------------------------------------------------------------\r\n";
            Msg += "Booking information posting to NGD system started Date of: " + DateTime.Now.ToString("dd-MM-yyyy") + " and Time of " + DateTime.Now.ToShortTimeString() + "\r\n";
            Msg += "-----------------------------------------------------------------------------------------------------\r\n\r\n\r\n";
            CreateLogFiles.WriteFile(Msg);
            InsertBookingInfoToNGD();
            Msg = string.Empty;
            Msg += "------------------------------------------------------------------------------------------------------\r\n";
            Msg += "Booking information posting to NGD system Ended at Date of: " + DateTime.Now.ToString("dd-MM-yyyy") + " and Time of " + DateTime.Now.ToShortTimeString() + "\r\n";
            Msg += "-----------------------------------------------------------------------------------------------------\r\n\r\n\r\n";
           // CreateLogFiles.WriteFile(Msg);

            

            Sw.Stop(); TimeSpan ts = Sw.Elapsed;



        }


        public static void InsertBookingInfoToNGD()
        {
            SqlConnection sqlConn;
            SqlCommand sqlcmd;
            DataTable dt = new DataTable();

            try
            {
                using (sqlConn = OpenDMSLiveConnection())
                {
                    using (sqlcmd = new SqlCommand())
                    {
                        sqlcmd.Connection = sqlConn;

                        sqlcmd.CommandType = CommandType.StoredProcedure;
                        sqlcmd.CommandText = "pr_GetBookingInfoServiceBus";
                        sqlcmd.CommandTimeout = 120;
                        dt.Load(sqlcmd.ExecuteReader());
                    }
                }
                List<BookingInfoObjDO> processedBookingLst = new List<BookingInfoObjDO>();

                List<BookingInfoObjDO> BookingInfoObjDOLst = dt.AsEnumerable()
                .Select(dr => new BookingInfoObjDO
                {
                    bookingId = Convert.ToString(dr["BOOKING_ID"]),
                    bookingNumber = Convert.ToString(dr["BOOKING_NO"]),
                    dealerId = dr["DEALER_ID"].ToString(),
                    branchId = dr["BRANCH_ID"].ToString(),
                    bookingDate = dr["BOOKING_DATE"].ToString(),
                    internetEnquiryId = dr["INTERNET_ENQUIRY_ID"].ToString(),
                    UUID= dr["UUID"].ToString(),
                }).ToList();

                foreach (var item in BookingInfoObjDOLst)
                {
                    if (PushDatatoNGDAPI(item))
                    {
                        processedBookingLst.Add(item);
                    };
                }

                if (processedBookingLst.Count > 0)
                {
                    DataTable bookingLst = ToDataTable(processedBookingLst);

                    sqlConn = OpenDMSLiveConnection();
                    using (sqlcmd = new SqlCommand())
                    {
                        sqlcmd.CommandType = CommandType.StoredProcedure;
                        sqlcmd.Connection = sqlConn;
                        sqlcmd.CommandText = "Pr_ProcessedBookingInfo";
                        sqlcmd.CommandTimeout = 120;
                        SqlDataAdapter SD = new SqlDataAdapter(sqlcmd);
                        sqlcmd.Parameters.AddWithValue("@tblBookings", bookingLst);
                        sqlcmd.ExecuteNonQuery();
                    }
                }

            }
            catch (Exception ex)
            {
                //CreateLogFiles.WriteFile(ex.Message + "\r\n");
                throw new Exception(ex.Message);
            }

        }

        public static bool PushDatatoNGDAPI(BookingInfoObjDO obj)
        {
            try
            {
                DMSUpdateQuickBookingStatusDTO enquiry = new DMSUpdateQuickBookingStatusDTO();


                enquiry.DMSBookingNo =Convert.ToInt64( obj.bookingNumber);
                enquiry.DMSEnquiryNo = Convert.ToInt64(obj.bookingNumber);
                //enquiry.UUID = obj.internetEnquiryId;
                enquiry.UUID = obj.UUID;
                //enquiry.UUID = "9245fe4a-d402-451c-b9ed-9c1a04247482";
                enquiry.BookingId = null;
                DateTime now = DateTime.Now;
                string newTime = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);
                enquiry.DMSCreatedDate = newTime /*"2023-06-15T20:36:32.656Z"*/;


                USERDO USER = new USERDO();
                USER.UserId = null;
                enquiry.User = USER;

                SendBookingConfirmationBusService(enquiry);
               
                

                return  true ;
            }
            catch (Exception ex)
            {
               // CreateLogFiles.WriteFile(ex.Message + "\r\n");
                throw new Exception(ex.Message);
            }
        }

        

        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }

            return dataTable;
        }

      
        public static async Task SendBookingConfirmationBusService(DMSUpdateQuickBookingStatusDTO requestBody)

        {
            string serviceBusConn = "Endpoint=sb://enquirygenerationbus.servicebus.windows.net/;SharedAccessKeyName=DMS_SAS_Policy;SharedAccessKey=ROAmHW12KnMIRrYRCrsaPvYan3JQl1O0/+ASbOtHRXg=;EntityPath=booking";
            //string serviceBusConn = ConfigurationManager.AppSettings["serviceBusConnnnectionEndPoint"];
            //string serviceBusConn = "Endpoint=sb://enquirygenerationbus.servicebus.windows.net/;SharedAccessKeyName=DMS_SAS_Policy;SharedAccessKey=ROAmHW12KnMIRrYRCrsaPvYan3JQl1O0/+ASbOtHRXg=;EntityPath=booking";
            //string serviceBustopic = ConfigurationManager.AppSettings["serviceBustopic"];
            string serviceBustopic = "booking";
            ServiceBusClient client = new ServiceBusClient(serviceBusConn);
            ServiceBusSender sender = client.CreateSender(serviceBustopic);
            try
            {
               // ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();
                string body = System.Text.Json.JsonSerializer.Serialize(requestBody);
                var serviceBusMessage = new ServiceBusMessage(body);
                serviceBusMessage.ApplicationProperties.Add(EventPropertyKeys.EventType, "BOOKING_CONFIRMATION");
                serviceBusMessage.ApplicationProperties.Add(EventPropertyKeys.CreatedAt, DateTime.UtcNow);
                serviceBusMessage.ApplicationProperties.Add(EventPropertyKeys.EventSource, "DMS");
                serviceBusMessage.ApplicationProperties.Add(EventPropertyKeys.Version, "1");
                serviceBusMessage.ApplicationProperties.Add(EventPropertyKeys.EventTarget, "ALL");
                //if (!messageBatch.TryAddMessage(serviceBusMessage))
                //{
                //    // log the error
                //}
                List<ServiceBusMessage> messages = new List<ServiceBusMessage>();
                messages.Add(serviceBusMessage);

                try
                {
                    await sender.SendMessagesAsync(messages);
                }
                catch (Exception ex)
                {
                    throw ex; 
                }
                //Responsecls result = new Responsecls() { StatusCode = 200, Message = "OK" };
                // return result;
            }
            catch (Exception ex)
            {
                //logs 
                //Responsecls result = new Responsecls() { StatusCode = 400, Message = ex.Message };
                //return result;
            }
            finally
            {
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }

        }

      
        
        
    }
}
