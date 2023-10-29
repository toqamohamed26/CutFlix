using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CUTFLI.Models;
using CUTFLI.ViewModels;
using NToastNotify;
using AutoMapper;
using System.Security.Claims;

namespace CUTFLI.Controllers
{
    public class VideosController : Controller
    {
        private List<string> allowedExtensions = new List<string>() { ".mp4", ".mkv", ".flv", ".mov" };
        private readonly CUTFLIDbContext _context;
        private readonly IToastNotification _toastNotification;
        private readonly IMapper _mapper;
        private readonly ILogger<VideosController> _logger;

        public VideosController(CUTFLIDbContext context,
            IToastNotification toastNotification,
            IMapper mapper,
            ILogger<VideosController> logger)
        {
            _context = context;
            _toastNotification = toastNotification;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var videos = await _context.Videos.ToListAsync();
                var model = _mapper.Map<List<Video>, List<VideoViewModel>>(videos);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "VideosController-Index exception :");
                _toastNotification.AddErrorToastMessage("Error Found");
                return View(new List<VideoViewModel>());
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VideoViewModel videoModel)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    var videoToDelete = await _context.Videos.FirstOrDefaultAsync();

                    string nameToUpload = string.Empty;
                    var files = Request.Form.Files;
                    if (files.Any())
                    {
                        var videoUploaded = files.FirstOrDefault();
                        nameToUpload = videoUploaded.FileName;

                        var file = Path.Combine("wwwroot", "Videos\\", videoUploaded.FileName);

                        using (var stream = new FileStream(file, FileMode.Create))
                        {
                            await videoUploaded.CopyToAsync(stream);
                        }

                    }
                    else
                    {
                        ModelState.AddModelError("Video", "Video required");
                        return View(videoModel);
                    }

                    var video = new Video()
                    {
                        VideoName = nameToUpload,
                        CreatedDate = DateTime.Now
                    };
                    await _context.AddAsync(video);
                    await _context.SaveChangesAsync();

                    if (videoToDelete != null)
                    {
                        _context.Remove(videoToDelete);
                    }
                    await _context.SaveChangesAsync();
                    _toastNotification.AddSuccessToastMessage("Video added successfully");

                    return RedirectToAction(nameof(Index));
                }
                return View(videoModel);
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                _logger.LogError(ex, "VideosController-Create exception :");
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Videos == null)
            {
                return NotFound();
            }

            var video = await _context.Videos.FindAsync(id);
            if (video == null)
            {
                return NotFound();
            }
            var videoModel = _mapper.Map<VideoViewModel>(video);
            return View(videoModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                if (_context.Videos == null)
                {
                    return Problem("Entity set 'CUTFLIDbContext.Videos'  is null.");
                }
                var video = await _context.Videos.FindAsync(id);
                if (video != null)
                {
                    _context.Videos.Remove(video);
                    await _context.SaveChangesAsync();

                    var file = Path.Combine("wwwroot", "Videos\\", video.VideoName);

                    if (System.IO.File.Exists(file))
                    {
                        System.IO.File.Delete(file);
                    }
                    _toastNotification.AddSuccessToastMessage("Video deleted successfully");
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                _logger.LogError(ex, "VideosController-DeleteConfirmed exception :");
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
