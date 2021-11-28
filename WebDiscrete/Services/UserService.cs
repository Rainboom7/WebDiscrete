using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using WebDiscrete.Data;
using WebDiscrete.Data.Entity;
using WebDiscrete.Models;

namespace WebDiscrete.Services
{
    public class UserService
    {
        private readonly PasswordHasher<User> _passwordHasher;
        private ApplicationDbContext _dbContext;
        private Random _random;
        private const string AdminName = "Admin";

        public UserService(PasswordHasher<User> passwordHasher, ApplicationDbContext dbContext)
        {
            _passwordHasher = passwordHasher;
            _dbContext = dbContext;
            _random = new Random();
        }

        public UserDto TryLogin(string username, string password)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Username.Equals(username));
            if (user == null)
            {
                throw new AuthenticationException("Неверные учетные данные");
            }

            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

            if (verificationResult == PasswordVerificationResult.Failed)
            {
                throw new AuthenticationException("Неверные учетные данные");
            }

            return new UserDto()
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.Username
            };
        }

        public void SetUserAccessType(int currentUserId, int objId, int targetUserId, AccessRightType right)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id.Equals(currentUserId));
            if (user.Username != AdminName)
            {
                throw new AuthenticationException("Операция только для администратора");
            }

            _dbContext.ObjectAccessRights.Add(new ObjectAccessRights()
            {
                UserId = targetUserId, ObjectId = objId,
                AccessType = right
            });
            _dbContext.SaveChanges();
        }

        public List<UserDto> GetUsersForObject(int objId, int currentUserId)
        {
            var users = _dbContext.Users.Where(u => u.Id != currentUserId)
                .Select(u => new UserDto()
                    {Id = u.Id, FirstName = u.FirstName, LastName = u.LastName, Username = u.Username})
                .ToList();

            foreach (var user in users)
            {
                user.AccessRights = _dbContext.ObjectAccessRights
                    .Where(right => right.ObjectId == objId &&
                                    right.UserId == user.Id)
                    .Select(right => right.AccessType).ToList();
            }

            return users;
        }

        public SystemObjectDto GetObject(int objectId)
        {
            var obj = _dbContext.SystemObjects.FirstOrDefault(o => o.Id == objectId);
            return new SystemObjectDto() {Id = objectId, Name = obj.Name};
        }

        public List<SystemObjectDto> GetObjects(int userId)
        {
            var objects = _dbContext.SystemObjects.Select(obj =>
                    new SystemObjectDto()
                    {
                        Id = obj.Id,
                        Name = obj.Name
                    })
                .ToList();
            foreach (var systemObject in objects)
            {
                systemObject.AccessRights = _dbContext.ObjectAccessRights
                    .Where(right => right.ObjectId == systemObject.Id &&
                                    right.UserId == userId)
                    .Select(right => right.AccessType).ToList();
            }

            return objects;
        }

        public void SetRights(int userId)
        {
            _dbContext.ObjectAccessRights.RemoveRange(_dbContext.ObjectAccessRights);
            var availableRights = Enum.GetValues(typeof(AccessRightType)).Cast<AccessRightType>()
                .ToList();
            var currentUser = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
            if (currentUser == null || currentUser.Username != AdminName)
            {
                throw new AuthenticationException("Только администратор может выставлять роли");
            }

            var objects = _dbContext.SystemObjects.ToList();
            var rightsToInsert = new List<ObjectAccessRights>();
            foreach (var systemObject in objects)
            {
                foreach (var right in availableRights)
                {
                    rightsToInsert.Add(new ObjectAccessRights()
                        {ObjectId = systemObject.Id, UserId = currentUser.Id, AccessType = right});
                }
            }

            foreach (var user in _dbContext.Users.Where(u => u.Id != userId).ToList())
            {
                foreach (var systemObject in objects)
                {
                    var accessTypes = GetRandomAccessTypes(availableRights);
                    foreach (var accessType in accessTypes)
                    {
                        rightsToInsert.Add(new ObjectAccessRights()
                            {ObjectId = systemObject.Id, UserId = user.Id, AccessType = accessType});
                    }
                }
            }

            _dbContext.ObjectAccessRights.AddRange(rightsToInsert);
            _dbContext.SaveChanges();
        }

        public List<AccessRightType> GetRandomAccessTypes(List<AccessRightType> availableRights)
        {
            var accessRightsList = new int[availableRights.Count];
            for (int i = 0; i < availableRights.Count; i++)
            {
                accessRightsList[i] = _random.Next(-1, availableRights.Count);
            }

            return accessRightsList
                .Where(r => r > 0)
                .Distinct()
                .Select(r => availableRights[r]).ToList();
        }
    }
}