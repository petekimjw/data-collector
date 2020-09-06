using AutoMapper;
using Cim.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cim
{
    public static class AutoMapper
    {
        public static Mapper Mapper = null;

        static AutoMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<AddressMap, AddressMap>();
                cfg.CreateMap<AddressMap, ModbusAddressMap>();
                cfg.CreateMap<AddressMap, AddressData>();


            });

            Mapper = new Mapper(config);
        }
        

    }
}
