﻿using System;
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
    public class Task
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string DueBy { get; set; }

        public string TaskPhoto { get; set; }

        public Task()
        {

        }
    }
}
