using System;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Text;

namespace HttpClientSample {
    public class Program {
        private static string API_KEY = "0591d8c076c8faeb2b9acfd16aadfffd";
        static HttpClient client = new HttpClient();

        static async Task<string?> GenerateVoice(string voiceId) {
            string url = $"https://api.elevenlabs.io/v1/text-to-speech/{voiceId}/stream";

            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Add("Accept", "audio/mpeg");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("xi-api-key", Program.API_KEY);

            // HTTP body content
            var content = new {
                text = "This is a test of how well exporting to an mp3 file works in csharp",
                model_id = "eleven_monolingual_v1",
                voice_settings = new {
                    stability = 0.5, similarity_boost = 0.5
                }
            };
            HttpContent jsonContent = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");

            // post to the server
            using(HttpResponseMessage response = await client.PostAsync(url, jsonContent)) {
                if(response.IsSuccessStatusCode) {
                    // read data as a stream
                    byte[] responseBytes = new byte[4096];


                    using(Stream audioChunk = await response.Content.ReadAsStreamAsync()) {
                        using (FileStream fs = File.Create("test.mp3")) {
                            int responseValue;

                            while((responseValue = await audioChunk.ReadAsync(responseBytes, 0, responseBytes.Length)) > 0) {
                                if(audioChunk.Length - audioChunk.Position < responseBytes.Length)
                                    fs.Write(responseBytes, 0, (int) (audioChunk.Length - audioChunk.Position));
                                else
                                    fs.Write(responseBytes, 0, responseBytes.Length);

                            }
                            // string responseText = await response.Content.ReadAsStringAsync();

                            return "success";
                        }
                    }
                }
            }
            
            return "failure";

            // return response.StatusCode.ToString();
        }
        static void Main() {
            Console.WriteLine(GenerateVoice("21m00Tcm4TlvDq8ikWAM").GetAwaiter().GetResult());
        }
    }
}