// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Cognitive Services: http://www.microsoft.com/cognitive
// 
// Microsoft Cognitive Services Github:
// https://github.com/Microsoft/Cognitive
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 

using System;
using VideoFrameAnalyzer;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;

namespace BasicConsoleSample
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Create grabber. 
            FrameGrabber<Face[]> grabber = new FrameGrabber<Face[]>();

            // Create Face API Client.
            FaceServiceClient faceClient = new FaceServiceClient("51d6cd88bd3b4e809c6b0ddb0df6c672");

            // Set up a listener for when we acquire a new frame.
            grabber.NewFrameProvided += (s, e) =>
            {
                Console.WriteLine("New frame acquired at {0}", e.Frame.Metadata.Timestamp);
            };

            // Set up Face API call.
            grabber.AnalysisFunction = async frame =>
            {
                Console.WriteLine("Submitting frame acquired at {0}", frame.Metadata.Timestamp);
                // Encode image and submit to Face API. 
                Bitmap b = new Bitmap(frame.Image.ToMemoryStream(".jpg"));

                b.Save(@"C:\newImage.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                b.Dispose();

                //frame.Image.ToMemoryStream(".jpg");
                return await faceClient.DetectAsync(frame.Image.ToMemoryStream(".jpg"));
            };

            // Set up a listener for when we receive a new result from an API call. 
            grabber.NewResultAvailable += (s, e) =>
            {

                if (e.TimedOut)
                    Console.WriteLine("API call timed out.");
                else if (e.Exception != null)
                    Console.WriteLine("API call threw an exception.");
                else
                {
                    Console.WriteLine("New result received for frame acquired at {0}. {1} faces detected", e.Frame.Metadata.Timestamp, e.Analysis.Length);
                    int height1 = e.Analysis[0].FaceRectangle.Height;
                    int width1 = e.Analysis[0].FaceRectangle.Width;
                    int top = e.Analysis[0].FaceRectangle.Top;
                    int left = e.Analysis[0].FaceRectangle.Left;
                    Rectangle cropRect = new Rectangle(top, left, width1, height1);
                    Bitmap src = Image.FromFile(@"C:\newImage.jpg") as Bitmap;
                    Bitmap bmpImage = new Bitmap(src);
                    bmpImage = bmpImage.Clone(cropRect, bmpImage.PixelFormat);

                    using (Bitmap newBitmap = new Bitmap(bmpImage))
                    {
                        // newBitmap.SetResolution(128, 128);

                        float width = 128;
                        float height = 128;
                        var image = new Bitmap(newBitmap);
                        float scale = Math.Min(width / image.Width, height / image.Height);
                        var bmp = new Bitmap((int)width, (int)height);
                        var graph = Graphics.FromImage(bmp);

                        // uncomment for higher quality output
                        //graph.InterpolationMode = InterpolationMode.High;
                        //graph.CompositingQuality = CompositingQuality.HighQuality;
                        //graph.SmoothingMode = SmoothingMode.AntiAlias;

                        var scaleWidth = (int)(image.Width * scale);
                        var scaleHeight = (int)(image.Height * scale);

                        graph.DrawImage(image, new Rectangle(((int)width - scaleWidth) / 2, ((int)height - scaleHeight) / 2, scaleWidth, scaleHeight));
                        bmp.Save(@"C:\newImage1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                        Rectangle rect = new Rectangle(0, 0, 128, 128);

                        Bitmap greyScale = ColorToGrayscale(bmp);

                        System.Drawing.Imaging.BitmapData bmpData =
                          greyScale.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, greyScale.PixelFormat);


                        IntPtr ptr = bmpData.Scan0;
                        int bytes1 = Math.Abs(bmpData.Stride) * greyScale.Height;
                        byte[] rgbValues = new byte[bytes1];


                        // Copy the RGB values into the array.
                        System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes1);
                        int[] pixels = new int[bytes1 + 1];
                        pixels[0] = 8;
                        int i = 1;
                        foreach (byte b in rgbValues)
                        {
                            pixels[i++] = (int)(b);
                        }
                        Console.WriteLine("inted pixels");
                        InvokeRequestResponseService(pixels).Wait();
                        image.Dispose();
                        //source.Dispose();
                        bmp.Dispose();
                        //newBitmap.Save(@"C:\newImage1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                    }

                    


                    bmpImage.Dispose();
                    src.Dispose();
                }

            };

            // Tell grabber when to call API.
            // See also TriggerAnalysisOnPredicate
            grabber.TriggerAnalysisOnInterval(TimeSpan.FromMilliseconds(3000));

            // Start running in the background.
            grabber.StartProcessingCameraAsync().Wait();

            // Wait for keypress to stop
            Console.WriteLine("Press any key to stop...");
            Console.ReadKey();

            // Stop, blocking until done.
            grabber.StopProcessingAsync().Wait();
        }

         
    static async Task InvokeRequestResponseService(int[] pixels)
        {
            using (var client = new HttpClient())
            {
            var scoreRequest = new
            {
                Inputs = new Dictionary<string, List<Dictionary<string, string>>>() {
                        {
                            "input1",
                            new List<Dictionary<string, string>>(){new Dictionary<string, string>(){
                                            {
                                                "Col1", "1"
                                            },


                                            {
                                                "Col16385", "1"
                                            },
                                }
                            }
                        },
                    },
                GlobalParameters = new Dictionary<string, string>()
                {
                }
            };
                var list = scoreRequest.Inputs["input1"];
                list.Clear();
                var y = new Dictionary<string, string>();
                for (int i = 0; i < pixels.Length; i++)
                {
                    
                    y.Add("Col" + (i + 1).ToString(), pixels[i].ToString());
                   
                }
                list.Add(y);


                const string apiKey = "CHTCnPyf15/wXvAk1XZke0E9NIljAskLUPF0xhAanR8viGVtKlUrsdmhO5mY7cQmO2mVZpxq+Mk6rFYMBghmsg=="; // Replace this with the API key for the web service
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.BaseAddress = new Uri("https://ussouthcentral.services.azureml.net/workspaces/d0469c21ee7a4c12b33025e552126623/services/9576ed621e9e4b7db9fdec58debc3418/execute?api-version=2.0&format=swagger");

                // WARNING: The 'await' statement below can result in a deadlock
                // if you are calling this code from the UI thread of an ASP.Net application.
                // One way to address this would be to call ConfigureAwait(false)
                // so that the execution does not attempt to resume on the original context.
                // For instance, replace code such as:
                //      result = await DoSomeTask()
                // with the following:
                //      result = await DoSomeTask().ConfigureAwait(false)
                string json = JsonConvert.SerializeObject(scoreRequest);
                HttpResponseMessage response = await client.PostAsync("", new StringContent(json, Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Result: {0}", result);
                }
                else
                {
                    Console.WriteLine(string.Format("The request failed with status code: {0}", response.StatusCode));

                    // Print the headers - they include the requert ID and the timestamp,
                    // which are useful for debugging the failure
                    Console.WriteLine(response.Headers.ToString());

                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(json);
                    Console.WriteLine(responseContent);
                }
            }
        }

        public static Bitmap ColorToGrayscale(Bitmap bmp)
        {
            int w = bmp.Width,
                h = bmp.Height,
                r, ic, oc, bmpStride, outputStride, bytesPerPixel;
            PixelFormat pfIn = bmp.PixelFormat;
            ColorPalette palette;
            Bitmap output;
            BitmapData bmpData, outputData;

            //Create the new bitmap
            output = new Bitmap(w, h, PixelFormat.Format8bppIndexed);

            //Build a grayscale color Palette
            palette = output.Palette;
            for (int i = 0; i < 256; i++)
            {
                Color tmp = Color.FromArgb(255, i, i, i);
                palette.Entries[i] = Color.FromArgb(255, i, i, i);
            }
            output.Palette = palette;

            //No need to convert formats if already in 8 bit
            if (pfIn == PixelFormat.Format8bppIndexed)
            {
                output = (Bitmap)bmp.Clone();

                //Make sure the palette is a grayscale palette and not some other
                //8-bit indexed palette
                output.Palette = palette;

                return output;
            }

            //Get the number of bytes per pixel
            switch (pfIn)
            {
                case PixelFormat.Format24bppRgb: bytesPerPixel = 3; break;
                case PixelFormat.Format32bppArgb: bytesPerPixel = 4; break;
                case PixelFormat.Format32bppRgb: bytesPerPixel = 4; break;
                default: throw new InvalidOperationException("Image format not supported");
            }

            //Lock the images
            bmpData = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly,
                                   pfIn);
            outputData = output.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly,
                                         PixelFormat.Format8bppIndexed);
            bmpStride = bmpData.Stride;
            outputStride = outputData.Stride;

            //Traverse each pixel of the image
            unsafe
            {
                byte* bmpPtr = (byte*)bmpData.Scan0.ToPointer(),
                outputPtr = (byte*)outputData.Scan0.ToPointer();

                if (bytesPerPixel == 3)
                {
                    //Convert the pixel to it's luminance using the formula:
                    // L = .299*R + .587*G + .114*B
                    //Note that ic is the input column and oc is the output column
                    for (r = 0; r < h; r++)
                        for (ic = oc = 0; oc < w; ic += 3, ++oc)
                            outputPtr[r * outputStride + oc] = (byte)(int)
                                (0.299f * bmpPtr[r * bmpStride + ic] +
                                 0.587f * bmpPtr[r * bmpStride + ic + 1] +
                                 0.114f * bmpPtr[r * bmpStride + ic + 2]);
                }
                else //bytesPerPixel == 4
                {
                    //Convert the pixel to it's luminance using the formula:
                    // L = alpha * (.299*R + .587*G + .114*B)
                    //Note that ic is the input column and oc is the output column
                    for (r = 0; r < h; r++)
                        for (ic = oc = 0; oc < w; ic += 4, ++oc)
                            outputPtr[r * outputStride + oc] = (byte)(int)
                                ((bmpPtr[r * bmpStride + ic] / 255.0f) *
                                (0.299f * bmpPtr[r * bmpStride + ic + 1] +
                                 0.587f * bmpPtr[r * bmpStride + ic + 2] +
                                 0.114f * bmpPtr[r * bmpStride + ic + 3]));
                }
            }

            //Unlock the images
            bmp.UnlockBits(bmpData);
            output.UnlockBits(outputData);

            return output;
        }


    }

}
