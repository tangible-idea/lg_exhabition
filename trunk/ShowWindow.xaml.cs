﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace lgshow
{
    /// <summary>
    /// Window1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ShowWindow : Window
    {
        System.Windows.Threading.DispatcherTimer TimerClock;
        public ShowWindow()
        {
            InitializeComponent();

            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Left = 0;
            this.Top = -480;
            //this.Width = SystemParameters.VirtualScreenWidth;
            //this.Height = SystemParameters.VirtualScreenHeight;
            this.Width = 800;
            this.Height = 480;
            //this.WindowState = WindowState.Maximized;
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            this.Topmost = true;
            this.Show();

            //this.WindowStartupLocation = WindowStartupLocation.Manual;
            //System.Drawing.Rectangle workingArea = System.Windows.Forms.Screen.AllScreens[1].WorkingArea;
            //this.Left = workingArea.Left;
            //this.Top = workingArea.Top;
            //this.Width = workingArea.Width;
            //this.Height = workingArea.Height;
            //this.WindowState = WindowState.Maximized;
            //this.WindowStyle = WindowStyle.None;
            //this.Topmost = true;
            //this.Show();

            media_lge.MediaEnded += new RoutedEventHandler(LGE_media_ended);

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            media_lge.Play();
            media_lge.Visibility = Visibility.Visible;
            img_lge.Visibility = Visibility.Hidden;

            TimerClock = new System.Windows.Threading.DispatcherTimer();
            TimerClock.Interval = new TimeSpan(0, 0, 0, 0, 60000); // milliseconds
            TimerClock.IsEnabled = true;
            TimerClock.Tick += new EventHandler(TimerClock_Tick);
        }

        void LGE_media_ended(object sender, RoutedEventArgs e)
        {
            media_lge.Stop();
            media_lge.Position = TimeSpan.FromSeconds(0);
            media_lge.Play();
        }

        // 이미지 상단으로 보여줄 떄 [10/13/2013 Administrator]
        public void ShowImage(ImageSource img_source)
        {
            TimerClock.Stop();
            TimerClock.Start();

            media_lge.Pause();
            media_lge.Visibility = Visibility.Hidden;
            img_lge.Visibility= Visibility.Visible;

            img_lge.Source = img_source;
        }

        void TimerClock_Tick(object sender, EventArgs e)
        {
            media_lge.Play();
            media_lge.Visibility = Visibility.Visible;
            img_lge.Visibility = Visibility.Hidden;
        }
    }
}