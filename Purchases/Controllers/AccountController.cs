using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Purchases.Models;
using System.Collections.Generic;
using Purchases.Functions;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using Purchases.MyLogic;

namespace Purchases.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private Entities db = new Entities();
        private SharedClass shared = new SharedClass();
        private RoleController RoleController = new RoleController();

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public AccountController()
        {
        }
        
        public ActionResult MyProfile()
        {
            RoleController role = new RoleController();
            string userid = User.Identity.GetUserId();
            var curreny = db.CurrencyTypes.FirstOrDefault(x => x.IsLocalCurrency == true);
            var userObj = db.AspNetUsers.Find(userid);
          
            ViewBag.FullName = userObj.FullName;
            ViewBag.Role = RoleController.GetRole(userid);
            ViewBag.UserName = userObj.UserName;
            ViewBag.PhoneNumber = "0"+userObj.PhoneNumber;
            ViewBag.Email = userObj.Email;
            ViewBag.LocalCurrency = curreny.Name;

            ViewBag.LocalCurrencyId = curreny.Id;

            ViewBag.CurrentYear = shared.GetCurrentFinancialCycleYear();
            ViewBag.UserCurrentYear = shared.GetUserCurrentFinancialCycleYear(userid);



            return View("Profile");
        }


        public string GetRole(string id)
        {
            ApplicationUser user = _userManager.FindById(id);
            string rolename = _userManager.GetRoles(user.Id).FirstOrDefault();
            if (string.IsNullOrEmpty(id))
            {
                rolename = " ";

            }

            return rolename;
        }


        // دي بجيب بيها البيانات عشان اعرضها في المخطط الخاص بكل موظف
        public ActionResult GetChartData(string type)
        {
            var userid = User.Identity.GetUserId();
            int CurrentFinancialCycleId = shared.GetUserCurrentFinancialCycleId(userid);
            string CurrentFinancialCycleYear = shared.GetUserCurrentFinancialCycleYear(userid);
            int currentYear = Convert.ToInt32(CurrentFinancialCycleYear);
            var transactions = db.Transactions.Where(t => t.FinancialCycleId == CurrentFinancialCycleId).ToList();

            if(type != "dashboard")
            {
                transactions = transactions.Where(t => t.CreatedBy == userid || t.UpdatedBy == userid).ToList();
            }

            var transactionCountsPerMonth = transactions.Where(q=> q.TransactionDate.Value.Year == currentYear)
                    .GroupBy(t => new { t.TransactionDate.Value.Year, t.TransactionDate.Value.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Count = g.Count()
                    })
                    .OrderBy(g => g.Year).ThenBy(g => g.Month)
                    .ToList();
            
            var labels = transactionCountsPerMonth.Select(t => $"{t.Month.ToString("D2")}").ToList();
            var values = transactionCountsPerMonth.Select(t => t.Count).ToList();

            // Return the data in a format suitable for Chart.js
            return Json(new
            {
                labels = labels,
                datasets = new[]
                {
            new {
                label = "عدد العمليات",
                data = values,
                borderColor = "#e09d0d",
                fill = false
            }
        }
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDoctTypeChartData(string type)
        {
            var userId = User.Identity.GetUserId();
            int CurrentFinancialCycleId = shared.GetUserCurrentFinancialCycleId(userId);

            var transactions = db.Transactions.Where(t => t.FinancialCycleId == CurrentFinancialCycleId).ToList();

            if (type != "dashboard")
            {
                var userid = User.Identity.GetUserId();
                transactions = transactions.Where(t => t.CreatedBy == userid || t.UpdatedBy == userid).ToList();
            }

            var transactionCountsPerMonth = transactions
                    .GroupBy(t => new { DocType = t.DocumentType.Name })
                    .Select(g => new
                    {
                        DocType = g.Key.DocType,
                        Count = g.Count()
                    })
                    .ToList();

            var labels = transactionCountsPerMonth.Select(t => t.DocType).ToList();
            var values = transactionCountsPerMonth.Select(t => t.Count).ToList();

            // Return the data in a format suitable for Chart.js
            return Json(new
            {
                labels = labels,
                datasets = new[]
                {
            new {
                label = "عدد العمليات",
                data = values,
                //borderColor = "#e09d0d",
                fill = false
            }
        }
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult LoadData()
        {
            IQueryable<AspNetUser> AspNetUser = db.AspNetUsers;
            
            var UserVM = new List<UserVM>();
            foreach (var item in AspNetUser)
            {
                UserVM.Add(new UserVM
                {
                    Id = item.Id,
                    UserName = item.UserName,
                    Name = item.FullName,
                    Phone = item.PhoneNumber,
                    Role = RoleController.GetRole(item.Id),
                    CompanyInfoId = item.UserWorkDetails.FirstOrDefault(q=> q.UserId == item.Id).CompanyInfoId,
                    CompanyInfoName = item.UserWorkDetails.FirstOrDefault(q=> q.UserId == item.Id).CompanyInfo.Name,
                    FinancialCycleId = item.UserWorkDetails.FirstOrDefault(q=> q.UserId == item.Id).FinancialCycleId,
                });
            }
            
            return Json(UserVM, JsonRequestBehavior.AllowGet);
        }
        
        public async Task<ActionResult> CreateMe(UserVM data)
        {
            // var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(dbContext));
            // var user = new ApplicationUser();
            //// var Phone = db.EmployeeContacts.FirstOrDefault(b => b.ContactTypeId == 1 && b.EmployeeId == data.EmployeeId).Contact;

            // user.UserName = data.Phone;
            // user.IsDefualtPassword = true;
            // var check = userManager.Create(user, "123456");
            // if (check.Succeeded)
            // {
            //     userManager.AddToRole(user.Id, data.Role);

            return Json("", JsonRequestBehavior.AllowGet);
            // }

            var user = new ApplicationUser { UserName = data.Phone, Email = "A@A.A"};
            user.IsDefualtPassword = true;
            var result = await UserManager.CreateAsync(user, "123456");
            if (result.Succeeded)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                UserManager.AddToRole(user.Id, data.Role);

                return Json(new { Message = " تم الحفظ بنجاح", Status = "success", Title = "نجاح" }, JsonRequestBehavior.AllowGet);
            }
                return Json(new { Message = "  عذرا حدث خطأ أثناء  عملية الاضافة ", Status = "error", Title = "خطأ" }, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> Create(UserVM RegisterViewModel)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();
                int CurrentFinancialCycleId = shared.GetUserCurrentFinancialCycleId(userId);
                try
                {
                    // string name = db.AspNetUsers.Single(x => x.UserName == RegisterViewModel.UserName).UserName;
                    if (!db.AspNetUsers.Any(x => x.PhoneNumber == RegisterViewModel.Phone))
                    {
                        var identityUser = new ApplicationUser
                        {
                            Email = RegisterViewModel.Phone + "@" + RegisterViewModel.Phone + ".com",
                            PhoneNumber = RegisterViewModel.Phone,
                            UserName = RegisterViewModel.Phone,
                            FullName = RegisterViewModel.UserName,
                            IsDefualtPassword = true,
                            AirportName = RegisterViewModel.AirportName,
                            FinancialCycleId = CurrentFinancialCycleId,
                        };

                        var creationReasult = await UserManager.CreateAsync(identityUser, "123456");

                        // User Created
                        if (creationReasult.Succeeded)
                        {
                            var UserId = identityUser.Id;
                            creationReasult = UserManager.AddToRole(UserId, RegisterViewModel.Role);

                            int FinancialCycleId = shared.GetCurrentFinancialCycleId();
                            var obj = new UserWorkDetail()
                            {
                                UserId = UserId,
                                CompanyInfoId = RegisterViewModel.CompanyInfoId,
                                FinancialCycleId = FinancialCycleId
                            };

                            db.UserWorkDetails.Add(obj);
                            db.SaveChanges();

                            return Json(new { Message = " تم الحفظ بنجاح", Status = "success", Title = "نجاح" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                catch(Exception e)
                {
                    return Json(new { Message = e.Message, Status = "error", Title = "خطا" }, JsonRequestBehavior.AllowGet);
                }
            }
            // RegisterViewModel.Message = "User Name Already Found";
            return Json(new { Message = " خطا في عملية الحفظ", Status = "error", Title = "خطا" }, JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        public ActionResult Edit(UserVM data)
        {
            int CurrentFinancialCycleId = shared.GetUserCurrentFinancialCycleId(data.Id);
            if (ModelState.IsValid)
            {
                AspNetUser AspNetUser = db.AspNetUsers.Find(data.Id);
                AspNetUser.Email = data.Phone + "@" + data.Phone + ".com";
                AspNetUser.PhoneNumber = data.Phone;
                AspNetUser.UserName = data.Phone;
                AspNetUser.AirportName = data.AirportName;
                AspNetUser.FullName = data.UserName;
                AspNetUser.FinancialCycleId = CurrentFinancialCycleId;

                var oldRole = RoleController.GetRole(data.Id);

                ApplicationDbContext dbContext = new ApplicationDbContext();

                var userWorkonDetails = db.UserWorkDetails.FirstOrDefault(q=> q.UserId == data.Id);
                
                if(userWorkonDetails != null)
                {
                    userWorkonDetails.CompanyInfoId = data.CompanyInfoId;
                    userWorkonDetails.FinancialCycleId = CurrentFinancialCycleId;

                    db.Entry(userWorkonDetails).State = EntityState.Modified;
                }
                else
                {
                    int FinancialCycleId = shared.GetCurrentFinancialCycleId();
                    var obj = new UserWorkDetail()
                    {
                        UserId = data.Id,
                        CompanyInfoId = data.CompanyInfoId,
                        FinancialCycleId = FinancialCycleId
                    };

                    db.UserWorkDetails.Add(obj);
                }

                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(dbContext));
                if (data.Id != "" && data.Role != "")
                {
                    userManager.RemoveFromRole(data.Id, oldRole);
                    userManager.AddToRole(data.Id, data.Role);

                }
                db.Entry(AspNetUser).State = EntityState.Modified;
                db.SaveChanges();

                return Json(new { Message = " تم التعديل بنجاح", Status = "success", Title = "نجاح" });

            }
            return Json(new { Message = "  عذرا حدث  خطأ أثناء عملية التعديل ", Status = "error", Title = "خطأ" });


        }

        [HttpPost]
        public ActionResult UpdateUserYear(int? Id)
        {
            string userId = User.Identity.GetUserId();

            if(Id == null || Id <= 0)
            {
                return Json(new { Message = "يجب اختيار العام المالي", Status = "error", Title = "خطأ" }, JsonRequestBehavior.AllowGet);            
            }

            if(db.AspNetUsers.Any(q=> q.Id == userId))
            {
                var User = db.AspNetUsers.Find(userId);

                User.FinancialCycleId = Id;
                db.Entry(User).State = EntityState.Modified;
                db.SaveChanges();

                return Json(new { Message = " تم التعديل بنجاح", Status = "success", Title = "نجاح" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Message = "بيانات المستخدم غير صحيحة حاول تسجيل الدخول مجددا", Status = "error", Title = "خطأ" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ResetPassword(UserVM data)
        {
            ApplicationDbContext dbContext = new ApplicationDbContext();

            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(dbContext));
            var userid = data.Id;
            int CurrentFinancialCycleId = shared.GetUserCurrentFinancialCycleId(userid);
            ApplicationUser user = userManager.FindById(userid);
            user.PasswordHash = userManager.PasswordHasher.HashPassword("123456");
            user.IsDefualtPassword = true;
            user.FinancialCycleId = CurrentFinancialCycleId;

            var result = userManager.Update(user);

            if (result.Succeeded)
            {

                return Json(new { Message = " تم تعين كلمة المرور بنجاح", Status = "success", Title = "نجاح" });
            }

            return Json(new { Message = " عذرا حدث خطأ أثناء عملية التعديل", Status = "error", Title = "خطأ" });

        }

        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View();
        }
        
        [HttpPost]
        public ActionResult ChangePassword(UserVM data)
        {
            ApplicationDbContext dbContext = new ApplicationDbContext();

            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(dbContext));
            var userid = User.Identity.GetUserId();

            int CurrentFinancialCycleId = shared.GetUserCurrentFinancialCycleId(userid);

            //var hashedpassword = db.AspNetUsers.Find(userid).PasswordHash;


            ApplicationUser user = userManager.FindById(userid);
            if (userManager.CheckPassword(user, data.OldPassword))
            {
                if (data.NewPassword == data.ConfirmPassword)
                {
                    user.PasswordHash = userManager.PasswordHasher.HashPassword(data.NewPassword);
                    user.IsDefualtPassword = false;
                    user.FinancialCycleId = CurrentFinancialCycleId;
                    var result = userManager.Update(user);

                    if (result.Succeeded)
                    {

                        return Json(new { Message = " تم تعين كلمة المرور بنجاح", Status = "success", Title = "نجاح" });
                    }
                }
                else
                {
                    return Json(new { Message = " كلمة المرور الجديدة غير متطابقة", Status = "warning", Title = "تنبيه" });
                }

            }
            else
            {
                return Json(new { Message = " كلمة المرور القديمة غير صحيحة", Status = "warning", Title = "تنبيه" });
            }

            return Json(new { Message = " عذرا حدث خطأ أثناء عملية التعديل", Status = "error", Title = "خطأ" });
        }

        //public ActionResult Create(RegisterViewModel data)
        //{
        //    var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

        //    var user = new ApplicationUser { UserName = data.UserName};

        //    //var user = new ApplicationUser();


        //    //user.UserName = data.UserName;
        //    //user.PhoneNumber = data.PhoneNumber;

        //    var check = userManager.Create(user, data.Password);

        //    if (check.Succeeded)
        //    {
        //        userManager.AddToRole(user.Id, data.Role);
        //    }


        //    return Json(new { Message = " تم الحفظ بنجاح", Status = "success", Title = "نجاح" });
        //}

        public ActionResult getAllRoles(string q)
        {
            var data = db.AspNetRoles.Select(p => new
            {
                id = p.Id,
                text = p.Name
            }).Where(f => f.text.Contains(q));
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getAirports(string q)
        {
            var data = db.CompanyInfoes.Select(p => new
            {
                id = p.Id,
                text = p.Name.Trim()
            }).Where(f => f.text.Contains(q)).Distinct().ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            //bool IsDefualtPassword = (bool)db.AspNetUsers.Single(x => x.UserName == model.UserName).IsDefualtPassword;

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "يجب أن لا يكون هنالك حقول فارغة");
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, shouldLockout: false);

            switch (result)
            {
                case SignInStatus.Success:
                    ApplicationDbContext dbContext = new ApplicationDbContext();

                    var user = UserManager.FindByName(model.UserName);
                    var userid = user.Id;

                    Session["FullName"] = user.FullName;
                    Session["Role"] = RoleController.GetRole(userid);
                    Session["UserId"] = userid;
                    Session["AirportName"] = user.AirportName;

                    Session["CurrentYear"] = db.FinancialCycles.FirstOrDefault(x=> x.CurrentYear == true).Year;
                    
                    return RedirectToAction("Index", "Home");
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "اسم المستخدم او كلمة المرور خاطئة");
                    return View(model);
            }
        }
        
        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        //[AllowAnonymous]
        //public ActionResult ResetPassword(string code)
        //{
        //    return code == null ? View("Error") : View();
        //}

        //
        // POST: /Account/ResetPassword
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }
        //    var user = await UserManager.FindByNameAsync(model.Email);
        //    if (user == null)
        //    {
        //        // Don't reveal that the user does not exist
        //        return RedirectToAction("ResetPasswordConfirmation", "Account");
        //    }
        //    var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
        //    if (result.Succeeded)
        //    {
        //        return RedirectToAction("ResetPasswordConfirmation", "Account");
        //    }
        //    AddErrors(result);
        //    return View();
        //}

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Account");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}