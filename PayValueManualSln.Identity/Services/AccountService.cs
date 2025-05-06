using AutoMapper;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using PayValueManualSln.Application.Interfaces;
using PayValueManualSln.Infrastructure.Identity.Models;
using PayValueManualSln.Infrastructure.Persistence.Contexts;
using PayValueManualSln.Shared.DapperServices;
using PayValueManualSln.Application.Enums;
using PayValueManualSln.Application.Constants;
using PayValueManualSln.Application.DTOs.Account;
using PayValueManualSln.Application.Exceptions;
using PayValueManualSln.Application.Helpers;
using PayValueManualSln.Application.Wrappers;
using PayValueManualSln.Domain.Entities.Setting;
using PayValueManualSln.Infrastructure.Identity.Contexts;
using PayValueV2.Infrastructure.Identity.Helpers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PayValueManualSln.Domain.Entities.Settings;
using PayValueManualSln.Application;
using PayValueManualSln.Application.DTOs.MenuSetup;

namespace PayValueManualSln.Infrastructure.Identity.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly ApplicationDbContext _context;
        private readonly IdentityContext _identitycontext;
        private readonly IMenuRepository _menuRepository;
        private readonly JWTSettings _jwtSettings;
        private readonly IDateTimeService _dateTimeService;
        private readonly HttpContext _httpContext;
        private readonly string _baseurl;
        ILogger<string> _logger;
        private readonly IDapper _dapper;
        private readonly Appsettings _appSettings;
        private readonly IMapper _mapper;

        public AccountService(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<JWTSettings> jwtSettings,
            IDateTimeService dateTimeService,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService,
            IAuthenticatedUserService authenticatedUserService,
            ApplicationDbContext context, IMenuRepository menuRepository, IHttpContextAccessor httpContext,
            IdentityContext identitycontext,
            ILogger<string> logger, IMapper mapper, IDapper dapper, IOptions<Appsettings> appsettings)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtSettings = jwtSettings.Value;
            _dateTimeService = dateTimeService;
            _signInManager = signInManager;
            this._emailService = emailService;
            _authenticatedUserService = authenticatedUserService;
            _context = context;
            _menuRepository = menuRepository;
            _httpContext = httpContext.HttpContext;
            //_baseurl = _httpContext.Request.Headers["Referer"];
            _identitycontext = identitycontext;
            _logger = logger;
            _mapper = mapper;
            _dapper = dapper;
            _appSettings = appsettings.Value;
        }
        public async Task<Response<AuthenticationResponse>> AuthenticateAsync(AuthenticationRequest request, string ipAddress)
        {
            var user = new ApplicationUser();
            var emailRegex = @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";

            bool isEmail = Regex.IsMatch(request.Email, emailRegex, RegexOptions.IgnoreCase);
            if (isEmail)
            {
                user = await _userManager.FindByEmailAsync(request.Email);
            }
            else
            {
                user = await _userManager.FindByNameAsync(request.Email);
            }
            if (user == null)
            {
                return new Response<AuthenticationResponse> { Data = null, Succeeded = false, Message = $"Invalid Credentials for '{request.Email}'.", ResponseCode = "-1", StatusCode = 401 };
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, request.Password, false, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                return new Response<AuthenticationResponse> { Data = null, Succeeded = false, Message = $"Invalid Credentials for '{request.Email}'.", ResponseCode = "-1", StatusCode = 401 };
            }
            if (user.IsActive == false)
            {
                return new Response<AuthenticationResponse> { Data = null, Succeeded = false, Message = $"Account for '{request.Email}' is inactive.", ResponseCode = "-1", StatusCode = 401 };
            }
            if (!user.EmailConfirmed)
            {
                return new Response<AuthenticationResponse> { Data = null, Succeeded = false, Message = $"Account for '{request.Email}' is inactive.", ResponseCode = "-1", StatusCode = 401 };
            }
            JwtSecurityToken jwtSecurityToken = await GenerateJWToken(user);
            AuthenticationResponse response = new AuthenticationResponse();
            var merchantInfo = await _context.MerchantConfig.FirstOrDefaultAsync(x => string.Equals(x.MerchantCode, user.MerchantCode));
            response.Id = user.Id;
            response.IsActive = user.IsActive;
            response.JWToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            response.MerchantCode = user.MerchantCode;
            response.Email = user.Email;
            response.UserName = user.UserName;
            response.FirstName = user.FirstName;
            response.AgencyCode = user.AgencyCode;
            response.AgencyName = user.AgencyName;
            response.LastName = user.LastName;
            response.ApprovalRankingId = user.ApprovalRankingId;
            response.IsManualAssessment = merchantInfo == null ? false : merchantInfo.IsManualAssessment;
            response.PhoneNumber = user.PhoneNumber;
            response.UserRolesId = await _identitycontext.UserRoles.Where(x => x.UserId == user.Id).Select(x => x.RoleId).FirstOrDefaultAsync();
            response.FullName = (user.FirstName != null ? user.FirstName : "") + " " + (user.LastName != null ? user.LastName : "");
            var rolesList = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            response.Roles = rolesList.ToList();
            response.IsVerified = user.EmailConfirmed;
            var refreshToken = GenerateRefreshToken(ipAddress);
            response.RefreshToken = refreshToken.Token;
            return ApplicationConstants.SuccessMessage<AuthenticationResponse>(response, $"Authenticated {user.UserName}");
        }

        public async Task<Response<string>> RegisterAsync(RegisterRequest request)
        {
            var userWithSameUserName = await _userManager.FindByEmailAsync(request.Email);
            if (userWithSameUserName != null)
            {
                return new Response<string> { Data = null, Message = $"Email '{request.Email}' is already taken.", ResponseCode = "-1", Succeeded = false, StatusCode = 400 };
            }

            if (_authenticatedUserService.RoleName == Enum.GetName(typeof(Roles), Roles.GlobalAdmin) && (request.AgencyCode == null || request.AgencyName == null))
            {
                return new Response<string> { Data = null, Message = $"Please supply AgencyCode and AgencyName to continue", ResponseCode = "-1", Succeeded = false, StatusCode = 400 };
            }
            else
            {
                var adminDetail = await _identitycontext.Users.Where(x => x.Email == _authenticatedUserService.Email).Select(x => new { x.AgencyCode, x.AgencyName }).FirstOrDefaultAsync();
                if (_authenticatedUserService.RoleName == Enum.GetName(typeof(Roles), Roles.AgencyAdmin))
                {
                    request.AgencyName = adminDetail.AgencyName;
                    request.AgencyCode = adminDetail.AgencyCode;
                }
            }

            var user = new ApplicationUser
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = (request.UserName == null || request.UserName == "") ? request.Email : request.UserName,
                EmailConfirmed = false,
                MerchantCode = request.MerchantCode,
                IsActive = false,
                PhoneNumber = request.PhoneNumber,
                CreatedBy = _authenticatedUserService.Email,
                DateCreated = DateTime.Now,
                ApprovalRankingId = request.ApprovalRankingId,
                AgencyCode = request.AgencyCode,
                AgencyName = request.AgencyName,
                IsDeleted = false
            };
            var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);
            if (userWithSameEmail == null)
            {
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    var role = new IdentityRole();


                    role = await _roleManager.FindByIdAsync(request.RoleId);

                    await _userManager.AddToRoleAsync(user, role.Name);

                    var verificationUri = await SendVerificationEmail(user, _appSettings.ApplicationBaseUrl);

                    //TODO: Attach Email Service here and configure it via appsettings
                    var emailTemplate = new EmailHelper();
                    var msgBody = EmailHelper.ConfirmEmailTemplate(user.Email, verificationUri);

                    try
                    {
                        //try to send email
                        _emailService.SendEmail(user.Email, user.LastName, "Account Activation", msgBody);
                    }
                    catch (Exception ex)
                    {
                        // failed sending email

                    }
                    return new Response<string> { Data = "User created", Message = $"User Registered. Please check your mail to set password and confirm your account", ResponseCode = "00", Succeeded = true, StatusCode = 200 };
                }
                else
                {
                    return new Response<string> { Data = null, Message = result.Errors.FirstOrDefault().Description, ResponseCode = "-2", Succeeded = false, StatusCode = 400 };
                }
            }
            else
            {
                return new Response<string> { Data = null, Message = $"Email {request.Email} is already registered.", ResponseCode = "-1", Succeeded = false, StatusCode = 400 };
            }
        }

        private async Task<JwtSecurityToken> GenerateJWToken(ApplicationUser user)
        {
            try
            {

                var userClaims = await _userManager.GetClaimsAsync(user);
                var roles = await _userManager.GetRolesAsync(user);
                var userrole = await (from u in _identitycontext.UserRoles
                                      join r in _identitycontext.Roles on u.RoleId equals r.Id
                                      where u.UserId == user.Id
                                      select new
                                      {
                                          u.UserId,
                                          u.RoleId,
                                          r.Name
                                      }).FirstOrDefaultAsync();//User is assumed to have one role for each account

                var roleClaims = new List<Claim>();

                for (int i = 0; i < roles.Count; i++)
                {
                    roleClaims.Add(new Claim("roles", roles[i]));
                }

                string ipAddress = IpHelper.GetIpAddress();
                var agency = new Agency();
                if (user.AgencyCode != null)
                {
                    agency = await _context.Agency.FirstOrDefaultAsync(x => x.Code == user.AgencyCode);
                }
                var claims = new[]
                {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id),
                new Claim("ip", ipAddress),
                new Claim("useremail", user.Email),
                new Claim("username", user.UserName),
                new Claim("roleid", userrole.RoleId),
                new Claim("rolename", userrole.Name),
                new Claim("name", ($"{user.FirstName} {user.LastName}")),
                new Claim("rankid", user.ApprovalRankingId.ToString() == null ? "" : user.ApprovalRankingId.ToString()),
                new Claim("merchantcode", user.MerchantCode == null ? "" : user.MerchantCode),
                new Claim("AgencyCode", user.AgencyCode == null ? "" : user.AgencyCode),
                new Claim("AgencyName",  agency.Name == null ? "" : agency.Name),
            }
                .Union(userClaims)
                .Union(roleClaims);

                var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
                var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

                var jwtSecurityToken = new JwtSecurityToken(
                    issuer: _jwtSettings.Issuer,
                    audience: _jwtSettings.Audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                    signingCredentials: signingCredentials);
                return jwtSecurityToken;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private string RandomTokenString()
        {
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[40];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            // convert random bytes to hex string
            return BitConverter.ToString(randomBytes).Replace("-", "");
        }

        public async Task<Response<string>> ConfirmEmailAsync(string userId, string code, string password, bool bypass = false)
        {
            try
            {
                var emailRegex = @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";

                bool isEmail = Regex.IsMatch(userId, emailRegex, RegexOptions.IgnoreCase);
                var user = new ApplicationUser();
                if (isEmail)
                {
                    user = await _userManager.FindByEmailAsync(userId);
                }
                else
                {
                    user = await _userManager.FindByIdAsync(userId);
                }
                //if(user.PasswordHash != null && user.IsActive == true && user.EmailConfirmed == true)
                //{
                //    return ApplicationConstants.FailureMessage($"This {user.Email} has been Confirmed previously . You can now Login to your account");
                //}
                if (bypass)
                {
                    code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                }
                else
                {
                    code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
                }

                var result = await _userManager.ConfirmEmailAsync(user, code);
                if (result.Succeeded)
                {
                    var passwordudpate = await _userManager.AddPasswordAsync(user, password);

                    List<string> error = new List<string>();
                    foreach (var item in passwordudpate.Errors)
                    {
                        error.Add(item.Description);
                    }
                    if (error.Count > 0)
                    {
                        var response = ApplicationConstants.FailureMessage($"{JsonConvert.SerializeObject(error)}");
                        response.Errors = error;
                        return response;
                    }
                    user.IsActive = true;
                    await _userManager.UpdateAsync(user);
                    return ApplicationConstants.SuccessMessage($"Account Confirmed for {user.Email}. You can now Login to your account");
                }
                else
                {
                    _logger.LogInformation($"Error confirming email for user {user.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "An error has occured");
                return ApplicationConstants.FailureMessage(ex.Message); ;
            }
            return ApplicationConstants.FailureMessage($"An error occured while confirming {userId}.");
            //var user = await _userManager.FindByIdAsync(userId);
            //code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            //var result = await _userManager.ConfirmEmailAsync(user, code);
            //if (result.Succeeded)
            //{
            //    user.IsActive = true;
            //    await _userManager.UpdateAsync(user);
            //    await _userManager.AddPasswordAsync(user, password);
            //    return new Response<string>(user.Id, message: $"Account Confirmed for {user.Email}. Please goto login page");
            //}
            //else
            //{
            //    throw new ApiException($"An error occured while confirming {user.Email}.");
            //}
        }

        private RefreshToken GenerateRefreshToken(string ipAddress)
        {
            return new RefreshToken
            {
                Token = RandomTokenString(),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };
        }

        public async Task<Response<string>> ForgotPassword(ForgotPasswordRequest model)
        {
            var account = await _userManager.FindByEmailAsync(model.Email);

            // always return ok response to prevent email enumeration
            if (account == null)
            {
                return new Response<string>("", message: $"This account does not exist on the system");
            }

            var resetLink = await SendVerificationEmailForgotPassword(account, _appSettings.ApplicationBaseUrl);

            var msgBody = EmailHelper.ResetPasswordMsg(account.Email, resetLink);

            try
            {
                //try to send email
                _emailService.SendEmail(account.Email, account.LastName, "Password Reset", msgBody);
            }
            catch (Exception ex)
            {
                // failed sending email

            }
            return new Response<string>(account.Id, message: $"Reset email sent");
        }

        private async Task<string> SendVerificationEmail(ApplicationUser user, string origin)
        {
            var generateCode = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(generateCode));
            var route = AssignedConstants.AddPassword;
            var _enpointUri = new Uri(string.Concat($"{origin}", route));
            var verificationUri = QueryHelpers.AddQueryString(_enpointUri.ToString(), "userId", user.Id);
            verificationUri = QueryHelpers.AddQueryString(verificationUri, "code", code);
            //Email Service Call Here
            return verificationUri;
        }


        private async Task<string> SendVerificationEmailForgotPassword(ApplicationUser user, string origin)
        {
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var route = AssignedConstants.AddPassword;
            var _enpointUri = new Uri(string.Concat($"{origin}", route));
            var verificationUri = QueryHelpers.AddQueryString(_enpointUri.ToString(), "userId", user.Id);
            verificationUri = QueryHelpers.AddQueryString(verificationUri, "code", code);
            verificationUri = QueryHelpers.AddQueryString(verificationUri, "email", user.Email);
            //Email Service Call Here
            return verificationUri;
        }

        public async Task<Response<string>> ResetPassword(ResetPasswordRequest model)
        {
            var account = await _userManager.FindByEmailAsync(model.Email);
            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
            var result = await _userManager.ResetPasswordAsync(account, code, model.Password);
            if (result.Succeeded)
            {
                return new Response<string> { Data = "Password Resetted.", Message = $"Password Resetted.", ResponseCode = "00", Succeeded = true, StatusCode = 200 };
            }
            else
            {
                return new Response<string> { Data = null, Message = $"Password rest failed.", ResponseCode = "-1", Succeeded = false, StatusCode = 400 };
            }
        }

        public async Task<Response<string>> Logout()
        {
            await _signInManager.SignOutAsync();
            return new Response<string> { Data = "User logged out Successfully", Message = "User logged out Successfully", Succeeded = true };
        }

        public async Task<List<UserDTO>> GetUsers(string loggedInUserId)
        {
            try
            {
                var userloggedIn = await _userManager.FindByIdAsync(loggedInUserId);
                //if (userloggedIn.IsGeneral)
                //{

                //}
                //var users = await _userManager.Users.Where(c => c.MerchantCode == userloggedIn.MerchantCode && c.IsDeleted == false && c.IsGeneral == userloggedIn.IsGeneral).ToListAsync();
                var users = await _userManager.Users.Where(c => c.MerchantCode == userloggedIn.MerchantCode && c.IsDeleted == false).ToListAsync();
                var userss = new List<ApplicationUser>();
                foreach (var user in users)
                {
                    //var rolename = Enum.GetName(typeof(Roles), Roles.GlobalAdmin);
                    //if ((await _userManager.IsInRoleAsync(user, rolename)))
                    //{

                    //}
                    //else
                    //{
                    userss.Add(user);
                    //}
                }
                var usersDto = userss.Where(c => c.Id != loggedInUserId).Select(c => new UserDTO
                {
                    Email = c.Email,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    UserId = c.Id,
                    IsActive = c.IsActive,
                    Status = c.IsActive == true ? "Active" : "In Active"
                }).ToList();
                foreach (var user in usersDto)
                {
                    var roles = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(user.UserId));
                    if (roles.Any())
                    {
                        var role = await _roleManager.FindByNameAsync(roles.ToList().FirstOrDefault());
                        user.RoleName = role.Name;
                        user.RoleId = role.Id;
                    }
                }
                return usersDto;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<UserDTO>> GetUsers(string loggedInUserId, int count)
        {
            var userloggedIn = await _userManager.FindByIdAsync(loggedInUserId);
            //if (userloggedIn.IsGeneral)
            //{

            ////}
            //var users = await _userManager.Users.Where(c => c.MerchantCode == userloggedIn.MerchantCode && c.IsDeleted == false && c.IsGeneral == userloggedIn.IsGeneral).OrderByDescending(c => c.Id).Take(count).ToListAsync();
            var users = await _userManager.Users.Where(c => c.MerchantCode == userloggedIn.MerchantCode && c.IsDeleted == false).OrderByDescending(c => c.DateCreated).Take(count).ToListAsync();
            var userss = new List<ApplicationUser>();
            foreach (var user in users)
            {
                //if ((await _userManager.IsInRoleAsync(user, "GlobalAdmin")))
                //{

                //}
                //else
                //{
                    userss.Add(user);
                //}
            }
            var usersDto = userss.Where(c => c.Id != loggedInUserId).Select(c => new UserDTO
            {
                Email = c.Email,
                FirstName = c.FirstName,
                LastName = c.LastName,
                UserId = c.Id,
                IsActive = c.IsActive,
                Status = c.IsActive == true ? "Active" : "In Active"
            }).ToList();
            foreach (var user in usersDto)
            {
                var roles = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(user.UserId));
                if (roles.Any())
                {
                    var role = await _roleManager.FindByNameAsync(roles.ToList().FirstOrDefault());
                    user.RoleName = role.Name;
                    user.RoleId = role.Id;
                }
            }
            return usersDto;

        }
        public async Task<Response<string>> RemoveUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new Response<string> { Data = "User not found", Message = "User not found", Succeeded = false };
            }
            user.IsDeleted = true;
            user.DeletedOn = DateTime.Now;
            user.DeletedBy = _authenticatedUserService.UserId;
            var response = await _userManager.UpdateAsync(user);
            if (response.Succeeded)
                return new Response<string> { Data = "User deleted successfully", Message = "User deleted successful", Succeeded = true };

            return new Response<string> { Data = "User not found", Message = "User not found", Succeeded = false };
        }

        public async Task<Response<List<RolesDTO>>> GetRoles()
        {
            var roles = new List<RolesDTO>();
            roles = await _roleManager.Roles.Where(c => c.Name != "GlobalAdmin").Select(c => new RolesDTO
            {
                RoleId = c.Id,
                RoleName = c.Name,
            }).ToListAsync();
            //if (isGeneral)
            //{
            //    roles = await _roleManager.Roles.Where(c => c.Name != "GlobalAdmin" && c.Name != "StoreAdmin" && c.Name != "Customer" && c.Name != "BranchAdmin").Select(c => new RolesDTO
            //    {
            //        RoleId = c.Id,
            //        RoleName = c.Name,
            //    }).ToListAsync();
            //}
            //else
            //{
            //    roles = await _roleManager.Roles.Where(c => c.Name != "GlobalAdmin" && c.Name != "Maker" && c.Name != "Approver").Select(c => new RolesDTO
            //    {
            //        RoleId = c.Id,
            //        RoleName = c.Name,
            //    }).ToListAsync();
            //}

            return new Response<List<RolesDTO>> { Data = roles, Message = "roles retrived successfully", Succeeded = true };
        }

        public async Task<int> Enable(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            user.IsActive = true;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                return 1;
            return 0;
        }

        public async Task<int> Disable(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            user.IsActive = false;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                return 1;
            return 0;
        }

        public async Task<Response<string>> ResendEmail(string email)
        {
            try
            {
                var user = _userManager.FindByEmailAsync(email).Result;
                if (user == null)
                {
                    return ApplicationConstants.NotFoundMessage("This email does not have an account with us");
                }
                var verificationUri = await SendVerificationEmail(user, _appSettings.ApplicationBaseUrl);

                //TODO: Attach Email Service here and configure it via appsettings
                var emailTemplate = new EmailHelper();
                var msgBody = EmailHelper.ConfirmEmailTemplate(user.FirstName, verificationUri);

                //try to send email
                _emailService.SendEmail(email, user.FirstName, "Account Activation", msgBody);
                return ApplicationConstants.SuccessMessage("Email sent successfully");
            }
            catch (Exception ex)
            {

                _logger.LogError(ex.Message, "An error has occured");
                return ApplicationConstants.FailureMessage(ex.Message);
            }

        }
        //private async Task<string> GetResetUrl(ApplicationUser user, string origin)
        //{
        //    var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        //    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        //    var route = "Auth/ResetPassword/";
        //    var _enpointUri = new Uri(string.Concat($"{origin}/", route));
        //    var verificationUri = QueryHelpers.AddQueryString(_enpointUri.ToString(), "email", user.Email);
        //    verificationUri = QueryHelpers.AddQueryString(verificationUri, "code", code);
        //    //Email Service Call Here
        //    return verificationUri;
        //}

        public async Task UpdateUser(UpdateUser updateUserDto)
        {
            try
            {
                var userToBeUpdated = await _userManager.FindByIdAsync(updateUserDto.UserId);
                userToBeUpdated.FirstName = updateUserDto.FirstName;
                userToBeUpdated.LastName = updateUserDto.LastName;
                userToBeUpdated.PhoneNumber = updateUserDto.PhoneNumber;
                userToBeUpdated.ApprovalRankingId = updateUserDto.ApprovalRankingId;
                userToBeUpdated.AgencyCode = updateUserDto.AgencyCode;
                userToBeUpdated.AgencyName = updateUserDto.AgencyName;
                var result = await _userManager.UpdateAsync(userToBeUpdated);

                //This bloc checks if the role of user changed.
                var role = await _roleManager.FindByIdAsync(updateUserDto.RoleId);
                if (!await _userManager.IsInRoleAsync(userToBeUpdated, role.Name))
                {
                    await UpdateUserRole(userToBeUpdated.Id, updateUserDto.RoleId);
                }

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private async Task UpdateUserRole(string userId, string roleId)
        {
            var newRole = await _roleManager.FindByIdAsync(roleId);
            var user = await _userManager.FindByIdAsync(userId);
            var userRoles = await _userManager.GetRolesAsync(user);
            //removes a user from all existing roles
            await _userManager.RemoveFromRolesAsync(user, userRoles);

            //adds the new role to the user.
            await _userManager.AddToRoleAsync(user, newRole.Name);

            //Delete all the page mapped to this user
            await DeletePagesAssignedToUser(userId);
        }

        private async Task DeletePagesAssignedToUser(string userId)
        {
            var pagesAssignedToUser = await _context.UsersRolePermission.Include(c => c.MenuSetup)
                .Where(c => c.UserId == userId)
                .Select(c => c.MenuSetup.MenuId)
                .ToListAsync();

            var result = await _menuRepository.DeleteMenuAssignedToUser(new MenuToUserRequest
            {
                Menus = pagesAssignedToUser,
                UserId = userId
            });
        }

        public async Task<int> GetUserCount(string loggedInUserId)
        {
            var userCount = await GetUsers(loggedInUserId);
            return userCount.Count();
        }

        public async Task<Response<string>> ChangePassword(ChangePasswordRequest model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(_authenticatedUserService.Email);
                var checkexistingpassword = await _userManager.CheckPasswordAsync(user, model.OldPassword);
                if (checkexistingpassword)
                {
                    var changepw = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                    if (changepw.Succeeded)
                    {
                        return new Response<string> { Data = null, Message = "password changed successfully", Succeeded = true };
                    }
                    else
                    {
                        List<string> errors = new List<string>();
                        foreach (var err in changepw.Errors)
                        {
                            errors.Add(err.Description);
                        }
                        var errResponse = string.Join(", ", errors);
                        return new Response<string> { Data = null, Message = errResponse, Succeeded = false };
                    }
                }
                else
                    return new Response<string> { Data = null, Message = "The old password is wrong", Succeeded = false };
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Response<string>> ChangePassword(ChangePasswordRequestByPass model)
        {
            try
            {
                var account = await _userManager.FindByEmailAsync(model.Email);
                var token = await _userManager.GeneratePasswordResetTokenAsync(account);
                token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
                var result = await _userManager.ResetPasswordAsync(account, code, model.NewPassword);
                if (result.Succeeded)
                {
                    return new Response<string> { Data = "Password Resetted.", Message = $"Password Resetted.", ResponseCode = "00", Succeeded = true, StatusCode = 200 };
                }
                else
                {
                    return new Response<string> { Data = null, Message = $"Password rest failed.", ResponseCode = "-1", Succeeded = false, StatusCode = 400 };
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<UserDTO>> GetUsers()
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("Status", Status.GETALL);
                var response = _dapper.GetAll<UserDTO>(AssignedConstants.Sp_User, param, commandType: CommandType.StoredProcedure);
                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Response<UserDTO> GetUserById(string userid)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("Status", Status.GETBYID);
                param.Add("UserId", userid);
                var response = _dapper.Get<UserDTO>(AssignedConstants.Sp_User, param, commandType: CommandType.StoredProcedure);
                return ApplicationConstants.SuccessMessage<UserDTO>(response, "Record retrieved successfully"); ;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<UserDTO>> GetUserForAgencyAdmin(string AgencyCode)
        {
            try
            {
                int userType = 0;
                if (_authenticatedUserService.RoleName == Enum.GetName(typeof(Roles), Roles.GlobalAdmin))
                {
                    userType = 1;
                }
                else
                {
                    userType = 2;
                }
                var param = new DynamicParameters();
                param.Add("Status", Status.GETALL);
                param.Add("UserType", userType);
                param.Add("AgencyCode", AgencyCode);
                var response = _dapper.GetAll<UserDTO>(AssignedConstants.Sp_User, param, commandType: CommandType.StoredProcedure);
                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<dynamic> GetUsersByRole(Roles roles)
        {
            var role = roles.ToString().ToLower().Trim();
            var userrole = await (from u in _identitycontext.Users.Where(x => x.IsDeleted != true && x.IsActive == true)
                                  join ur in _identitycontext.UserRoles on u.Id equals ur.UserId
                                  join r in _identitycontext.Roles.Where(x => x.Name.ToLower().Trim() == role) on ur.RoleId equals r.Id
                                  select new
                                  {
                                      Name = u.FirstName + " " + u.LastName,
                                      UserId = u.Id,
                                      RoleName = r.Name,
                                      AgencyCode = u.AgencyCode
                                  })
                                  .Where(x => x.AgencyCode == _authenticatedUserService.AgencyCode)
                                  .OrderBy(c => c.Name).ToListAsync();
            return userrole.ToList();
        }

        public async Task<Response<string>> MapInitiatorToValidator(MapInitiatorRequestDto request)
        {
            try
            {
                if ((request.IsApprovalGeneral == true) && request.InitiatorValidatorId.Count > 0)
                {
                    return ApplicationConstants.FailureMessage("You cannot set the approval to general and map initiator to validator. Kindly pick one");
                }
                if ((request.IsApprovalGeneral == true || request.IsApprovalGeneral == false) && request.InitiatorValidatorId.Count < 1)
                {
                    var existingSetting = await _context.ModuleApprovalConfig.FirstOrDefaultAsync(x => x.AgencyCode == _authenticatedUserService.AgencyCode);
                    if (existingSetting != null)
                    {
                        existingSetting.IsApprovalForGeneralValidators = request.IsApprovalGeneral;
                        existingSetting.LastUpdated = DateTime.Now;
                        existingSetting.UpdatedBy = _authenticatedUserService.UserId;

                        _context.ModuleApprovalConfig.Update(existingSetting);
                    }
                    else
                    {
                        var moduleConfig = new ModuleApprovalConfig
                        {
                            IsApprovalForGeneralValidators = request.IsApprovalGeneral,
                            AgencyCode = _authenticatedUserService.AgencyCode,
                            CreatedBy = _authenticatedUserService.UserId,
                            CreatedOn = DateTime.Now
                        };
                        await _context.ModuleApprovalConfig.AddAsync(moduleConfig);
                    }
                    var save = await _context.SaveChangesAsync();
                    if (save > 0)
                    {
                        return ApplicationConstants.SuccessMessage("Approval setting was successful");
                    }
                    else
                    {
                        return ApplicationConstants.FailureMessage("Approval setting was not successful");
                    }
                }

                var saveList = new MapUserApproval();
                int exist = 0;
                int success = 0;
                int fail = 0;

                if (request.IsApprovalGeneral == null && request.InitiatorValidatorId.Count > 0)
                {
                    foreach (var item in request.InitiatorValidatorId)
                    {
                        if (string.Equals(item.InitiatorId, item.ValidatorId))
                        {
                            fail++;
                            break;
                        }

                        var isExist = await _context.MapUserApproval.AnyAsync(u => u.InitiatorId == item.InitiatorId && u.ValidatorId == item.ValidatorId && u.AgencyCode == _authenticatedUserService.AgencyCode);
                        if (!isExist)
                        {
                            var record = new MapUserApproval
                            {
                                AgencyCode = _authenticatedUserService.AgencyCode,
                                InitiatorId = item.InitiatorId,
                                ValidatorId = item.ValidatorId,
                                CreatedBy = _authenticatedUserService.UserId,
                                CreatedOn = DateTime.Now
                            };
                            await _context.MapUserApproval.AddAsync(record);
                            var save = await _context.SaveChangesAsync();
                            if (save > 0)
                            {
                                success++;
                            }
                            else
                            {
                                fail++;
                            }
                        }
                        else
                        {
                            exist++;
                        }
                    }
                    return ApplicationConstants.SuccessMessage($"{success} record(s) mapped. {exist} record(s) exist and {fail} record(s) failed");
                }
                return ApplicationConstants.FailureMessage($"Mapping failed. Kindly try again");
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message.ToString());
                Log.Error(ex.Message, "An error has occured on MapInitiatorToValidator, Account Service");
                return ApplicationConstants.FailureMessage("An error occured");
            }
        }
    }
}
