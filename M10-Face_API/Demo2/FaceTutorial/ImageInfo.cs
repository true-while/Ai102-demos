using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace FaceTutorial
{
    public class ImageInfo 
    {
        public ImageInfo(DetectedFace face)
        {
            detectedFace = face;
            Name = "Unknown";
            Confidence = "";
        }
        public string Confidence { get; set; }
        public string Name { get; set; }
        public DetectedFace detectedFace { get; set;}
    }
}