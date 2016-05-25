using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OTA_Console
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //ReservationError resErr = new ReservationError(ERR.Hotel_not_active, EWT.Processing_exception, "hello");

            //API.OTA_PingRS(username, password);

            //OTA_ResRetrieveRS reservationsResponse = API.OTA_ReadRQ(pmsID, username, password, hotelCode, ResStatus.All);
        }
    }
}
