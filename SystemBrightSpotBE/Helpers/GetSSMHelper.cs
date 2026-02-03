using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
namespace SystemBrightSpotBE.Helpers
{
    public class GetSSMHelper
{
    public static async Task<string> GetSecureParameter(string name)
{
    var ssm = new AmazonSimpleSystemsManagementClient();

    var res = await ssm.GetParameterAsync(new GetParameterRequest
    {
        Name = name,
        WithDecryption = true
    });

    return res.Parameter.Value;
}
}
}
