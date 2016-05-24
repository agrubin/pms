using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication2
{
    class Program
    {
        const string username = "SPIOrangeTest";
        const string password = "ymdqMsjBNutXLQMdVvtJZVXe";
        const string pmsID = "SPIORANGE";
        const string hotelCode = "SPI516";


        static void Main(string[] args)
        {
            ReservationError resErr = new ReservationError(ERR.Hotel_not_active, EWT.Processing_exception, "hello");
            pmsXchange.OTA_PingRS response = SiteMinder.OTA_PingRS(username, password);
            pmsXchange.OTA_ResRetrieveRS reservations = SiteMinder.OTA_ReadRQ(pmsID, username, password, hotelCode, ResStatus.All);
            
        }
    }
}
