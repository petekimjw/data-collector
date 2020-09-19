using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tectone.Common.Mvvm;
using Tectone.Common.Utils;
using Tectone.Wpf.Controls;

namespace Cim.Manager
{
    public abstract class WindowViewModelBase : ViewModelBase
    {

        protected Window ThisWindow = null;

        public override void Init()
        {
            
        }

        public void InitWindow(UIElement control)
        {
            ThisWindow = control.AncestorsAndSelf<Window>()?.FirstOrDefault();
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
                if (ThisWindow?.WindowState == WindowState.Normal)
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
                        ThisWindow?.DragMove();

                }
                catch (Exception ex)
                {
                    //Debug.WriteLine(ex);
                }
            }
        }

        public virtual void MinimizeWindow()
        {
            ThisWindow.WindowState = WindowState.Minimized;
        }

        public virtual void MaximizeWindow()
        {
            var MaximizeButton = ThisWindow.FindName("MaximizeButton") as Button;
            var RestoreMaxButton = ThisWindow.FindName("RestoreMaxButton") as Button;
            MaximizeButton.Visibility = Visibility.Collapsed;
            RestoreMaxButton.Visibility = Visibility.Visible;

            ThisWindow.WindowState = WindowState.Maximized;
        }

        public virtual void RestoreMaxWindow()
        {
            var MaximizeButton = ThisWindow.FindName("MaximizeButton") as Button;
            var RestoreMaxButton = ThisWindow.FindName("RestoreMaxButton") as Button;
            MaximizeButton.Visibility = Visibility.Visible;
            RestoreMaxButton.Visibility = Visibility.Collapsed;

            ThisWindow.WindowState = WindowState.Normal;
        }

        public virtual void CloseWindow(bool showDialog=true)
        {
            try
            {
                if (showDialog)
                    RadMessageBox.Confirm("Manager를 종료하시겠습니까?", this.MainWindow, async (sender, args) =>
                    {
                        if (args.DialogResult == true)
                        {
                            if (ThisWindow == MainWindow)
                                Application.Current.Shutdown();
                            else
                                ThisWindow.Close();
                        }
                    });
                else
                {
                    if (ThisWindow == MainWindow)
                        Application.Current.Shutdown();
                    else
                        ThisWindow.Close();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        #endregion

    }
}
