# Azure Database Connection Setup Guide

## Problem
When deploying to Azure App Service, the application cannot connect to the on-premises SQL Server (`CERM-DATA\CRMDB`) because:
1. The connection string needs to be configured in Azure Portal
2. Azure App Service cannot directly access on-premises servers without proper networking

## Solution Steps

### Step 1: Configure Connection String in Azure Portal

1. **Navigate to Azure Portal**
   - Go to https://portal.azure.com
   - Find your App Service: `WiseLabels20251125155258`

2. **Configure Connection String**
   - In the left menu, go to **Environment Variables**
   - Click on the **Connection strings** tab
   - Click **+ New connection string**
   - Configure:
     - **Name**: `CermDatabase`
     - **Value**: `Server=CERM-DATA\CRMDB;Database=sqlb00;user id=CermSys;password=SysCerm01.;TrustServerCertificate=True;`
     - **Type**: Select `SQLServer` (or `SQLAzure` if using Azure SQL)
   - Click **Apply** twice to save

### Step 2: Network Connectivity Options

Azure App Service cannot reach on-premises SQL Server without proper networking. Choose one of these options:

#### Option A: Azure Hybrid Connection (Recommended for On-Premises)

1. **Create Hybrid Connection in Azure**
   - In your App Service, go to **Networking** → **Hybrid connections**
   - Click **Configure hybrid connection endpoints**
   - Create a new Hybrid Connection:
     - **Endpoint name**: `CERM-SQL-Server`
     - **Hostname**: `CERM-DATA` (or the FQDN if available)
     - **Port**: `1433` (default SQL Server port)

2. **Install Hybrid Connection Manager on On-Premises Server**
   - Download Hybrid Connection Manager from Azure Portal
   - Install on a machine that can reach `CERM-DATA\CRMDB`
   - Configure it to connect to your Azure Hybrid Connection

3. **Update Connection String** (if needed)
   - If using FQDN, update the connection string in Azure Portal:
     - `Server=CERM-DATA.yourdomain.com\CRMDB;Database=sqlb00;user id=CermSys;password=SysCerm01.;TrustServerCertificate=True;`

#### Option B: VPN Gateway (For Enterprise Networks)

1. **Set up Azure VPN Gateway**
   - Create a Virtual Network in Azure
   - Create a VPN Gateway
   - Configure Site-to-Site VPN to your on-premises network
   - Integrate App Service with the Virtual Network

2. **Update Connection String**
   - Use the internal IP or hostname of the SQL Server
   - Example: `Server=10.0.1.100\CRMDB;Database=sqlb00;user id=CermSys;password=SysCerm01.;TrustServerCertificate=True;`

#### Option C: Public SQL Server Endpoint (Not Recommended - Security Risk)

**⚠️ WARNING: Only use if SQL Server is already exposed to the internet with proper firewall rules**

1. **Update Connection String in Azure Portal**
   - Use the public IP or FQDN:
     - `Server=your-public-ip-or-fqdn\CRMDB;Database=sqlb00;user id=CermSys;password=SysCerm01.;TrustServerCertificate=True;`

2. **Ensure SQL Server Firewall Allows Azure IPs**
   - Azure App Service outbound IPs change
   - Consider using Azure SQL Database instead for better security

### Step 3: Verify Configuration

1. **Check Application Logs**
   - In Azure Portal, go to **Log stream** or **Logs**
   - Look for connection errors or successful database connections

2. **Test the Endpoint**
   - Navigate to: `https://wiselabels20251125155258-btbvc6cwdwetbjdc.canadacentral-01.azurewebsites.net/`
   - Try to load cutting die options
   - Check if the error is resolved

### Step 4: Alternative - Use Application Settings

If Connection Strings don't work, you can use Application Settings instead:

1. In Azure Portal, go to **Configuration** → **Application settings**
2. Click **+ New application setting**
3. Add:
   - **Name**: `ConnectionStrings:CermDatabase`
   - **Value**: `Server=CERM-DATA\CRMDB;Database=sqlb00;user id=CermSys;password=SysCerm01.;TrustServerCertificate=True;`
4. Click **Save**

## Troubleshooting

### Error: "Server was not found or was not accessible"
- **Cause**: Network connectivity issue
- **Solution**: Set up Hybrid Connection or VPN Gateway (see Step 2)

### Error: "Login failed for user"
- **Cause**: Incorrect credentials or SQL Server authentication issue
- **Solution**: Verify username/password in connection string

### Error: "Certificate chain was issued by an authority that is not trusted"
- **Cause**: SSL certificate validation issue
- **Solution**: Ensure `TrustServerCertificate=True;` is in the connection string

### Connection String Not Being Used
- **Cause**: App Service might be using cached configuration
- **Solution**: 
  1. Restart the App Service after saving configuration
  2. Clear any cached settings
  3. Verify the connection string name matches exactly: `CermDatabase`

## Security Best Practices

1. **Use Azure Key Vault** for storing sensitive connection strings
2. **Enable SQL Server encryption** for data in transit
3. **Use least-privilege SQL accounts** (not `sa` or admin accounts)
4. **Monitor connection attempts** in Azure App Service logs
5. **Consider migrating to Azure SQL Database** for better security and management

## Additional Resources

- [Azure App Service Hybrid Connections](https://docs.microsoft.com/azure/app-service/app-service-hybrid-connections)
- [Azure VPN Gateway](https://docs.microsoft.com/azure/vpn-gateway/)
- [Configure Connection Strings in Azure App Service](https://docs.microsoft.com/azure/app-service/configure-common#configure-connection-strings)

