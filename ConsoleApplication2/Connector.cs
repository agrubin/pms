using System;

public class Connector {
	static int ConnectSample()
		{
			PmsXchangeService service = new PmsXchangeService(); 
			System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
			//The security header is untyped due to the lax processing instruction
			System.Xml.XmlElement token = doc.CreateElement("UsernameToken");
			System.Xml.XmlElement password = doc.CreateElement("Password");
			System.Xml.XmlElement username = doc.CreateElement("Username");

			System.Xml.XmlText usernameText = doc.CreateTextNode("SPIOrangeTest");
			System.Xml.XmlText passwordText = doc.CreateTextNode("YOURPASSWORD");
			username.AppendChild(usernameText);
			password.AppendChild(passwordText);
			token.AppendChild(username);
			token.AppendChild(password);
	 
			System.Xml.XmlElement[] elements = new System.Xml.XmlElement[] { token };

			SecurityHeaderType type = new SecurityHeaderType();
			service.Security = type;
			service.Security.Any = elements;
		
			OTA_ReadRQ otaReadRQ = new OTA_ReadRQ();
			otaReadRQ.Version = 1.0M;
			OTA_ReadRQReadRequests rr = new OTA_ReadRQReadRequests();
			OTA_ReadRQReadRequestsHotelReadRequest hotelR = new OTA_ReadRQReadRequestsHotelReadRequest();

			hotelR.HotelCode = "123";

			object[] ob = new object[] { hotelR };
			rr.Items = ob;

		   OTA_ReadRQReadRequestsHotelReadRequestSelectionCriteria crit = new OTA_ReadRQReadRequestsHotelReadRequestSelectionCriteria();
		   OTA_ReadRQReadRequestsHotelReadRequestSelectionCriteriaSelectionType selType = OTA_ReadRQReadRequestsHotelReadRequestSelectionCriteriaSelectionType.Undelivered;
		   crit.SelectionType = selType;
		   crit.SelectionTypeSpecified = true;    

			hotelR.SelectionCriteria = crit;
			otaReadRQ.ReadRequests = rr;
			// Retrieve the response
			Console.WriteLine("About to make request :::");
			OTA_ResRetrieveRS resRetrieveRS = service.ReadRQ(otaReadRQ);
			Console.WriteLine("Received response :::");

			//Do further work ....
			// ....
			return 0;
		}
}