using MediaToolkit;
using MediaToolkit.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;
using System.Threading.Tasks;

namespace YoutubeToMp3Downloader.Controllers
{
    public class HomeController : Controller
    {
        private IDbContext context;

        public HomeController()
        {
            this.context = new DbContext();
        }

        private string GetPath(string fileName)
        {
            var _root = Server.MapPath(@"~/App_Data");
            var _fullPath = Path.Combine(_root, fileName);

            return _fullPath;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> CreateDownloadUrl(YoutubeReqeust request)
        {
            var _youtubeClient = new YoutubeClient();
            var _videoInfo = await _youtubeClient.GetVideoAsync(request.VideoID);
            var _streamInfo = _videoInfo.MuxedStreamInfos.OrderByDescending(x => x.VideoQuality).FirstOrDefault();

            var _size = _streamInfo.Size;
            var _sizeMb = (_size / 1024 / 1024);

            if (_sizeMb > 10)
                throw new ArgumentOutOfRangeException("下載檔案超過 10MB");

            if (_streamInfo == null)
                throw new ArgumentNullException(nameof(_streamInfo));

            var _fileExtension = _streamInfo.Container.GetFileExtension();

            //下載的影片檔案名稱與儲存路徑
            var _videoFileName = $"{_videoInfo.Id}.{_fileExtension}";
            var _videoFullPath = this.GetPath(_videoFileName);

            //轉換後的 MP3 檔案名稱與儲存路徑
            var _mp3FileName = $"{_videoInfo.Id}.mp3";
            var _mp3FullPath = this.GetPath(_mp3FileName);

            //下載 Youtube 影片
            await _youtubeClient.DownloadMediaStreamAsync(_streamInfo, _videoFullPath);

            //進行 MP3 格式檔案轉換
            var _inputMediaFile = new MediaFile() { Filename = _videoFullPath };
            var _outMediaFile = new MediaFile() { Filename = _mp3FullPath };

            using (var _engine = new Engine())
            {
                _engine.GetMetadata(_inputMediaFile);
                _engine.Convert(_inputMediaFile, _outMediaFile);
            }

            //影片轉換成 MP3 完成後刪除影片檔
            System.IO.File.Delete(_videoFullPath);

            return Json(_videoInfo.Id);
        }
        
        [HttpGet]
        public async Task<FileResult> DownloadMp3File(string id)
        {
            return await Task.Run(() => {
                var _mp3FileName = $"{id}.mp3";
                var _mp3FullPath = this.GetPath(_mp3FileName);
                var _disposition = String.Format("{0}; filename=\"{1}\"", "attachment", "demo.mp3");

                Response.AddHeader("Content-Disposition", _disposition);

                return File(_mp3FullPath, "audio/mp3");
            });
        }
        
    }
}