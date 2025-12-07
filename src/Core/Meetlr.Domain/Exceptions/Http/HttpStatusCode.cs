namespace Meetlr.Domain.Exceptions.Http;

/// <summary>
/// HTTP status codes for exceptions
/// </summary>
public enum HttpStatusCode
{
    /// <summary>
    /// Request succeeded (200)
    /// </summary>
    OK = 200,

    /// <summary>
    /// Bad request - client error (400)
    /// </summary>
    BadRequest = 400,

    /// <summary>
    /// Unauthorized - authentication required (401)
    /// </summary>
    Unauthorized = 401,

    /// <summary>
    /// Forbidden - insufficient permissions (403)
    /// </summary>
    Forbidden = 403,

    /// <summary>
    /// Resource not found (404)
    /// </summary>
    NotFound = 404,

    /// <summary>
    /// Conflict - resource already exists or state conflict (409)
    /// </summary>
    Conflict = 409,

    /// <summary>
    /// Unprocessable Entity - validation error (422)
    /// </summary>
    UnprocessableEntity = 422,

    /// <summary>
    /// Internal server error (500)
    /// </summary>
    InternalServerError = 500
}
