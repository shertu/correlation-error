using System.Linq;

namespace Zukte.Utilities.Pagination {

	public interface IPaginationService<T> {
		/// <value>The maximum number of results in a single page.</value>
		int MaxResultsMax { get; }

		/// <value>The default number of results in a single page.</value>
		int MaxResultsDefault { get; }

		/// <summary>
		/// Applies a LIMIT constraint on a SQL query.
		/// </summary>
		IQueryable<T> ApplyMaxResults(IQueryable<T> query);

		/// <summary>
		/// Applies a LIMIT constraint on a SQL query.
		/// </summary>
		/// <param name="top">The requested LIMIT constraint value.</param>
		IQueryable<T> ApplyMaxResults(IQueryable<T> query, int top);

		/// <summary>
		/// Applies an ORDER BY transformation on a SQL query.
		/// </summary>
		IQueryable<T> ApplyOrderTransform(IQueryable<T> query);

		/// <summary>
		/// Applies an OFFSET constraint on a SQL query.
		/// </summary>
		/// <param name="skip">The requested OFFSET constraint value.</param>
		IQueryable<T> ApplySkipTransform(IQueryable<T> query, int skip);
	}
}