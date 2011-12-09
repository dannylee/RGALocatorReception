using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Navigation;

using System.IO;
using System.Xml;
using System.Xml.Linq;
using RGALocatorReception.BusinessObjects;

namespace RGALocatorReception
{
    public partial class ColleagueDetails : Page
    {
        private WebClient client;

        private string userName;

        private Colleague colleague;

        public ColleagueDetails()
        {
            InitializeComponent();
        }

        // Executes when the user navigates to this page.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (!this.NavigationContext.QueryString.ContainsKey("userName") ||
                string.IsNullOrEmpty(this.NavigationContext.QueryString["userName"]))
            {
                return;
            }
            userName = this.NavigationContext.QueryString["userName"];
            client = new WebClient();
            client.OpenReadCompleted += OnEmployeeServiceOpenReadComplete;
            client.OpenReadAsync(new Uri("http://locator.rgahosting.com/LocatorService.svc/employees/" + userName + "?date=" + DateTime.Now.ToString()));
        }

        private void OnEmployeeServiceOpenReadComplete(object sender, OpenReadCompletedEventArgs e)
        {
            var reader = new StreamReader(e.Result);
            var result = reader.ReadToEnd();
            XmlReader XMLReader = XmlReader.Create(new MemoryStream(System.Text.UnicodeEncoding.Unicode.GetBytes(result)));
            XDocument data = XDocument.Load(XMLReader);

            var query = from emp in data.Elements("employee")
                        select new Colleague
                        {
                            ColleagueID = emp.Element("guid").Value,
                            FirstName = emp.Element("firstname").Value,
                            LastName = emp.Element("lastname").Value,
                            Title = emp.Element("title").Value,
                            CurrentLocation = emp.Element("currentlocation").Value,
                            PhotoURL = string.Format("http://{0}", emp.Element("photourl").Value),
                            ThumbnailURL = string.Format("http://{0}", emp.Element("thumbnailurl").Value),
                            MobilePhone = emp.Element("mobilephonenumber").Value
                        };
            colleague = query.SingleOrDefault();
            this.DataContext = colleague;

        }

    }
}
