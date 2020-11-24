using IdentityModel.Client;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Client
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("Diligent Entities API App - Copyright 2020");
            Console.WriteLine("\n");

            // test input arguments were supplied
            if (args.Length != 2 || args[0] != "--RefreshToken")
            {
                Console.WriteLine("Please pass a refresh token.");
                Console.WriteLine("\n");
                return;
            }

            // discover endpoints from metadata
            var client = new HttpClient();

            var disco = await client.GetDiscoveryDocumentAsync("https://ids-dev11.blueprintserver.com/ngdev-60");
            if (disco.IsError)
            {
                Console.WriteLine("Discovery error : " + disco.Error);
                Console.WriteLine("\n");
                return;
            }

            // exchange refresh token
            var refreshToken = args[1];

            Console.WriteLine("Refresh Token entered: " + refreshToken);
            Console.WriteLine("\n");

            var tokenResponse = await client.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = "https://ids-dev11.blueprintserver.com/ngdev-60/connect/token",

                ClientId = "public.api",
                ClientSecret = "87d19a99040284d433e9becd800670d1ec8bbd5dfcec9df724cfd901dfc602b9",

                RefreshToken = refreshToken
            }); ;

            if (tokenResponse.IsError)
            {
                Console.WriteLine("Token error: " + tokenResponse.Error);
                Console.WriteLine("\n");
                return;
            }

            // BTL Console.WriteLine(tokenResponse.Json);
            Console.WriteLine("Access Token exchanged: " + tokenResponse.AccessToken);
            Console.WriteLine("\n");

            Console.WriteLine("New Refresh Token: " + tokenResponse.RefreshToken);
            Console.WriteLine("\n");

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
                    Console.WriteLine("Payload: " + json);
                    Console.WriteLine("\n");

                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var httpResponseMessage = await apiClient.PostAsync(
                        "https://dil-as-test.azure-api.net/entities/nextgen/companies/search",
                        content);
                    string resp = await httpResponseMessage.Content.ReadAsStringAsync();
                    Console.WriteLine("Summary: Companies refreshed");
                    Console.WriteLine("\n");
                    Console.WriteLine("Response: " + resp);
                    Console.WriteLine("\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Caught exception : " + ex);
                Console.WriteLine("\n");
            }
        }
    }
}