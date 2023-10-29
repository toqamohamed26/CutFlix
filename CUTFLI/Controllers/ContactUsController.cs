using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CUTFLI.Models;
using AutoMapper;
using CUTFLI.ViewModels;
using NToastNotify;

namespace CUTFLI.Controllers
{
    public class ContactUsController : Controller
    {
        private readonly CUTFLIDbContext _context;
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;

        public ContactUsController(CUTFLIDbContext context, IMapper mapper, IToastNotification toastNotification)
        {
            _context = context;
            _mapper = mapper;
            _toastNotification = toastNotification;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var contactUs = await _context.ContactUs.OrderByDescending(x => x.CreatedDate).ToListAsync();
                var model = _mapper.Map<List<ContactUsViewModel>>(contactUs);
                return View(model);
            }
            catch (Exception)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                return RedirectToAction("Index","Home");
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.ContactUs == null)
            {
                return NotFound();
            }

            var contact = await _context.ContactUs.FindAsync(id);
            if (contact == null)
            {
                return NotFound();
            }
            var contactModel = _mapper.Map<ContactUsViewModel>(contact);
            return View(contactModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                if (_context.ContactUs == null)
                {
                    return Problem("Entity set 'CUTFLIDbContext.ContactUs'  is null.");
                }
                var contactUs = await _context.ContactUs.FindAsync(id);
                if (contactUs != null)
                {
                    _context.ContactUs.Remove(contactUs);
                }

                await _context.SaveChangesAsync();
                _toastNotification.AddSuccessToastMessage("Record deleted successfully");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
