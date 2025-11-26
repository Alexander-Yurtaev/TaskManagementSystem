using Grpc.Core;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Text;
using TMS.Common.Grpc.Token;
using TMS.Common.Helpers;
using TMS.Common.Validators;

namespace TMS.AuthService.Services.Grpc
{
    /// <summary>
    ///
    /// </summary>
    public class AuthService : Auth.AuthBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        /// <summary>
        ///
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        public AuthService(IConfiguration configuration, ILogger<AuthService> logger)
        {
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<ValidateTokenReply> ValidateToken(ValidateTokenRequest request, ServerCallContext context)
        {
            JwtValidator.ThrowIfNotValidate(_configuration);

            var token = request.Token;
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtKey = _configuration["JWT_KEY"]!;
            var issuer = _configuration["JWT_ISSUER"];
            var audience = _configuration["JWT_AUDIENCE"];
            var key = Encoding.ASCII.GetBytes(jwtKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidIssuer = issuer,
                ValidAudience = audience
            };

            var result = new ValidateTokenReply();

            try
            {
                tokenHandler.ValidateToken(token, validationParameters, out var securityToken);
                if (securityToken is not JwtSecurityToken jwtToken)
                {
                    return Task.FromResult(result);
                }

                result.IsValidate = true;

                // Преобразование формата gRPC в Claims
                var claims = jwtToken.Claims.Select(c => new Claim
                {
                    Type = c.Type,
                    Value = c.Value
                });
                result.Claims.AddRange(claims);
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogError(ex, "Ошибка валидации токена.");
                result.IsValidate = false;
                return Task.FromResult(result);
            }

            return Task.FromResult(result);
        }
    }
}