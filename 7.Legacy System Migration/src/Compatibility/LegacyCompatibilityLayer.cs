using Microsoft.FeatureManagement;

namespace LegacyMigration.Compatibility
{
    public interface ILegacyCompatibilityLayer
    {
        Task<bool> IsFeatureEnabledAsync(string featureFlag);
        T GetImplementation<T>(T legacyImpl, T modernImpl, string featureFlag);
    }

    public class LegacyCompatibilityLayer : ILegacyCompatibilityLayer
    {
        private readonly IFeatureManager _featureManager;

        public LegacyCompatibilityLayer(IFeatureManager featureManager)
        {
            _featureManager = featureManager;
        }

        public async Task<bool> IsFeatureEnabledAsync(string featureFlag)
        {
            return await _featureManager.IsEnabledAsync(featureFlag);
        }

        public T GetImplementation<T>(T legacyImpl, T modernImpl, string featureFlag)
        {
            return _featureManager.IsEnabledAsync(featureFlag).Result ? modernImpl : legacyImpl;
        }
    }
}
