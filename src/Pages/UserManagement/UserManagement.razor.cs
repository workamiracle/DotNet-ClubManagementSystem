﻿using ClubManagementSystem.Data.Entities;
using ClubManagementSystem.Extensions;
using ClubManagementSystem.Resources;
using ClubManagementSystem.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using MudBlazor;
using static ClubManagementSystem.Areas.Identity.Pages.Account.RegisterModel;

namespace ClubManagementSystem.Pages.UserManagement
{
    public class UserManagementBase : ComponentBase
    {
        [Inject]
        private UserManager<User> _userManager { get; set; } = null!;

        [CascadingParameter]
        private Task<AuthenticationState> _authStateTask { get; set; } = null!;

        [Inject]
        private IDialogService _dialogService { get; set; } = null!;

        [Inject]
        private ISnackbar _snackbar { get; set; } = null!;

        [Inject]
        protected IStringLocalizer<Common> Localizer { get; set; } = null!;

        protected List<User> Users { get; set; } = new();
        protected string? SearchQuery { get; set; }
        protected bool IsLoading { get; set; }
        protected string CurrentUserId { get; set; } = null!;

        private async Task LoadUsers()
        {
            IsLoading = true;
            Users = await _userManager.Users.ToListAsync();
            var authState = await _authStateTask;
            CurrentUserId = authState.User.GetId()!;
            IsLoading = false;
        }

        protected override async Task OnInitializedAsync()
            => await LoadUsers();

        protected bool Filter(User user) => FilterUsers(user, SearchQuery);

        protected bool FilterUsers(User user, string? searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
                return true;
            if (user.Email.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                return true;
            if (user.FirstName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                return true;
            if (user.LastName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        protected async Task CreateNewUser()
        {
            var result = await _dialogService.Show<CreateNewUser>("Create New User").Result;

            if (!result.Cancelled)
            {
                var userData = (UserDto)result.Data;
                var user = new User
                {
                    FirstName = userData.FirstName,
                    LastName = userData.LastName,
                    Email = userData.Email,
                    UserName = userData.Email
                };
                var identityResult = await _userManager.CreateAsync(user, userData.Password);
                if (identityResult.Succeeded)
                {
                    _snackbar.Add(string.Format(Messages.SuccessfulCreationFormat, user.FirstName),
                        Severity.Success);
                    Users.Add(user);
                }
                else
                {
                    _snackbar.Add(string.Join(Environment.NewLine,
                        identityResult.Errors.Select(e => e.Description)), Severity.Error);
                }
            }
        }

        protected async Task AddToRoles(User user)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            var dialogParams = new DialogParameters 
            {
                ["roles"] = currentRoles,
                ["selectMultipleRoles"] = true
            };
            var result = await _dialogService.Show<AddToRoles>("Add to Roles", dialogParams).Result;

            if (!result.Cancelled)
            {
                IdentityResult? identityResult = null;
                var newRoles = (IEnumerable<string>)result.Data;
                var rolesToRemove = currentRoles.Except(newRoles);
                var rolesToAdd = newRoles.Except(currentRoles);

                if (rolesToRemove.Any())
                {
                    identityResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                    if (!identityResult.Succeeded)
                    {
                        _snackbar.Add(string.Join(Environment.NewLine,
                            identityResult.Errors.Select(e => e.Description)), Severity.Error);
                        return;
                    }
                }
                if (rolesToAdd.Any())
                {
                    identityResult = await _userManager.AddToRolesAsync(user, newRoles);
                    if (!identityResult.Succeeded)
                    {
                        _snackbar.Add(string.Join(Environment.NewLine,
                            identityResult.Errors.Select(e => e.Description)), Severity.Error);
                        return;
                    }
                }
                if (identityResult != null)
                {
                    _snackbar.Add(string.Format(Messages.SuccessfulUpdateFormat, user.FullName),
                        Severity.Success);
                }
            }
        }

        protected async Task DeleteUser(User user)
        {
            var dialogParams = new DialogParameters
            {
                ["TitleText"] = "Delete User",
                ["ContentText"] = $"Are you sure you want to delete {user.FullName}?"
            };
            var result = await _dialogService.Show<DeleteEntityModal>("", dialogParams).Result;

            if (!result.Cancelled)
            {
                var identityResult = await _userManager.DeleteAsync(user);
                if (identityResult.Succeeded)
                {
                    _snackbar.Add(
                        string.Format(Messages.SuccessfulDeletionFormat, user.FullName),
                        Severity.Success);
                    Users.Remove(user);
                }
                else
                {
                    _snackbar.Add(string.Join(Environment.NewLine,
                        identityResult.Errors.Select(e => e.Description)), Severity.Error);
                }
            }
        }
    }
}
