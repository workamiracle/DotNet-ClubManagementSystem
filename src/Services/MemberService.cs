﻿using ClubManagementSystem.Data;
using ClubManagementSystem.Data.Entities;
using ClubManagementSystem.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClubManagementSystem.Services
{
    public class MemberService : IMemberService
    {
        private readonly AppDbContext _context;

        public MemberService(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            _context = dbContextFactory.CreateDbContext();
        }

        public async Task<List<Member>> GetAllMembers()
        {
            return await _context
                .Members
                .Include(m => m.Membership)
                .ToListAsync();
        }

        public async Task<bool> CreateMember(Member member)
        {
            try
            {
                await _context.Members.AddAsync(member);
                await _context.SaveChangesAsync();
            }
            catch (Exception) { return false; }
            return true;
        }

        public async Task<bool> UpdateMember(Member member)
        {
            try
            {
                _context.Members.Update(member);
                await _context.SaveChangesAsync();
            }
            catch (Exception) { return false; }
            return true;
        }

        public async Task<bool> RenewMembership(Member member, Membership newMembership)
        {
            try
            {
                if (member.Membership == null)
                {
                    member.Membership = newMembership;
                }
                else
                {
                    var isMembershipExpired = !member.Membership.IsActive;
                    if (isMembershipExpired)
                    {
                        var membershipHistory = new MembershipHistory
                        {
                            Member = member,
                            StartDate = member.Membership.StartDate,
                            EndDate = member.Membership.EndDate,
                            Fee = member.Membership.Fee
                        };
                        await _context.MembershipHistories.AddAsync(membershipHistory);
                    }

                    member.Membership.StartDate = newMembership.StartDate;
                    member.Membership.EndDate = newMembership.EndDate;
                    member.Membership.Fee = newMembership.Fee;
                }

                _context.Members.Update(member);
                await _context.SaveChangesAsync();
            }
            catch (Exception) { return false; }
            return true;
        }

        public async Task<bool> DeleteMember(Member member)
        {
            try
            {
                _context.Members.Remove(member);
                await _context.SaveChangesAsync();
            }
            catch (Exception) { return false; }
            return true;
        }

        public void Dispose() => _context.Dispose();
    }
}
