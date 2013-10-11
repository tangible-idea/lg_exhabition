using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using Clarity.Demo.ImageSequencer;

namespace lgshow
{
    public partial class MainWindow : Window
    {
        ManipulationModes currentMode = ManipulationModes.All;
        List<List<BitmapSource>> lst2xPNG;
        Image m_currImage1 = null;
        Image m_currImage2 = null;
        System.Media.SoundPlayer m_SP = null;

        public MainWindow()
        {
            InitializeComponent();

            //m_SP = new System.Media.SoundPlayer();
            //System.IO.Stream stream = new System.IO.MemoryStream(Properties.Resources.water_bubble_high);
            //m_SP.Stream = stream;   //(리소스에 등록한 파일명을 Properties.Recources를 통해 바로 불러올 수있음)
            m_SP = new System.Media.SoundPlayer("water_bubble_high.mp3");

            if (!m_SP.IsLoadCompleted)
                MessageBox.Show("sound load failed");

            //MessageBox.Show("new");
            ShowWindow win4K = new ShowWindow();
            //win2.Owner = this;
            win4K.Show();

            // Build list of radio buttons
            //foreach (ManipulationModes mode in Enum.GetValues(typeof(ManipulationModes)))
            //{
            //    RadioButton radio = new RadioButton
            //    {
            //        Content = mode,
            //        IsChecked = mode == currentMode,
            //    };
            //    radio.Checked += new RoutedEventHandler(OnRadioChecked);
            //    modeList.Children.Add(radio);
            //}
        }

        // png 전부 로드 [10/9/2013 Administrator]
        public void LoadImages(string strPath, ImageSequencerControl seq)
        {
            List<BitmapSource> lstSeq = new List<BitmapSource>();
            string str = strPath;
            string str2 = ".png";

            Assembly executingAssembly = Assembly.GetExecutingAssembly();

            for (int i = 0; i <= 39; i++)
            {
                string filename = string.Format("{0}{1}{2}{3}", this.GetType().Namespace, str, i.ToString("00000"), str2);
                BitmapImage item = new BitmapImage();
                item.BeginInit();
                item.StreamSource = executingAssembly.GetManifestResourceStream(filename);
                item.CacheOption = BitmapCacheOption.OnLoad;
                item.CreateOptions = BitmapCreateOptions.None;
                item.EndInit();
                item.Freeze();
                lstSeq.Add(item);
            }

            lst2xPNG.Add(lstSeq);

            seq.Load(lstSeq);
            seq.Play();
        }



        void OnRadioChecked(object sender, RoutedEventArgs args)
        {
            currentMode = (ManipulationModes)(sender as RadioButton).Content;
        }

        protected override void OnManipulationStarting(ManipulationStartingEventArgs args)
        {
            args.ManipulationContainer = this;
            args.Mode = currentMode;

            // Adjust Z-order
            FrameworkElement element = args.Source as FrameworkElement;
            Panel pnl = element.Parent as Panel;

            for (int i = 0; i < pnl.Children.Count; i++)
                Panel.SetZIndex(pnl.Children[i],
                    pnl.Children[i] == element ? pnl.Children.Count : i);

            args.Handled = true;
            base.OnManipulationStarting(args);
        }

        protected override void OnManipulationDelta(ManipulationDeltaEventArgs args)
        {
            UIElement element = args.Source as UIElement;
            MatrixTransform xform = element.RenderTransform as MatrixTransform;
            Matrix matrix = xform.Matrix;
            ManipulationDelta delta = args.DeltaManipulation;
            Point center = args.ManipulationOrigin;
            matrix.Translate(-center.X, -center.Y);
            matrix.Scale(delta.Scale.X, delta.Scale.Y);
            matrix.Rotate(delta.Rotation);
            matrix.Translate(center.X, center.Y);
            matrix.Translate(delta.Translation.X, delta.Translation.Y);
            xform.Matrix = matrix;

            args.Handled = true;
            base.OnManipulationDelta(args);
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
          //player1.Play();
          //button1.Tag = "pause";
          //button1.Content = "일시정지";
        }

        private void player1_MouseUp(object sender, MouseButtonEventArgs e)
        {
          if (sender.GetType().ToString() == "System.Windows.Controls.MediaElement")
          {
            MediaElement me = sender as MediaElement;
            MessageBox.Show("11");
          }
        }


        //  [10/12/2013 Administrator]
        private void Image_Gesture(object sender, StylusSystemGestureEventArgs e)
        {
            //this.Title = e.SystemGesture.ToString();
            //MessageBox.Show("event");

            //Image img = sender as Image;

            //GeneralTransform transform = img.TransformToAncestor(this); 
            //Point rootPoint = transform.Transform(new Point(0, 0));

            //if ((rootPoint.Y < 100.0f))
            //{
            //    MessageBox.Show(img.ToString());
            //}

            //if (e.SystemGesture == SystemGesture.Drag)
            //{
                //MessageBox.Show("Drag");
            //}
        }

        private void Image_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            Image img = sender as Image;

            GeneralTransform transform = img.TransformToAncestor(this);
            Point rootPoint = transform.Transform(new Point(0, 0));
            
            if ((rootPoint.Y < 100.0f))
            {
                MessageBox.Show(img.Name);
            }
        }


