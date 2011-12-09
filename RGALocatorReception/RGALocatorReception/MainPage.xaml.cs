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
using System.Text.RegularExpressions;
using System.Windows.Navigation;

namespace RGALocatorReception
{
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;
    using RGALocatorReception.BusinessObjects;

    

    public partial class MainPage : UserControl
    {
        //private WebClient client;

        private bool busy = false;

        private string locationId = "d0dcdeab-27a3-449f-9cc4-c1ff65640f3c";

        private Colleague colleague;

        private IList<Colleague> colleagues;


        private DispatcherTimer locatorTimer;

        private DispatcherTimer colleagueTimer;

        private DispatcherTimer twitterTimer;

        bool firstRun = true;

        public MainPage()
        {
            var timeTimer = new DispatcherTimer();
            timeTimer.Interval = TimeSpan.FromSeconds(1);
            timeTimer.Tick += new EventHandler(UpdateSummaryControls);
            timeTimer.Start();


            locatorTimer = new DispatcherTimer();
            locatorTimer.Interval = TimeSpan.FromSeconds(5);
            locatorTimer.Tick += new EventHandler(UpdateLocatorInfo);
            locatorTimer.Start();


            twitterTimer = new DispatcherTimer();
            twitterTimer.Interval = TimeSpan.FromSeconds(10);
            twitterTimer.Tick += new EventHandler(UpdateTwitterFeed);
            twitterTimer.Start();

            colleagueTimer = new DispatcherTimer();
            colleagueTimer.Interval = TimeSpan.FromSeconds(10);
            colleagueTimer.Tick += new EventHandler(GoHome);


            

            InitializeComponent();

            SetUpStaticInfo();

            LoadLocatorInfo();

            LoadTwitterFeed();
        }



        private void SetUpStaticInfo()
        {
            imgBuildingPhoto.Source = new BitmapImage(new Uri(string.Format("/Images/Locations/{0}.jpg",locationId), UriKind.Relative));
        }

        private void UpdateLocatorInfo(object sender, EventArgs e)
        {
            LoadLocatorInfo();
        }

        private void UpdateTwitterFeed(object sender, EventArgs e)
        {
            LoadTwitterFeed();
        }

        private void LoadLocatorInfo()
        {
            if (locatorTimer.IsEnabled)
            {
                locatorTimer.Stop();
            }
            //R/GA Locator Services...
            var locatorClient = new WebClient();
            var uri = "http://locator.rgahosting.com/LocatorService.svc/history/?location=" + locationId + "&date=" + DateTime.Now.ToString();
            locatorClient.OpenReadCompleted += OnHistoryServiceOpenReadComplete;
            locatorClient.OpenReadAsync(new Uri("http://locator.rgahosting.com/LocatorService.svc/history/?location=" + locationId + "&date=" + DateTime.Now.ToString() + "&test=one"));

        }

        private void OnHistoryServiceProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            var percentProgress = e.ProgressPercentage;
        }

        private void OnHistoryServiceOpenReadComplete(object sender, OpenReadCompletedEventArgs e)
        {
            var reader = new StreamReader(e.Result);
            var result = reader.ReadToEnd();
            XmlReader XMLReader = XmlReader.Create(new MemoryStream(System.Text.UnicodeEncoding.Unicode.GetBytes(result)));
            XDocument xml = XDocument.Load(XMLReader);

            var xColleagues = from emp in xml.Descendants("history")
                              select emp;
            colleagues = new List<Colleague>();
            var cnt = 0;
            lstColleagues.Items.Clear();
            List<string> latestArrivals = new List<string>();

            foreach (var xColleague in xColleagues)
            {
                DateTime lastAction;
                DateTime.TryParse(xColleague.Element("timestamp").Value, out lastAction);
                var colleague = new Colleague
                {
                    ColleagueID = xColleague.Element("employeeguid").Value,
                    FullName = xColleague.Element("employeename").Value,
                    UserName = xColleague.Element("employeeusername").Value,
                    LastAction = lastAction,
                    PhotoURL = string.Format("/Images/Colleagues/{0}.jpg", xColleague.Element("employeeusername").Value.Replace(".", "_"))
                };
                colleagues.Add(colleague);
                if (!latestArrivals.Contains(colleague.UserName))
                {
                    if (cnt < 5)
                    {
                        lstColleagues.Items.Add(colleague);
                        latestArrivals.Add(colleague.UserName);
                        cnt++;
                    }
                }
                
                
            }
            var latestEntrants = from c in colleagues
                                 orderby c.LastAction descending
                                 select c;
            var latestEntrant = latestEntrants.First();
            var tenSecondsAgo = DateTime.Now.AddSeconds(-10);
            if (latestEntrant.LastAction >= tenSecondsAgo)
            {
                //txtWelcome.Text = string.Format("Good morning, {0}!", latestEntrant.FullName);
                this.ShowColleague(latestEntrant.UserName);

            }
            locatorTimer.Start();
        }

