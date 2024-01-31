using System.Web.Http;
using WebActivatorEx;
using GPAS.IdGenrator.Services;
using Swashbuckle.Application;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace GPAS.IdGenrator.Services
{
    public class SwaggerConfig
    {
        public static void Register()
        {
            var thisAssembly = typeof(SwaggerConfig).Assembly;

            GlobalConfiguration.Configuration
              .EnableSwagger(c => c.SingleApiVersion("v1", "Id Generator Web API"))
              .EnableSwaggerUi();
        }
    }
}
