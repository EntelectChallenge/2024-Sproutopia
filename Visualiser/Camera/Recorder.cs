namespace Visualiser.Camera
{
    public class Recorder
    {
        /*        private MediaOutput[] videos;

                public Recorder(List<Bot> bots, string videoFolder)
                {
                    var settings = new VideoEncoderSettings(width: 1920, height: 1080, framerate: 30, codec: VideoCodec.H264);
                    FFmpegLoader.FFmpegPath = Path.GetFullPath("./ffmpeg");
                    settings.EncoderPreset = EncoderPreset.Fast;
                    settings.CRF = 17;

                    Directory.CreateDirectory(Path.GetFullPath(videoFolder));

                    videos = bots.Select(bot =>
                    {
                        return MediaBuilder.CreateContainer($"{Path.GetFullPath(Path.Combine(videoFolder, bot.NickName))}.mp4").WithVideo(settings).Create();
                    }).ToArray();
                }

                public void RecordFrame(CameraWindow cameraWindow)
                {
                    var renderTarget = cameraWindow.RenderTarget;
                    byte[] image = new byte[renderTarget.Width * renderTarget.Height * 4];
                    using MemoryStream stream = new();
                    renderTarget.GetData(image);
                    videos[cameraWindow.BotIndex].Video.AddFrame(ImageData.FromArray(image, ImagePixelFormat.Bgra32, renderTarget.Width, renderTarget.Height));
                }

                public void Dispose()
                {
                    videos.ToList().ForEach(video => video.Dispose());
                }*/
    }
}
