using DevExtreme.AspNet.Data;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayValueManualSln.Application;
using PayValueManualSln.Application.DTOs.Account;
using PayValueManualSln.Application.DTOs.Tutorial;
using PayValueManualSln.Application.Enums;
using PayValueManualSln.Application.Helpers;
using PayValueManualSln.Application.Interfaces;
using PayValueManualSln.Application.Wrappers;
using PayValueManualSln.Domain.Entities.Settings;

namespace PayValueManualSln.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IAuthenticatedUserService _authenticatedUserService;
      

        public AccountController(IAccountService accountService, IAuthenticatedUserService authenticatedUserService,List<UserCredential> users,JwtService jwtService)
        {
            _accountService = accountService;
            _authenticatedUserService = authenticatedUserService;
           
        }
        [HttpPost("login")]
        public async Task<IActionResult> AuthenticateAsync(AuthenticationRequest request)
        {
            return Ok(await _accountService.AuthenticateAsync(request, GenerateIPAddress()));
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(RegisterRequest request)
        {
            return Ok(await _accountService.RegisterAsync(request));
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmailAsync([FromBody] ConfirmEmailDTO confirmEmailDTO)
        {
            var origin = Request.Headers["origin"];
            var confirm = await _accountService.ConfirmEmailAsync(confirmEmailDTO.UserId, confirmEmailDTO.Code, confirmEmailDTO.Password);
            if (confirm.StatusCode == 200)
                return Ok(confirm);
            else if (confirm.StatusCode == 409)
                return StatusCode(409, confirm);
            else
                return BadRequest(confirm);
        }

        //[HttpGet("confirm-email")]
        //public async Task<IActionResult> ConfirmEmailAsync([FromQuery] string userId, [FromQuery] string code, [FromQuery] string password)
        //{
        //    var origin = Request.Headers["origin"];
        //    return Ok(await _accountService.ConfirmEmailAsync(userId, code, password));
        //}

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest model)
        {
            return Ok(await _accountService.ForgotPassword(model));
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest model)
        {
            return Ok(await _accountService.ResetPassword(model));
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest model)
        {
            var response = await _accountService.ChangePassword(model);
            if (response.Succeeded)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpGet("logout")]
        public async Task<IActionResult> LogoutAsync()
        {
            return Ok(await _accountService.Logout());
        }


        //[Authorize("GlobalAdmin")]
        [HttpGet("get-users-globaladmin")]
        public async Task<IActionResult> GetUsersGlobalAdmin(DataSourceLoadOptions loadOptions)
        {
            var response = new List<UserDTO>();

            response = await _accountService.GetUsers();
            response = response.Where(x => x.IsActive == true && x.IsDeleted != true).ToList();
            if (response != null)
            {
                loadOptions.PrimaryKey = new[] { $"userId" };
                return Ok(DataSourceLoader.Load(response.OrderBy(x => x.FirstName), loadOptions));
            }
            return Ok(DataSourceLoader.Load(new List<UserDTO>().OrderBy(x => x.FirstName), loadOptions));
        }

        //[Authorize("GlobalAdmin")]
        [HttpGet("GetUserForAgencyAdmin")]
        public async Task<IActionResult> GetUserForAgencyAdmin(DataSourceLoadOptions loadOptions, [FromQuery] string AgencyCode)
        {
            var response = new List<UserDTO>();

            response = await _accountService.GetUserForAgencyAdmin(AgencyCode);

            if (response != null)
            {
                loadOptions.PrimaryKey = new[] { $"userId" };
                return Ok(DataSourceLoader.Load(response.OrderBy(x => x.FirstName), loadOptions));
            }
            return Ok(DataSourceLoader.Load(new List<UserDTO>().OrderBy(x => x.FirstName), loadOptions));
        }

        [HttpGet("GetUserForAgencyAdmin-Dropdown")]
        public async Task<IActionResult> GetUserForAgencyAdmin()
        {
            string AgencyCode = _authenticatedUserService.AgencyCode;

            var users = await _accountService.GetUserForAgencyAdmin(AgencyCode);
            var response = ApplicationConstants.SuccessMessage<List<UserDTO>>(users, "Success");
            return Ok(response);
        }

        [HttpGet("get-users")]
        [Authorize]
        public async Task<IActionResult> GetUsers(DataSourceLoadOptions loadOptions)
        {
            var hasHeaderOfRecentlyCreatedNumber = HttpContext.Request.Headers["RecentlyCreated"];
            var response = new List<UserDTO>();
            if ((!string.IsNullOrEmpty(hasHeaderOfRecentlyCreatedNumber)) && hasHeaderOfRecentlyCreatedNumber == "5")
            {
                response = await _accountService.GetUsers(_authenticatedUserService.UserId, int.Parse(hasHeaderOfRecentlyCreatedNumber));
            }
            else
            {
                response = await _accountService.GetUsers(_authenticatedUserService.UserId);
            }

            if (response != null)
            {
                loadOptions.PrimaryKey = new[] { $"userId" };
                return Ok(DataSourceLoader.Load(response.OrderBy(x => x.FirstName), loadOptions));
            }
            return Ok(DataSourceLoader.Load(new List<UserDTO>().OrderBy(x => x.FirstName), loadOptions));
        }

        [HttpPost("DeleteUser/{userId}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            return Ok(await _accountService.RemoveUser(userId));
        }

        [HttpGet("GetUserCount/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetUserCount(string userId)
        {
            return Ok(await _accountService.GetUserCount(userId));
        }

        [HttpGet("get-roles")]
        [Authorize]
        public async Task<IActionResult> GetRoles()
        {
            //var header = Request.Headers["IsGeneral"].ToString();
            //var isGeneral = bool.Parse(header);

            return Ok(await _accountService.GetRoles());
        }

        [Authorize]
        [HttpGet("get-userbyId/{UserId}")]
        public IActionResult GetUserById([FromRoute] string UserId)
        {
            return Ok(_accountService.GetUserById(UserId));
        }

        [Authorize]
        [HttpGet("ResendEmail/{email}")]
        public async Task<IActionResult> ResendEmail(string email)
        {
            //var webUrl = HttpContext.Request.Headers["webUrl"].ToString();
            var response = await _accountService.ResendEmail(email);
            if (response.StatusCode == 200)
                return Ok(response);
            else if (response.StatusCode == 409)
                return StatusCode(409, response);
            else
                return BadRequest(response);
        }
        private string GenerateIPAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }

        [HttpPost]
        [Route("Enable/{userId}")]
        [ProducesResponseType(typeof(Response<string>), 200)]
        public async Task<IActionResult> Enable(string userId)
        {
            var response = await _accountService.Enable(userId);
            if (response > 0)
                return Ok(ApplicationConstants.SuccessMessage("User was enabled successfully"));
            return Ok(ApplicationConstants.FailureMessage("Failure enabled user"));
        }

        [HttpPost]
        [Route("Disable/{userId}")]
        [ProducesResponseType(typeof(Response<string>), 200)]
        public async Task<IActionResult> Disable(string userId)
        {
            var response = await _accountService.Disable(userId);
            if (response > 0)
                return Ok(ApplicationConstants.SuccessMessage("User was disabled successfully"));
            return Ok(ApplicationConstants.FailureMessage("Failure disabled user"));
        }

        [HttpPost]
        [Route("UpdateUser")]
        [ProducesResponseType(typeof(Response<string>), 200)]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUser model)
        {
            await _accountService.UpdateUser(model);
            return Ok(ApplicationConstants.SuccessMessage("User was updated successfully"));
        }



        [HttpGet]
        [Route("GetUserByRole")]
        public async Task<IActionResult> GetUserByRole(/*DataSourceLoadOptions loadOptions,*/ [FromQuery] Roles role)
        {
            var result = await _accountService.GetUsersByRole(role);
            return Ok(result);
            //if (result == null)
            //{
            //    return Ok(DataSourceLoader.Load(result, loadOptions));
            //}
            ////loadOptions.PrimaryKey = new[] { "UserId" };
            //return Ok(DataSourceLoader.Load(result, loadOptions));
        }

        [HttpPost]
        [Route("MapInitiatorToValidator")]
        [ProducesResponseType(typeof(Response<string>), 200)]
        public async Task<IActionResult> MapInitiatorToValidator([FromBody] MapInitiatorRequestDto model)
        {
            var response = await _accountService.MapInitiatorToValidator(model);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost]
        [Route("GeneratePassword")]
        [ProducesResponseType(typeof(Response<string>), 200)]
        public async Task<IActionResult> GeneratePassword([FromBody] AuthenticationRequest request)
        {
            var response = await _accountService.ConfirmEmailAsync(request.Email, null, request.Password, true);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost]
        [Route("ChangePasswordByPass")]
        [ProducesResponseType(typeof(Response<string>), 200)]
        public async Task<IActionResult> GeneratePassword([FromBody] ChangePasswordRequestByPass request)
        {
            var response = await _accountService.ChangePassword(request);
            return StatusCode(response.StatusCode, response);
        }
    }
}
