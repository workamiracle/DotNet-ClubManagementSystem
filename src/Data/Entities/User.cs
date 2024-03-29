﻿using Microsoft.AspNetCore.Identity;

namespace ClubManagementSystem.Data.Entities
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string FullName => $"{FirstName} {LastName}";
    }
}
