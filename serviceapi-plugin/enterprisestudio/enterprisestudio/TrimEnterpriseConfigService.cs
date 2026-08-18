using System;
using System.Collections.Generic;
using ServiceStack;
using TRIM.SDK;

namespace SAPIPlugins
{
    // -----------------------------------------------------------------
    // Dataset DTOs
    // -----------------------------------------------------------------

    public class TrimDatasetInfo
    {
        public string DatasetId { get; set; }
        public string DatasetName { get; set; }
        public string DatasetType { get; set; }
    }

    [Route("/TrimDatasets", "GET")]
    public class ListTrimDatasets : IReturn<ListTrimDatasetsResponse> { }

    public class ListTrimDatasetsResponse
    {
        public List<TrimDatasetInfo> Datasets { get; set; }
    }

    [Route("/TrimDatasets/{DatasetId}", "GET")]
    public class GetTrimDataset : IReturn<GetTrimDatasetResponse>
    {
        public string DatasetId { get; set; }
    }

    public class GetTrimDatasetResponse
    {
        public TrimDatasetInfo Dataset { get; set; }
    }

    [Route("/TrimDatasets", "POST")]
    public class CreateTrimDataset : IReturn<GetTrimDatasetResponse>
    {
        public string DatasetId { get; set; }
        public string DatasetName { get; set; }

        /// <summary>Name of a TRIM.SDK.DatabaseTypes enum value, e.g. "SqlServer".</summary>
        public string DatabaseType { get; set; }

        public string ConnectionString { get; set; }
        public string ConnectionPassword { get; set; }

        /// <summary>If true, also calls config.Deploy() after saving.</summary>
        public bool Deploy { get; set; }
    }

    [Route("/TrimDatasets/{DatasetId}", "PUT")]
    public class UpdateTrimDataset : IReturn<GetTrimDatasetResponse>
    {
        public string DatasetId { get; set; }

        // Null/omitted fields are left unchanged.
        public string DatasetName { get; set; }
        public string ConnectionString { get; set; }
        public string ConnectionPassword { get; set; }

        public bool Deploy { get; set; }
    }

    [Route("/TrimDatasets/{DatasetId}", "DELETE")]
    public class DeleteTrimDataset : IReturnVoid
    {
        public string DatasetId { get; set; }
        public bool Deploy { get; set; }
    }

    // -----------------------------------------------------------------
    // Workgroup server DTOs
    // -----------------------------------------------------------------

    public class TrimWorkgroupServerInfo
    {
        public string Id { get; set; }
        public string ServerName { get; set; }
        public int PortNumber { get; set; }
        public string TCPAddress { get; set; }

        public bool HTTPEnabled { get; set; }
        public string HTTPAddress { get; set; }
        public int HTTPPortNumber { get; set; }

        public bool HTTPSEnabled { get; set; }
        public string HTTPSAddress { get; set; }
        public int HTTPSPortNumber { get; set; }
        public string HTTPSCertSubject { get; set; }
        public string HTTPSCertThumbprint { get; set; }

        public bool CanProcessEvents { get; set; }
        public bool PublishDatasets { get; set; }
        public short SharePointFarmId { get; set; }
        public bool UsesDocumentCache { get; set; }
        public int DocumentCacheSizeLimitMg { get; set; }

        public bool LoggingEnabled { get; set; }
        public bool LogWgsInternalCalls { get; set; }
        public bool LogUserQueryStrings { get; set; }
        public bool LogSQLWithNoParams { get; set; }
        public bool LogRCFCalls { get; set; }
    }

    [Route("/TrimWorkgroupServers", "GET")]
    public class ListTrimWorkgroupServers : IReturn<ListTrimWorkgroupServersResponse> { }

    public class ListTrimWorkgroupServersResponse
    {
        public List<TrimWorkgroupServerInfo> Servers { get; set; }
    }

