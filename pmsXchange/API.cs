﻿using System;
using System.Configuration;
using System.ServiceModel.Configuration;
using System.ServiceModel;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;

namespace pmsXchange
{
    //
    // The .NET service reference for pmsXchange uses a special version of the SiteMinder
    // WSDL file generated specifically for .NET clients:
    //
    // https://cmtpi.siteminder.com/pmsxchangev2/services/SPIORANGE/pmsxchange_flat.wsdl 
    //

    public enum ResStatus
    {
        All,        // All reservations.
        Book,       // New reservations.
        Modify,     // Modified reservations.
        Cancel      // Cancelled reservations.
    }

    // EWT      Error Type              Description
    // ---      ----------              -----------      
    // 1	    Unknown                 Indicates an unknown error.
    // 3	    Biz rule                Indicates that the XML message has passed a low-level validation check, but that the business rules for the request message were not met.
    // 4	    Authentication          Indicates the message lacks adequate security credentials.
    // 6	    Authorization           Indicates the message lacks adequate security credentials.
    // 10	    Required field missing  Indicates that an element or attribute that is required in by the schema (or required by agreement between trading partners) is missing from the message.
    //                                  For PmsXchange this type will also be returned if the xml message does not meet the restrictions (e.g data types) specified by the xml schema.
    // 12	    Processing exception    Indicates that during processing of the request that a not further defined exception occurred.

    public enum EWT
    {
        Unknown = 1,
        Biz_rule = 3,
        Authentication = 4,
        Authorization = 6,
        Required_field_missing = 10,
        Processing_exception = 12
    }

    //  ERR     Error Code                                      Description
    //  ---     ----------                                      -----------
    //  249     Invalid rate code                               Rate does not exist.
    //  375     Hotel not active                                Hotel is not enabled to receive inventory updates.
    //  385     Invalid confirmation or cancellation number     Confirmation or cancellation number does not exist.
    //  392     Invalid hotel code                              Hotel does not exist.
    //  402     Invalid room type                               Room does not exist.
    //  448     System error
    //  450     Unable to process	 
    //  783     Room or rate not found                          Room and rate combination does not exist.

    public enum ERR
    {
        Invalid_rate_code = 249,
        Hotel_not_active = 375,
        Invalid_confirmation_or_cancellation_number = 385,
        Invalid_hotel_code = 392,
        Invalid_room_type = 402,
        System_error = 448,
        Unable_to_process = 450,
        Room_or_rate_not_found = 783
    }

    public class ReservationError
    {
        public ERR err { get; set; }
        public EWT ewt { get; set; }
        public string errorText { get; set; }
        public ReservationError(ERR _err, EWT _ewt, string _errorText)
        {
            err = _err;
            ewt = _ewt;

            //
            // Since this text is going into an XML node, invalid chars must be escaped with XML entities.
            //

            string xml = _errorText;
            errorText = xml.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }
    }

    //
    // The service connection is implemeted as a singleton so it is only instantiated and initalized one time
    // upon the first access, then the same connection is returned on each subsequent access.
    //
    public sealed class ServiceConnection
    {
        public static ServiceConnection Instance { get { return lazyConnection.Value; }  }
        private const string endpointURI = "https://cmtpi.siteminder.com/pmsxchangev2/services/SPIORANGE";  // Provided by SiteMinder.
        private static readonly Lazy<ServiceConnection> lazyConnection = new Lazy<ServiceConnection>(() => new ServiceConnection());
        public pmsXchangeService.PmsXchangeServiceClient service { get; private set; }

        private ServiceConnection()
        {
            InitializeService();
        }

        public void InitializeService()
        {
            BasicHttpBinding binding = new BasicHttpBinding();
            binding.Security.Mode = BasicHttpSecurityMode.Transport;

            EndpointAddress address = new EndpointAddress(endpointURI);
            service = new pmsXchangeService.PmsXchangeServiceClient(binding, address);
        }
    }

    public static class API
    {
        private const string requestorIDType = "22";  // This value is provided by SiteMinder.
        private const string textType = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText";

        static public void OTA_NotifReportRQ(string usernameAuthenticate, string passwordAuthenticate)
        {
            pmsXchangeService.PmsXchangeServiceClient service = new pmsXchangeService.PmsXchangeServiceClient();

            pmsXchangeService.OTA_NotifReportRQ otaRequestBody = new pmsXchangeService.OTA_NotifReportRQ();

            service.NotifReportRQ(CreateSecurityHeader(usernameAuthenticate, passwordAuthenticate), otaRequestBody);
        }

