using IdentityModel.Client;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Text.Json;
using MvcClientEntitiesAPI.Models;
using System.Collections.Generic;

namespace MvcClient.EntitiesAPI
{
    public class Client
    {
        public string accessToken { get; set; }
        public string apiResp { get; set; }
        public string apiPayload { get; set; }
        public string apiSummary { get; set; }
        public string newRefToken { get; set; }

        // Refresh
        public async Task Refresh(string refreshToken)
        {
            // test input argument were supplied
            if (refreshToken == "")
            {
                apiResp = "Please pass a refresh token.";
                return;
            }

            // discover endpoints from metadata
            var client = new HttpClient();

            var disco = await client.GetDiscoveryDocumentAsync("https://ids-dev11.blueprintserver.com/ngdev-60");
            if (disco.IsError)
            {
                apiResp = disco.Error;
                return;
            }

            // exchange refresh token
            var tokenResponse = await client.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = "https://ids-dev11.blueprintserver.com/ngdev-60/connect/token",

                ClientId = "public.api",
                ClientSecret = "87d19a99040284d433e9becd800670d1ec8bbd5dfcec9df724cfd901dfc602b9",

                RefreshToken = refreshToken
            });

            if (tokenResponse.IsError)
            {
                apiResp = tokenResponse.Error;
                return;
            }

            accessToken = tokenResponse.AccessToken;
            newRefToken = tokenResponse.RefreshToken;

            // call api
            var apiClient = new HttpClient();
            try
            {
                apiClient.DefaultRequestHeaders.Add("Tenant-Id", "ngdev-60");
                apiClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokenResponse.AccessToken);

                var options = new JsonWriterOptions
                {
                    Indented = true
                };

                // format payload
                using (var stream = new MemoryStream())
                {
                    using (var writer = new Utf8JsonWriter(stream, options))
                    {
                        writer.WriteStartObject();

                        writer.WriteStartArray("sortCriteria");
                        writer.WriteEndArray();

                        writer.WriteStartArray("groupCriteria");
                        writer.WriteEndArray();

                        writer.WriteStartArray("searchCriteria");
                        writer.WriteEndArray();

                        writer.WriteStartArray("columns");
                        writer.WriteEndArray();

                        writer.WriteStartObject("parameters");
                        writer.WriteEndObject();

                        writer.WriteNumber("filterCombinator", 0);
                        writer.WriteNumber("pageSize", 10);
                        writer.WriteNumber("pageNumber", 0);

                        writer.WriteEndObject();
                    }

                    string json = Encoding.UTF8.GetString(stream.ToArray());
                    apiPayload = json;

                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var httpResponseMessage = await apiClient.PostAsync(
                        "https://dil-as-test.azure-api.net/entities/nextgen/companies/search",
                        content);
                    apiResp = await httpResponseMessage.Content.ReadAsStringAsync();

                    // parse response
                    int count = 0;

                    using (JsonDocument document = JsonDocument.Parse(apiResp))
                    {
                        JsonElement root = document.RootElement;
                        JsonElement companyElements = root.GetProperty("rows");

                        count = companyElements.GetArrayLength();

                        List<Company> companyList = new List<Company>();

                        // create company objects
                        foreach (JsonElement companyElement in companyElements.EnumerateArray())
                        {
                            if (companyElement.TryGetProperty("COMPANIES$QR", out JsonElement qrElement) && companyElement.TryGetProperty("COMPANIES$NAME", out JsonElement nameElement))
                            {
                                companyList.Add(new Company(qrElement.ToString(), nameElement.ToString()));
                            }
                        }

                        // format summary
                        apiSummary = "Response parsed, created " + count + " Company objects...";
                        count = 0;

                        foreach (var company in companyList)
                        {
                            apiSummary += "<br><br>" + (count + 1) + ". Name: " + company.Name + "<br>&nbsp;&nbsp;&nbsp;&nbsp;QR: " + company.QR;
                            count++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                apiResp = "Caught exception : " + ex;
            }
        }
    }
}