namespace Zukte.Utilities.Account {
	/// <summary>
	/// Represents errors that occur when creating an account which already exists.
	/// </summary>
	[System.Serializable]
	public class PostApplicationUserConflictException : System.Exception {
		public PostApplicationUserConflictException() { }
		public PostApplicationUserConflictException(string message) : base(message) { }
		public PostApplicationUserConflictException(string message, System.Exception inner) : base(message, inner) { }
		protected PostApplicationUserConflictException(
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
