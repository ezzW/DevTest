// Emtelaak.UserRegistration.Infrastructure/Services/IdentityService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Emtelaak.UserRegistration.Application.Interfaces;
using Emtelaak.UserRegistration.Domain.Models;
using Emtelaak.UserRegistration.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Infrastructure.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationUserManager _appUserManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly ILogger<IdentityService> _logger;

        public IdentityService(
            UserManager<ApplicationUser> userManager,
            ApplicationUserManager appUserManager,
            RoleManager<ApplicationRole> roleManager,
            IMapper mapper,
            ILogger<IdentityService> logger)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _appUserManager = appUserManager ?? throw new ArgumentNullException(nameof(appUserManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IdentityResultModel> CreateUserAsync(AuthUserModel user, string password)
        {
            _logger.LogInformation("Creating user: {Email}", user.Email);

            // Map domain model to infrastructure type
            var applicationUser = _mapper.Map<ApplicationUser>(user);

            // Create user
            var result = await _userManager.CreateAsync(applicationUser, password);

            // Map result back to domain model
            return _mapper.Map<IdentityResultModel>(result);
        }

        public async Task<AuthUserModel> FindUserByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user != null ? _mapper.Map<AuthUserModel>(user) : null;
        }

        public async Task<AuthUserModel> FindUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user != null ? _mapper.Map<AuthUserModel>(user) : null;
        }

        public async Task<bool> CheckPasswordAsync(AuthUserModel user, string password)
        {
            var applicationUser = await _userManager.FindByIdAsync(user.Id.ToString());
            return applicationUser != null && await _userManager.CheckPasswordAsync(applicationUser, password);
        }

        public async Task<AuthResult> ValidateUserCredentialsAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("User not found: {Email}", email);
                return new AuthResult
                {
                    Succeeded = false,
                    FailureReason = "Invalid email or password",
                    RemainingAttempts = 5
                };
            }

            // Check if user is locked out
            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("User is locked out: {Email}", email);
                return new AuthResult
                {
                    Succeeded = false,
                    FailureReason = "Account is locked. Please try again later or contact support.",
                    RemainingAttempts = 0
                };
            }

            // Check if email is confirmed
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                _logger.LogWarning("Email not confirmed: {Email}", email);
                return new AuthResult
                {
                    Succeeded = false,
                    FailureReason = "Email not confirmed. Please confirm your email before logging in.",
                    RemainingAttempts = 5
                };
            }

            // Validate password
            var passwordValid = await _userManager.CheckPasswordAsync(user, password);
            if (!passwordValid)
            {
                _logger.LogWarning("Invalid password for user: {Email}", email);

                // Increment access failed count
                await _userManager.AccessFailedAsync(user);

                // Get remaining attempts before lockout
                var remainingAttempts = _userManager.Options.Lockout.MaxFailedAccessAttempts - user.AccessFailedCount;

                return new AuthResult
                {
                    Succeeded = false,
                    FailureReason = "Invalid email or password",
                    RemainingAttempts = remainingAttempts > 0 ? remainingAttempts : 0
                };
            }

            // Reset access failed count
            await _userManager.ResetAccessFailedCountAsync(user);

            return new AuthResult { Succeeded = true };
        }

        public async Task<IdentityResultModel> ConfirmEmailAsync(AuthUserModel user, string token)
        {
            var applicationUser = await _userManager.FindByIdAsync(user.Id.ToString());
            if (applicationUser == null)
            {
                return new IdentityResultModel
                {
                    Succeeded = false,
                    Errors = new List<IdentityErrorModel>
                    {
                        new IdentityErrorModel { Code = "UserNotFound", Description = "User not found" }
                    }
                };
            }

            var result = await _userManager.ConfirmEmailAsync(applicationUser, token);
            return _mapper.Map<IdentityResultModel>(result);
        }

        public async Task<IdentityResultModel> VerifyEmailAsync(AuthUserModel user, string token)
        {
            var applicationUser = await _userManager.FindByIdAsync(user.Id.ToString());
            if (applicationUser == null)
            {
                return new IdentityResultModel
                {
                    Succeeded = false,
                    Errors = new List<IdentityErrorModel>
                    {
                        new IdentityErrorModel { Code = "UserNotFound", Description = "User not found" }
                    }
                };
            }

            var result = await _userManager.ConfirmEmailAsync(applicationUser, token);
            if (result.Succeeded)
            {
                applicationUser.EmailConfirmed = true;
                await _userManager.UpdateAsync(applicationUser);
            }

            return _mapper.Map<IdentityResultModel>(result);
        }

        public async Task<string> GenerateEmailVerificationTokenAsync(AuthUserModel user)
        {
            var applicationUser = await _userManager.FindByIdAsync(user.Id.ToString());
            if (applicationUser == null)
            {
                throw new ArgumentException($"User not found with ID: {user.Id}");
            }

            return await _userManager.GenerateEmailConfirmationTokenAsync(applicationUser);
        }

        public async Task<IdentityResultModel> VerifyPhoneNumberAsync(AuthUserModel user, string token)
        {
            var applicationUser = await _userManager.FindByIdAsync(user.Id.ToString());
            if (applicationUser == null)
            {
                return new IdentityResultModel
                {
                    Succeeded = false,
                    Errors = new List<IdentityErrorModel>
                    {
                        new IdentityErrorModel { Code = "UserNotFound", Description = "User not found" }
                    }
                };
            }

            var result = await _userManager.VerifyChangePhoneNumberTokenAsync(applicationUser, token, applicationUser.PhoneNumber);
            if (result)
            {
                applicationUser.PhoneNumberConfirmed = true;
                applicationUser.PhoneVerified = true;
                var updateResult = await _userManager.UpdateAsync(applicationUser);
                return _mapper.Map<IdentityResultModel>(updateResult);
            }

            return new IdentityResultModel
            {
                Succeeded = false,
                Errors = new List<IdentityErrorModel>
                {
                    new IdentityErrorModel { Code = "InvalidToken", Description = "Invalid verification code" }
                }
            };
        }

        public async Task<string> GeneratePhoneVerificationTokenAsync(AuthUserModel user)
        {
            var applicationUser = await _userManager.FindByIdAsync(user.Id.ToString());
            if (applicationUser == null)
            {
                throw new ArgumentException($"User not found with ID: {user.Id}");
            }

            return await _userManager.GenerateChangePhoneNumberTokenAsync(applicationUser, applicationUser.PhoneNumber);
        }

        public async Task<IdentityResultModel> ChangePasswordAsync(AuthUserModel user, string currentPassword, string newPassword)
        {
            var applicationUser = await _userManager.FindByIdAsync(user.Id.ToString());
            if (applicationUser == null)
            {
                return new IdentityResultModel
                {
                    Succeeded = false,
                    Errors = new List<IdentityErrorModel>
                    {
                        new IdentityErrorModel { Code = "UserNotFound", Description = "User not found" }
                    }
                };
            }

            var result = await _userManager.ChangePasswordAsync(applicationUser, currentPassword, newPassword);
            return _mapper.Map<IdentityResultModel>(result);
        }

        public async Task<string> GeneratePasswordResetTokenAsync(AuthUserModel user)
        {
            var applicationUser = await _userManager.FindByIdAsync(user.Id.ToString());
            if (applicationUser == null)
            {
                throw new ArgumentException($"User not found with ID: {user.Id}");
            }

            return await _userManager.GeneratePasswordResetTokenAsync(applicationUser);
        }

        public async Task<IdentityResultModel> ResetPasswordAsync(AuthUserModel user, string token, string newPassword)
        {
            var applicationUser = await _userManager.FindByIdAsync(user.Id.ToString());
            if (applicationUser == null)
            {
                return new IdentityResultModel
                {
                    Succeeded = false,
                    Errors = new List<IdentityErrorModel>
                    {
                        new IdentityErrorModel { Code = "UserNotFound", Description = "User not found" }
                    }
                };
            }

            var result = await _userManager.ResetPasswordAsync(applicationUser, token, newPassword);
            return _mapper.Map<IdentityResultModel>(result);
        }

        public async Task<bool> IsEmailConfirmedAsync(AuthUserModel user)
        {
            var applicationUser = await _userManager.FindByIdAsync(user.Id.ToString());
            return applicationUser != null && await _userManager.IsEmailConfirmedAsync(applicationUser);
        }

        public async Task<bool> IsPhoneNumberConfirmedAsync(AuthUserModel user)
        {
            var applicationUser = await _userManager.FindByIdAsync(user.Id.ToString());
            return applicationUser != null && await _userManager.IsPhoneNumberConfirmedAsync(applicationUser);
        }

        public async Task<bool> IsTwoFactorEnabledAsync(AuthUserModel user)
        {
            var applicationUser = await _userManager.FindByIdAsync(user.Id.ToString());
            return applicationUser != null && await _userManager.GetTwoFactorEnabledAsync(applicationUser);
        }

        public async Task<IList<string>> GetValidTwoFactorProvidersAsync(AuthUserModel user)
        {
            var applicationUser = await _userManager.FindByIdAsync(user.Id.ToString());
            return applicationUser != null
                ? await _userManager.GetValidTwoFactorProvidersAsync(applicationUser)
                : new List<string>();
        }

        public async Task<bool> VerifyTwoFactorTokenAsync(AuthUserModel user, string provider, string token)
        {
            var applicationUser = await _userManager.FindByIdAsync(user.Id.ToString());
            return applicationUser != null && await _userManager.VerifyTwoFactorTokenAsync(applicationUser, provider, token);
        }

        public async Task<string> GenerateTwoFactorTokenAsync(AuthUserModel user, string provider)
        {
            var applicationUser = await _userManager.FindByIdAsync(user.Id.ToString());
            if (applicationUser == null)
            {
                throw new ArgumentException($"User not found with ID: {user.Id}");
            }

            // Special handling for Authenticator provider
            if (provider == "Authenticator")
            {
                // Get the current key or create a new one if it doesn't exist
                var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(applicationUser);
                if (string.IsNullOrEmpty(unformattedKey))
                {
                    // Reset will generate a new key if one doesn't exist
                    await _userManager.ResetAuthenticatorKeyAsync(applicationUser);
                    unformattedKey = await _userManager.GetAuthenticatorKeyAsync(applicationUser);
                }

                return unformattedKey;
            }

            // For other providers (SMS, Email, etc.)
            return await _userManager.GenerateTwoFactorTokenAsync(applicationUser, provider);

        }

        public async Task<IdentityResultModel> SetTwoFactorEnabledAsync(AuthUserModel user, bool enabled)
        {
            var applicationUser = await _userManager.FindByIdAsync(user.Id.ToString());
            if (applicationUser == null)
            {
                return new IdentityResultModel
                {
                    Succeeded = false,
                    Errors = new List<IdentityErrorModel>
                    {
                        new IdentityErrorModel { Code = "UserNotFound", Description = "User not found" }
                    }
                };
            }

            var result = await _userManager.SetTwoFactorEnabledAsync(applicationUser, enabled);
            return _mapper.Map<IdentityResultModel>(result);
        }

        public async Task<IdentityResultModel> SetPhoneNumberAsync(AuthUserModel user, string phoneNumber)
        {
            var applicationUser = await _userManager.FindByIdAsync(user.Id.ToString());
            if (applicationUser == null)
            {
                return new IdentityResultModel
                {
                    Succeeded = false,
                    Errors = new List<IdentityErrorModel>
                    {
                        new IdentityErrorModel { Code = "UserNotFound", Description = "User not found" }
                    }
                };
            }

            var token = await _userManager.GenerateChangePhoneNumberTokenAsync(applicationUser, phoneNumber);
            var result = await _userManager.ChangePhoneNumberAsync(applicationUser, phoneNumber, token);
            return _mapper.Map<IdentityResultModel>(result);
        }

        public async Task<IdentityResultModel> AddToRoleAsync(AuthUserModel user, string role)
        {
            var applicationUser = await _userManager.FindByIdAsync(user.Id.ToString());
            if (applicationUser == null)
            {
                return new IdentityResultModel
                {
                    Succeeded = false,
                    Errors = new List<IdentityErrorModel>
                    {
                        new IdentityErrorModel { Code = "UserNotFound", Description = "User not found" }
                    }
                };
            }

            // Check if role exists, create if not
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new ApplicationRole { Name = role });
            }

            var result = await _userManager.AddToRoleAsync(applicationUser, role);
            return _mapper.Map<IdentityResultModel>(result);
        }

        public async Task<IdentityResultModel> RemoveFromRoleAsync(AuthUserModel user, string role)
        {
            var applicationUser = await _userManager.FindByIdAsync(user.Id.ToString());
            if (applicationUser == null)
            {
                return new IdentityResultModel
                {
                    Succeeded = false,
                    Errors = new List<IdentityErrorModel>
                    {
                        new IdentityErrorModel { Code = "UserNotFound", Description = "User not found" }
                    }
                };
            }

            var result = await _userManager.RemoveFromRoleAsync(applicationUser, role);
            return _mapper.Map<IdentityResultModel>(result);
        }

        public async Task<IList<string>> GetRolesAsync(AuthUserModel user)
        {
            var applicationUser = await _userManager.FindByIdAsync(user.Id.ToString());
            return applicationUser != null
                ? await _userManager.GetRolesAsync(applicationUser)
                : new List<string>();
        }

        public async Task<bool> IsInRoleAsync(AuthUserModel user, string role)
        {
            var applicationUser = await _userManager.FindByIdAsync(user.Id.ToString());
            return applicationUser != null && await _userManager.IsInRoleAsync(applicationUser, role);
        }

        public async Task<IdentityResultModel> LockoutUserAsync(AuthUserModel user, DateTimeOffset endDate)
        {
            var applicationUser = await _userManager.FindByIdAsync(user.Id.ToString());
            if (applicationUser == null)
            {
                return new IdentityResultModel
                {
                    Succeeded = false,
                    Errors = new List<IdentityErrorModel>
                    {
                        new IdentityErrorModel { Code = "UserNotFound", Description = "User not found" }
                    }
                };
            }

            await _userManager.SetLockoutEnabledAsync(applicationUser, true);
            var result = await _userManager.SetLockoutEndDateAsync(applicationUser, endDate);
            return _mapper.Map<IdentityResultModel>(result);
        }

        public async Task<IdentityResultModel> RemoveLockoutAsync(AuthUserModel user)
        {
            var applicationUser = await _userManager.FindByIdAsync(user.Id.ToString());
            if (applicationUser == null)
            {
                return new IdentityResultModel
                {
                    Succeeded = false,
                    Errors = new List<IdentityErrorModel>
                    {
                        new IdentityErrorModel { Code = "UserNotFound", Description = "User not found" }
                    }
                };
            }

            var result = await _userManager.SetLockoutEndDateAsync(applicationUser, null);
            return _mapper.Map<IdentityResultModel>(result);
        }

        public async Task<AuthUserModel> FindByDomainUserIdAsync(Guid domainUserId)
        {
            var applicationUser = await _appUserManager.FindByDomainUserIdAsync(domainUserId);
            return applicationUser != null ? _mapper.Map<AuthUserModel>(applicationUser) : null;
        }

        public async Task<IdentityResultModel> UpdateLastLoginDateAsync(AuthUserModel user)
        {
            var applicationUser = await _userManager.FindByIdAsync(user.Id.ToString());
            if (applicationUser == null)
            {
                return new IdentityResultModel
                {
                    Succeeded = false,
                    Errors = new List<IdentityErrorModel>
                    {
                        new IdentityErrorModel { Code = "UserNotFound", Description = "User not found" }
                    }
                };
            }

            applicationUser.LastLoginAt = DateTime.UtcNow;
            var result = await _userManager.UpdateAsync(applicationUser);
            return _mapper.Map<IdentityResultModel>(result);
        }

        public async Task<IdentityResultModel> UpdateUserAsync(AuthUserModel user)
        {
            var applicationUser = await _userManager.FindByIdAsync(user.Id.ToString());
            if (applicationUser == null)
            {
                return new IdentityResultModel
                {
                    Succeeded = false,
                    Errors = new List<IdentityErrorModel>
            {
                new IdentityErrorModel { Code = "UserNotFound", Description = "User not found" }
            }
                };
            }

            // Update user properties
            applicationUser.FirstName = user.FirstName;
            applicationUser.LastName = user.LastName;
            applicationUser.PhoneNumber = user.PhoneNumber;
            applicationUser.DateOfBirth = user.DateOfBirth;
            applicationUser.CountryOfResidence = user.CountryOfResidence;
            applicationUser.PhoneNumberConfirmed = user.PhoneNumberConfirmed;
            applicationUser.PhoneVerified = user.PhoneVerified;
            applicationUser.EmailConfirmed = user.EmailConfirmed;
            applicationUser.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(applicationUser);
            return _mapper.Map<IdentityResultModel>(result);
        }

        public async Task<string> GenerateEmailVerificationCodeAsync(AuthUserModel user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            // Generate a 6-digit code
            var random = new Random();
            var code = random.Next(100000, 999999).ToString();

            // Store this code securely in the database with the user
            var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
            if (appUser == null)
                throw new InvalidOperationException($"User not found with ID: {user.Id}");

            // Use the security stamp as a placeholder to store the code temporarily
            appUser.VerificationCode = code;
            appUser.VerificationCodeExpiry = DateTime.UtcNow.AddHours(24);

            await _userManager.UpdateAsync(appUser);

            return code;
        }

        public async Task<IdentityResultModel> VerifyEmailWithCodeAsync(AuthUserModel user, string code)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
            if (appUser == null)
                throw new InvalidOperationException($"User not found with ID: {user.Id}");

            // Check if code is valid and not expired
            if (appUser.VerificationCode != code)
            {
                return new IdentityResultModel
                {
                    Succeeded = false,
                    Errors = new List<IdentityErrorModel>
            {
                new IdentityErrorModel { Code = "InvalidCode", Description = "The verification code is invalid." }
            }
                };
            }

            if (appUser.VerificationCodeExpiry < DateTime.UtcNow)
            {
                return new IdentityResultModel
                {
                    Succeeded = false,
                    Errors = new List<IdentityErrorModel>
            {
                new IdentityErrorModel { Code = "ExpiredCode", Description = "The verification code has expired." }
            }
                };
            }

            // Mark email as confirmed
            appUser.EmailConfirmed = true;
            appUser.VerificationCode = null;
            appUser.VerificationCodeExpiry = null;

            var result = await _userManager.UpdateAsync(appUser);
            return MapIdentityResult(result);
        }

        public async Task<string> GeneratePasswordResetCodeAsync(AuthUserModel user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            // Generate a 6-digit code
            var random = new Random();
            var code = random.Next(100000, 999999).ToString();

            // Store this code securely in the database with the user
            var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
            if (appUser == null)
                throw new InvalidOperationException($"User not found with ID: {user.Id}");

            // Store the code and set expiry (30 minutes)
            appUser.PasswordResetCode = code;
            appUser.PasswordResetCodeExpiry = DateTime.UtcNow.AddMinutes(30);

            await _userManager.UpdateAsync(appUser);

            return code;
        }

        public async Task<IdentityResultModel> ResetPasswordWithCodeAsync(AuthUserModel user, string code, string newPassword)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
            if (appUser == null)
                throw new InvalidOperationException($"User not found with ID: {user.Id}");

            // Check if code is valid and not expired
            if (appUser.PasswordResetCode != code)
            {
                return new IdentityResultModel
                {
                    Succeeded = false,
                    Errors = new List<IdentityErrorModel>
            {
                new IdentityErrorModel { Code = "InvalidCode", Description = "The verification code is invalid." }
            }
                };
            }

            if (appUser.PasswordResetCodeExpiry == null || appUser.PasswordResetCodeExpiry < DateTime.UtcNow)
            {
                return new IdentityResultModel
                {
                    Succeeded = false,
                    Errors = new List<IdentityErrorModel>
            {
                new IdentityErrorModel { Code = "ExpiredCode", Description = "The verification code has expired." }
            }
                };
            }

            // Reset the password
            var token = await _userManager.GeneratePasswordResetTokenAsync(appUser);
            var result = await _userManager.ResetPasswordAsync(appUser, token, newPassword);

            // Clear the reset code if successful
            if (result.Succeeded)
            {
                appUser.PasswordResetCode = null;
                appUser.PasswordResetCodeExpiry = null;
                await _userManager.UpdateAsync(appUser);
            }

            return MapIdentityResult(result);
        }

        private IdentityResultModel MapIdentityResult(Microsoft.AspNetCore.Identity.IdentityResult identityResult)
        {
            if (identityResult == null)
                throw new ArgumentNullException(nameof(identityResult));

            return new IdentityResultModel
            {
                Succeeded = identityResult.Succeeded,
                Errors = identityResult.Errors.Select(e => new IdentityErrorModel
                {
                    Code = e.Code,
                    Description = e.Description
                }).ToList()
            };
        }
    }
}