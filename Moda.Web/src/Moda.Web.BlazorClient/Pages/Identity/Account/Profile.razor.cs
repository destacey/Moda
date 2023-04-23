using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Moda.Web.BlazorClient.Infrastructure.ApiClient;
using Moda.Web.BlazorClient.Infrastructure.Auth;
using Moda.Web.BlazorClient.Shared;
using MudBlazor;

namespace Moda.Web.BlazorClient.Pages.Identity.Account;

public partial class Profile
{
    [CascadingParameter]
    public Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IAuthenticationService AuthService { get; set; } = default!;
    [Inject]
    protected IProfileClient ProfileClient { get; set; } = default!;

    private readonly UpdateProfileRequest _profileModel = new();

    private string? _imageUrl;
    private string? _userId;
    private char _firstLetterOfName;

    private CustomValidation? _customValidation;

    protected override async Task OnInitializedAsync()
    {
        if ((await AuthState).User is { } user)
        {
            _userId = user.GetUserId();
            _profileModel.Email = user.GetEmail() ?? string.Empty;
            _profileModel.FirstName = user.GetFirstName() ?? string.Empty;
            _profileModel.LastName = user.GetSurname() ?? string.Empty;
            _profileModel.PhoneNumber = user.GetPhoneNumber();
            _imageUrl = null;
            //_imageUrl = string.IsNullOrEmpty(user?.GetImageUrl()) ? string.Empty : (Config[ConfigNames.ApiBaseUrl] + user?.GetImageUrl());
            if (_userId is not null) _profileModel.Id = _userId;
        }

        if (_profileModel.FirstName?.Length > 0)
        {
            _firstLetterOfName = _profileModel.FirstName.ToUpper().FirstOrDefault();
        }
    }

    private async Task UpdateProfileAsync()
    {
        if (await ApiHelper.ExecuteCallGuardedAsync(
            () => ProfileClient.UpdateAsync(_profileModel), Snackbar, _customValidation))
        {
            Snackbar.Add("Your Profile has been updated. Please Login again to Continue.", Severity.Success);
            await AuthService.ReLoginAsync(Navigation.Uri);
        }
    }

    //private async Task UploadFiles(InputFileChangeEventArgs e)
    //{
    //    var file = e.File;
    //    if (file is not null)
    //    {
    //        string? extension = Path.GetExtension(file.Name);
    //        if (!ApplicationConstants.SupportedImageFormats.Contains(extension.ToLower()))
    //        {
    //            Snackbar.Add("Image Format Not Supported.", Severity.Error);
    //            return;
    //        }

    //        string? fileName = $"{_userId}-{Guid.NewGuid():N}";
    //        fileName = fileName[..Math.Min(fileName.Length, 90)];
    //        var imageFile = await file.RequestImageFileAsync(ApplicationConstants.StandardImageFormat, ApplicationConstants.MaxImageWidth, ApplicationConstants.MaxImageHeight);
    //        byte[]? buffer = new byte[imageFile.Size];
    //        await imageFile.OpenReadStream(ApplicationConstants.MaxAllowedSize).ReadAsync(buffer);
    //        string? base64String = $"data:{ApplicationConstants.StandardImageFormat};base64,{Convert.ToBase64String(buffer)}";
    //        //_profileModel.Image = new FileUploadRequest() { Name = fileName, Data = base64String, Extension = extension };

    //        await UpdateProfileAsync();
    //    }
    //}

    //public async Task RemoveImageAsync()
    //{
    //    string deleteContent = "You're sure you want to delete your Profile Image?";
    //    var parameters = new DialogParameters
    //    {
    //        { nameof(DeleteConfirmation.ContentText), deleteContent }
    //    };
    //    var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
    //    var dialog = DialogService.Show<DeleteConfirmation>("Delete", parameters, options);
    //    var result = await dialog.Result;
    //    if (!result.Cancelled)
    //    {
    //        _profileModel.DeleteCurrentImage = true;
    //        await UpdateProfileAsync();
    //    }
    //}
}