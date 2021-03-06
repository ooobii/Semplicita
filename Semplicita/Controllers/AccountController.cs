﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Semplicita.Helpers;
using Semplicita.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using static Semplicita.Helpers.Util;

namespace Semplicita.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController() {
        }

        public AccountController( ApplicationUserManager userManager, ApplicationSignInManager signInManager ) {
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
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login( string returnUrl ) {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login( LoginViewModel model, string returnUrl ) {
            if ( !ModelState.IsValid ) {
                return View( model );
            }

            //Check to see if the email was confirmed (if user exists).
            var user = await UserManager.FindByNameAsync(model.Email);
            if ( user != null ) {
                if ( !await UserManager.IsEmailConfirmedAsync( user.Id ) ) {
                    ModelState.AddModelError( "", "You need to confirm your email in order to login." );
                    return View( model );
                }
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch ( result ) {
                case SignInStatus.Success:
                    return RedirectToLocal( returnUrl );

                case SignInStatus.LockedOut:
                    return View( "Lockout" );

                case SignInStatus.RequiresVerification:
                    return RedirectToAction( "SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe } );

                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError( "", "Invalid login attempt." );
                    return View( model );
            }
        }

        [AllowAnonymous]
        public ActionResult DemoLogin( string returnUrl ) {
            return View( "DemoLogin" );
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DemoLogin( string emailKey, string returnUrl ) {
            var loginEmail = GetSetting(emailKey);
            var loginPassword = GetSetting("demo:Password");

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(loginEmail, loginPassword, isPersistent: false, shouldLockout: false);
            switch ( result ) {
                case SignInStatus.Success:
                    return RedirectToAction( "Index", "Home" );

                case SignInStatus.Failure:
                default:
                    return RedirectToAction( "Login", "Account" );
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode( string provider, string returnUrl, bool rememberMe ) {
            // Require that the user has already logged in via username/password or external login
            if ( !await SignInManager.HasBeenVerifiedAsync() ) {
                return View( "Error" );
            }
            return View( new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe } );
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode( VerifyCodeViewModel model ) {
            if ( !ModelState.IsValid ) {
                return View( model );
            }

            // The following code protects for brute force attacks against the two factor codes.
            // If a user enters incorrect codes for a specified amount of time then the user account
            // will be locked out for a specified amount of time.
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch ( result ) {
                case SignInStatus.Success:
                    return RedirectToLocal( model.ReturnUrl );

                case SignInStatus.LockedOut:
                    return View( "Lockout" );

                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError( "", "Invalid code." );
                    return View( model );
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register() {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register( RegisterViewModel model ) {
            if ( ModelState.IsValid ) {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };
                var result = await UserManager.CreateAsync(user, model.Password);
                if ( result.Succeeded ) {
                    //this is commented out to prevent the user from being signed in until email is confirmed.
                    //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    //create confirmation code and callback URL to trigger email confirmation.
                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);

                    //begin attempt to send email
                    try {
                        //fetch 'from' from configuration
                        var from = new System.Net.Mail.MailAddress(WebConfigurationManager.AppSettings["emailsvcusr"], WebConfigurationManager.AppSettings["emailsvcdisplay"]);
                        var emailMsg = new MailMessage(from.ToString(), user.Email)
                        {
                            Subject = $"Simplicita: Confirm your account,{user.FullNameStandard}!",
                            Body = "<p>Thank you for registering! We're glad to have you.</p>" +
                                  $"<p>To activate your account, please <a href=\"{callbackUrl}\">click here</a>! After confirming your email, you will be able to login.</p>",
                            IsBodyHtml = true
                        };

                        var emailSvc = new EmailService();
                        await emailSvc.SendAsync( emailMsg );

                        //redirect to confirmation sent, since login never occured (notify user of requirement to confirm).
                        ViewBag.EmailSentTo = user.Email;
                        return View( "ConfirmationSent" );
                    } catch ( Exception ex ) {
                        //delete user since confirmation email failed to send.
                        await UserManager.DeleteAsync( user );

                        //print to IIS log
                        Console.WriteLine( ex.ToString() );

                        //add error to model state
                        ModelState.AddModelError( "Email Send Failure", "The account was not created. The confirmation email was unable to be sent, but is required for login. This erorr has been reported, please try again later." );
                    }
                }
                AddErrors( result );
            }

            // If we got this far, something failed, redisplay form
            return View( model );
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail( string userId, string code ) {
            if ( userId == null || code == null ) {
                return View( "Error" );
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View( result.Succeeded ? "ConfirmEmail" : "Error" );
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ResendEmailConfirmation() {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResendEmailConfirmation( ForgotPasswordViewModel model ) {
            var user = await UserManager.FindByNameAsync(model.Email);

            if ( user != null ) {
                string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                var callbackUrl = Url.Action("ConfirmEmail", "Account", new
                {
                    userId = user.Id,
                    code = code
                }, protocol: Request.Url.Scheme);

                try {
                    var from = new System.Net.Mail.MailAddress(WebConfigurationManager.AppSettings["emailsvcusr"], WebConfigurationManager.AppSettings["emailsvcdisplay"]);
                    var emailMsg = new MailMessage(from.ToString(), user.Email)
                    {
                        Subject = $"Simplicita: Confirm your account,{user.FullNameStandard}!",
                        Body = "<p>Here is a new copy of a confirmation email for your account.</p>" +
                              $"<p>Please <a href=\"{callbackUrl}\">click here</a>! After confirming your email, you will be able to login.</p>",
                        IsBodyHtml = true
                    };

                    var emailSvc = new EmailService();
                    await emailSvc.SendAsync( emailMsg );

                    ViewBag.EmailSentTo = user.Email;
                } catch ( Exception ex ) {
                    //print error to IIS log
                    Debug.WriteLine( ex.ToString() );
                }
            }

            return View( "ConfirmationSent" );
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword() {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword( ForgotPasswordViewModel model ) {
            if ( ModelState.IsValid ) {
                var user = await UserManager.FindByNameAsync(model.Email);
                if ( user == null ) {
                    // Don't reveal that the user does not exist or is not confirmed
                    ViewBag.EmailSentTo = model.Email;
                    return View( "ForgotPasswordConfirmation" );
                }

                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);

                try {
                    var from = new System.Net.Mail.MailAddress(WebConfigurationManager.AppSettings["emailsvcusr"], WebConfigurationManager.AppSettings["emailsvcdisplay"]);
                    var emailMsg = new MailMessage(from.ToString(), user.Email)
                    {
                        Subject = "Simplicita: Reset Password Confirmation Email",
                        Body = $"<p>To reset your account, please <a href=\"{callbackUrl}\">click here</a>!</p>",
                        IsBodyHtml = true
                    };

                    var emailSvc = new EmailService();
                    await emailSvc.SendAsync( emailMsg );

                    TempData.Add( "EmailSentTo", user.Email );
                    return RedirectToAction( "ForgotPasswordConfirmation", "Account" );
                } catch ( Exception ex ) {
                    Console.WriteLine( ex.ToString() );
                    await Task.FromResult( 0 );
                }

                return RedirectToAction( "ForgotPasswordConfirmation", "Account" );
            }

            // If we got this far, something failed, redisplay form
            return View( model );
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation() {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword( string code ) {
            return code == null ? View( "Error" ) : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword( ResetPasswordViewModel model ) {
            if ( !ModelState.IsValid ) {
                return View( model );
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if ( user == null ) {
                // Don't reveal that the user does not exist
                return RedirectToAction( "ResetPasswordConfirmation", "Account" );
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if ( result.Succeeded ) {
                return RedirectToAction( "ResetPasswordConfirmation", "Account" );
            }
            AddErrors( result );
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation() {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin( string provider, string returnUrl ) {
            // Request a redirect to the external login provider
            return new ChallengeResult( provider, Url.Action( "ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl } ) );
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode( string returnUrl, bool rememberMe ) {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if ( userId == null ) {
                return View( "Error" );
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View( new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe } );
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode( SendCodeViewModel model ) {
            if ( !ModelState.IsValid ) {
                return View();
            }

            // Generate the token and send it
            if ( !await SignInManager.SendTwoFactorCodeAsync( model.SelectedProvider ) ) {
                return View( "Error" );
            }
            return RedirectToAction( "VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe } );
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback( string returnUrl ) {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if ( loginInfo == null ) {
                return RedirectToAction( "Login" );
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch ( result ) {
                case SignInStatus.Success:
                    return RedirectToLocal( returnUrl );

                case SignInStatus.LockedOut:
                    return View( "Lockout" );

                case SignInStatus.RequiresVerification:
                    return RedirectToAction( "SendCode", new { ReturnUrl = returnUrl, RememberMe = false } );

                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View( "ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email } );
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation( ExternalLoginConfirmationViewModel model, string returnUrl ) {
            if ( User.Identity.IsAuthenticated ) {
                return RedirectToAction( "Index", "Manage" );
            }

            if ( ModelState.IsValid ) {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if ( info == null ) {
                    return View( "ExternalLoginFailure" );
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if ( result.Succeeded ) {
                    result = await UserManager.AddLoginAsync( user.Id, info.Login );
                    if ( result.Succeeded ) {
                        await SignInManager.SignInAsync( user, isPersistent: false, rememberBrowser: false );
                        return RedirectToLocal( returnUrl );
                    }
                }
                AddErrors( result );
            }

            ViewBag.ReturnUrl = returnUrl;
            return View( model );
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff() {
            AuthenticationManager.SignOut( DefaultAuthenticationTypes.ApplicationCookie );
            return RedirectToAction( "Index", "Home" );
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure() {
            return View();
        }

        #region info fetching

        public ActionResult GetFullNameStandard() {
            var userid = User.Identity.GetUserId();
            var user = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(userid);
            return Content( user.FullNameStandard );
        }

        public ActionResult GetAvatarUrl() {
            var userid = User.Identity.GetUserId();
            var user = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(userid);

            var path = user.AvatarImagePath;
            if(string.IsNullOrEmpty(path)) { path = "/img/avatars/user.jpeg"; }

            return Content( path );
        }

        public ActionResult GetRoleBadges() {
            var userid = User.Identity.GetUserId();
            var user = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(userid);
            var rolesHelper = new PermissionsHelper();
            return Content( user.GetRoleBadges().ToHtmlString() );
        }

        #endregion info fetching

        protected override void Dispose( bool disposing ) {
            if ( disposing ) {
                if ( _userManager != null ) {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if ( _signInManager != null ) {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose( disposing );
        }

        #region Helpers

        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager {
            get {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors( IdentityResult result ) {
            foreach ( var error in result.Errors ) {
                ModelState.AddModelError( "", error );
            }
        }

        private ActionResult RedirectToLocal( string returnUrl ) {
            if ( Url.IsLocalUrl( returnUrl ) ) {
                return Redirect( returnUrl );
            }
            return RedirectToAction( "Index", "Home" );
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult( string provider, string redirectUri )
                : this( provider, redirectUri, null ) {
            }

            public ChallengeResult( string provider, string redirectUri, string userId ) {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult( ControllerContext context ) {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if ( UserId != null ) {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge( properties, LoginProvider );
            }
        }

        #endregion Helpers
    }
}