        static public pmsXchangeService.OTA_ResRetrieveRS OTA_ReadRQ(string pmsID, string usernameAuthenticate, string passwordAuthenticate, string hotelCode, ResStatus resStatus)
        {
            pmsXchangeService.PmsXchangeServiceClient service = ServiceConnection.Instance.service;

            pmsXchangeService.OTA_ReadRQ readRequestBody = new pmsXchangeService.OTA_ReadRQ();
            readRequestBody.Version = 1.0M;
            readRequestBody.EchoToken = Guid.NewGuid().ToString();  // Echo token must be unique.            
            readRequestBody.TimeStamp = DateTime.Now;
            readRequestBody.TimeStampSpecified = true;
            readRequestBody.POS = CreatePOS(pmsID);

            pmsXchangeService.OTA_ReadRQReadRequests readRequests = new pmsXchangeService.OTA_ReadRQReadRequests();
            pmsXchangeService.OTA_ReadRQReadRequestsHotelReadRequest hotelReadRequest = new pmsXchangeService.OTA_ReadRQReadRequestsHotelReadRequest();
            hotelReadRequest.HotelCode = hotelCode;
            readRequests.Items = new object[] { hotelReadRequest };

            pmsXchangeService.OTA_ReadRQReadRequestsHotelReadRequestSelectionCriteria selectionCriteria = new pmsXchangeService.OTA_ReadRQReadRequestsHotelReadRequestSelectionCriteria();
            selectionCriteria.SelectionType = pmsXchangeService.OTA_ReadRQReadRequestsHotelReadRequestSelectionCriteriaSelectionType.Undelivered;
            selectionCriteria.SelectionTypeSpecified = true;  // Must be set to true, or ReadRQ returns an error.

            if (resStatus != ResStatus.All)
            {
                selectionCriteria.ResStatus = Enum.GetName(typeof(ResStatus), resStatus);
            }

            hotelReadRequest.SelectionCriteria = selectionCriteria;


            readRequestBody.ReadRequests = readRequests;

            //
            // Send a retrieve reservations request.
            //

            return service.ReadRQ(CreateSecurityHeader(usernameAuthenticate, passwordAuthenticate), readRequestBody);
        }

        static public async Task<pmsXchangeService.PingRQResponse> OTA_PingRS(string usernameAuthenticate, string passwordAuthenticate)
        {
            pmsXchangeService.PmsXchangeServiceClient service = ServiceConnection.Instance.service;

            pmsXchangeService.OTA_PingRQ pingRequestBody = new pmsXchangeService.OTA_PingRQ();
            pingRequestBody.Version = 1.0M;
            pingRequestBody.EchoToken = Guid.NewGuid().ToString();  // Echo token must be unique.            
            pingRequestBody.TimeStamp = DateTime.Now;
            pingRequestBody.TimeStampSpecified = true;
            pingRequestBody.EchoData = "good echo";

            //
            // Send a ping request.
            //
           
            return await service.PingRQAsync(CreateSecurityHeader(usernameAuthenticate, passwordAuthenticate), pingRequestBody);
        }

        static private pmsXchangeService.SecurityHeaderType CreateSecurityHeader(string usernameAuthenticate, string passwordAuthenticate)
        {
            pmsXchangeService.SecurityHeaderType securityHeader = new pmsXchangeService.SecurityHeaderType();
            securityHeader.Any = CreateUserNameToken(usernameAuthenticate, passwordAuthenticate);
            return securityHeader;
        }

        static private pmsXchangeService.SourceType[] CreatePOS(string pmsID)
        {
            pmsXchangeService.SourceTypeRequestorID strid = new pmsXchangeService.SourceTypeRequestorID();
            strid.Type = requestorIDType;
            strid.ID = pmsID;

            pmsXchangeService.SourceType sourcetype = new pmsXchangeService.SourceType();
            sourcetype.RequestorID = strid;

            return new pmsXchangeService.SourceType[] { sourcetype };
        }

        static private System.Xml.XmlElement[] CreateUserNameToken(string usernameAuthenticate, string passwordAuthenticate)
        {
            //
            // Not all the SOAP envelope elements are available through the request block intellisense.
            // Those that aren't must be added as XmlElements.
            //

            XmlDocument doc = new XmlDocument();
            XmlElement usernametoken = doc.CreateElement("UsernameToken");
            XmlElement password = doc.CreateElement("Password");
            XmlElement username = doc.CreateElement("Username");

            //
            // Password is transmitted in plain text.
            //

            password.SetAttribute("Type", textType);

            XmlText usernameText = doc.CreateTextNode(usernameAuthenticate);
            XmlText passwordText = doc.CreateTextNode(passwordAuthenticate);

            username.AppendChild(usernameText);
            password.AppendChild(passwordText);
            usernametoken.AppendChild(username);
            usernametoken.AppendChild(password);

            return new XmlElement[] { usernametoken };
        }
    }
}
