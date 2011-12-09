using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace RGALocatorReception.BusinessObjects
{
    public class Colleague
    {
        public string ColleagueID { get; set; }

        public string UserName { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName { get; set; }

        public string Title { get; set; }

        public string CurrentLocation { get; set; }

        public string CurrentLocationId { get; set; }

        public string PhotoURL { get; set; }

        public string ThumbnailURL { get; set; }

        public string MobilePhone { get; set; }

        public DateTime LastAction { get; set; }

        public Colleague()
        {

        }
    }
}
