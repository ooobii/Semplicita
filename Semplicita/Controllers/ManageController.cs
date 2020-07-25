﻿using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Semplicita.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Semplicita.Helpers;

namespace Semplicita.Controllers
{
    [Authorize]
    public class ManageController : Controller {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationDbContext db = new ApplicationDbContext();

        public ManageController() {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager) {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager {
            get {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager {
            get {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set {
                _userManager = value;
            }
        }

        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message) {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            var userId = User.Identity.GetUserId();
            var model = new IndexViewModel {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId),
                ThisUser = db.Users.Find(User.Identity.GetUserId()),
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            };
            return View(model);
        }



        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey) {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if( result.Succeeded ) {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if( user != null ) {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            } else {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("Index", new { Message = message });
        }
        

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model) {
            if( !ModelState.IsValid ) {
                return View(model);
            }
            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if( UserManager.SmsService != null ) {
                var message = new IdentityMessage {
                    Destination = model.Number,
                    Body = "Your security code is: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("Index");
        }

        //
        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePhoneNumber() {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if( !result.Succeeded ) {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if( user != null ) {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }
        
        //
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber) {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            // Send an SMS through the SMS provider to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model) {
            if( !ModelState.IsValid ) {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if( result.Succeeded ) {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if( user != null ) {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Failed to verify phone");
            return View(model);
        }




        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication() {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if( user != null ) {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication() {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if( user != null ) {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }





        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model) {
            if( !ModelState.IsValid ) {
                TempData.AddDangerToast("Unable to update your password. Please verify that you entered your current password correctly, and that both of the new passwords entered are the same.");
                return RedirectToAction("Index");
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if( result.Succeeded ) {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if( user != null ) {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                TempData.AddSuccessToast("Your password has been updated!");
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return RedirectToAction("Index");
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword() {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model) {
            if( ModelState.IsValid ) {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if( result.Succeeded ) {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if( user != null ) {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay manage page (errors will be added to alert queues)
            return RedirectToAction("Index");
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [MultipleButton(Name = "accountUpdate", Argument = "ChangeName")]
        public async Task<ActionResult> ChangeName(ChangeNameViewModel model) {
            var userId = User.Identity.GetUserId();

            try {
                if( !ModelState.IsValid ) { throw new Exception(); }
                var oldFullName = db.Users.Find(userId).FullNameStandard;

                db.Users.Find(User.Identity.GetUserId()).FirstName = model.FirstName;
                db.Users.Find(User.Identity.GetUserId()).LastName = model.LastName;
                await db.SaveChangesAsync();

                TempData.AddSuccessToast($"Your name has been changed from '{oldFullName}' to '{model.FirstName} {model.LastName}'.");
            } catch {
                TempData.AddDangerToast("We were unable to update your name. Please try again later.");
            }

            var viewModel = new IndexViewModel() {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId),
                ThisUser = db.Users.Find(User.Identity.GetUserId())
            };

            return View(model.ReturnViewName, viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [MultipleButton(Name = "accountUpdate", Argument = "UpdateEmail")]
        public async Task<ActionResult> UpdateEmail(ChangeEmailViewModel model) {
            var userId = User.Identity.GetUserId();

            if (model.NewEmail == model.NewEmailConfirmed) {
                try {
                    var oldEmail = db.Users.Find(userId).Email;

                    db.Users.Find(User.Identity.GetUserId()).Email = model.NewEmail;
                    db.Users.Find(User.Identity.GetUserId()).UserName = model.NewEmail;
                    db.Users.Find(User.Identity.GetUserId()).EmailConfirmed = false;
                    await db.SaveChangesAsync();

                    ViewBag.AccountUpdateEmailMessage = $"Your email has been changed from '{oldEmail}' to '{model.NewEmail}'.";
                    HttpContext.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

                    
                } catch {
                    ViewBag.AccountUpdateEmailError = "We were unable to update the email address for your account. Please try again later.";
                }
            } else {
                ViewBag.AccountUpdateEmailError = "The emails you entered were not the same. Please verify your email entries and try again.";
            }

            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            var viewModel = new IndexViewModel() {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId),
                ThisUser = db.Users.Find(User.Identity.GetUserId()),
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            };
            return View("Index", viewModel);
        }


        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider) {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback() {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if( loginInfo == null ) {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("Index") : RedirectToAction("Index", new { Message = ManageMessageId.Error });
        }

        protected override void Dispose(bool disposing) {
            if( disposing && _userManager != null ) {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers

        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager {
            get {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result) {
            foreach( var error in result.Errors ) {
                TempData.Add("tdngr-iderr" + result.Errors.ToList().IndexOf(error).ToString(), error);
                //ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword() {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if( user != null ) {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber() {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if( user != null ) {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

        #endregion Helpers
    }
}