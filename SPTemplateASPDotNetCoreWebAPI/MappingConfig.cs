using SPTemplateASPDotNetCoreWebAPI.Models.DTO;
using SPTemplateASPDotNetCoreWebAPI.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace SPTemplateASPDotNetCoreWebAPI
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<User, UserDto>().ReverseMap();
        }
    
    }
}
