using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Zukte.Database;
using Zukte.Message.ApplicationUser;

namespace Zukte.Utilities.Account {
	/// <inheritdoc/>
	public class AccountCreationService : IAccountCreationService {
		private readonly ApplicationDbContext databaseService;

		public AccountCreationService(ApplicationDbContext databaseService) {
			this.databaseService = databaseService;
		}

		public async Task<ApplicationUser> PostApplicationUser(ApplicationUser applicationUser) {
			if (databaseService.ApplicationUsers == null)
				throw new ArgumentNullException(nameof(databaseService.ApplicationUsers));

			databaseService.ApplicationUsers.Add(applicationUser);
			try {
				await databaseService.SaveChangesAsync();
			} catch (DbUpdateException) {
				if (ApplicationUserExists(applicationUser.Id)) {
					throw new PostApplicationUserConflictException(
						$"an application user with id, {applicationUser.Id}, already exists");
				} else {
					throw;
				}
			}

			return applicationUser;
		}

		/// <summary>
		/// Checks if an application user exists in the system.
		/// </summary>
		/// <param name="id">The id of the application user to check for.</param>
		private bool ApplicationUserExists(string id) {
			if (databaseService.ApplicationUsers == null)
				throw new ArgumentNullException(nameof(databaseService.ApplicationUsers));

			return databaseService.ApplicationUsers.Any(e => e.Id == id);
		}
	}
}
