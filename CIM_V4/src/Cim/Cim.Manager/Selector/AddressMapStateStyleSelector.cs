using Cim.Manager.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Cim.Manager.Selector
{
    public class AddressMapStateStyleSelector : StyleSelector
    {
        public Style NoneStyle { get; set; }
        public Style UpdateStyle { get; set; }
        public Style InsertStyle { get; set; }
        public Style DeleteStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            var wrapper = item as AddressDataWrapper;
            if (wrapper == null)
                return NoneStyle;

            switch (wrapper.State)
            {
                case Tectone.Common.Mvvm.State.None:
                    return NoneStyle;
                case Tectone.Common.Mvvm.State.Delete:
                    return DeleteStyle;
                case Tectone.Common.Mvvm.State.Insert:
                    return InsertStyle;
                case Tectone.Common.Mvvm.State.Update:
                    return UpdateStyle;
                default:
                    break;
            }
            return NoneStyle;
        }
    }
}
