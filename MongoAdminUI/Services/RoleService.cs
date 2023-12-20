using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Runtime;
using MongoAdminUI.Models.RoleModels;

namespace MongoAdminUI.Services
{
    public class RoleService
    {
        private readonly IMongoCollection<RoleModel> _roles;        

        public RoleService(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("MongoIDDatabase"));
            var database = client.GetDatabase("MongoID");
            _roles = database.GetCollection<RoleModel>("applicationRoles");
        }

        // Get all roles
        public async Task<List<RoleModel>> GetRolesAsync()
        {
            return await _roles.Find(role => true).ToListAsync();
        }

        // Get a single role by name
        public async Task<RoleModel> GetRoleByNameAsync(string name)
        {
            return await _roles.Find<RoleModel>(role => role.Name == name).FirstOrDefaultAsync();
        }

        // Add a new role
        public async Task AddRoleAsync(RoleModel role)
        {
            await _roles.InsertOneAsync(role);
        }

        // Update an existing role
        public async Task UpdateRoleAsync(string name, RoleModel updatedRole)
        {
            await _roles.ReplaceOneAsync(role => role.Name == name, updatedRole);
        }

        // Delete a role
        public async Task DeleteRoleAsync(string name)
        {
            await _roles.DeleteOneAsync(role => role.Name == name);
        }
    }
}

