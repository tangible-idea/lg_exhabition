using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace lgshow
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static String[] mArgs;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            mArgs = e.Args;
        }
        //private void btn_Display_Secondary_Monitor_Click(object sender, System.EventArgs e)
        //{
        //    Window1 frm_second_mon = new Window1();
 
        //    Screen [] screens = Screen.AllScreens;
            
        //    Screen secondary_screen = null;
 
        //    foreach (Screen screen in screens)
        //    {
        //        if (screen.Primary == false)
        //        {
        //            secondary_screen = screen;
        //            second_mon.Bounds = secondary_screen.Bounds;
        //            s_mon.Show();
 
        //            break;
        //        }
        //    }
        //}
    }
}
