using System.Collections.Generic;
using System.Linq;
using Google.Apis.Auth.AspNetCore3;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Zukte.Authentication;
using Zukte.Authorization.Handlers;
using Zukte.Database;
using Zukte.Middleware;
using Zukte.Utilities.Account;

namespace Zukte {
	public class Startup {
		/// <summary>
		/// The name of the CORS policy used during development to allow
		/// webpack-dev-server origins to connect to the .NET server.
		/// </summary>
		private const string CORS_POLICY_NAME_DEVLOPMENT = "AllowAllOrigins";

		/// <summary>
		/// The appliction configuration.
		/// </summary>
		private readonly IConfiguration _configuration;

		public Startup(IConfiguration configuration) {
			_configuration = configuration;
		}

		public void ConfigureServices(IServiceCollection services) {
			// #region Azure Blob Storage
			// // Account -> Container -> Blob
			// string? azureStorageConnectionString = _configuration.GetConnectionString("AzureStorageConnection");

			// if (!string.IsNullOrEmpty(azureStorageConnectionString)) {
			// 	BlobServiceClient blobServiceClient = new BlobServiceClient(azureStorageConnectionString);
			// 	services.AddSingleton(blobServiceClient);
			// }
			// #endregion

			#region database
			string? databaseConnectionString = _configuration.GetConnectionString("DatabaseConnection");

			if (!string.IsNullOrEmpty(databaseConnectionString)) {
				services.AddDbContext<ApplicationDbContext>(options =>
				  options.UseMySQL(databaseConnectionString));
			}
			#endregion

			#region Authentication
			// This configures Google.Apis.Auth.AspNetCore3 for use in this app.
			services
				.AddAuthentication(options => {
					// // This forces challenge results to be handled by Google OpenID Handler, so there's no
					// // need to add an AccountController that emits challenges for Login.
					// o.DefaultChallengeScheme = GoogleOpenIdConnectDefaults.AuthenticationScheme;

					// This forces forbid results to be handled by Google OpenID Handler, which checks if
					// extra scopes are required and does automatic incremental auth.
					// options.DefaultForbidScheme = GoogleOpenIdConnectDefaults.AuthenticationScheme;

					// Default scheme that will handle everything else.
					// Once a user is authenticated, the OAuth2 token info is stored in cookies.
					// After a user is signed in, auto create an account
					options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
				}).AddCookie(options => {
					options.EventsType = typeof(CustomCookieAuthenticationEvents);
					options.LoginPath = "/api/Account/Login";
					options.LogoutPath = "/api/Account/Logout";
					// options.Cookie.SecurePolicy = CookieSecurePolicy.None;
					// options.Cookie.HttpOnly = false;
					// options.Cookie.SameSite = SameSiteMode.None;
				}).AddGoogleOpenIdConnect(options => {
					options.ClientId = _configuration["Authentication:Google:ClientId"];
					options.ClientSecret = _configuration["Authentication:Google:ClientSecret"];
				});
			#endregion

			services.AddScoped<IAccountCreationService, AccountCreationService>();
			services.AddScoped<CustomCookieAuthenticationEvents>();

			#region Authorization
			services.AddAuthorization();
			services.AddSingleton<IAuthorizationHandler, SuperuserAuthorizationHandler>();
			services.AddSingleton<IAuthorizationHandler, MineApplicationUserAuthorizationHandler>();
			#endregion

			#region CORS
			services.AddCors(options => {
				options.AddPolicy(name: CORS_POLICY_NAME_DEVLOPMENT, builder => {
					builder.AllowAnyOrigin();
				});
			});
			#endregion

			services.AddControllers();

			#region OpenAPI
			services.AddSwaggerDocument();
			#endregion
		}

		public void Configure(
			IApplicationBuilder app,
			IWebHostEnvironment env,
			ApplicationDbContext dbContext) {
			if (env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
				//LogConfigurationRecursive(logger, _configuration.GetChildren());
				_ = SeedDatabaseMiddleware.InvokeAsync(dbContext);
			}

			#region UseRewriter
			RewriteOptions rewriteOptions = new RewriteOptions()
				//   .AddRewrite("privacy-policy", "index.html", true)
				.AddRewrite("authentication-demo", "/", true)
			  //   .AddRewrite("image-share-demo", SPA_ENTRY_FILENAME, true)
			  //   .AddRewrite("map-demo", SPA_ENTRY_FILENAME, true)
			  ;

			app.UseRewriter(rewriteOptions);
			#endregion

			app.UseDefaultFiles();

			#region UseStaticFiles
			StaticFileOptions staticFileOptions = new StaticFileOptions {
				OnPrepareResponse = staticFileResponseContext => {
					staticFileResponseContext.Context.Response.Headers.Add("Cache-Control", $"public, max-age={System.TimeSpan.FromDays(7).Seconds}");
				}
			};
			app.UseStaticFiles(staticFileOptions);
			#endregion

			app.UseRouting();

			if (env.IsDevelopment()) {
				app.UseCors(CORS_POLICY_NAME_DEVLOPMENT);
			}

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints => {
				_ = endpoints.MapControllers();
			});

			#region OpenAPI
			_ = app.UseOpenApi();
			_ = app.UseSwaggerUi3();
			#endregion
		}

		/// <summary>
		/// Log the values in the application configuration.
		/// </summary>
		private void LogConfigurationRecursive(ILogger logger, IEnumerable<IConfigurationSection> sections) {
			foreach (var section in sections) {
				var children = section.GetChildren();

				if (children.Count() == 0) {
					logger.LogInformation($"{section.Path} {section.Value}");
				}

				LogConfigurationRecursive(logger, children);
			}
		}
	}
}
