using Autofac;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Tectone.Common.Mvvm;
using Tectone.Wpf.Controls;

namespace OChang.AutoCotnrol.Pie
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
            //IoC
            var builder = new ContainerBuilder();
            builder.RegisterType<RadMessageBox>().As<IMessageBox>();
            builder.RegisterType<RadAlertManager>().As<IAlertManager>();

            ViewModelBase.Container = builder.Build();
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            logger.Error(e.Exception);
        }
    }
}
