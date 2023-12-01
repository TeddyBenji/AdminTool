using MongoDB.Driver;
using MongoAdminUI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MongoAdminUI.Services
{
    public class PolicyService
    {
        private readonly IMongoCollection<PolicyModel> _policies;

        public PolicyService(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("ChemoMetecDatabase"));
            var database = client.GetDatabase("Chemometec");
            _policies = database.GetCollection<PolicyModel>("Policy");
        }

        // Get all policies
        public async Task<List<PolicyModel>> GetPoliciesAsync()
        {
            return await _policies.Find(policy => true).ToListAsync();
        }

        // Get a single policy by Name
        public async Task<PolicyModel> GetPolicyByNameAsync(string policyName)
        {
            return await _policies.Find<PolicyModel>(policy => policy.Name == policyName).FirstOrDefaultAsync();
        }

        // Add a new policy
        public async Task AddPolicyAsync(PolicyModel policy)
        {
            await _policies.InsertOneAsync(policy);
        }

        // Update an existing policy
        public async Task UpdatePolicyAsync(string policyName, PolicyModel updatedPolicy)
        {
            await _policies.ReplaceOneAsync(policy => policy.Name == policyName, updatedPolicy);
        }

        // Delete a policy
        public async Task DeletePolicyAsync(string policyName)
        {
            await _policies.DeleteOneAsync(policy => policy.Name == policyName);
        }
    }
}

