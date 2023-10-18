using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CUTFLI.Models;
using CUTFLI.ViewModels;
using AutoMapper;
using System.Security.Claims;
using NToastNotify;
using Microsoft.AspNetCore.Authorization;

namespace CUTFLI.Controllers
{
    [Authorize]
    public class ServicesController : Controller
    {
        private List<string> allowedExtensions = new List<string>() { ".png", ".jpg" };
        private readonly CUTFLIDbContext _context;
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;

        public ServicesController(CUTFLIDbContext context, IMapper mapper, IToastNotification toastNotification)
        {
            _context = context;
            _mapper = mapper;
            _toastNotification = toastNotification;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var services = await _context.Services.OrderByDescending(x => x.CreatedDate).ToListAsync();
                var model = _mapper.Map<List<Service>, List<ServiceViewModel>>(services);
                return View(model);
            }
            catch (Exception)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    int? currentUserId = Convert.ToInt32(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var files = Request.Form.Files;
                    using var stream = new MemoryStream();
                    if (files.Any())
                    {
                        var image = files.FirstOrDefault();
                        if (allowedExtensions.Contains(Path.GetExtension(image.FileName).ToLower()))
                        {
                            if (image.Length < 3145728) // 3 MB
                            {
                                await image.CopyToAsync(stream);
                            }
                            else
                            {
                                ModelState.AddModelError("Image", "Image can't be larger than 3 MB");
                                return View(model);
                            }
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("Image", "Image required");
                        return View(model);
                    }
                    var service = _mapper.Map<ServiceViewModel, Service>(model);
                    service.CreatedDate = DateTime.Now;
                    service.CreatedBy = currentUserId;
                    service.Image = files.Any() ? stream.ToArray() : null;
                    await _context.AddAsync(service);
                    await _context.SaveChangesAsync();
                    _toastNotification.AddSuccessToastMessage("Service Added successfully");
                    return RedirectToAction(nameof(Index));
                }
                return View(model);
            }
            catch (Exception)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Services == null)
            {
                return NotFound();
            }

            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }
            var model = _mapper.Map<Service, ServiceViewModel>(service);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ServiceViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var files = Request.Form.Files;
                    using var stream = new MemoryStream();
                    if (files.Any())
                    {
                        var image = files.FirstOrDefault();
                        if (allowedExtensions.Contains(Path.GetExtension(image.FileName).ToLower()))
                        {
                            if (image.Length < 3145728) // 3 MB
                            {
                                await image.CopyToAsync(stream);
                            }
                            else
                            {
                                ModelState.AddModelError("Image", "Image can't be larger than 3 MB");
                                return View(model);
                            }
                        }
                    }
                    var service = await _context.Services.FindAsync(model.Id);
                    if (service != null)
                    {
                        if (_context.Services.Where(x => x.Name.ToLower() == model.Name.ToLower() && x.Id != model.Id).Any())
                        {
                            ModelState.AddModelError("Name", "Service exist");
                            return View(model);
                        }
                        service.Name = model.Name;
                        service.Price = model.Price;
                        service.Description = model.Description;
                        service.Gender = model.Gender;
                        service.Image = files.Any() ? stream.ToArray() : service.Image;
                        _context.Update(service);
                        await _context.SaveChangesAsync();
                    }
                    return RedirectToAction(nameof(Index));
                }
                return View(model);
            }
            catch (Exception)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                return RedirectToAction(nameof(Index));
            }

        }

        public IActionResult Delete(int? id)
        {
            if (id == null || _context.Services == null)
            {
                return NotFound();
            }

            var service = new ServiceViewModel().Id = id;
            return PartialView("_deleteService", service);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Services == null)
            {
                return Problem("Entity set 'CUTFLIDbContext.Services'  is null.");
            }
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.Id == id);
        }
    }
}
