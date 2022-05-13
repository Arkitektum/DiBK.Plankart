using Microsoft.Extensions.Configuration;

namespace DiBK.Plankart.Application.Services
{
    public class AccessTokenProvider : IAccessTokenProvider
    {
        private readonly IConfiguration _configuration;

        public AccessTokenProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CesiumIonToken()
        {
            return _configuration.GetSection("AccessTokens")["CesiumIon"];
        }
    }
}
