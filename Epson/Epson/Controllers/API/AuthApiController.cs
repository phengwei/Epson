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
using Epson.Models.Users;
using Epson.Services.Interface.Users;
using Epson.Core.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Epson.Model.Users;
using AngleSharp.Css;

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
        private readonly JwtService _jwtService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthApiController(IOptions<Saml2Configuration> saml2Config, IConfiguration configuration, Serilog.ILogger logger, JwtService jwtService, UserManager<ApplicationUser> userManager)
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
                ),
                AudienceRestricted = true, 
            };

            var allowedAudienceUris = configuration["Saml2:AllowedAudienceUris"];
            config.AllowedAudienceUris.Add(allowedAudienceUris);

            var validationCerts = configuration.GetSection("Saml2:SignatureValidationCertificates").Get<List<string>>();
            foreach (var certPath in validationCerts)
            {
                config.SignatureValidationCertificates.Add(new X509Certificate2(certPath));
            }
            _jwtService = jwtService;
            _userManager = userManager;
        }

        [HttpGet("ssoLogin")]
        public IActionResult ssoLogin(string returnUrl = null)
        {
            var binding = new Saml2PostBinding();
            binding.SetRelayStateQuery(new Dictionary<string, string> { { relayStateReturnUrl, returnUrl ?? Url.Content("~/") } });

            return binding.Bind(new Saml2AuthnRequest(config)).ToActionResult();
        }

        [HttpPost("AssertionConsumerService")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> AssertionConsumerService()
        {
            try
            {
                // Log the entire request including headers and body
                var requestHeaders = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
                var requestBody = new Dictionary<string, string>
                {
                    { "SAMLResponse", Request.Form["SAMLResponse"] },
                    { "RelayState", Request.Form["RelayState"] }
                };

                var fullRequestLog = new
                {
                    Headers = requestHeaders,
                    Body = requestBody
                };

                logger.Information($"Full HTTP Request: {System.Text.Json.JsonSerializer.Serialize(fullRequestLog)}");

                var rawSamlResponse = Request.Form["SAMLResponse"];
                logger.Information($"Raw SAML Response: {rawSamlResponse}");

                var saml2AuthnResponse = new Saml2AuthnResponse(config);

                logger.Information("Reading SAML response from HTTP request...");
                var binding = new Saml2PostBinding();
                binding.ReadSamlResponse(Request.ToGenericHttpRequest(), saml2AuthnResponse);

                if (saml2AuthnResponse.Status != Saml2StatusCodes.Success)
                {
                    logger.Error($"SAML Response status: {saml2AuthnResponse.Status}");
                    throw new AuthenticationException($"SAML Response status: {saml2AuthnResponse.Status}");
                }

                logger.Information("Extracting claims from SAML response...");
                var claims = saml2AuthnResponse.ClaimsIdentity.Claims;
                var email = claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
                var uniqueIdentifier = claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
                var givenName = claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname")?.Value;
                var surname = claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname")?.Value;

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(uniqueIdentifier))
                {
                    logger.Error("Email or Unique Identifier claim not found in SAML response.");
                    return StatusCode(500, "Internal server error: Missing required claims.");
                }

                var user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    logger.Information($"User found for email {email}, updating SSO identifier if necessary...");
                    if (string.IsNullOrEmpty(user.ssoIdentifier))
                    {
                        user.ssoIdentifier = uniqueIdentifier;
                        await _userManager.UpdateAsync(user);
                    }
                }
                else
                {
                    logger.Information($"No user found for email {email}, creating new user...");
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        ssoIdentifier = uniqueIdentifier,
                        firstName = givenName,
                        lastName = surname,
                        TeamId = 7,
                        IsActive = true
                    };

                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        logger.Error($"Error creating new user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        return StatusCode(500, "Internal server error: Unable to create new user.");
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, "Sales");
                    }
                }

                logger.Information("Creating session for the authenticated user...");
                await saml2AuthnResponse.CreateSession(HttpContext, claimsTransform: (claimsPrincipal) => ClaimsTransform.Transform(claimsPrincipal));

                //var relayStateQuery = binding.GetRelayStateQuery();
                //var returnUrl = relayStateQuery.ContainsKey(relayStateReturnUrl) ? relayStateQuery[relayStateReturnUrl] : Url.Content("~/");

                var relayState = Request.Form["RelayState"].ToString();
                logger.Information($"RelayState received: {relayState}");
                //var returnUrl = !string.IsNullOrEmpty(relayState) ? relayState : "https://ums.epson.com.my/handle-sso";
                var returnUrl = "https://localhost:7223/handle-sso";

                logger.Information($"Generating JWT token for user {user.Email}...");
                var generatedToken = await _jwtService.GenerateToken(user);

                logger.Information($"Redirecting to {returnUrl}");
                return Redirect($"{returnUrl}?token={generatedToken}");

            }
            catch (Exception ex)
            {
                logger.Error($"Unhandled exception in AssertionConsumerService: {ex}");
                return StatusCode(500, "Internal server error: An unexpected error occurred.");
            }
        }

        [HttpPost("AuthLogout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Redirect(Url.Content("~/"));
            }

            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            _jwtService.InvalidateToken(token);

            Response.Headers.Remove("Authorization");

            var binding = new Saml2PostBinding();
            var saml2LogoutRequest = await new Saml2LogoutRequest(config, User).DeleteSession(HttpContext);
            return binding.Bind(saml2LogoutRequest).ToActionResult();
        }

        [HttpGet("authLoggedout")]
        public IActionResult LoggedOut()
        {
            var httpRequest = Request.ToGenericHttpRequest(validate: true);
            httpRequest.Binding.Unbind(httpRequest, new Saml2LogoutResponse(config));

            return Redirect(Url.Content("~/login"));
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
