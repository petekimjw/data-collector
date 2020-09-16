using AutoMapper;
using Cim.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cim.Domain
{
    public static class AutoMapper
    {
        public static Mapper Mapper = null;

        public static MapperConfiguration Config = null;

        static AutoMapper()
        {
            Config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<AddressMap, AddressMap>();
                cfg.CreateMap<AddressMap, ModbusAddressMap>();
                cfg.CreateMap<AddressMap, AddressData>();
                cfg.CreateMap<ModbusAddressMap, AddressData>();
                //cfg.CreateMap<AddressMap, AddressDataWrapper>();
            });
            
            Mapper = new Mapper(Config);
        }
        
    }
}
