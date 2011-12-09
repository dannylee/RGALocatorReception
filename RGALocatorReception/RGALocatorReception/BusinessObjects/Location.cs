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
    public class Location
    {
        public string LocationID { get; set; }

        public string Name { get; set; }

        public string Building { get; set; }

        public string Floor { get; set; }

        public string Room { get; set; }

        public string Address { get; set; }
    }
}
