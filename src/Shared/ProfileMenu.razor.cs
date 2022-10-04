﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using ClubManagementSystem.Data.Entities;
using System.Security.Claims;
using ClubManagementSystem.Extensions;

namespace ClubManagementSystem.Shared
{
    public class ProfileMenuBase : ComponentBase
    {
        protected string? FullName { get; set; }
        protected string? Role { get; set; }
        protected char? FirstLetterOfFirstName { get; set; }

        [Inject]
        protected AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

        [Inject]
        protected UserManager<User> UserManager { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var userId = authState.User.GetUserId();
            var isAdmin = authState.User.IsInRole(Roles.Admin);
            if (userId != null)
            {
                var user = await UserManager.FindByIdAsync(userId);
                FirstLetterOfFirstName = user?.FirstName?[0];
                FullName = user?.FullName;
                Role = isAdmin ? Roles.Admin : string.Empty;
            }
        }
    }
}
