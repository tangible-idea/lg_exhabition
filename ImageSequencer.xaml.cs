using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Clarity.Demo.ImageSequencer
{
    /// <summary>
    /// Interaction logic for ImageSequencer.xaml
    /// </summary>
    public partial class ImageSequencerControl : UserControl
    {
        private int currentIndex;
        private List<BitmapSource> images;
        private DispatcherTimer updateImageTimer;

        public ImageSequencerControl()
        {
            InitializeComponent();

            this.updateImageTimer = new DispatcherTimer(DispatcherPriority.Render);
            this.updateImageTimer.Interval = TimeSpan.FromMilliseconds(30.0);
            this.updateImageTimer.Tick += new EventHandler(this.updateImageTimer_Tick);
        }

        public void Load(List<BitmapSource> images)
        {
            this.updateImageTimer.Stop();
            this.images = images;
            this.currentIndex = 0;
            this.LoadCurrentIndex();
        }

        private void LoadCurrentIndex()
        {
            if (((this.images != null) && (this.currentIndex < this.images.Count)) && (this.currentIndex >= 0))
            {
                this.image.Source = this.images[this.currentIndex];
            }
        }

        public void Play()
        {
            this.currentIndex = 0;
            this.updateImageTimer.Start();
        }

        public void Stop()
        {
            this.updateImageTimer.Stop();
        }


        private void updateImageTimer_Tick(object sender, EventArgs e)
        {
            if (this.currentIndex == this.images.Count)
            {
                this.currentIndex = 0;
            }
            this.currentIndex++;
            if (this.images != null)
            {
                this.LoadCurrentIndex();
            }
        }
    }
}
