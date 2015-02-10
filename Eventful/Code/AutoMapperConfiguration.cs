using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using eventfulBackend;
using eventfulBackend.eventfulAggregation;
using eventfulBackend.eventfulReporting;
using eventfulBackend.Klaxon;
using eventful.Models;

namespace eventful.Code
{
	public static class AutoMapperConfiguration
	{
		public static void ConfigureMappings()
		{
			Mapper.CreateMap<eventfulAggregateModel, AggregateRequest>();
			Mapper.CreateMap<AggregateRequest, eventfulAggregateModel>();
			Mapper.CreateMap<eventfulReportModel, eventfulReport>();
			Mapper.CreateMap<eventfulReport, eventfulReportModel>();
			Mapper.CreateMap<eventfulUserModel, eventfulUser>();
			Mapper.CreateMap<eventfulUser, eventfulUserModel>();
			Mapper.CreateMap<KlaxonModel, Klaxon>();
			Mapper.CreateMap<Klaxon, KlaxonModel>();
		}
	}
}