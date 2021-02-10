using System.Threading.Tasks;
using Zukte.Message.ApplicationUser;

namespace Zukte.Utilities.Account {
	public interface IAccountCreationService {
		/// <summary>
		/// Adds an account to the database and web application.
		/// </summary>
		/// <param name="applicationUser">The application user to be created.</param>
		Task<ApplicationUser> PostApplicationUser(ApplicationUser applicationUser);
	}
}
