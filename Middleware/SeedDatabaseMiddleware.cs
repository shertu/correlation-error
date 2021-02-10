using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Zukte.Middleware {
	/// <summary>
	/// Middleware to seed a <see cref="DbContext"/> database for development and testing.
	/// </summary>
	public class SeedDatabaseMiddleware {
		public static async Task InvokeAsync<T>(T dbContext) where T : DbContext {
			bool created = dbContext.Database.EnsureCreated();

			foreach (var item in GetDatabaseSeeders<T>()) {
				IDatabaseSeeder<T>? instance = Activator.CreateInstance(item) as IDatabaseSeeder<T>;

				if (instance == null) {
					continue;
				}

				await instance.InvokeSeed(dbContext);
			}

			dbContext.SaveChanges();
		}

		private static IEnumerable<Type> GetDatabaseSeeders<T>() where T : DbContext {
			var type = typeof(IDatabaseSeeder<T>);
			var types = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(s => s.GetTypes())
				.Where(p => type.IsAssignableFrom(p) && p.IsClass);

			return types;
		}
	}
}