    [Route("/TrimWorkgroupServers/{ServerId}", "GET")]
    public class GetTrimWorkgroupServer : IReturn<GetTrimWorkgroupServerResponse>
    {
        public string ServerId { get; set; }
    }

    public class GetTrimWorkgroupServerResponse
    {
        public TrimWorkgroupServerInfo Server { get; set; }
    }

    [Route("/TrimWorkgroupServers", "POST")]
    public class CreateTrimWorkgroupServer : IReturn<GetTrimWorkgroupServerResponse>
    {
        public string ServerName { get; set; }
        public int TcpPortNumber { get; set; }
        public bool Deploy { get; set; }
    }

    /// <summary>
    /// Updates a workgroup server. NOTE: ServerName and PortNumber are
    /// read-only on TrimEnterpriseWorkgroupServer (fixed at creation) so
    /// they cannot be changed here - only the fields below are settable.
    /// </summary>
    [Route("/TrimWorkgroupServers/{ServerId}", "PUT")]
    public class UpdateTrimWorkgroupServer : IReturn<GetTrimWorkgroupServerResponse>
    {
        public string ServerId { get; set; }

        public string TCPAddress { get; set; }
        public bool? CanProcessEvents { get; set; }
        public bool? PublishDatasets { get; set; }
        public short? SharePointFarmId { get; set; }
        public bool? UsesDocumentCache { get; set; }
        public int? DocumentCacheSizeLimitMg { get; set; }
        public bool? LoggingEnabled { get; set; }
        public bool? LogWgsInternalCalls { get; set; }
        public bool? LogUserQueryStrings { get; set; }
        public bool? LogSQLWithNoParams { get; set; }
        public bool? LogRCFCalls { get; set; }

        // HTTP: set EnableHttp = true with HttpAddress/HttpPort to turn on,
        // or EnableHttp = false to call DisableHTTP().
        public bool? EnableHttp { get; set; }
        public string HttpAddress { get; set; }
        public int? HttpPort { get; set; }

        // HTTPS: same pattern as HTTP, plus cert details.
        public bool? EnableHttps { get; set; }
        public string HttpsAddress { get; set; }
        public int? HttpsPort { get; set; }
        public string HttpsCertSubject { get; set; }
        public string HttpsCertThumbprint { get; set; }

        // Optional: name of a TRIM.SDK.AuthenticationMethod enum value.
        public string AuthenticationMethod { get; set; }
        public bool? AuthenticationMethodSupported { get; set; }

        public bool Deploy { get; set; }
    }

    [Route("/TrimWorkgroupServers/{ServerId}", "DELETE")]
    public class DeleteTrimWorkgroupServer : IReturnVoid
    {
        public string ServerId { get; set; }

        /// <summary>
        /// Required if this server currently does rendering or event
        /// processing - id of another workgroup server to take over.
        /// </summary>
        public string ReplacementServerId { get; set; }

        public bool Deploy { get; set; }
    }

    // -----------------------------------------------------------------
    // Config-level DTOs (explicit save / deploy without other changes)
    // -----------------------------------------------------------------

    [Route("/TrimConfig/Save", "POST")]
    public class SaveTrimConfig : IReturnVoid { }

    [Route("/TrimConfig/Deploy", "POST")]
    public class DeployTrimConfig : IReturnVoid { }

    // -----------------------------------------------------------------
    // Service implementation
    // -----------------------------------------------------------------

    public class TrimEnterpriseConfigService : IService
    {
        // ---------- Datasets ----------

        public object Get(ListTrimDatasets request)
        {
            var config = CreateConfig();
            var datasets = new List<TrimDatasetInfo>();

            uint idx = 0;
            TrimEnterpriseDataset dataset;
            while ((dataset = SafeGetDataset(config, idx)) != null)
            {
                datasets.Add(ToDatasetInfo(dataset));
                idx++;
            }

            return new ListTrimDatasetsResponse { Datasets = datasets };
        }

