/// original source project https://github.com/Azure-Samples/Cognitive-Face-CSharp-sample


using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.Configuration;
using FaceTutorialCS;
using System.Linq;

namespace FaceTutorial
{
    
    public partial class MainWindow : Window
    {
        // Add your Face subscription key to your environment variables.
        private static string subscriptionKey;
        // Add your Face endpoint to your environment variables.
        private static string faceEndpoint;
        private List<Person> persons = new List<Person>();
        private IFaceClient faceClient;
        private string personGroupId = "g7";
        private bool isGroupCreated = false;
        // The list of detected faces.
        private IList<ImageInfo> faceList;
        // The list of descriptions for the detected faces.
        private string[] faceDescriptions;
        // The resize factor for the displayed image.
        private double resizeFactor;

        private const string defaultStatusBarText =
            "Place the mouse pointer over a face to see the face description.";
        // </snippet_mainwindow_fields>

        // <snippet_mainwindow_constructor>
        public MainWindow()
        {
            Config();

            InitializeComponent();

            if (Uri.IsWellFormedUriString(faceEndpoint, UriKind.Absolute))
            {
                faceClient.Endpoint = faceEndpoint;
            }
            else
            {
                MessageBox.Show(faceEndpoint,
                    "Invalid URI", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }
        }
        // </snippet_mainwindow_constructor>

       
        // <snippet_browsebuttonclick_start>
        // Displays the image and calls UploadAndDetectFaces.
        private async void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the image file to scan from the user.
            var openDlg = new Microsoft.Win32.OpenFileDialog();

            openDlg.Filter = "JPEG Image(*.jpg)|*.jpg";
            bool? result = openDlg.ShowDialog(this);

            // Return if canceled.
            if (!(bool)result)
            {
                return;
            }

            // Display the image file.
            string filePath = openDlg.FileName;

            Uri fileUri = new Uri(filePath);
            BitmapImage bitmapSource = new BitmapImage();

            bitmapSource.BeginInit();
            bitmapSource.CacheOption = BitmapCacheOption.None;
            bitmapSource.UriSource = fileUri;
            bitmapSource.EndInit();

            FacePhoto.Source = bitmapSource;
            // </snippet_browsebuttonclick_start>

            // <snippet_browsebuttonclick_mid>
            // Detect any faces in the image.
            Title = "Detecting...";
            faceList = await UploadAndDetectFaces(filePath);
            Title = String.Format(
                "Detection Finished. {0} face(s) detected", faceList.Count);

            if (faceList.Count > 0)
            {
                // Prepare to draw rectangles around the faces.
                DrawingVisual visual = new DrawingVisual();
                DrawingContext drawingContext = visual.RenderOpen();
                drawingContext.DrawImage(bitmapSource,
                    new Rect(0, 0, bitmapSource.Width, bitmapSource.Height));
                double dpi = bitmapSource.DpiX;
                // Some images don't contain dpi info.
                resizeFactor = (dpi == 0) ? 1 : 96 / dpi;
                faceDescriptions = new String[faceList.Count];

                for (int i = 0; i < faceList.Count; ++i)
                {
                    DetectedFace face = faceList[i].detectedFace;

                    // Draw a rectangle on the face.
                    drawingContext.DrawRectangle(
                        Brushes.Transparent,
                        new Pen(Brushes.Red, 7),
                        new Rect(
                            face.FaceRectangle.Left * resizeFactor,
                            face.FaceRectangle.Top * resizeFactor,
                            face.FaceRectangle.Width * resizeFactor,
                            face.FaceRectangle.Height * resizeFactor
                            )
                    );

                    // Store the face description.
                    faceDescriptions[i] = FaceDescription(faceList[i]);
                }

                drawingContext.Close();

                // Display the image with the rectangle around the face.
                RenderTargetBitmap faceWithRectBitmap = new RenderTargetBitmap(
                    (int)(bitmapSource.PixelWidth * resizeFactor),
                    (int)(bitmapSource.PixelHeight * resizeFactor),
                    96,
                    96,
                    PixelFormats.Pbgra32);

                faceWithRectBitmap.Render(visual);
                FacePhoto.Source = faceWithRectBitmap;

                // Set the status bar text.
                faceDescriptionStatusBar.Text = defaultStatusBarText;
            }
        }

        private async Task<bool> IfGroupCreated()
        {
            IList<PersonGroup> existedgtoup = await faceClient.PersonGroup.ListAsync();

            return existedgtoup.Any(p => p.PersonGroupId == personGroupId);
        }

