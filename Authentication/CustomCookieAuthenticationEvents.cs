using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System;
using System.Threading.Tasks;
using Zukte.Utilities;
using Zukte.Utilities.Account;

namespace Zukte.Authentication {
	// Same as base class with some additional generalizations
	// https://github.com/aspnet/Security/blob/master/src/Microsoft.AspNetCore.Authentication.Cookies/Events/CookieAuthenticationEvents.cs
	public class CustomCookieAuthenticationEvents : CookieAuthenticationEvents {
		public IAccountCreationService accountCreationService;

		public CustomCookieAuthenticationEvents(IAccountCreationService accountCreationService) {
			this.accountCreationService = accountCreationService;
		}

		public override Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context) {
			context.Response.Headers["Location"] = context.RedirectUri;
			context.Response.StatusCode = 403;
			return Task.CompletedTask;
		}

		public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context) {
			context.Response.Headers["Location"] = context.RedirectUri;
			context.Response.StatusCode = 401;
			return Task.CompletedTask;
		}

		public override Task RedirectToLogout(RedirectContext<CookieAuthenticationOptions> context) {
			context.Response.Headers["Location"] = context.RedirectUri;
			return Task.CompletedTask;
		}

		public override Task RedirectToReturnUrl(RedirectContext<CookieAuthenticationOptions> context) {
			context.Response.Headers["Location"] = context.RedirectUri;
			return Task.CompletedTask;
		}

		/// <summary>
		/// This variation on the base method will attempt to
		/// create an account in the system after sign in has completed.
		/// </summary>
		public override Task SignedIn(CookieSignedInContext context) {
			// create an account in the system if possible
			if (accountCreationService != null) {
				var principal = context.Principal ?? throw new ArgumentNullException();
				var applicationUser = principal.CreateApplicationUser();
				var createAccountTask = accountCreationService.PostApplicationUser(applicationUser);

				// https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/exception-handling-task-parallel-library
				try {
					createAccountTask.Wait();
				} catch (AggregateException ae) {
					// Call the Handle method to handle the custom exception,
					// otherwise rethrow the exception.
					ae.Handle(ex => {
						if (ex is PostApplicationUserConflictException) {
							Console.WriteLine(ex.Message);
							return true;
						}

						return false;
					});
				}
			}

			return base.SignedIn(context);
		}

		public override Task SigningIn(CookieSigningInContext context) {
			return base.SigningIn(context);
		}

		public override Task SigningOut(CookieSigningOutContext context) {
			return base.SigningOut(context);
		}

		public override Task ValidatePrincipal(CookieValidatePrincipalContext context) {
			return base.ValidatePrincipal(context);
		}
	}
}
