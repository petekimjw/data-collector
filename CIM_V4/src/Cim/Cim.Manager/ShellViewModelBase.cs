using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tectone.Common.Mvvm;
using Tectone.Wpf.Controls;

namespace Cim.Manager
{
    public abstract class ShellViewModelBase : ViewModelBase
    {

        public override void Init()
        {

        }

        #region 창 제어

        public RadMessageBox RadMessageBox { get; set; } = new RadMessageBox();

        /// <summary>최대/최소, 창이동
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public virtual void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                //헤더 더블클릭
                if (MainWindow?.WindowState == WindowState.Normal)
                {
                    MaximizeWindow();
                }
                else
                {
                    RestoreMaxWindow();
                }
            }
            else
            {
                try
                {
                    if (e.ChangedButton == MouseButton.Left && e.LeftButton == MouseButtonState.Pressed)
                        MainWindow?.DragMove();

                }
                catch (Exception ex)
                {
                    //Debug.WriteLine(ex);
                }
            }
        }

        public virtual void MinimizeWindow()
        {
            MainWindow.WindowState = WindowState.Minimized;
        }

        public virtual void MaximizeWindow()
        {
            var MaximizeButton = MainWindow.FindName("MaximizeButton") as Button;
            var RestoreMaxButton = MainWindow.FindName("RestoreMaxButton") as Button;
            MaximizeButton.Visibility = Visibility.Collapsed;
            RestoreMaxButton.Visibility = Visibility.Visible;

            MainWindow.WindowState = WindowState.Maximized;
        }

        public virtual void RestoreMaxWindow()
        {
            var MaximizeButton = MainWindow.FindName("MaximizeButton") as Button;
            var RestoreMaxButton = MainWindow.FindName("RestoreMaxButton") as Button;
            MaximizeButton.Visibility = Visibility.Visible;
            RestoreMaxButton.Visibility = Visibility.Collapsed;

            MainWindow.WindowState = WindowState.Normal;
        }

        public virtual void CloseWindow()
        {
            try
            {
                RadMessageBox.Confirm("Manager를 종료하시겠습니까?", this.MainWindow, async (sender, args) =>
                {
                    if (args.DialogResult == true)
                    {

                        Application.Current.Shutdown();
                    }
                });

            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        #endregion

    }
}
