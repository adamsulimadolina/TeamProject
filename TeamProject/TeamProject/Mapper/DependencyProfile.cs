using AutoMapper;
using FormGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamProject.DTOs.FieldDependency;
using TeamProject.Infrastructure.Enums;
using TeamProject.Models.FieldDependencyModels;

namespace TeamProject.Mapper
{
    public class DependencyProfile : Profile
    {
        public DependencyProfile()
        {
            CreateMap<CreateDependencyDTO, FieldFieldDependency>()
                .ForMember(dest => dest.DependencyType, m => m.MapFrom(src => (src.DependencyType == "FieldDuplication" ? FieldFieldDependencyType.FieldDuplication : FieldFieldDependencyType.FieldVisibly)))
                .ForPath(dest => dest.ThisField.Name, m => m.MapFrom(src => src.SuperiorFieldName));
        }
    }
}
