using Microsoft.AspNetCore.WebUtilities;

namespace Moda.Infrastructure.Identity;

internal partial class UserService
{
    public Task<string> ForgotPasswordAsync(ForgotPasswordRequest request, string origin)
    {        
        throw new NotImplementedException("Forgot Password functionality not implemented.");

        //var user = await _userManager.FindByEmailAsync(request.Email.Normalize());
        //if (user is null || !await _userManager.IsEmailConfirmedAsync(user))
        //{
        //    // Don't reveal that the user does not exist or is not confirmed
        //    throw new InternalServerException("An Error has occurred!");
        //}

        //// For more information on how to enable account confirmation and password reset please
        //// visit https://go.microsoft.com/fwlink/?LinkID=532713
        //string code = await _userManager.GeneratePasswordResetTokenAsync(user);
        //const string route = "account/reset-password";
        //var endpointUri = new Uri(string.Concat($"{origin}/", route));
        //string passwordResetUrl = QueryHelpers.AddQueryString(endpointUri.ToString(), "Token", code);
        //var mailRequest = new MailRequest(
        //    new List<string> { request.Email },
        //    "Reset Password",
        //    $"Your Password Reset Token is '{code}'. You can reset your password using the {endpointUri} Endpoint.");
        //_jobService.Enqueue(() => _mailService.SendAsync(mailRequest));

        //return "Password Reset Mail has been sent to your authorized Email.";
    }

    public async Task<string> ResetPasswordAsync(ResetPasswordRequest request)
    {
        _ = request.Email ?? throw new NotFoundException("User Not Found.");

        var user = await _userManager.FindByEmailAsync(request.Email.Normalize());

        // Don't reveal that the user does not exist
        if (user is null || request.Token is null || request.Password is null)
            throw new InternalServerException("An Error has occurred!");

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.Password);

        return result.Succeeded
            ? "Password Reset Successful!"
            : throw new InternalServerException("An Error has occurred!");
    }

    public async Task ChangePasswordAsync(ChangePasswordRequest model, string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        _ = user ?? throw new NotFoundException("User Not Found.");

        var result = await _userManager.ChangePasswordAsync(user, model.Password, model.NewPassword);

        if (!result.Succeeded)
        {
            throw new InternalServerException("Change password failed");
        }
    }
}