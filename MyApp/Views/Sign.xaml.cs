﻿using MyApp.Entities;
using MyApp.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MyApp.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Signup : Page
    {
        private Member currentMember;
        private static StorageFile file;
        private static string UploadUrl;
        public Signup()
        {
            this.currentMember = new Member();
            this.InitializeComponent();
        }
        private async void Capture_Photo(object sender, RoutedEventArgs e)
        {
            CameraCaptureUI captureUI = new CameraCaptureUI();
            captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            captureUI.PhotoSettings.CroppedSizeInPixels = new Size(200, 200);
            file = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);
            if (file == null)
            {
                // User cancelled photo capture
                return;
            }
            HttpUploadFile(UploadUrl, "myFile", "image/png");
        }

        private static async void GetUploadUrl()
        {
            Windows.Web.Http.HttpClient httpClient = new Windows.Web.Http.HttpClient();
            Uri requestUri = new Uri("https://2-dot-backup-server-002.appspot.com/get-upload-token");
            Windows.Web.Http.HttpResponseMessage httpResponse = new Windows.Web.Http.HttpResponseMessage();
            string httpResponseBody = "";
            try
            {
                httpResponse = await httpClient.GetAsync(requestUri);
                httpResponse.EnsureSuccessStatusCode();
                httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
            }
            Debug.WriteLine(httpResponseBody);
            UploadUrl = httpResponseBody;
        }

        public async void HttpUploadFile(string url, string paramName, string contentType)
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            Debug.WriteLine(url);
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";

            Stream rs = await wr.GetRequestStreamAsync();
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string header = string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n", paramName, "path_file", contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            // write file.
            Stream fileStream = await file.OpenStreamForReadAsync();
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);

            WebResponse wresp = null;
            try
            {
                wresp = await wr.GetResponseAsync();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                //Debug.WriteLine(string.Format("File uploaded, server response is: @{0}@", reader2.ReadToEnd()));
                //string imgUrl = reader2.ReadToEnd();
                Uri u = new Uri(reader2.ReadToEnd(), UriKind.Absolute);
                Debug.WriteLine(u.AbsoluteUri);
                ImageUrl.Text = u.AbsoluteUri;
                MyAvatar.Source = new BitmapImage(u);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error uploading file", ex.StackTrace);
                Debug.WriteLine("Error uploading file", ex.InnerException);
                if (wresp != null)
                {
                    wresp = null;
                }
            }
            finally
            {
                wr = null;
            }
        }

        private void Select_Gender(object sender, RoutedEventArgs e)
        {
            RadioButton radioGender = sender as RadioButton;
            this.currentMember.gender = Int32.Parse(radioGender.Tag.ToString());
            Debug.WriteLine(this.currentMember.gender);
        }


        private void Sign_In(object sender, TappedRoutedEventArgs e)
        {
            var rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(Views.LoginForm));
        }

        private async void Handle_Signup(object sender, TappedRoutedEventArgs e)
        {
            Dictionary<String, String> LoginInfor = new Dictionary<string, string>();
            LoginInfor.Add("email", this.Email.Text);
            LoginInfor.Add("password", this.Password.Password);
            LoginInfor.Add("Confirm_Password_Message", this.Confirm_Password.Password);
            LoginInfor.Add("firstName", this.FirstName.Text);
            LoginInfor.Add("lastName", this.LastName.Text);
            LoginInfor.Add("avatar", this.ImageUrl.Text);
            LoginInfor.Add("phone", this.Phone.Text);
            LoginInfor.Add("address", this.Address.Text);

            this.currentMember.email = Email.Text;
            this.currentMember.password = Password.Password;
            this.currentMember.introduction = Introduction.Text;
            this.currentMember.firstName = FirstName.Text;
            this.currentMember.lastName = LastName.Text;
            this.currentMember.avatar = ImageUrl.Text;
            this.currentMember.phone = Phone.Text;
            this.currentMember.address = Address.Text;

            var httpResponseMessage = ApiHandle.Sign_Up(this.currentMember);
            if (httpResponseMessage.Result.StatusCode == HttpStatusCode.Created)
            {
                Debug.WriteLine("Success a Duy");
            }
            else
            {
                var errorJson = await httpResponseMessage.Result.Content.ReadAsStringAsync();
                ErrorResponse errResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorJson);
                foreach (var errorField in errResponse.error.Keys)
                {
                    TextBlock textBlock = this.FindName(errorField) as TextBlock;
                    textBlock.Text = errResponse.error[errorField];
                }
            }

            //Debug.WriteLine(jsonResult);
            //var rs = JObject.Parse(jsonResult);
            //if ((int)rs["status"] != 201)
            //{
            //    ErrorResponse errResponse = JsonConvert.DeserializeObject<ErrorResponse>(jsonResult);
            //    Debug.WriteLine(errResponse);
            //}

        }

        private void Change_Birthday(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            this.currentMember.birthday = sender.Date.Value.ToString("yyyy-MM-dd");
        }
    }
}
