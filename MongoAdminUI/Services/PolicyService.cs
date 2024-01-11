using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using MongoAdminUI.Models.PolicyModels;

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

        
        public async Task UpdatePolicyAsync(string policyName, List<string> newRolesList)
        {
            // Fetch the current policy
            var policy = await _policies.Find<PolicyModel>(p => p.Name == policyName).FirstOrDefaultAsync();

            if (policy == null)
            {
                throw new InvalidOperationException("Policy not found.");
            }

            
            var updatedRolesList = policy.Roles.Union(newRolesList).Distinct().ToList();

            
            var update = Builders<PolicyModel>.Update.Set(p => p.Roles, updatedRolesList);

            
            var result = await _policies.UpdateOneAsync(p => p.Name == policyName, update);

            
            if (result.ModifiedCount == 0)
            {
                
            }
        }




        // Delete a policy
        public async Task DeletePolicyAsync(string policyName)
        {
            await _policies.DeleteOneAsync(policy => policy.Name == policyName);
        }
    }
}

