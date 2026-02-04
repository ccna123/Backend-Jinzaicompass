using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
namespace SystemBrightSpotBE.Helpers
{
    public class GetSSMHelper
    {
        private readonly IAmazonSimpleSystemsManagement _ssm;

        public GetSSMHelper(IAmazonSimpleSystemsManagement ssm)
        {
            _ssm = ssm;
        }

        public async Task<string> GetSecureParameter(string name)
        {
            try
            {
                var response = await _ssm.GetParameterAsync(new GetParameterRequest
                {
                    Name = name,
                    WithDecryption = true
                });

                return response.Parameter.Value;
            }
            catch (ParameterNotFoundException)
            {
                throw new Exception(
                    $"[Config Error] SSM parameter not found: '{name}'. Check region or parameter name.");
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"[SSM Error] Failed to read parameter '{name}': {ex.Message}");
            }
        }
    }
}
