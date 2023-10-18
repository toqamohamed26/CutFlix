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
using Microsoft.AspNetCore.Authorization;
using CUTFLI.ActionFilter;
using NToastNotify;

namespace CUTFLI.Controllers
{
    [Authorize]
    public class PeopleController : Controller
    {
        private readonly CUTFLIDbContext _context;
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;

        public PeopleController(CUTFLIDbContext context, IMapper mapper, IToastNotification toastNotification)
        {
            _context = context;
            _mapper = mapper;
            _toastNotification = toastNotification;
        }

        [ServiceFilter(typeof(AdminFilter))]
        public async Task<IActionResult> Index()
        {
            try
            {
                var people = await _context.Peoples.ToListAsync();
                var peopleModel = _mapper.Map<List<People>, List<PeopleViewModel>>(people);
                return View(peopleModel);
            }
            catch (Exception)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                return View(new List<PeopleViewModel>());
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null || _context.Peoples == null)
                {
                    return NotFound();
                }

                var people = await _context.Peoples
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (people == null)
                {
                    return NotFound();
                }
                var peopleModel = _mapper.Map<People, PeopleViewModel>(people);
                return View(peopleModel);
            }
            catch (Exception)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                return View(new PeopleViewModel());
            }
        }

        [ServiceFilter(typeof(AdminFilter))]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(AdminFilter))]
        public async Task<IActionResult> Create(PeopleViewModel peopleModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (_context.Peoples.Any(x => x.PhoneNumber == peopleModel.PhoneNumber))
                    {
                        ModelState.AddModelError("PhoneNumber", "PhoneNumber is used");
                        return View(peopleModel);
                    }
                    if (_context.Peoples.Any(x => x.Email == peopleModel.Email))
                    {
                        ModelState.AddModelError("Email", "Email is used");
                        return View(peopleModel);
                    }
                    var people = _mapper.Map<PeopleViewModel, People>(peopleModel);
                    people.CreatedDate = DateTime.Now;
                    await _context.AddAsync(people);
                    await _context.SaveChangesAsync();
                    _toastNotification.AddSuccessToastMessage("Customer Added successfully");
                    return RedirectToAction(nameof(Index));
                }
                return View(peopleModel);
            }
            catch (Exception)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                return View(peopleModel);
            }
        }

        [ServiceFilter(typeof(AdminFilter))]
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null || _context.Peoples == null)
                {
                    return NotFound();
                }

                var people = await _context.Peoples.FindAsync(id);
                if (people == null)
                {
                    return NotFound();
                }
                var peopleModel = _mapper.Map<People, PeopleViewModel>(people);
                return View(peopleModel);
            }
            catch (Exception)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                return View(new PeopleViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(AdminFilter))]
        public async Task<IActionResult> Edit(PeopleViewModel peopleModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (_context.Peoples.Any(x => x.PhoneNumber == peopleModel.PhoneNumber))
                    {
                        ModelState.AddModelError("PhoneNumber", "PhoneNumber is used");
                        return View(peopleModel);
                    }
                    if (_context.Peoples.Any(x => x.Email == peopleModel.Email))
                    {
                        ModelState.AddModelError("Email", "Email is used");
                        return View(peopleModel);
                    }
                    var customer = await _context.Peoples.FindAsync(peopleModel.Id);
                    if (customer != null)
                    {
                        customer.Email = peopleModel.Email;
                        customer.PhoneNumber = peopleModel.PhoneNumber;
                        _toastNotification.AddSuccessToastMessage("Customer updated successfully");
                        await _context.SaveChangesAsync();
                    }
                    return RedirectToAction(nameof(Index));
                }
                return View(peopleModel);
            }
            catch (Exception)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                return View(peopleModel);
            }
        }

        [ServiceFilter(typeof(AdminFilter))]
        public IActionResult Delete(int? id)
        {
            if (id == null || _context.Peoples == null)
            {
                return NotFound();
            }

            var customer = new PeopleViewModel().Id = id;
            return PartialView("_deleteCustomer", customer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(AdminFilter))]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                if (_context.Peoples == null)
                {
                    return Problem("Entity set 'CUTFLIDbContext.Peoples'  is null.");
                }
                var people = await _context.Peoples.FindAsync(id);
                if (people != null)
                {
                    _context.Peoples.Remove(people);
                }

                await _context.SaveChangesAsync();
                _toastNotification.AddSuccessToastMessage("Customer deleted successfully");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                return RedirectToAction(nameof(Index));
            }
        }

        private bool PeopleExists(int id)
        {
            return _context.Peoples.Any(e => e.Id == id);
        }
    }
}
