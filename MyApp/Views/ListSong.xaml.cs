using MyApp.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MyApp.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ListSong : Page
    {
        private ObservableCollection<Song> listSong;

        internal ObservableCollection<Song> ListSongs { get => listSong; set => listSong = value; }
        public static String SONG_API = "http://2-dot-backup-server-002.appspot.com/_api/v2/songs/get-mine";
        public ListSong()
        {
            this.ListSongs = new ObservableCollection<Song>();
            this.InitializeComponent();

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", "Basic iBi1orSXvZisyF48jdnnOYmZURdkyz3rv1Jg0KBhFLsJOa90Tlu6DxxBKPYjm4b6");
            HttpResponseMessage responseMessage = httpClient.GetAsync(SONG_API).Result;
            string content = responseMessage.Content.ReadAsStringAsync().Result;
            Debug.WriteLine(content);
            if (responseMessage.StatusCode == HttpStatusCode.OK)
            {
                ObservableCollection<Song> songResponse = JsonConvert.DeserializeObject<ObservableCollection<Song>>(content);
                foreach (var song in songResponse)
                {
                    this.ListSongs.Add(song);
                }
                Debug.WriteLine("Oke, đã tạo thành công.");
            }
            else
            {
                ErrorResponse errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(content);
                foreach (var key in errorResponse.error.Keys)
                {
                    //if (this.FindName is TextBlock textBlock)
                    //{
                    //    textBlock.Text = errorResponse.error[key];
                    //}
                }
            }
        }
    }
}