        private void LoadTwitterFeed()
        {
            if (twitterTimer.IsEnabled)
            {
                twitterTimer.Stop();
            }
            //R/GA Twitter Feed...
            var twitterClient = new WebClient();
            twitterClient.OpenReadCompleted += OnTwitterServiceCompleted;
            twitterClient.OpenReadAsync(new Uri("http://search.twitter.com/search.atom?q=RGA"));
        }

        private void OnTwitterServiceCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                return;
            }
            var res = e.Result;

            var reader = new StreamReader(res);
            var result = reader.ReadToEnd();
            XmlReader XMLReader = XmlReader.Create(new MemoryStream(System.Text.UTF8Encoding.UTF8.GetBytes(result)));

            XDocument xmlTweets = XDocument.Load(XMLReader);
            XNamespace ns = "http://www.w3.org/2005/Atom";
            var xTweets = xmlTweets.Root.Descendants(ns + "entry");

            this.lstTwitter.Items.Clear();
            var cnt = 0;
            foreach (var xTweet in xTweets)
            {
                if (cnt < 4)
                {
                    var link = xTweet.Elements(ns + "link").Where(x => x.Attribute("rel").Value == "image").SingleOrDefault();
                    var message = Regex.Replace(xTweet.Element(ns + "content").Value, "<.*?>", string.Empty);
                    var tweet = new TwitterItem
                    {
                        ImageSource = link.Attribute("href").Value,
                        Message = message,
                        UserName = xTweet.Element(ns + "author").Element(ns + "name").Value
                    };
                    this.lstTwitter.Items.Add(tweet);
                }
                cnt++;
            }
            twitterTimer.Start();
        }

        

        private void UpdateSummaryControls(object sender, EventArgs e)
        {
            txtHomeHeader.Text = DateTime.Today.ToString("dddd");
            txtDay.Text = DateTime.Today.ToString("dddd");
            txtMonthYear.Text = DateTime.Today.ToString("MMMM yy");
            txtTime.Text = DateTime.Now.ToString("HH:mm");
            txtAMPM.Text = DateTime.Now.ToString("tt");
            txtNYTime.Text = DateTime.Now.AddHours(-5).ToString("HH:mm");
            txtStockholmTime.Text = DateTime.Now.AddHours(2).ToString("HH:mm");
        }



        private void ShowColleague(string userName)
        {
            if (twitterTimer.IsEnabled)
            {
                twitterTimer.Stop();
            }
            if (locatorTimer.IsEnabled)
            {
                locatorTimer.Stop();
            }
            var colleagueClient = new WebClient();
            colleagueClient.OpenReadCompleted += OnEmployeeServiceOpenReadComplete;
            colleagueClient.OpenReadAsync(new Uri("http://locator.rgahosting.com/LocatorService.svc/employees/" + userName + "?date=" + DateTime.Now.ToString()));
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
                            PhotoURL = string.Format("/Images/Colleagues/{0}.jpg", emp.Element("username").Value.Replace(".","_")),
                            ThumbnailURL = string.Format("http://{0}", emp.Element("thumbnailurl").Value),
                            MobilePhone = emp.Element("mobilephonenumber").Value
                        };
            colleague = query.SingleOrDefault();
            //this.DataContext = colleague;

            txtColleagueHeader.Text = string.Format("Welcome, {0} {1}", colleague.FirstName, colleague.LastName);

            PopulateMeetings();
            PopulateTasks();

            /*
            cell01Colleague.Children.Clear();
            var colleaguePhoto = new Image();
            colleaguePhoto.Source = new BitmapImage(new Uri(colleague.PhotoURL, UriKind.Absolute));
            
            cell01Colleague.Children.Add(colleaguePhoto);
            colleaguePhoto.Width = cell01Colleague.Width;
            colleaguePhoto.Height = cell01Colleague.Height;
            colleaguePhoto.Stretch = Stretch.Fill;
            */
            imgColleaguePhoto.Source = new BitmapImage(new Uri(colleague.PhotoURL,UriKind.Relative));
            imgColleaguePhoto.Height = 180;
            imgColleaguePhoto.Width = 170;
            


            cell00Home.Visibility = Visibility.Collapsed;
            cell02Home.Visibility = Visibility.Collapsed;

            cell00Colleague.Visibility = Visibility.Visible;
            cell02Colleague.Visibility = Visibility.Visible;

            cell12Home.Visibility = Visibility.Collapsed;
            cell12Colleague.Visibility = Visibility.Visible;

            cell13Home.Visibility = Visibility.Collapsed;
            cell13Colleague.Visibility = Visibility.Visible;

            cell01Home.Visibility = Visibility.Collapsed;
            cell01Colleague.Visibility = Visibility.Visible;

            cell01Home.Visibility = Visibility.Collapsed;
            cell01Colleague.Visibility = Visibility.Visible;

            cell20Home.Visibility = Visibility.Collapsed;
            cell20Colleague.Visibility = Visibility.Visible;

            cell21Home.Visibility = Visibility.Collapsed;
            cell21Colleague.Visibility = Visibility.Visible;

            cell11Home.Visibility = Visibility.Collapsed;

            imgBuildingPhoto.Visibility = Visibility.Collapsed;
            cell10Colleague.Visibility = Visibility.Visible;

            txtColleagueDay.Text = DateTime.Now.ToString("dd");
            txtColleagueDate.Text = DateTime.Now.ToString("MMMM yyyy");
            

            

            
            colleagueTimer.Start();
        }
        private void PopulateMeetings()
        {
            lstMeetings.Items.Clear();
            var meeting1 = new Meeting
            {
                Title = "Weekly Tech Meeting",
                Location = "42 SJS - Coffee Bar",
                Time = "10:30AM",
                LocationPhoto = "/Images/Locations/771edc9a-f534-4a48-8bb6-a053dff155ec.jpg"
            };
            lstMeetings.Items.Add(meeting1);
            var meeting2 = new Meeting
            {
                Title = "Make Day Presentations",
                Location = "3rd Floor Rosebery",
                Time = "15:00PM",
                LocationPhoto = "/Images/Locations/771edc9a-f534-4a48-8bb6-a053dff155ec.jpg"
            };
            lstMeetings.Items.Add(meeting2);
            var meeting3 = new Meeting
            {
                Title = "Christmas Party",
                Location = "TBC",
                Time = "16:30PM",
                LocationPhoto = "/Images/ICN_calendar_red.png"
            };
            lstMeetings.Items.Add(meeting2);
        }

        private void PopulateTasks()
        {
            lstTasks.Items.Clear();
            var task1 = new Task
            {
                Title = "Secret Santa Present",
                Description = "Buy and wrap Secret Santa Present - spend no more than a deep sea diver.",
                DueBy = "10:00AM",
                TaskPhoto = "/Images/ICN_time_red.png"
            };
            lstTasks.Items.Add(task1);
            var task2 = new Task
            {
                Title = "Finish Locator Phone App",
                Description = "Change checkin icons, add thumbnails to colleague view, and location images to locations view.",
                DueBy = "14:00PM",
                TaskPhoto = "/Images/ICN_time_red.png"
            };
            lstTasks.Items.Add(task2);
            var task3 = new Task
            {
                Title = "Finish Reception Screen Silverlight App",
                Description = "Tidy up the database, add location and weather icons.",
                DueBy = "14:00PM",
                TaskPhoto = "/Images/ICN_time_red.png"
            };
            lstTasks.Items.Add(task3);
        }

        private void GoHome(object sender, EventArgs e)
        {
            colleagueTimer.Stop();

            cell00Home.Visibility = Visibility.Visible;
            cell02Home.Visibility = Visibility.Visible;

            cell00Colleague.Visibility = Visibility.Collapsed;
            cell02Colleague.Visibility = Visibility.Collapsed;

            cell12Home.Visibility = Visibility.Visible;
            cell12Colleague.Visibility = Visibility.Collapsed;

            cell13Home.Visibility = Visibility.Visible;
            cell13Colleague.Visibility = Visibility.Collapsed;

            cell20Home.Visibility = Visibility.Visible;
            cell20Colleague.Visibility = Visibility.Collapsed;

            cell21Home.Visibility = Visibility.Visible;
            cell21Colleague.Visibility = Visibility.Collapsed;

            cell01Home.Visibility = Visibility.Visible;
            cell01Colleague.Visibility = Visibility.Collapsed;

            imgBuildingPhoto.Visibility = Visibility.Visible;
            cell10Colleague.Visibility = Visibility.Collapsed;

            cell11Home.Visibility = Visibility.Visible;

            locatorTimer.Start();
            twitterTimer.Start();
        }
    }
}