        public object Get(GetTrimDataset request)
        {
            var config = CreateConfig();
            var dataset = config.FindDataset(request.DatasetId);

            if (dataset == null)
                throw HttpError.NotFound($"No dataset found with id '{request.DatasetId}'");

            return new GetTrimDatasetResponse { Dataset = ToDatasetInfo(dataset) };
        }

        public object Post(CreateTrimDataset request)
        {
            if (string.IsNullOrEmpty(request.DatasetId))
                throw new ArgumentException("DatasetId is required.");

            DatabaseTypes dbType;
            if (!System.Enum.TryParse(request.DatabaseType, true, out dbType))
                throw new ArgumentException($"Unrecognised DatabaseType '{request.DatabaseType}'.");

            var config = CreateConfig();

            if (config.FindDataset(request.DatasetId) != null)
                throw HttpError.Conflict($"Dataset '{request.DatasetId}' already exists.");

            var dataset = config.NewDataset(dbType, request.DatasetId, request.DatasetName);

            if (!string.IsNullOrEmpty(request.ConnectionString))
                dataset.SetConnectionString(request.ConnectionString);

            if (!string.IsNullOrEmpty(request.ConnectionPassword))
                dataset.SetConnectionPassword(request.ConnectionPassword);

            config.Save();
            if (request.Deploy)
                config.Deploy();

            return new GetTrimDatasetResponse { Dataset = ToDatasetInfo(dataset) };
        }

        public object Put(UpdateTrimDataset request)
        {
            var config = CreateConfig();
            var dataset = config.FindDataset(request.DatasetId);

            if (dataset == null)
                throw HttpError.NotFound($"No dataset found with id '{request.DatasetId}'");

            if (!string.IsNullOrEmpty(request.DatasetName))
                dataset.DatasetName = request.DatasetName;

            if (!string.IsNullOrEmpty(request.ConnectionString))
                dataset.SetConnectionString(request.ConnectionString);

            if (!string.IsNullOrEmpty(request.ConnectionPassword))
                dataset.SetConnectionPassword(request.ConnectionPassword);

            config.Save();
            if (request.Deploy)
                config.Deploy();

            return new GetTrimDatasetResponse { Dataset = ToDatasetInfo(dataset) };
        }

        public void Delete(DeleteTrimDataset request)
        {
            var config = CreateConfig();
            var dataset = config.FindDataset(request.DatasetId);

            if (dataset == null)
                throw HttpError.NotFound($"No dataset found with id '{request.DatasetId}'");

            dataset.Remove();
            config.Save();
            if (request.Deploy)
                config.Deploy();
        }

        // ---------- Workgroup servers ----------

        public object Get(ListTrimWorkgroupServers request)
        {
            var config = CreateConfig();
            var servers = new List<TrimWorkgroupServerInfo>();

            uint idx = 0;
            TrimEnterpriseWorkgroupServer server;
            while ((server = SafeGetWorkgroupServer(config, idx)) != null)
            {
                servers.Add(ToServerInfo(server));
                idx++;
            }

            return new ListTrimWorkgroupServersResponse { Servers = servers };
        }

        public object Get(GetTrimWorkgroupServer request)
        {
            var config = CreateConfig();
            var server = config.FindWorkgroupServer(request.ServerId);

            if (server == null)
                throw HttpError.NotFound($"No workgroup server found with id '{request.ServerId}'");

            return new GetTrimWorkgroupServerResponse { Server = ToServerInfo(server) };
        }

        public object Post(CreateTrimWorkgroupServer request)
        {
            if (string.IsNullOrEmpty(request.ServerName))
                throw new ArgumentException("ServerName is required.");

            var config = CreateConfig();

            // NewWorkgroupServer auto-allocates a new id, so there's
            // nothing to clash-check beforehand.
            var server = config.NewWorkgroupServer(request.ServerName, request.TcpPortNumber);

