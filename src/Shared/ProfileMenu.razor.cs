﻿using ClubManagementSystem.Data.Entities;
using ClubManagementSystem.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

namespace ClubManagementSystem.Shared
{
    public class ProfileMenuBase : ComponentBase
    {
        [CascadingParameter]
        private Task<AuthenticationState> _authStateTask { get; set; } = null!;

        [Inject]
        private UserManager<User> _userManager { get; set; } = null!;

        [Inject]
        private NavigationManager _navManager { get; set; } = null!;

        protected string FullName { get; set; } = null!;
        protected string? Role { get; set; }
        protected char Initial { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var authState = await _authStateTask;
            var userId = authState.User.GetId();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                _navManager.NavigateTo(Constants.LoginPath, true);

            Initial = user!.FirstName[0];
            FullName = user.FullName;
            Role = authState.User.GetRole();
        }
    }
}
