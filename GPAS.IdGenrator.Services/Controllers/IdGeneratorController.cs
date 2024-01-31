using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace GPAS.IdGenrator.Services.Controllers
{
    [RoutePrefix("api/IdGenerator")]
    public class IdGeneratorController : ApiController
    {
        private readonly RepositoryProvider repositoryProvider;

        public IdGeneratorController()
        {
            repositoryProvider = new RepositoryProvider();
        }

        [HttpGet]
        [Route("NewObjectId")]
        public long GetNewObjectId()
        {
            return repositoryProvider.GetNewObjectId();
        }

        [HttpGet]
        [Route("NewObjectIdRange/{range}")]
        public long GetNewObjectIdRange([FromUri] long range)
        {
            if (range == 0)
            {
                HttpResponseMessage response = Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "The range must be more than zero!");
                throw new HttpResponseException(response);
            }

            return repositoryProvider.GetNewObjectIdRange(range);
        }

        [HttpGet]
        [Route("NewPropertyId")]
        public long GetNewPropertyId()
        {
            return repositoryProvider.GetNewPropertyId();
        }

        [HttpGet]
        [Route("NewPropertyIdRange/{range}")]
        public long GetNewPropertyIdRange([FromUri] long range)
        {
            if (range == 0)
            {
                HttpResponseMessage response = Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "The range must be more than zero!");
                throw new HttpResponseException(response);
            }

            return repositoryProvider.GetNewPropertyIdRange(range);
        }

        [HttpGet]
        [Route("NewRelationId")]
        public long GetNewRelationId()
        {
            return repositoryProvider.GetNewRelationId();
        }

        [HttpGet]
        [Route("NewRelationIdRange/{range}")]
        public long GetNewRelationIdRange([FromUri] long range)
        {
            if (range == 0)
            {
                HttpResponseMessage response = Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "The range must be more than zero!");
                throw new HttpResponseException(response);
            }

            return repositoryProvider.GetNewRelationIdRange(range);
        }

        [HttpGet]
        [Route("NewMediaId")]
        public long GetNewMediaId()
        {
            return repositoryProvider.GetNewMediaId();
        }

        [HttpGet]
        [Route("NewMediaIdRange/{range}")]
        public long GetNewMediaIdRange([FromUri] long range)
        {
            if (range == 0)
            {
                HttpResponseMessage response = Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "The range must be more than zero!");
                throw new HttpResponseException(response);
            }

            return repositoryProvider.GetNewMediaIdRange(range);
        }

        [HttpGet]
        [Route("NewDataSourceId")]
        public long GetNewDataSourceId()
        {
            return repositoryProvider.GetNewDataSourceId();
        }

        [HttpGet]
        [Route("NewGraphId")]
        public long GetNewGraphId()
        {
            return repositoryProvider.GetNewGraphId();
        }

        [HttpGet]
        [Route("NewInvestigationId")]
        public long GetNewInvestigationId()
        {
            return repositoryProvider.GetNewInvestigationId();
        }
    }
}