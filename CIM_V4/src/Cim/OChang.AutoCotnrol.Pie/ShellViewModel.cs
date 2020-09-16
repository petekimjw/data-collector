using Cim.Manager;
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

namespace OChang.AutoCotnrol.Pie
{
    public class ShellViewModel: ShellViewModelBase
    {

        public override void Init()
        {

        }

        #region Test

        public void Test1()
        {
            BusyMessage = "test 111";
            IsBusy = true;
        }

        public void Test2()
        {
            IsBusy = false;
        }

        #endregion

    }
}
