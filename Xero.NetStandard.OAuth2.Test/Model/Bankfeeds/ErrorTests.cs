/* 
 * Bank Feeds API
 *
 * This specifing endpoints Xero Bank feeds API
 *
 * The version of the OpenAPI document: 2.6.1
 * Contact: api@xero.com
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */


using Xunit;

using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Model.Bankfeeds;
using Xero.NetStandard.OAuth2.Client;
using System.Reflection;
using Newtonsoft.Json;
using RestSharp;

namespace Xero.NetStandard.OAuth2.Test.Model.Bankfeeds
{
    /// <summary>
    ///  Class for testing Error
    /// </summary>
    /// <remarks>
    /// This file is automatically generated by OpenAPI Generator (https://openapi-generator.tech).
    /// Please update the test case below to test the model.
    /// </remarks>
    public class ErrorTests : IDisposable
    {
        public ErrorTests()
        {
            // TODO uncomment below to create an instance of Error
            //instance = new Error();
        }

        public void Dispose()
        {
            // Cleanup when everything is done.
        }

        /// <summary>
        /// Test the property 'Type' deserialises from valid inputs
        /// </summary>
        [Theory]
        [InlineData("invalid-request", Error.TypeEnum.InvalidRequest)]
        [InlineData("invalid-application", Error.TypeEnum.InvalidApplication)]
        [InlineData("invalid-feed-connection", Error.TypeEnum.InvalidFeedConnection)]
        [InlineData("duplicate-statement", Error.TypeEnum.DuplicateStatement)]
        [InlineData("invalid-end-balance", Error.TypeEnum.InvalidEndBalance)]
        [InlineData("invalid-start-and-end-date", Error.TypeEnum.InvalidStartAndEndDate)]
        [InlineData("invalid-start-date", Error.TypeEnum.InvalidStartDate)]
        [InlineData("internal-error", Error.TypeEnum.InternalError)]
        [InlineData("feed-already-connected-in-current-organisation", Error.TypeEnum.FeedAlreadyConnectedInCurrentOrganisation)]
        [InlineData("invalid-end-date", Error.TypeEnum.InvalidEndDate)]
        [InlineData("statement-not-found", Error.TypeEnum.StatementNotFound)]
        [InlineData("feed-connected-in-different-organisation", Error.TypeEnum.FeedConnectedInDifferentOrganisation)]
        [InlineData("feed-already-connected-in-different-organisation", Error.TypeEnum.FeedAlreadyConnectedInDifferentOrganisation)]
        [InlineData("bank-feed-not-found", Error.TypeEnum.BankFeedNotFound)]
        [InlineData("invalid-country-specified", Error.TypeEnum.InvalidCountrySpecified)]
        [InlineData("invalid-organisation-bank-feeds", Error.TypeEnum.InvalidOrganisationBankFeeds)]
        [InlineData("invalid-organisation-multi-currency", Error.TypeEnum.InvalidOrganisationMultiCurrency)]
        [InlineData("invalid-feed-connection-for-organisation", Error.TypeEnum.InvalidFeedConnectionForOrganisation)]
        [InlineData("invalid-user-role", Error.TypeEnum.InvalidUserRole)]
        [InlineData("account-not-valid", Error.TypeEnum.AccountNotValid)]
        public void Type_ValidInput_Deserialises(string input, Error.TypeEnum expected)
        {
            var response = new RestResponse();
            response.Content = $@"{{
                ""Type"": ""{input}""
            }}";
            response.StatusCode = System.Net.HttpStatusCode.OK;

            var deserializer = new CustomJsonCodec(new Configuration());
            var actual = deserializer.Deserialize<Error>(response);

            Assert.Equal(expected, actual.Type);
        }

        /// <summary>
        /// Test the property 'Type' deserialises from null into 0
        /// </summary>
        [Fact]
        public void Type_NullInput_DeserialisesTo0()
        {
            var response = new RestResponse();
            response.Content = $@"{{
                ""Type"": null
            }}";
            response.StatusCode = System.Net.HttpStatusCode.OK;

            var deserializer = new CustomJsonCodec(new Configuration());
            var actual = deserializer.Deserialize<Error>(response);

            Assert.Equal(0, (int) actual.Type);
        }

        /// <summary>
        /// Test the property 'Type' deserialises to 0 when not present
        /// </summary>
        [Fact]
        public void Type_NotPresentInInput_DeserialisesTo0()
        {
            var response = new RestResponse();
            response.Content = "{}";
            response.StatusCode = System.Net.HttpStatusCode.OK;

            var deserializer = new CustomJsonCodec(new Configuration());
            var actual = deserializer.Deserialize<Error>(response);

            Assert.Equal(0, (int) actual.Type);
        }

    }

}
