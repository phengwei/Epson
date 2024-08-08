using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.Schemas;
using ITfoxtec.Identity.Saml2.MvcCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Authentication;
using Epson.Infrastructure;
using Microsoft.Extensions.Options;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;

namespace Epson.Controllers.API
{
    [AllowAnonymous]
    [Route("Auth")]
    public class AuthApiController : Controller
    {
        const string relayStateReturnUrl = "ReturnUrl";
        private readonly IConfiguration configuration;
        private readonly Saml2Configuration config;
        private readonly Serilog.ILogger logger;

        public AuthApiController(IOptions<Saml2Configuration> saml2Config, IConfiguration configuration, Serilog.ILogger logger)
        {
            this.configuration = configuration;
            this.logger = logger;
            
            config = new Saml2Configuration
            {
                Issuer = configuration["Saml2:Issuer"],
                SingleSignOnDestination = new Uri(configuration["Saml2:SingleSignOnDestination"]),
                SingleLogoutDestination = new Uri(configuration["Saml2:SingleLogoutDestination"]),
                SignatureAlgorithm = configuration["Saml2:SignatureAlgorithm"],
                SigningCertificate = new X509Certificate2(
                    configuration["Saml2:SigningCertificateFile"],
                    configuration["Saml2:SigningCertificatePassword"]
                )
            };

            var validationCerts = configuration.GetSection("Saml2:SignatureValidationCertificates").Get<List<string>>();
            foreach (var certPath in validationCerts)
            {
                config.SignatureValidationCertificates.Add(new X509Certificate2(certPath));
            }

        }

        [HttpGet("ssoLogin")]
        public IActionResult ssoLogin(string returnUrl = null)
        {
            var binding = new Saml2PostBinding();
            binding.SetRelayStateQuery(new Dictionary<string, string> { { relayStateReturnUrl, returnUrl ?? Url.Content("~/") } });

            return binding.Bind(new Saml2AuthnRequest(config)).ToActionResult();
        }

        [HttpPost("AssertionConsumerService")]
        public async Task<IActionResult> AssertionConsumerService()
        {
            var httpRequest = Request.ToGenericHttpRequest(validate: true);
            var saml2AuthnResponse = new Saml2AuthnResponse(config);

            logger.Information("Reading SAML response from HTTP request...");
            httpRequest.Binding.ReadSamlResponse(httpRequest, saml2AuthnResponse);

            if (saml2AuthnResponse.Status != Saml2StatusCodes.Success)
            {
                logger.Error($"SAML Response status: {saml2AuthnResponse.Status}");
                throw new AuthenticationException($"SAML Response status: {saml2AuthnResponse.Status}");
            }

            logger.Information("Unbinding SAML response from HTTP request...");
            httpRequest.Binding.Unbind(httpRequest, saml2AuthnResponse);

            logger.Information("Creating session for the authenticated user...");
            await saml2AuthnResponse.CreateSession(HttpContext,
                claimsTransform: (claimsPrincipal) => ClaimsTransform.Transform(claimsPrincipal));

            var relayStateQuery = httpRequest.Binding.GetRelayStateQuery();
            var returnUrl = relayStateQuery.ContainsKey(relayStateReturnUrl) ? relayStateQuery[relayStateReturnUrl] : Url.Content("~/");

            logger.Information($"Redirecting to {returnUrl}");
            return Redirect(returnUrl);
        }


        [HttpPost("AuthLogout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Redirect(Url.Content("~/"));
            }

            var binding = new Saml2PostBinding();
            var saml2LogoutRequest = await new Saml2LogoutRequest(config, User).DeleteSession(HttpContext);
            return binding.Bind(saml2LogoutRequest).ToActionResult();
        }

        [HttpGet("authLoggedout")]
        public IActionResult LoggedOut()
        {
            var httpRequest = Request.ToGenericHttpRequest(validate: true);
            httpRequest.Binding.Unbind(httpRequest, new Saml2LogoutResponse(config));

            return Redirect(Url.Content("~/"));
        }

        [HttpPost("singleLogout")]
        public async Task<IActionResult> SingleLogout()
        {
            Saml2StatusCodes status;
            var httpRequest = Request.ToGenericHttpRequest(validate: true);
            var logoutRequest = new Saml2LogoutRequest(config, User);
            try
            {
                httpRequest.Binding.Unbind(httpRequest, logoutRequest);
                status = Saml2StatusCodes.Success;
                await logoutRequest.DeleteSession(HttpContext);
            }
            catch (Exception exc)
            {
                // log exception
                logger.Error($"SingleLogout error: {exc}");
                status = Saml2StatusCodes.RequestDenied;
            }

            var responsebinding = new Saml2PostBinding();
            responsebinding.RelayState = httpRequest.Binding.RelayState;
            var saml2LogoutResponse = new Saml2LogoutResponse(config)
            {
                InResponseToAsString = logoutRequest.IdAsString,
                Status = status,
            };
            return responsebinding.Bind(saml2LogoutResponse).ToActionResult();
        }
    }
}
