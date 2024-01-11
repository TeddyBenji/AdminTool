using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using MongoAdminUI.Models.RoleModels;
using MongoAdminUI.Models.UserModels;

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
                    roleNames.Add(role.Name);
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
        public async Task UpdateUserAsync(string UserName, UserModel updatedUser, List<string> roleNamesToUpdate)
        {
            // Fetch all roles from the database
            var allRoles = await _roles.Find(_ => true).ToListAsync();

            // Translate role names to their corresponding Guid IDs
            var validRoleIds = allRoles
                                .Where(role => roleNamesToUpdate.Contains(role.Name))
                                .Select(role => role.Id)
                                .ToList();

            // Check if all role names have corresponding role IDs
            if (validRoleIds.Count != roleNamesToUpdate.Count)
            {
                throw new InvalidOperationException("One or more roles do not exist.");
            }

            // Update the user with the new role IDs
            var filter = Builders<UserModel>.Filter.Eq(user => user.UserName, UserName);
            var update = Builders<UserModel>.Update
                .Set(user => user.Name, updatedUser.Name)
                .Set(user => user.Email, updatedUser.Email)
                .Set(user => user.Roles, validRoleIds);

            await _users.UpdateOneAsync(filter, update);
        }


        // Delete a user
        public async Task DeleteUserAsync(string username)
        {
            await _users.DeleteOneAsync(user => user.UserName == username);
        }

        public async Task<List<string>> GetUserRolesAsync(string username)
        {
            var user = await _users.Find<UserModel>(u => u.UserName == username).FirstOrDefaultAsync();
            if (user == null)
            {
                return new List<string>();
            }

            var roleIds = user.Roles; 

            var roleNames = await GetRoleNamesFromUUIDsAsync(roleIds);

            return roleNames;
        }

    }
}

