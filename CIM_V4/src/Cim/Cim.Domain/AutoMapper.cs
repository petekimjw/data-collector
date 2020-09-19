using AutoMapper;
using AutoMapper.Configuration;
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
        public static MapperConfigurationExpression BaseMapping = new MapperConfigurationExpression();

        private static MapperConfiguration config = null;


        static AutoMapper()
        {
            BaseMapping.CreateMap<AddressMap, AddressMap>();
            BaseMapping.CreateMap<AddressMap, AddressData>();

            config = new MapperConfiguration(BaseMapping); 
            Mapper = new Mapper(config);
        }

        public static void Init(MapperConfigurationExpression mapping)
        {
            config = new MapperConfiguration(mapping);
            Mapper = new Mapper(config);

            //Mapper.ConfigurationProvider.GetAllTypeMaps()
        }

    }
}