            config.Save();
            if (request.Deploy)
                config.Deploy();

            return new GetTrimWorkgroupServerResponse { Server = ToServerInfo(server) };
        }

        public object Put(UpdateTrimWorkgroupServer request)
        {
            var config = CreateConfig();
            var server = config.FindWorkgroupServer(request.ServerId);

            if (server == null)
                throw HttpError.NotFound($"No workgroup server found with id '{request.ServerId}'");

            // ServerName / PortNumber are read-only on this class and
            // cannot be changed after creation.

            if (!string.IsNullOrEmpty(request.TCPAddress))
                server.TCPAddress = request.TCPAddress;

            if (request.CanProcessEvents.HasValue)
                server.CanProcessEvents = request.CanProcessEvents.Value;
            if (request.PublishDatasets.HasValue)
                server.PublishDatasets = request.PublishDatasets.Value;
            if (request.SharePointFarmId.HasValue)
                server.SharePointFarmId = request.SharePointFarmId.Value;
            if (request.UsesDocumentCache.HasValue)
                server.UsesDocumentCache = request.UsesDocumentCache.Value;
            if (request.DocumentCacheSizeLimitMg.HasValue)
                server.DocumentCacheSizeLimitMg = request.DocumentCacheSizeLimitMg.Value;
            if (request.LoggingEnabled.HasValue)
                server.LoggingEnabled = request.LoggingEnabled.Value;
            if (request.LogWgsInternalCalls.HasValue)
                server.LogWgsInternalCalls = request.LogWgsInternalCalls.Value;
            if (request.LogUserQueryStrings.HasValue)
                server.LogUserQueryStrings = request.LogUserQueryStrings.Value;
            if (request.LogSQLWithNoParams.HasValue)
                server.LogSQLWithNoParams = request.LogSQLWithNoParams.Value;
            if (request.LogRCFCalls.HasValue)
                server.LogRCFCalls = request.LogRCFCalls.Value;

            if (request.EnableHttp == true)
            {
                if (string.IsNullOrEmpty(request.HttpAddress) || !request.HttpPort.HasValue)
                    throw new ArgumentException("HttpAddress and HttpPort are required to enable HTTP.");
                server.EnableHTTP(request.HttpAddress, request.HttpPort.Value);
            }
            else if (request.EnableHttp == false)
            {
                server.DisableHTTP();
            }

            if (request.EnableHttps == true)
            {
                if (string.IsNullOrEmpty(request.HttpsAddress) || !request.HttpsPort.HasValue
                    || string.IsNullOrEmpty(request.HttpsCertSubject) || string.IsNullOrEmpty(request.HttpsCertThumbprint))
                    throw new ArgumentException("HttpsAddress, HttpsPort, HttpsCertSubject and HttpsCertThumbprint are required to enable HTTPS.");
                server.EnableHTTPS(request.HttpsAddress, request.HttpsPort.Value, request.HttpsCertSubject, request.HttpsCertThumbprint);
            }
            else if (request.EnableHttps == false)
            {
                server.DisableHTTPS();
            }

            if (!string.IsNullOrEmpty(request.AuthenticationMethod) && request.AuthenticationMethodSupported.HasValue)
            {
                TRIM.SDK.AuthenticationMethod method;
                if (!System.Enum.TryParse(request.AuthenticationMethod, true, out method))
                    throw new ArgumentException($"Unrecognised AuthenticationMethod '{request.AuthenticationMethod}'.");

                server.SetAuthenticationMethod(method, request.AuthenticationMethodSupported.Value);
            }

            config.Save();
            if (request.Deploy)
                config.Deploy();

            return new GetTrimWorkgroupServerResponse { Server = ToServerInfo(server) };
        }

