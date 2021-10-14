using Microsoft.Extensions.DependencyInjection;
using System;

namespace DiBK.Plankart.Config
{
    public static class ValidatorConfig
    {
        public static void AddValidators(this IServiceCollection services, Action<ValidatorOptions> options)
        {
            services.Configure(options);
        }
    }
}
