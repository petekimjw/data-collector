using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tectone.Common.Extensions;

namespace Cim.Manager.Views
{
    public class AddressMapEditViewModel : WindowViewModelBase
    {
        public AddressMapEditViewModel()
        {
            
        }

        private void AddressMapEditViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (PropertyChangeds.ContainsKey(e.PropertyName))
                PropertyChangeds[e.PropertyName] = SelectedAddressDataWrapper.GetPropertyValue(e.PropertyName);
            else
                PropertyChangeds.Add(e.PropertyName, SelectedAddressDataWrapper.GetPropertyValue(e.PropertyName));
        }

        public Dictionary<string, object> PropertyChangeds = new Dictionary<string, object>();

        private AddressDataWrapper _SelectedAddressDataWrapper;
        public AddressDataWrapper SelectedAddressDataWrapper
        {
            get { return _SelectedAddressDataWrapper; }
            set 
            { 
                Set(ref _SelectedAddressDataWrapper, value);
                if(value != null)
                    value.PropertyChanged += AddressMapEditViewModel_PropertyChanged;
            }
        }


        public void Confirm()
        {
            if(SelectedAddressDataWrapper != null)
                SelectedAddressDataWrapper.PropertyChanged -= AddressMapEditViewModel_PropertyChanged;

            ThisWindow.DialogResult = true;
            ThisWindow.Close();
        }
    }
}