        public async Task<string> BuildLibraryAsync(Action<string> outptu)
        {
            outptu.Invoke("Building image dictionary");

            Dictionary<string, string[]> personDictionary = new Dictionary<string, string[]>();

            var faceLib = Path.Combine(Environment.CurrentDirectory, "FaceLib");
            string[] folders = Directory.GetDirectories(faceLib);

            int filesCount = 0;
            foreach (var folder in folders)
            {
                string[] files = Directory.GetFiles(Path.Combine(faceLib, folder), "*.jpg");
                filesCount += files.Length;
                personDictionary.Add(Path.GetFileName(folder), files);
            }

            outptu.Invoke($"Found {filesCount} files for {folders.Length} faces. Building a group..");


            if (await IfGroupCreated()) await faceClient.PersonGroup.DeleteAsync(personGroupId);
                           
                await faceClient.PersonGroup.CreateAsync(personGroupId, RecognitionModel.Recognition01);

            // The similar faces will be grouped into a single person group person.
            foreach (var groupedFace in personDictionary.Keys)
            {
                // Limit TPS
                await Task.Delay(250);
                Person person = await faceClient.PersonGroupPerson.CreateAsync(personGroupId: personGroupId, name: groupedFace);
                person.Name = groupedFace;
                outptu.Invoke(String.Format($"Create a person group person '{groupedFace}'."));

                int faceNum = 0;

                // Add face to the person group person.
                foreach (var similarImage in personDictionary[groupedFace])
                {
                    outptu.Invoke($"Add {faceNum++} face to person '{Path.GetFileName(groupedFace)}'");
                    using (FileStream img = File.OpenRead(similarImage))
                    {
                        PersistedFace face = await faceClient.PersonGroupPerson.AddFaceFromStreamAsync(personGroupId, person.PersonId, img);
                    }
                }
                persons.Add(person);
            }            
            
            isGroupCreated = true;

            outptu.Invoke($"Train the person group.....");

            await faceClient.PersonGroup.TrainAsync(personGroupId);

            // Wait until the training is completed.
            while (true)
            {
                await Task.Delay(1000);
                var trainingStatus = await faceClient.PersonGroup.GetTrainingStatusAsync(personGroupId);
                Console.WriteLine($"Training status: {trainingStatus.Status}.");
                if (trainingStatus.Status == TrainingStatusType.Succeeded) { break; }
                if (trainingStatus.Status == TrainingStatusType.Failed) { throw new Exception("Person group training fail. Please restart the app."); }
            }

            outptu.Invoke($"Train completed....");

            return "completed";
        }

        // <snippet_mousemove_start>
        // Displays the face description when the mouse is over a face rectangle.
        private void FacePhoto_MouseMove(object sender, MouseEventArgs e)
        {
            // </snippet_mousemove_start>

            // <snippet_mousemove_mid>
            // If the REST call has not completed, return.
            if (faceList == null)
                return;

            // Find the mouse position relative to the image.
            Point mouseXY = e.GetPosition(FacePhoto);

            ImageSource imageSource = FacePhoto.Source;
            BitmapSource bitmapSource = (BitmapSource)imageSource;

            // Scale adjustment between the actual size and displayed size.
            var scale = FacePhoto.ActualWidth / (bitmapSource.PixelWidth / resizeFactor);

            // Check if this mouse position is over a face rectangle.
            bool mouseOverFace = false;

            for (int i = 0; i < faceList.Count; ++i)
            {
                FaceRectangle fr = faceList[i].detectedFace.FaceRectangle;
                double left = fr.Left * scale;
                double top = fr.Top * scale;
                double width = fr.Width * scale;
                double height = fr.Height * scale;
               

                // Display the face description if the mouse is over this face rectangle.
                if (mouseXY.X >= left && mouseXY.X <= left + width &&
                    mouseXY.Y >= top && mouseXY.Y <= top + height)
                {
                    faceDescriptionStatusBar.Text = faceDescriptions[i];
                    FacePhoto.ToolTip = faceDescriptions[i].Replace(";","\r\n");
                    mouseOverFace = true;
                    break;
                }
            }

            // String to display when the mouse is not over a face rectangle.
            if (!mouseOverFace)
            {
                faceDescriptionStatusBar.Text = defaultStatusBarText;
                FacePhoto.ToolTip = null; // "not a face";
            }

 
        }
  
        private async Task<IList<ImageInfo>> UploadAndDetectFaces(string imageFilePath)
        {
            // The list of Face attributes to return.
            IList<FaceAttributeType?> faceAttributes =
                new FaceAttributeType?[]
                {
                    FaceAttributeType.Gender, FaceAttributeType.Age,
                    FaceAttributeType.Smile, FaceAttributeType.Emotion,
                    FaceAttributeType.Glasses, FaceAttributeType.Hair
                };

            // Call the Face API.
            try
            {

                using (Stream imageFileStream = File.OpenRead(imageFilePath))
                {
                    // The second argument specifies to return the faceId, while
                    // the third argument specifies not to return face landmarks.
                    IList<DetectedFace> faceList = 
                        await faceClient.Face.DetectWithStreamAsync(
                            imageFileStream, true, false, faceAttributes);

                    List<ImageInfo> faces = new List<ImageInfo>();

                    if (isGroupCreated)
                    {
                        foreach (DetectedFace face in faceList)
                        {
                            ImageInfo info = new ImageInfo(face);
                            if (isGroupCreated)
                                foreach (Person searchedForPerson in persons)
                                {
                                    var result = await MatchFaceAsync((Guid)face.FaceId, searchedForPerson);
                                    if (result.Item1)
                                    {
                                        info.Name = searchedForPerson.Name;
                                        info.Confidence = result.Item2;
                                        faces.Add(info);
                                    }
                                }
                        }
                    }else
                    {
                        foreach (DetectedFace face in faceList)
                            faces.Add(new ImageInfo(face));
                    }

                    return faces;
                }
            }
            // Catch and display Face API errors.
            catch (APIErrorException f)
            {
                MessageBox.Show(f.Message);
                return new List<ImageInfo>();
            }
            // Catch and display all other errors.
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error");
                return new List<ImageInfo>();
            }
        }

