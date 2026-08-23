namespace FinancialPortfolio.Api.Controllers.Base
{
    using FinancialPortfolio.Models.Abstractions.ICurrentUser;
    using FinancialPortfolio.Models.Common.Response;
    using FinancialPortfolio.Models.Common.Utilities;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Base controller for all API controllers
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    public abstract class BaseApiController : ControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseApiController"/> class.
        /// </summary>
        /// <param name="currentUser">The currentUser<see cref="ICurrentUserService"/></param>
        protected BaseApiController(ICurrentUserService currentUser)
        {
            CurrentUser = Guard.AgainstNull(currentUser, nameof(currentUser));
        }

        /// <summary>
        /// Gets the CurrentUser
        /// </summary>
        protected ICurrentUserService CurrentUser { get; }

        /// <summary>
        /// Returns HTTP 200 with the supplied response
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response">The response<see cref="ApiResponse{T}"/></param>
        /// <returns>The <see cref="OkObjectResult"/></returns>
        protected IActionResult Success<T>(ApiResponse<T> response)
        {
            return Ok(response);
        }

        /// <summary>
        /// Returns HTTP 201 with the supplied response
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="actionName">The actionName<see cref="string"/></param>
        /// <param name="routeValues">The routeValues<see cref="object?"/></param>
        /// <param name="response">The response<see cref="ApiResponse{T}"/></param>
        /// <returns>The <see cref="CreatedAtActionResult"/></returns>
        protected IActionResult CreatedResponse<T>(string actionName, object? routeValues, ApiResponse<T> response)
        {
            return CreatedAtAction(actionName, routeValues, response);
        }

        /// <summary>
        /// Returns HTTP 204
        /// </summary>
        /// <returns>The <see cref="NoContentResult"/></returns>
        protected IActionResult NoContentResponse()
        {
            return NoContent();
        }
    }
}
