using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Media.Effects;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using Clarity.Demo.ImageSequencer;

namespace lgshow
{

    #region public class MatrixAnimation
    public class MatrixAnimation : MatrixAnimationBase
    {
        public Matrix? From
        {
            set { SetValue(FromProperty, value); }
            get { return (Matrix)GetValue(FromProperty); }
        }

        public static DependencyProperty FromProperty =
            DependencyProperty.Register("From", typeof(Matrix?), typeof(MatrixAnimation),
                new PropertyMetadata(null));

        public Matrix? To
        {
            set { SetValue(ToProperty, value); }
            get { return (Matrix)GetValue(ToProperty); }
        }

        public static DependencyProperty ToProperty =
            DependencyProperty.Register("To", typeof(Matrix?), typeof(MatrixAnimation),
                new PropertyMetadata(null));

        public IEasingFunction EasingFunction
        {
            get { return (IEasingFunction)GetValue(EasingFunctionProperty); }
            set { SetValue(EasingFunctionProperty, value); }
        }

        public static readonly DependencyProperty EasingFunctionProperty =
            DependencyProperty.Register("EasingFunction", typeof(IEasingFunction), typeof(MatrixAnimation),
                new UIPropertyMetadata(null));

        public MatrixAnimation()
        {
        }

        public MatrixAnimation(Matrix toValue, Duration duration)
        {
            To = toValue;
            Duration = duration;
        }

        public MatrixAnimation(Matrix toValue, Duration duration, FillBehavior fillBehavior)
        {
            To = toValue;
            Duration = duration;
            FillBehavior = fillBehavior;
        }

        public MatrixAnimation(Matrix fromValue, Matrix toValue, Duration duration)
        {
            From = fromValue;
            To = toValue;
            Duration = duration;
        }

        public MatrixAnimation(Matrix fromValue, Matrix toValue, Duration duration, FillBehavior fillBehavior)
        {
            From = fromValue;
            To = toValue;
            Duration = duration;
            FillBehavior = fillBehavior;
        }

        protected override Freezable CreateInstanceCore()
        {
            return new MatrixAnimation();
        }

        protected override Matrix GetCurrentValueCore(Matrix defaultOriginValue, Matrix defaultDestinationValue, AnimationClock animationClock)
        {
            if (animationClock.CurrentProgress == null)
            {
                return Matrix.Identity;
            }

            var normalizedTime = animationClock.CurrentProgress.Value;
            if (EasingFunction != null)
            {
                normalizedTime = EasingFunction.Ease(normalizedTime);
            }

            var from = From ?? defaultOriginValue;
            var to = To ?? defaultDestinationValue;

            var newMatrix = new Matrix(
                    ((to.M11 - from.M11) * normalizedTime) + from.M11,
                    ((to.M12 - from.M12) * normalizedTime) + from.M12,
                    ((to.M21 - from.M21) * normalizedTime) + from.M21,
                    ((to.M22 - from.M22) * normalizedTime) + from.M22,
                    ((to.OffsetX - from.OffsetX) * normalizedTime) + from.OffsetX,
                    ((to.OffsetY - from.OffsetY) * normalizedTime) + from.OffsetY);

            return newMatrix;
        }
    }
    #endregion

    #region public class LinearMatrixAnimation
    public class LinearMatrixAnimation : AnimationTimeline
    {

        public Matrix? From
        {
            set { SetValue(FromProperty, value); }
            get { return (Matrix)GetValue(FromProperty); }
        }
        public static DependencyProperty FromProperty = DependencyProperty.Register("From", typeof(Matrix?), typeof(LinearMatrixAnimation), new PropertyMetadata(null));

        public Matrix? To
        {
            set { SetValue(ToProperty, value); }
            get { return (Matrix)GetValue(ToProperty); }
        }
        public static DependencyProperty ToProperty = DependencyProperty.Register("To", typeof(Matrix?), typeof(LinearMatrixAnimation), new PropertyMetadata(null));

        public LinearMatrixAnimation()
        {
        }

        public LinearMatrixAnimation(Matrix from, Matrix to, Duration duration)
        {
            Duration = duration;
            From = from;
            To = to;
        }

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            if (animationClock.CurrentProgress == null)
            {
                return null;
            }

            double progress = animationClock.CurrentProgress.Value;
            Matrix from = From ?? (Matrix)defaultOriginValue;

            if (To.HasValue)
            {
                Matrix to = To.Value;
                Matrix newMatrix = new Matrix(((to.M11 - from.M11) * progress) + from.M11, 0, 0, ((to.M22 - from.M22) * progress) + from.M22,
                                              ((to.OffsetX - from.OffsetX) * progress) + from.OffsetX, ((to.OffsetY - from.OffsetY) * progress) + from.OffsetY);
                return newMatrix;
            }