        private async Task<Tuple<bool,string>> MatchFaceAsync(Guid faceId, Person searchedForPerson)
        {
            string confidence = "";
            if ((faceId == Guid.Empty) || (searchedForPerson?.PersonId == null)) { return new Tuple<bool, string>(false, confidence); }

            VerifyResult results;
            try
            {
                results = await faceClient.Face.VerifyFaceToPersonAsync(
                    faceId, searchedForPerson.PersonId, personGroupId);
                confidence = results.Confidence.ToString("P");

            }
            catch (APIErrorException ae)
            {
                Console.WriteLine("MatchFaceAsync: " + ae.Message);
                return new Tuple<bool, string>(false, confidence);
            }

            return new Tuple<bool, string>(results.IsIdentical, confidence);
        }

        // Creates a string out of the attributes describing the face.
        private string FaceDescription(ImageInfo face)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Face: ");
            if (face.Confidence != "")
                sb.Append($"{face.Name} { face.Confidence} ;");

            // Add the gender, age, and smile.
            sb.Append(face.detectedFace.FaceAttributes.Gender);
            sb.Append(", ");
            sb.Append(face.detectedFace.FaceAttributes.Age);
            sb.Append("; ");
            sb.Append(String.Format("smile {0:F1}%, ", face.detectedFace.FaceAttributes.Smile * 100));

            // Add the emotions. Display all emotions over 10%.
            sb.Append("Emotion: ");
            Emotion emotionScores = face.detectedFace.FaceAttributes.Emotion;
            if (emotionScores.Anger >= 0.1f) sb.Append(
                String.Format("anger {0:F1}%, ", emotionScores.Anger * 100));
            if (emotionScores.Contempt >= 0.1f) sb.Append(
                String.Format("contempt {0:F1}%, ", emotionScores.Contempt * 100));
            if (emotionScores.Disgust >= 0.1f) sb.Append(
                String.Format("disgust {0:F1}%, ", emotionScores.Disgust * 100));
            if (emotionScores.Fear >= 0.1f) sb.Append(
                String.Format("fear {0:F1}%, ", emotionScores.Fear * 100));
            if (emotionScores.Happiness >= 0.1f) sb.Append(
                String.Format("happiness {0:F1}%, ", emotionScores.Happiness * 100));
            if (emotionScores.Neutral >= 0.1f) sb.Append(
                String.Format("neutral {0:F1}%, ", emotionScores.Neutral * 100));
            if (emotionScores.Sadness >= 0.1f) sb.Append(
                String.Format("sadness {0:F1}%, ", emotionScores.Sadness * 100));
            if (emotionScores.Surprise >= 0.1f) sb.Append(
                String.Format("surprise {0:F1}%, ", emotionScores.Surprise * 100));

            // Add glasses.
            sb.Append(face.detectedFace.FaceAttributes.Glasses);
            sb.Append("; ");

            // Add hair.
            sb.Append("Hair: ");

            // Display baldness confidence if over 1%.
            if (face.detectedFace.FaceAttributes.Hair.Bald >= 0.01f)
                sb.Append(String.Format("bald {0:F1}% ", face.detectedFace.FaceAttributes.Hair.Bald * 100));

            // Display all hair color attributes over 10%.
            IList<HairColor> hairColors = face.detectedFace.FaceAttributes.Hair.HairColor;
            foreach (HairColor hairColor in hairColors)
            {
                if (hairColor.Confidence >= 0.1f)
                {
                    sb.Append(hairColor.Color.ToString());
                    sb.Append(String.Format(" {0:F1}% ", hairColor.Confidence * 100));
                }
            }

            // Return the built string.
            return sb.ToString();
        }
        // </snippet_facedesc>


        public void Config()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();

            subscriptionKey = configuration.GetSection("subscriptionKey").Value;
            faceEndpoint = configuration.GetSection("faceEndpoint").Value;

            faceClient = new FaceClient(
                    new ApiKeyServiceClientCredentials(subscriptionKey),
                    new System.Net.Http.DelegatingHandler[] { });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (isGroupCreated) faceClient.PersonGroup.DeleteAsync(personGroupId).Wait();
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Error");
            }
        }

        private async void LoadFaces_Click(object sender, RoutedEventArgs e)
        {
            Progress bar = new Progress();
            try
            {
                bar.Show();
                await BuildLibraryAsync((msg) => bar.AddOutput(msg)).ContinueWith((context)=>Dispatcher.Invoke(new Action(() => bar.Close())));
            }
            catch (Exception ex)
            {
                bar.Close();
                MessageBox.Show(ex.Message.ToString(), "Error");
            }
                            

        }
    }
}
