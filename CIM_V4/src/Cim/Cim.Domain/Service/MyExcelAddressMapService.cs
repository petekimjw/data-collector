using Cim.Model;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cim.Service
{
    public class MyExcelAddressMapService : ExcelAddressMapService
    {
        public override AddressMap ParseCustomAddressMap(AddressMap input, List<string> columns, IXLTableRow row)
        {
            //input = base.ParseCustomAddressMap(input, columns, row);



            return input;
        }
    }
}
