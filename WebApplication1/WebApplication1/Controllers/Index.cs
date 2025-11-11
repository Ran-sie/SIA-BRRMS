using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using System.IO;

namespace WebApplication1.Controllers
{
    public class IndexController : Controller
    {
        private readonly EvacDbSet _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public IndexController(EvacDbSet context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // --- Dashboard ---
        public IActionResult Index()
        {
            // Count missing and found based on Status field
            var missingCount = _context.MissingReports.Count(m => m.Status == "Missing");
            var foundCount = _context.MissingReports.Count(m => m.Status == "Found");

            // Other dashboard info
            var dashboardData = new DashboardViewModel
            {
                TotalEvacuationCenters = _context.EvacuationAreas.Count(),
                TotalEvacuees = _context.Evacuees.Count(),
                TotalMissing = missingCount,         
                TotalUpcomingDrills = 0,             
                MissingCount = missingCount,         
                FoundCount = foundCount,             
                FamiliesPerZone = _context.Evacuees
                    .GroupBy(e => e.EvacuationCenterAssigned)
                    .ToDictionary(g => g.Key ?? "Unassigned", g => g.Count())
            };

            // Evacuation vs Safe (optional)
            dashboardData.EvacuatedCount = _context.Evacuees.Sum(e => e.TotalMembers ?? 0);
            dashboardData.SafeCount = dashboardData.EvacuatedCount - missingCount;

            ViewData["ActivePage"] = "Dashboard";
            return View("~/Views/Home/Index.cshtml", dashboardData);
        }

        // --- Mapping ---
        public IActionResult Mapping()
        {
            ViewData["ActivePage"] = "Mapping";
            var evacAreas = _context.EvacuationAreas.ToList(); // fetch all areas
            return View("~/Views/Home/Mapping.cshtml", evacAreas);
        }


        [HttpGet]
        public IActionResult CreateMapping(double lat, double lng)
        {
            var model = new EvacuationAreaViewModel
            {
                Latitude = lat,
                Longitude = lng
            };
            return View("~/Views/Home/CreateMapping.cshtml", model);
        }

        // -- CreateMapping

        [HttpPost]
        public async Task<IActionResult> CreateMapping(EvacuationAreaViewModel model)
        {
            if (ModelState.IsValid)
            {
                var entity = new EvacuationAreaViewModel
                {
                    Latitude = model.Latitude,
                    Longitude = model.Longitude,
                    Name = model.Name,
                    Type = model.Type,
                    Capacity = model.Capacity,
                    Facilities = model.Facilities
                };

                _context.EvacuationAreas.Add(entity);
                await _context.SaveChangesAsync();
                return RedirectToAction("Mapping");
            }

            return View(model);
        }

        // -- Update Mapping

        public IActionResult UpdateMapping(int id)
        {
            var site = _context.EvacuationAreas.Find(id); // MappingId is the key
            if (site == null) return NotFound();

            // Convert the entity to ViewModel if you are using EvacuationAreaViewModel
            var model = new EvacuationAreaViewModel
            {
                MappingId = site.MappingId,
                Name = site.Name,
                Type = site.Type,
                Capacity = site.Capacity,
                Facilities = site.Facilities,
                Latitude = site.Latitude,
                Longitude = site.Longitude
            };

            return View("~/Views/Home/UpdateMapping.cshtml", model);
        }

        [HttpPost]
        public IActionResult UpdateMapping(EvacuationAreaViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var existing = _context.EvacuationAreas.Find(model.MappingId);
            if (existing == null) return NotFound();

            existing.Name = model.Name;
            existing.Type = model.Type;
            existing.Capacity = model.Capacity;
            existing.Facilities = model.Facilities;

            _context.SaveChanges();

            return RedirectToAction("Mapping");
        }


        // --- Missing Reports (GET) ---

        public IActionResult CreateMissing()
        {
            return View("~/Views/Home/CreateMissing.cshtml");
        }
        public async Task<IActionResult> Missing()
        {
            ViewData["ActivePage"] = "Missing";
            var reports = await _context.MissingReports.ToListAsync();
            return View("~/Views/Home/Missing.cshtml", reports);
        }

        // --- Missing Reports (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMissing(MissingView1 model)
        {
            Console.WriteLine("✅ POST /Index/Missing reached");

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle image upload if provided
                    if (model.PhotoFile != null && model.PhotoFile.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "image");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.PhotoFile.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.PhotoFile.CopyToAsync(fileStream);
                        }

                        model.PhotoUrl = "/image/" + uniqueFileName;
                    }

                    _context.MissingReports.Add(model);
                    await _context.SaveChangesAsync();

