using MongoDB.Driver;
using MongoAdminUI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MongoAdminUI.Services
{
    public class UserService
    {
        private readonly IMongoCollection<UserModel> _users;
        private readonly IMongoCollection<RoleModel> _roles;

        public UserService(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("MongoIDDatabase"));
            var database = client.GetDatabase("MongoID");
            _users = database.GetCollection<UserModel>("applicationUsers");
            _roles = database.GetCollection<RoleModel>("applicationRoles");
        }


        public async Task<List<string>> GetRoleNamesFromUUIDsAsync(List<Guid> uuids)
        {
            var roleNames = new List<string>();

            // Fetch all roles from the database
            var roles = await _roles.Find(_ => true).ToListAsync();

            // Iterate over the fetched roles and add their names to the list if their ID is in uuids
            foreach (var role in roles)
            {
                if (uuids.Contains(role.Id))
                {
                    roleNames.Add(role.Name); // Assuming RoleModel has a 'Name' property
                }
            }

            return roleNames;
        }







        public async Task<List<UserViewModel>> GetUserViewModelsAsync()
        {
            var userModels = await GetUsersAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var userModel in userModels)
            {
                var userViewModel = new UserViewModel
                {
                    Id = userModel.Id,
                    UserName = userModel.UserName,
                    Name = userModel.Name,
                    Email = userModel.Email,
                    Roles = await GetRoleNamesFromUUIDsAsync(userModel.Roles)
                };

                userViewModels.Add(userViewModel);
            }

            return userViewModels;
        }


        // Get all users
        public async Task<List<UserModel>> GetUsersAsync()
        {
            return await _users.Find(user => true).ToListAsync();
        }

        // Get a single user by Username
        public async Task<UserModel> GetUserByUsernameAsync(string username)
        {
            return await _users.Find<UserModel>(user => user.UserName == username).FirstOrDefaultAsync();
        }

        // Add a new user
        public async Task AddUserAsync(UserModel user)
        {
            await _users.InsertOneAsync(user);
        }

        // Update an existing user
        public async Task UpdateUserAsync(string username, UserModel updatedUser)
        {
            await _users.ReplaceOneAsync(user => user.UserName == username, updatedUser);
        }

        // Delete a user
        public async Task DeleteUserAsync(string username)
        {
            await _users.DeleteOneAsync(user => user.UserName == username);
        }

    }
}

