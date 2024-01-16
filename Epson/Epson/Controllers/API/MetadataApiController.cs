using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.MvcCore;
using ITfoxtec.Identity.Saml2.Schemas;
using ITfoxtec.Identity.Saml2.Schemas.Metadata;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Epson.Controllers.API
{
    [AllowAnonymous]
    [Route("api/metadata")]
    public class MetadataApiController : ControllerBase
    {
        private readonly Saml2Configuration config;

        public MetadataApiController(Saml2Configuration config)
        {
            this.config = config;
        }

        [HttpGet("GetMetadata")]
        public async Task<IActionResult> GetMetadata()
        {
            //var defaultSite = new Uri($"{Request.Scheme}://{Request.Host.ToUriComponent()}/");
            var defaultSite = "https://epson-asia.azurewebsites.net/";

            var entityDescriptor = new EntityDescriptor(config);
            entityDescriptor.ValidUntil = 365;
            entityDescriptor.SPSsoDescriptor = new SPSsoDescriptor
            {
                AuthnRequestsSigned = config.SignAuthnRequest,
                WantAssertionsSigned = true,
                SigningCertificates = new X509Certificate2[]
                {
                    config.SigningCertificate
                },

                SingleLogoutServices = new SingleLogoutService[]
                {
                    new SingleLogoutService { Binding = ProtocolBindings.HttpPost, Location = new Uri(defaultSite + "Auth/SingleLogout"), ResponseLocation = new Uri(defaultSite + "Auth/LoggedOut") }
                },
                NameIDFormats = new Uri[] { NameIdentifierFormats.X509SubjectName },
                AssertionConsumerServices = new AssertionConsumerService[]
                {
                    new AssertionConsumerService { Binding = ProtocolBindings.HttpPost, Location = new Uri(defaultSite + "Auth/acs") },
                },
                AttributeConsumingServices = new AttributeConsumingService[]
                {
                    new AttributeConsumingService { ServiceName = new ServiceName("Unity Management System", "en"), RequestedAttributes = CreateRequestedAttributes().ToList() }
                },

            };
            entityDescriptor.ContactPersons = new[] {
                new ContactPerson(ContactTypes.Administrative)
                {
                    Company = "Lavish",
                    GivenName = "Richard",
                    SurName = "Ng",
                    EmailAddress = "richardng@lavishteam.com",
                    TelephoneNumber = "011-11000508",
                }
            };

            var saml2Metadata = new Saml2Metadata(entityDescriptor);
            var metadataXml = saml2Metadata.CreateMetadata().ToXml();

            return Content(metadataXml, "application/xml");
        }

        private IEnumerable<RequestedAttribute> CreateRequestedAttributes()
        {
            yield return new RequestedAttribute("urn:oid:2.5.4.4");
            yield return new RequestedAttribute("urn:oid:2.5.4.3", false);
            yield return new RequestedAttribute("urn:xxx", "test-value");
            yield return new RequestedAttribute("urn:yyy", "123") { AttributeValueType = "xs:integer" };
        }
    }
}