        public void Delete(DeleteTrimWorkgroupServer request)
        {
            var config = CreateConfig();
            var server = config.FindWorkgroupServer(request.ServerId);

            if (server == null)
                throw HttpError.NotFound($"No workgroup server found with id '{request.ServerId}'");

            TrimEnterpriseWorkgroupServer replacement = null;
            if (!string.IsNullOrEmpty(request.ReplacementServerId))
            {
                replacement = config.FindWorkgroupServer(request.ReplacementServerId);
                if (replacement == null)
                    throw HttpError.NotFound($"Replacement workgroup server '{request.ReplacementServerId}' not found.");
            }

            // If this server currently does rendering/event processing,
            // the SDK requires a replacement - pass null when it doesn't.
            server.Remove(replacement);

            config.Save();
            if (request.Deploy)
                config.Deploy();
        }

        // ---------- Config-level save / deploy ----------

        public void Post(SaveTrimConfig request)
        {
            var config = CreateConfig();
            config.Save();
        }

        public void Post(DeployTrimConfig request)
        {
            var config = CreateConfig();
            config.Deploy();
        }

        // ---------- Helpers ----------

        /// <summary>
        /// Wraps the TrimEnterpriseConfiguration constructor so that any
        /// failure (e.g. wrong machine/identity, missing .tcfg, permissions)
        /// surfaces the real exception message back over HTTP instead of
        /// a bare 500 with no detail. Remove/tighten this once you've
        /// diagnosed the underlying issue - returning raw exception
        /// messages to callers isn't something you want left in long-term.
        /// </summary>
        private static TrimEnterpriseConfiguration CreateConfig()
        {
            try
            {
                return new TrimEnterpriseConfiguration();
            }
            catch (Exception ex)
            {
                throw HttpError.Unauthorized($"TrimEnterpriseConfiguration failed: {ex.Message}");
            }
        }

        private static TrimDatasetInfo ToDatasetInfo(TrimEnterpriseDataset dataset)
        {
            return new TrimDatasetInfo
            {
                DatasetId = dataset.DatasetID,
                DatasetName = dataset.DatasetName,
                DatasetType = dataset.DatasetType.ToString()
            };
        }

        private static TrimWorkgroupServerInfo ToServerInfo(TrimEnterpriseWorkgroupServer server)
        {
            return new TrimWorkgroupServerInfo
            {
                Id = server.Id,
                ServerName = server.ServerName,
                PortNumber = server.PortNumber,
                TCPAddress = server.TCPAddress,

                HTTPEnabled = server.HTTPEnabled,
                HTTPAddress = server.HTTPAddress,
                HTTPPortNumber = server.HTTPPortNumber,

                HTTPSEnabled = server.HTTPSEnabled,
                HTTPSAddress = server.HTTPSAddress,
                HTTPSPortNumber = server.HTTPSPortNumber,
                HTTPSCertSubject = server.HTTPSCertSubject,
                HTTPSCertThumbprint = server.HTTPSCertThumbprint,

                CanProcessEvents = server.CanProcessEvents,
                PublishDatasets = server.PublishDatasets,
                SharePointFarmId = server.SharePointFarmId,
                UsesDocumentCache = server.UsesDocumentCache,
                DocumentCacheSizeLimitMg = server.DocumentCacheSizeLimitMg,

                LoggingEnabled = server.LoggingEnabled,
                LogWgsInternalCalls = server.LogWgsInternalCalls,
                LogUserQueryStrings = server.LogUserQueryStrings,
                LogSQLWithNoParams = server.LogSQLWithNoParams,
                LogRCFCalls = server.LogRCFCalls
            };
        }

        private static TrimEnterpriseDataset SafeGetDataset(TrimEnterpriseConfiguration config, uint idx)
        {
            try { return config.GetDataset(idx); }
            catch (Exception) { return null; }
        }

        private static TrimEnterpriseWorkgroupServer SafeGetWorkgroupServer(TrimEnterpriseConfiguration config, uint idx)
        {
            try { return config.GetWorkgroupServer(idx); }
            catch (Exception) { return null; }
        }
    }
}