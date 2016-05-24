using pmsXchange;
using pmsXchange.pmsXchangeService;


namespace SiteMinder
{
    class Program
    {
        const string username = "SPIOrangeTest";
        const string password = "ymdqMsjBNutXLQMdVvtJZVXe";
        const string pmsID = "SPIORANGE";
        const string hotelCode = "SPI516";


        static void Main(string[] args)
        {
            
            //ReservationError resErr = new ReservationError(ERR.Hotel_not_active, EWT.Processing_exception, "hello");
            
            OTA_PingRS pingResponse =  API.OTA_PingRS(username, password);

            OTA_ResRetrieveRS reservationsResponse = API.OTA_ReadRQ(pmsID, username, password, hotelCode, ResStatus.All);
            
        }
    }
}
