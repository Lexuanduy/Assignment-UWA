using MyApp.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.Service
{
    class ApiHandle
    {
        public static String API_REGISTER = "https://2-dot-backup-server-002.appspot.com/_api/v2/members";
        private static String API_LOGIN = "https://2-dot-backup-server-002.appspot.com/_api/v2/members/authentication";
        public static String API_CREATE_SONG = "http://2-dot-backup-server-002.appspot.com/_api/v2/songs";
        public static String API_MY_SONG = "http://2-dot-backup-server-002.appspot.com/_api/v2/songs/get-mine";

        public async static Task<HttpResponseMessage> Sign_Up(Member member)
        {
            HttpClient httpClient = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(member), System.Text.Encoding.UTF8, "application/json");
            var response = httpClient.PostAsync(API_REGISTER, content);
            return response.Result;
        }

        public async static Task<string> Create_Song(Song song)
        {
            HttpClient httpClient = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(song), System.Text.Encoding.UTF8, "application/json");
            var response = httpClient.PostAsync(API_CREATE_SONG, content);
            var contents = await response.Result.Content.ReadAsStringAsync();
            Debug.WriteLine(contents);
            return contents;
        }
    }
}

