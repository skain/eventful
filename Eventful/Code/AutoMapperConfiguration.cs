using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using EventfulBackend;
using EventfulBackend.EventfulAggregation;
using EventfulBackend.EventfulReporting;
using EventfulBackend.Klaxon;
using Eventful.Models;

namespace Eventful.Code
{
	public static class AutoMapperConfiguration
	{
		public static void ConfigureMappings()
		{
			Mapper.CreateMap<EventfulAggregateModel, AggregateRequest>();
			Mapper.CreateMap<AggregateRequest, EventfulAggregateModel>();
			Mapper.CreateMap<EventfulReportModel, EventfulReport>();
			Mapper.CreateMap<EventfulReport, EventfulReportModel>();
			Mapper.CreateMap<EventfulUserModel, EventfulUser>();
			Mapper.CreateMap<EventfulUser, EventfulUserModel>();
			Mapper.CreateMap<KlaxonModel, Klaxon>();
			Mapper.CreateMap<Klaxon, KlaxonModel>();
		}
	}
}