                    TempData["Message"] = "✅ Report submitted successfully!";
                    return RedirectToAction("Missing", "Index");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ File upload failed: " + ex.Message);
                    TempData["Message"] = "❌ Failed to submit report.";
                }
            }

            var reports = await _context.MissingReports.ToListAsync();
            return View("~/Views/Home/Missing.cshtml", reports);
        }

        // --- Missing Delete --- 
        [HttpGet]
        public async Task<IActionResult> DeleteMissing(int id)
        {
            var missing = await _context.MissingReports.FindAsync(id);
            if (missing == null)
            {
                TempData["Message"] = "❌ Record not found.";
                return RedirectToAction("Missing");
            }

            // 🧹 Delete the image file if it exists
            if (!string.IsNullOrEmpty(missing.PhotoUrl))
            {
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, missing.PhotoUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.MissingReports.Remove(missing);
            await _context.SaveChangesAsync();

            TempData["Message"] = "✅ Record deleted successfully!";
            return RedirectToAction("Missing");
        }
        // --- Missing Update

        [HttpGet]
        public async Task<IActionResult> UpdateMissing(int Id)
        {
            var missing = await _context.MissingReports.FindAsync(Id);
            if (missing == null) return NotFound();

            return View("~/Views/Home/UpdateMissing.cshtml", missing);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMissing(MissingView1 model)
        {
            Console.WriteLine("✅ POST /Index/UpdateMissing reached");

            if (!ModelState.IsValid)
            {
                // Return form with validation errors
                return View("~/Views/Home/UpdateMissing.cshtml", model);
            }

            var missingRecord = await _context.MissingReports.FindAsync(model.Id);
            if (missingRecord == null)
            {
                return NotFound();
            }

            try
            {
                // Handle image upload if provided
                if (model.PhotoFile != null && model.PhotoFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "image");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.PhotoFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.PhotoFile.CopyToAsync(fileStream);
                    }

                    missingRecord.PhotoUrl = "/image/" + uniqueFileName;
                }

                // Update properties
                missingRecord.Name = model.Name;
                missingRecord.DateMissing = model.DateMissing;
                missingRecord.Age = model.Age;
                missingRecord.Gender = model.Gender;
                missingRecord.LastSeenLocation = model.LastSeenLocation;
                missingRecord.Address = model.Address;
                missingRecord.Description = model.Description;
                missingRecord.ContactNumber = model.ContactNumber;
                missingRecord.Status = model.Status;

                _context.Update(missingRecord);
                await _context.SaveChangesAsync();

                TempData["Message"] = "✅ Report updated successfully!";
                return RedirectToAction("Missing", "Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Update failed: " + ex.Message);
                TempData["Message"] = "❌ Failed to update report.";
                return View("~/Views/Home/UpdateMissing.cshtml", model);
            }
        }

        // GET: /Index/DetailsMissing/5
        [HttpGet]
        public async Task<IActionResult> DetailsMissing(int id)
        {
            var report = await _context.MissingReports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }
            return View("~/Views/Home/DetailsMissing.cshtml", report);
        }


        // --- Evacuees (GET) ---
        public async Task<IActionResult> Evacuess(int? selectedMappingId)
        {
            ViewData["ActivePage"] = "Evacuess";

            // Get all evacuation areas for the dropdown
            var evacAreas = await _context.EvacuationAreas.ToListAsync();
            ViewBag.EvacuationAreas = evacAreas ?? new List<EvacuationAreaViewModel>();

            // Pass the selected MappingId to the view
            ViewBag.SelectedMappingId = selectedMappingId;

            // Filter evacuees by MappingId if selected
            var evacueesQuery = _context.Evacuees.AsQueryable();
            if (selectedMappingId.HasValue)
            {
                evacueesQuery = evacueesQuery.Where(e => e.MappingId == selectedMappingId.Value);
            }

            var evacueeList = await evacueesQuery.ToListAsync();

            return View("~/Views/Home/Evacuess.cshtml", evacueeList);
        }



        [HttpGet]
        public async Task<IActionResult> CreateEvacuee()
        {
            ViewData["ActivePage"] = "Evacuess";

            // Fetch evacuation areas for dropdown
            var areas = await _context.EvacuationAreas.ToListAsync();
            ViewBag.EvacuationAreas = areas;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CreateEvacuess()
        {
            ViewData["ActivePage"] = "Evacuess";

            // Fetch evacuation areas for the dropdown
            var areas = await _context.EvacuationAreas.ToListAsync();
            ViewBag.EvacuationAreas = areas ?? new List<EvacuationAreaViewModel>();

            return View("~/Views/Home/CreateEvacuess.cshtml");
        }


        // --- Evacuees (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvacuess(EvacModel model)
        {
            if (ModelState.IsValid)
            {
                _context.Evacuees.Add(model);
                await _context.SaveChangesAsync();
                TempData["Message"] = "✅ Evacuee added successfully!";
                return RedirectToAction("Evacuess");
            }

            var evacuees = await _context.Evacuees.ToListAsync();
            return View("~/Views/Home/Evacuess.cshtml", evacuees);
        }

        // --- Delete Evacuee ---
        [HttpGet]
        public IActionResult DeleteEvacuee(int FamilyId)
        {
            var evacuee = _context.Evacuees.FirstOrDefault(e => e.FamilyId == FamilyId);
            if (evacuee != null)
            {
                _context.Evacuees.Remove(evacuee);
                _context.SaveChanges();
                TempData["Message"] = "✅ Evacuee deleted successfully!";
            }
            else
            {
                TempData["Message"] = "⚠️ Evacuee not found.";
            }

            return RedirectToAction("Evacuess");
        }

        // GET: show pre-filled form
        [HttpGet]
        public async Task<IActionResult> UpdateEvacuess(int FamilyId)
        {
            var evacuee = await _context.Evacuees.FindAsync(FamilyId);
            if (evacuee == null) return NotFound();

            return View("~/Views/Home/UpdateEvacuess.cshtml", evacuee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEvacuess(EvacModel model)
        {
            if (ModelState.IsValid)
            {
                _context.Evacuees.Update(model);
                await _context.SaveChangesAsync();
                TempData["Message"] = "✅ Evacuee updated successfully!";

                var evacuees = await _context.Evacuees.ToListAsync();
                return View("~/Views/Home/Evacuess.cshtml", evacuees);
            }

            return View("~/Views/Home/UpdateEvacuess.cshtml", model);
        }


    }
}
