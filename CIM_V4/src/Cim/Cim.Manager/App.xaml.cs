using AutoMapper;
using Cim.Manager.Resources;
using Cim.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Telerik.Windows.Automation.Peers;
using Telerik.Windows.Controls;
using Telerik.Windows.Input.Touch;

namespace Cim
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public App()
        {
            Startup += App_Startup;
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            //Telerik 속도향상
            //http://docs.telerik.com/devtools/wpf/controls/radgridview/performance/tips-tricks
            AutomationManager.AutomationMode = AutomationMode.Advanced;
            TouchManager.IsTouchEnabled = false;


        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            logger.Error(e.Exception);
        }


    }

    /// <summary>여러개의 리소스 파일을 접근하기 위한 래퍼 클래스.
    /// resx파일의 생성자가 internal이므로 public static으로 한번더 래핑해야 에러가 안난다.
    /// </summary>
    public class LocalizedResource
    {
        public LocalizedResource()
        {
            //Telerik 지역화
            LocalizationManager.Manager = new LocalizationManager
            {
                ResourceManager = TelerikResource.ResourceManager
            };
        }
        public static LocalizedStrings Strings { get; set; } = new LocalizedStrings();
    }

}
