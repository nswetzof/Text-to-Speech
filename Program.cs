﻿using System;
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
using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;

namespace HttpClientSample {
    public class Program {
        private static string API_KEY = "0591d8c076c8faeb2b9acfd16aadfffd";
        static readonly HttpClient client = new HttpClient();

        static async Task<string?> GetVoiceAsync(string voiceName) {
            client.BaseAddress = new Uri("http://api.elevenlabs.io/v1");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("s-api-key", Program.API_KEY);

            using (HttpResponseMessage response = await client.GetAsync(new Uri($"{client.BaseAddress}/voices"))) {
                if(response.IsSuccessStatusCode) {
                    string? voiceString = JsonConvert.SerializeObject(await response.Content.ReadAsStringAsync());
                    
                    JObject responseJson = JObject.Parse(voiceString);

                    var id = 
                        from item in responseJson["voices"]
                        where (string) (item["name"]) == voiceName
                        select (string) item["voice_id"];

                    return id.First();
                }
                else
                    return "failure: " + response.ReasonPhrase;
            }
        }

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
                    using (FileStream fs = File.Create("test.mp3")) {
                        byte[] byteStream = await response.Content.ReadAsByteArrayAsync();

                        fs.Write(byteStream, 0, (int) byteStream.Length);

                        return "success";
                    }
                }
            }
            
            return "failure";

            // return response.StatusCode.ToString();
        }

        private static async Task UploadVoice(string file) {
            string url = "https://api.elevenlabs.io/v1/voices/add";
            client.BaseAddress = new Uri(url);

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));
            client.DefaultRequestHeaders.Add("x-api-key", Program.API_KEY);
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            // create form data handler
            var content = new MultipartFormDataContent
            {
                { new StringContent("Obama"), "name" },
                { new StringContent("Obama speaking in a normal tone"), "description" }
            };
            // content.Add(new StringContent("{\"accent\":\"American\"}"));
            // var dataContent = new FormUrlEncodedContent(new[] {
            //     new KeyValuePair<string, string>("accent", "American")
            // });
            // content.Add(dataContent);
            // byte[] bytes = await File.ReadAllBytesAsync(file);
            content.Add(new ByteArrayContent(await File.ReadAllBytesAsync(file)), "audio/mpeg", "sample1.mp3");

            using(HttpResponseMessage response = await client.PostAsync(url, content)) {
                if(response.IsSuccessStatusCode) {
                    Console.WriteLine("Success!");
                }
                else
                    Console.WriteLine(response.ReasonPhrase);
            }

        }
        static void Main() {
            // Console.WriteLine(GenerateVoice("21m00Tcm4TlvDq8ikWAM").GetAwaiter().GetResult());
            Console.WriteLine(GetVoiceAsync("Emma Watson").GetAwaiter().GetResult());
            // UploadVoice("sample1.mp3").GetAwaiter().GetResult();
        }
    }
}