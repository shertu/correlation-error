using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
                }).AddGoogleOpenIdConnect(options => {
                    options.ClientId = _configuration["Authentication:Google:ClientId"];
                    options.ClientSecret = _configuration["Authentication:Google:ClientSecret"];
                });
            #endregion

            #region Authorization
            services.AddAuthorization();
            #endregion

            services.AddControllers();
        }

        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                _ = endpoints.MapControllers();
            });
        }
    }
}