        private void player1_StylusSystemGesture(object sender, StylusSystemGestureEventArgs e)
        {
          //if (e.SystemGesture == SystemGesture.Tap)
          //{

          //  Button b = button1;
          //  if (b.Tag.Equals("pause"))
          //  {
          //    b.Tag = "play";
          //    b.Content = "재생";
          //    player1.Pause();
          //  }
          //  else if (b.Tag.Equals("play"))
          //  {
          //    b.Tag = "pause";
          //    b.Content = "일시정지";
          //    player1.Play();
          //  }
          //}
        }

        private void RectOverlap()
        {
            rct_fadeout.Visibility = Visibility.Visible;
            rct_fadeout.Opacity = 0.4;
           // btn_multiv.set

            if (m_SP.IsLoadCompleted)
                m_SP.Play();
            
        }

        private void RectHidden()
        {
            rct_fadeout.Visibility = Visibility.Hidden;
            rct_fadeout.Opacity = 0.0;
        }

        private void ImageFocusOver(Image img)
        {
            //if (m_currImage1==null)
            //{
            //}
            //else
            //{
            //    m_currImage1.Visibility = Visibility.Hidden;
            //}
            img.Visibility = Visibility.Visible;
            img.SetValue(Canvas.LeftProperty, 0d);
            img.SetValue(Canvas.TopProperty, 0d);
            //img.Margin = new Thickness(320, 180, 320, 180);
            //img.HorizontalAlignment = HorizontalAlignment.Center;
            //img.VerticalAlignment = VerticalAlignment.Center;
            m_currImage1 = img;
        }

        private void ImageFocusOver(Image img1, Image img2)
        {
            m_currImage1 = img1;
            m_currImage2 = img2;

            img1.Visibility = Visibility.Visible;
            img1.SetValue(Canvas.LeftProperty, -320d);
            img1.SetValue(Canvas.TopProperty, 0d);

            img2.Visibility = Visibility.Visible;
            img2.SetValue(Canvas.LeftProperty, 320d);
            img2.SetValue(Canvas.TopProperty, 0d);
        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
            if ( (m_currImage1 == null) || (m_currImage2 == null) ) 
            {
            }
            else
            {
                m_currImage1.Visibility = Visibility.Hidden;
                m_currImage2.Visibility = Visibility.Hidden;
                RectHidden();
            }

            if ((m_currImage1 != null))
            {
                m_currImage1.Visibility = Visibility.Hidden;
                RectHidden();
            }
        }



        private void btn_multiv_Click(object sender, RoutedEventArgs e)
        {
            this.ImageFocusOver(img_multiv);
            RectOverlap();
            
        }

       
        private void btn_turboheat_Click(object sender, RoutedEventArgs e)
        {
            this.ImageFocusOver(img_turbo);

            RectOverlap();
        }

        private void btn_ghp_Click(object sender, RoutedEventArgs e)
        {
            this.ImageFocusOver(img_ghp);

            RectOverlap();
        }

        private void btn_name_Click(object sender, RoutedEventArgs e)
        {
            this.ImageFocusOver(img_name);

            RectOverlap();
        }

        private void btn_bms_Click(object sender, RoutedEventArgs e)
        {
            this.ImageFocusOver(img_bms1, img_bms2);

            RectOverlap();
        }

        private void btn_tms_Click(object sender, RoutedEventArgs e)
        {
            this.ImageFocusOver(img_tms1, img_tms2);

            RectOverlap();
        }

        private void btn_solar_Click(object sender, RoutedEventArgs e)
        {
            this.ImageFocusOver(img_solar);

            RectOverlap();
        }

        private void btn_geo_Click(object sender, RoutedEventArgs e)
        {
            this.ImageFocusOver(img_geo1, img_geo2);
            RectOverlap();
        }

        private void btn_heat_Click(object sender, RoutedEventArgs e)
        {
            this.ImageFocusOver(img_heat1, img_heat2);
            RectOverlap();
        }

        private void btn_chiller_Click(object sender, RoutedEventArgs e)
        {
            this.ImageFocusOver(img_chiller1, img_chiller2);
            RectOverlap();
        }

        private void btn_light_Click(object sender, RoutedEventArgs e)
        {
            this.ImageFocusOver(img_light);
            RectOverlap();
        }

        private void btn_school_Click(object sender, RoutedEventArgs e)
        {
            this.ImageFocusOver(img_school1, img_school2);
            RectOverlap();
        }

        private void btn_building_Click(object sender, RoutedEventArgs e)
        {
            this.ImageFocusOver(img_building1, img_building2);
            RectOverlap();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lst2xPNG = new List<List<BitmapSource>>();

            LoadImages(".Images.icon.bms.빌딩관리시스템_", seq_bms);
            LoadImages(".Images.icon.building.building_", seq_building);
            LoadImages(".Images.icon.chiller.흡수식칠러_", seq_chiller);
            LoadImages(".Images.icon.geo_all.지열 3_", seq_geo_all);
            //LoadImages(".Images.icon.geo1.지열_", seq_geo1);
            //LoadImages(".Images.icon.geo2.지열 2_", seq_geo2);
            LoadImages(".Images.icon.home.home_", seq_home);
            LoadImages(".Images.icon.lg.LG_", seq_lgenergy);
            LoadImages(".Images.icon.school.school_", seq_school);
            LoadImages(".Images.icon.solar.태양광_", seq_solar);
            LoadImages(".Images.icon.tms.원격유지보수_", seq_tms);
            LoadImages(".Images.icon.heat.히트펌프_", seq_heat);
        }





    }
}