            return Matrix.Identity;
        }

        protected override System.Windows.Freezable CreateInstanceCore()
        {
            return new LinearMatrixAnimation();
        }

        public override System.Type TargetPropertyType
        {
            get { return typeof(Matrix); }
        }
    }
    #endregion 

    public partial class MainWindow : Window
    {
        ManipulationModes currentMode = ManipulationModes.All;
        List<List<BitmapSource>> lst2xPNG;
        Image m_currImage1 = null;
        Image m_currImage2 = null;
        System.Media.SoundPlayer m_SP = null;
        ShowWindow win4K = null;

        Matrix currImgDestination;
        Vector currImgVelocity;
        Image currManipulImage = null;

        enum EImageStat { STAT_PIC2, STAT_PIC1, STAT_ONEHASGONE, STAT_ALLGONE };

        EImageStat nImageOnLoaded;// 현재 올라와 있는 이미지 개수 [10/13/2013 Administrator]
        Point ptCurrPos = new Point(); // 사진 위치1[10/13/2013 Administrator]
        Point ptOldPos = new Point(); // 사진 위치2 [10/13/2013 Administrator]

        public MainWindow()
        {
            InitializeComponent();

            label1.Content = "";

            //m_SP = new System.Media.SoundPlayer();
            //System.IO.Stream stream = new System.IO.MemoryStream(Properties.Resources.water_bubble_high);
            //m_SP.Stream = stream;   //(리소스에 등록한 파일명을 Properties.Recources를 통해 바로 불러올 수있음)
            m_SP = new System.Media.SoundPlayer("water_bubble_high.mp3");

            //if (!m_SP.IsLoadCompleted)
            //    MessageBox.Show("sound load failed");

            //MessageBox.Show("new");
            win4K = new ShowWindow();
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
            try
            {
                UIElement element = args.Source as UIElement;
                MatrixTransform xform = element.RenderTransform as MatrixTransform;
                Matrix matrix = xform.Matrix;
                ManipulationDelta delta = args.DeltaManipulation;
                Point center = args.ManipulationOrigin;
                /*
                matrix.Translate(-center.X, -center.Y);
                matrix.Scale(delta.Scale.X, delta.Scale.Y);
                matrix.Rotate(delta.Rotation);
                matrix.Translate(center.X, center.Y);
                matrix.Translate(delta.Translation.X, delta.Translation.Y);           
                xform.Matrix = matrix;
                */
                Matrix to = matrix;
                to.Translate(-center.X, -center.Y);
                to.Scale(delta.Scale.X, delta.Scale.Y);
                to.Rotate(delta.Rotation);
                to.Translate(center.X, center.Y);
                to.Translate(delta.Translation.X, delta.Translation.Y);

                MatrixAnimation b = new MatrixAnimation()
                {
                    From = matrix,
                    To = to,
                    Duration = TimeSpan.FromMilliseconds(0),
                    FillBehavior = FillBehavior.HoldEnd
                };
                (element.RenderTransform as MatrixTransform).BeginAnimation(MatrixTransform.MatrixProperty, b);


                //tbTranslate.Text = string.Format("Translation: {0}, {1}", delta.Translation.X, delta.Translation.Y);
                //tbTranslate.Text += string.Format("\r\nTotal Translation: {0}, {1}", args.CumulativeManipulation.Translation.X, args.CumulativeManipulation.Translation.Y);

                args.Handled = true;
                base.OnManipulationDelta(args);
            }
            catch
            {
                //MessageBox.Show("OnManipulationDelta");
            };
        }

        protected override void OnManipulationCompleted(ManipulationCompletedEventArgs e)
        {
            try
            {

                // tbCompleted.Text = string.Format("{0}", e.FinalVelocities.LinearVelocity);
                // tbCompleted.Text += string.Format("\r\n{0}", e.TotalManipulation.Translation);
                UIElement el = e.Source as UIElement;
                //el.Effect = new BlurEffect() { Radius = 10.0 };

                MatrixTransform xform = el.RenderTransform as MatrixTransform;
                Matrix matrix = xform.Matrix;
                Matrix from = matrix;
                Matrix to = matrix;
                to.Translate(
                    e.TotalManipulation.Translation.X * Math.Abs(e.FinalVelocities.LinearVelocity.X),
                    e.TotalManipulation.Translation.Y * Math.Abs(e.FinalVelocities.LinearVelocity.Y));

                if (Math.Abs(e.FinalVelocities.LinearVelocity.X) > 0.5 || Math.Abs(e.FinalVelocities.LinearVelocity.Y) > 0.5)
                {
                    MatrixAnimation b = new MatrixAnimation()
                    {
                        From = from,
                        To = to,
                        Duration = TimeSpan.FromMilliseconds(500),
                        FillBehavior = FillBehavior.HoldEnd
                    };
                    b.Completed += new EventHandler(b_Completed);
                    (el.RenderTransform as MatrixTransform).BeginAnimation(MatrixTransform.MatrixProperty, b);
                }

                base.OnManipulationCompleted(e);

                label1.Content = "to : " + to.OffsetY.ToString() + "\nel : " + xform.Matrix.OffsetY.ToString() + "\ne.total : " + e.TotalManipulation.Translation.Y.ToString() + "\ne.final : " + e.FinalVelocities.LinearVelocity.Y.ToString();
                currImgDestination = to;
                currImgVelocity = e.FinalVelocities.LinearVelocity;
                currManipulImage = el as Image;
            }
            catch
            {
                //MessageBox.Show("OnManipulationCompleted");
            };
        }

        void b_Completed(object sender, EventArgs e)
        {
            if (currImgDestination.OffsetY < 50.0f) // 최종 목적지 : 위
            {
                this.ImageToScreen((currManipulImage));
            }
            else if (currImgDestination.OffsetY > 920.0f) // 최종 목적지 : 아래
            {
                this.ImageDestory((currManipulImage));
            }
            else if (currImgDestination.OffsetX < 100.0f) // 최종 목적지 : 왼쪽
            {
                this.ImageDestory((currManipulImage));
            }
            else if (currImgDestination.OffsetX > 1820.0f) // 최종 목적지 : 오른쪽
            {
                this.ImageDestory((currManipulImage));
            }


           // throw new NotImplementedException();
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

        // 동작 실시간 확인하면서 [10/12/2013 Administrator]
        private void Image_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
        //    if (nImageOnLoaded == 0)    // 중복 이벤트 무시
          //      return;

            Image img = sender as Image;

            GeneralTransform transform = img.TransformToAncestor(this);
            ptCurrPos = transform.Transform(new Point(0, 0));

            Vector vtVelocity = e.Velocities.LinearVelocity;

            //label1.Content = vtVelocity.Y.ToString();
           
            


            if ((ptCurrPos.Y < 100.0f) && (vtVelocity.Y < -0.75f))
            {
                //ImageToScreen(img);
            }

           // ptOldPos = ptCurrPos;
        }

        private void ImageDestory(Image img)
        {
            //if (win4K.GetCurrentImage() == img.Source)
              //  return;

            //win4K.ShowImage(img.Source);


            if ((nImageOnLoaded == EImageStat.STAT_ONEHASGONE) || (nImageOnLoaded == EImageStat.STAT_PIC1))
            {
                img_slideup.Visibility = Visibility.Hidden; // 올려주세요 이미지 사라짐.  
                rct_fadeout.Visibility = Visibility.Hidden; // 페이드 사라짐 
            }

            if (nImageOnLoaded == EImageStat.STAT_PIC2) // 사진 2개면
                nImageOnLoaded = EImageStat.STAT_ONEHASGONE;    // 한개 보내고
            else if (nImageOnLoaded == EImageStat.STAT_PIC1)    // 사진 1개면
                nImageOnLoaded = EImageStat.STAT_ALLGONE;   // 모두 가고
            else if (nImageOnLoaded == EImageStat.STAT_ONEHASGONE)  // 사진 1개면
                nImageOnLoaded = EImageStat.STAT_ALLGONE;   // 모두 가고

            img.Visibility = Visibility.Hidden; // 이미지 사라짐
        }

        private void ImageToScreen(Image img)
        {
            if (win4K.GetCurrentImage() == img.Source)  // 이미 같은게 올라와 있다면
            {
            }
            else
            {
                win4K.ShowImage(img.Source);
            }

            if ((nImageOnLoaded == EImageStat.STAT_ONEHASGONE) || (nImageOnLoaded == EImageStat.STAT_PIC1))
            {
                img_slideup.Visibility = Visibility.Hidden; // 올려주세요 이미지 사라짐.  
                rct_fadeout.Visibility = Visibility.Hidden; // 페이드 사라짐 
            }


            if (nImageOnLoaded == EImageStat.STAT_PIC2) // 사진 2개면
                nImageOnLoaded = EImageStat.STAT_ONEHASGONE;    // 한개 보내고
            else if (nImageOnLoaded == EImageStat.STAT_PIC1)    // 사진 1개면
                nImageOnLoaded = EImageStat.STAT_ALLGONE;   // 모두 가고
            else if (nImageOnLoaded == EImageStat.STAT_ONEHASGONE)  // 사진 1개면
                nImageOnLoaded = EImageStat.STAT_ALLGONE;   // 모두 가고


            //img.SetValue(Canvas.LeftProperty, 0d);// 이미지 위치 원래대로 
            //img.SetValue(Canvas.TopProperty, 0d);

            img.Visibility = Visibility.Hidden; // 이미지 사라짐
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

            img_slideup.Visibility = Visibility.Visible;

            if (m_SP.IsLoadCompleted)
                m_SP.Play();
            
        }

        private void RectHidden()
        {
            rct_fadeout.Visibility = Visibility.Hidden;
            rct_fadeout.Opacity = 0.0;
            img_slideup.Visibility = Visibility.Hidden;
        }

        //  [10/06/2013 Administrator]
        private void ImageFocusOver(Image img)
        {
            nImageOnLoaded = EImageStat.STAT_PIC1;

            //GeneralTransform transform = img.TransformToAncestor(this);
            //ptCurrPos = transform.Transform(new Point(0, 0));

            //double lfPosGapX = 600d - ptCurrPos.X;
            //double lfPosGapY = 400d - ptCurrPos.Y;

            //img.Visibility = Visibility.Visible;
            //img.SetValue(Canvas.LeftProperty, lfPosGapX);
            //img.SetValue(Canvas.TopProperty, lfPosGapY);

            //img.Width = 1250;
            //img.Height = 726;
            //img.RenderTransformOrigin = new Point(0.5, 0.5);

            //ScaleTransform myScaleTransform = new ScaleTransform();
            //myScaleTransform.ScaleY = 0.5;
            //myScaleTransform.ScaleX = 0.5;

            //RotateTransform myRotateTransform = new RotateTransform();
            //myRotateTransform.Angle = 0;

            //TranslateTransform myTranslate = new TranslateTransform();
            //myTranslate.X = 600;
            //myTranslate.Y = 400;

            MatrixTransform myMatrix = new MatrixTransform(0.5, 0, 0, 0.5d, 660, 380);

            //TransformGroup myTransformGroup = new TransformGroup();
            //myTransformGroup.Children.Add(myScaleTransform);
            //myTransformGroup.Children.Add(myRotateTransform);
            //myTransformGroup.Children.Add(myTranslate);
            //myTransformGroup.Children.Add(myMatrix);

            //img.RenderTransform = myTransformGroup;
            img.RenderTransform = myMatrix;
            img.Visibility = Visibility.Visible;

            m_currImage1 = img;
        }

        private void ImageFocusOver(Image img1, Image img2)
        {
            nImageOnLoaded = EImageStat.STAT_PIC2;

            m_currImage1 = img1;
            m_currImage2 = img2;

            //GeneralTransform transform = m_currImage1.TransformToAncestor(this);
            //ptCurrPos = transform.Transform(new Point(0, 0));

            //ptCurrPos.X = 360;
            //ptCurrPos.Y = 360;

            //img1.Visibility = Visibility.Visible;
            //img1.SetValue(Canvas.LeftProperty, -320d);
            //img1.SetValue(Canvas.TopProperty, 0d);

            //img2.Visibility = Visibility.Visible;
            //img2.SetValue(Canvas.LeftProperty, 320d);
            //img2.SetValue(Canvas.TopProperty, 0d);

            MatrixTransform myMatrix1 = new MatrixTransform(0.5, 0, 0, 0.5d, 340, 380);
            MatrixTransform myMatrix2 = new MatrixTransform(0.5, 0, 0, 0.5d, 1000, 380);

            m_currImage1.RenderTransform = myMatrix1;
            m_currImage2.RenderTransform = myMatrix2;

            m_currImage1.Visibility = Visibility.Visible;
            m_currImage2.Visibility = Visibility.Visible;


        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
            //if ( (m_currImage1 == null) || (m_currImage2 == null) ) 
            //{
            //}
            //else
            //{
            //    m_currImage1.Visibility = Visibility.Hidden;
            //    m_currImage2.Visibility = Visibility.Hidden;
            //    RectHidden();
            //}

            //if ((m_currImage1 != null))
            //{
            //    m_currImage1.Visibility = Visibility.Hidden;
            //    RectHidden();
            //}
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
            img_slideup.Visibility = Visibility.Hidden;

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
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //e.Cancel = true;
            win4K.Close();
        }



    }
}
