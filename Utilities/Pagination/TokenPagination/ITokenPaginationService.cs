using System.Linq;

namespace Zukte.Utilities.Pagination.TokenPagination {
	/// <summary>
	/// Note that pagination seek fields are required to be unique, ordinal, and immutable.
	/// </summary>
	/// <typeparam name="T">The type of the page tokens.</typeparam>
	public interface ITokenPaginationService<T> : IPaginationService<T> {
		/// <summary>
		/// Decrypts a page token.
		/// </summary>
		/// <param name="ciphertext">The encrypted page token.</param>
		T? DecryptPageToken(string? ciphertext);

		/// <summary>
		/// Encrypts a page token.
		/// </summary>
		/// <param name="value">The decrypted page token.</param>
		string? EncryptPageToken(T? value);

		// currently support ascend order only

		/// <summary>
		/// Applies a SEEK constraint on a SQL query.
		/// </summary>
		/// <param name="pageToken">A decrypted page token.</param>
		IQueryable<T> ApplyPageToken(IQueryable<T> query, T? pageToken);

		/// <summary>
		/// Generates a page token to be used for seek operations.
		/// </summary>
		/// <param name="query">The query used to fetch the items.</param>
		/// <param name="items">The items fetched by the query.</param>
		/// <param name="prevToken">Generate a previous page token?</param>
		/// <returns>A decrypted page token.</returns>
		T? GeneratePageToken(IQueryable<T> query, T[] items, bool prevToken);
	}
}