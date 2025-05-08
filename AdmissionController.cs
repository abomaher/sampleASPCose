using Core.Entities;
using Core.Interfaces;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Web;
using Web.ViewModels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Web.Controllers
{
    [Route("[controller]")]
    public class AdmissionController : Controller
    {
        private readonly DataContext _context;
        private readonly SignInManager<Users> _signInManager;
        private readonly UserManager<Users> _userManager;
        private readonly IUnitOfWork<Countries> _country;
        private readonly IUnitOfWork<Cities> _city;
        private readonly IUnitOfWork<Colleges> _college;
        private readonly IUnitOfWork<Programs> _program;
        private readonly IWebHostEnvironment _hosting;
        private readonly IUnitOfWork<Admission> _admission;

        public AdmissionController(DataContext context, SignInManager<Users> signInManager, UserManager<Users> userManager, IUnitOfWork<Countries> country, IUnitOfWork<Cities> city, IUnitOfWork<Colleges> college, IUnitOfWork<Programs> program,
            IWebHostEnvironment hosting, IUnitOfWork<Admission> admission)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
            _country = country;
            _city = city;
            _college = college;
            _program = program;
            _hosting = hosting;
            _admission = admission;
        }

        [HttpGet ("Index")]
        public IActionResult Index()
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            var NowYear = DateTime.Now.Year;

            var GraduationYearsList = new List<int>();

            for (var i = NowYear-100; i <= NowYear; i++)
            {
                GraduationYearsList.Add(i); 
            }

            var model = new AdmissionViewModel
            {
                Countries = FillCountriesSelectList("country"),
                NationalityList = FillCountriesSelectList("nationality"),
                GraduationYears = GraduationYearsList.OrderByDescending(i => i).ToList(),
                Colleges = FillCollegesSelectList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("NewAdmission")]
        public IActionResult Index(AdmissionViewModel model)
        {

            var NowYear = DateTime.Now.Year;

            var GraduationYearsList = new List<int>();

            for (var i = NowYear - 100; i <= NowYear; i++)
            {
                GraduationYearsList.Add(i);
            }

            var model2 = new AdmissionViewModel
            {
                Countries = FillCountriesSelectList("country"),
                NationalityList = FillCountriesSelectList("nationality"),
                GraduationYears = GraduationYearsList.OrderByDescending(i => i).ToList(),
                Colleges = FillCollegesSelectList()
            };

            if (ModelState.IsValid)
            {
                try
                {
                    string personalPhoto = UploadFile(model.PersonalPhoto, "") ?? string.Empty;

                    //if (model.CountryOfResidence == "")
                    //{
                    //    ViewBag.Message = "Please select an author from the list!";

                    //    return View(GetAllAuthors());
                    //}

                    Admission admission = new Admission
                    {
                        FullName = model.FullName,
                        FullNameArabic = model.FullNameArabic,
                        PassportNumber = model.PassportNumber,
                        PersonalIdentityNumber = model.PersonalIdentityNumber,
                        Gender = model.Gender,
                        MaritalStatus = model.MaritalStatus,
                        DateOfBirth = model.DateOfBirth,
                        PlaceOfBirth = model.PlaceOfBirth,
                        Nationality = model.Nationality,
                        MotherNationality = model.MotherNationality,
                        CountryOfResidence = model.CountryOfResidence,
                        CityOfResidence = model.CityOfResidence,
                        AddressResidence = model.AddressResidence,
                        AddressResidenceArabic = model.AddressResidenceArabic,
                        LastAcademicCertificate = model.LastAcademicCertificate,
                        GraduationYear = model.GraduationYear,
                        GraduationGPA = model.GraduationGPA,
                        College = model.College,
                        Program = model.Program,
                        Company = model.Company,
                        Job = model.Job,
                        Disability = model.Disability,
                        EmergencyContactName = model.EmergencyContactName,
                        EmergencyContactNameArabic = model.EmergencyContactNameArabic,
                        EmergencyContactMobileNumber = model.EmergencyContactMobileNumber,
                        PhoneNumber = model.PhoneNumber,
                        MobileNumber = model.MobileNumber,
                        SecondMobileNumber = model.SecondMobileNumber,
                        Email = model.Email,
                        Password = model.Password,
                        PersonalPhotoUrl = personalPhoto,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    _admission.Entity.Insert(admission);
                    _admission.Save();

                    //return RedirectToAction(nameof(Index));
                    return View(model2);
                }
                catch(Exception ex)
                {
                    ViewBag.ErrorMessage = ex.Message;
                    return View(model2);
                }
            }
            else 
            {
                ModelState.AddModelError("", "You have to fill all the required fields!");
                return View(model2); 
            }
        }

        string UploadFile(IFormFile file, string imageUrl)
        {
            if (file != null)
            {
                string uploads = Path.Combine(_hosting.WebRootPath, "uploads");

                string newPath = Path.Combine(uploads, file.FileName);
                string oldPath = Path.Combine(uploads, imageUrl);

                if (oldPath != newPath)
                {
                    System.IO.File.Delete(oldPath);
                    file.CopyTo(new FileStream(newPath, FileMode.Create));
                }

                return file.FileName;
            }

            return imageUrl;
        }

        public List<Countries> FillCountriesSelectList(string type)
        {
            var countries = _country.Entity.GetAll().ToList();
            return countries;
        }

        public List<Colleges> FillCollegesSelectList()
        {
            var colleges = _college.Entity.GetAll().ToList();
            return colleges;
        }

        [HttpGet("cityDetails/{CountryId}")]
        public List<Cities> FillCitiesSelectList(int CountryId)
        {
            var cityList = new List<Cities>();

            try
            {
                cityList = _context.Cities.Where(a => a.CountryId == CountryId).ToList();
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            return cityList;
        }

        [HttpGet("programDetails/{CollegeId}")]
        public List<Programs> FillProgramssSelectList(string CollegeId)
        {
            var programList = new List<Programs>();

            try
            {
                programList = _context.Programs.Where(a => a.CollegeId == CollegeId).ToList();
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            return programList;
        }

    }
}
