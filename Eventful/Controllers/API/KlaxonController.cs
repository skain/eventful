using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Eventful.Models;
using NLog;
using EventfulLogger.LoggingUtils;

namespace Eventful.Controllers.API
{
	public class KlaxonController : ApiController
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		[Route("api/klaxon/getall")]
		public IEnumerable<KlaxonModel> GetAll()
		{
			IEnumerable<KlaxonModel> ks = KlaxonModel.GetAll();
			return ks;
		}

		public KlaxonModel Post(KlaxonModel k)
		{
			k.Create();
			return k;
		}

		public KlaxonModel Get(string id)
		{
			KlaxonModel km = KlaxonModel.GetById(id);
			return km;
		}

		public KlaxonModel Put(KlaxonModel km)
		{
			km.Update(true);
			return km;
		}
	}
}
