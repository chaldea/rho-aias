// The Chaldea licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Chaldea.Fate.RhoAias;

public class CertOptions
{
    public string CertRootDirectory { get; set; } = "certs";
    public string CountryName { get; set; } = "CN";
    public string State { get; set; } = "Shanghai";
    public string Locality { get; set; } = "Shanghai";
    public string Organization { get; set; } = "Chaldea";
    public string OrganizationUnit { get; set; } = "Development";
    public string CertPassword { get; set; } = "Aa123456";
}
