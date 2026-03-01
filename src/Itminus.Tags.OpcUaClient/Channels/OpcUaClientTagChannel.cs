using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Client;

namespace Itminus.Tags.OpcUaClient;
public class OpcUaClientTagChannel : ITagChannel
{
    private readonly ILogger<OpcUaClientTagChannel> _logger;
    private readonly ApplicationConfiguration _appConfig;

    private SemaphoreSlim _connSignal = new SemaphoreSlim(1, 1);

    #region 配置
    private readonly OpcUaClientTagChannelOpt _channelOpt;

    public string ClientName => _channelOpt.ClientName;
    public OpcUaServerOpt ServerOpt => _channelOpt.ServerOpt;
    #endregion



    public string ChannelName { get; }
    public virtual string Driver => OpcUaClientNames.DriverName;

    public OpcUaClientTagChannel(string channelName, OpcUaClientTagChannelOpt uaChannelOpt, ILogger<OpcUaClientTagChannel> logger)
    {
        ChannelName = channelName;
        _channelOpt = uaChannelOpt;
        _logger = logger;
        _appConfig = PrepareOpcUaAppConfig();
    }


    protected virtual ApplicationConfiguration PrepareOpcUaAppConfig()
    {
        var config = new ApplicationConfiguration()
        {
            ApplicationName = "MyClient",
            ApplicationUri = Utils.Format(@"urn:{0}:MyClient", System.Net.Dns.GetHostName()),
            ApplicationType = ApplicationType.Client,
            SecurityConfiguration = new SecurityConfiguration
            {
                ApplicationCertificate = new CertificateIdentifier { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\MachineDefault", SubjectName = "MyClientSubjectName" },
                TrustedIssuerCertificates = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Certificate Authorities" },
                TrustedPeerCertificates = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Applications" },
                RejectedCertificateStore = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\RejectedCertificates" },
                AutoAcceptUntrustedCertificates = true,
                RejectSHA1SignedCertificates = false,
                MinimumCertificateKeySize = 1024,
                NonceLength = 32,
            },
            TransportConfigurations = new TransportConfigurationCollection(),
            TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
            ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 },
            TraceConfiguration = new TraceConfiguration()
        };


        // 设置证书验证事件，用于自动接受不受信任的证书
        if (config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
        {
            config.CertificateValidator.CertificateValidation += (s, e) => { e.Accept = e.Error.StatusCode == StatusCodes.BadCertificateUntrusted; };
        }

        return config;
    }

    protected virtual async Task<Session> CreateSessionAsync()
    {
        // 验证应用配置对象
        await _appConfig.Validate(ApplicationType.Client);
        var _opcServerOpt = _channelOpt.ServerOpt;
        // 创建一个会话对象，用于连接到 OPC UA 服务器
        EndpointDescription epDescription = CoreClientUtils.SelectEndpoint(this._appConfig, _opcServerOpt.DiscoveryUrl, true);
        EndpointConfiguration epConfiguration = EndpointConfiguration.Create(_appConfig);
        ConfiguredEndpoint endpoint = new(null, epDescription, epConfiguration);
        var iden =
            _opcServerOpt.UsePassword ?
            new UserIdentity(_opcServerOpt.UserName, _opcServerOpt.Password) :
            new UserIdentity();
        Session session = await Session.Create(_appConfig, endpoint, false, false, "DataCollector", 60000, iden, null);
        this._logger.LogInformation("OPC UA 连接成功：{url}", _opcServerOpt.DiscoveryUrl);
        //await _mediator.Publish(new UILogNotification(new LogMessage()
        //{
        //    EventSource = "OpcUa",
        //    EventGroup = "OpcUa",
        //    Content = $"OPC UA 连接成功：{this._opcServerOpt.DiscoveryUrl}",
        //    Level = LogLevel.Information,
        //    Timestamp = DateTime.Now,
        //}));
        return session;
    }

    #region 连接
    private Session? _session { get; set; }

    /// <summary>
    /// 确保已经建立连接
    /// </summary>
    /// <returns></returns>
    public async Task EnsureConnectedAsync(bool force,CancellationToken ct)
    {
        _session ??= await CreateSessionAsync();

        if (!_session.Connected)
        {
            await _session.ReconnectAsync(ct);
        }
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task DisconnectAsync(CancellationToken ct)
    {
        try
        {
            if (this._session != null)
            {
                await _session.CloseAsync(ct);
            }
        }
        finally
        {
            this._session = null;
        }

    }
    #endregion



    #region 读写

    /// <summary>
    /// OpcUA 不支持按字节读取，这个方法未实现
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public Task<byte[]> ReadAsync(string address, int count, CancellationToken ct)
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// OpcUA 不支持按字节写入，这个方法未实现
    /// </summary>
    /// <param name="address"></param>
    /// <param name="bytes"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    /// <exception cref="NotImplementedException"></exception>
    public Task WriteAsync(string address, byte[] bytes, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region
    public async Task<(DataValueCollection values, IList<ServiceResult> errs)> ReadAsync(IList<NodeId> nodeIds, CancellationToken ct)
    {
        if(this._session is null)
        {
            throw new InvalidOperationException("会话未创建");
        }
        if(this._session.Connected == false)
        {
            throw new InvalidOperationException("会话未连接");
        }

        var (values, errs) = await this._session.ReadValuesAsync(nodeIds, ct);
        return (values, errs);
    }

    public async Task WriteAsync(IDictionary<NodeId, DataValue> toBeWritten, CancellationToken ct)
    {
        if (this._session is null)
        {
            throw new InvalidOperationException("会话未创建");
        }

        var writeValues = new WriteValueCollection();
        foreach (var kvp in toBeWritten)
        {
            // 更新要批量写入的buffer
            var nv = kvp.Value;
            var wv = new WriteValue()
            {
                NodeId = kvp.Key,
                AttributeId = Attributes.Value,
                Value = nv,
            };
            writeValues.Add(wv);
        }

        // 远程写入
        var resp = await this._session.WriteAsync(null, writeValues, CancellationToken.None);
        ClientBase.ValidateResponse(resp.Results, writeValues);
        ClientBase.ValidateDiagnosticInfos(resp.DiagnosticInfos, writeValues);

        var isNotAllGood = resp.Results.Any(r => StatusCode.IsNotGood(r));
        if (isNotAllGood)
        {
            var notgoods = resp.Results.Zip(writeValues)
                .Where(r => StatusCode.IsNotGood(r.First))
                .Select(r => new WriteValueErr(r.Second.NodeId, r.First))
                .ToList();
            throw new Exception($"通道写入失败:通道={this.ChannelName}。异常={string.Join(";", notgoods)}。");
        }
    }
    #endregion


    public void Dispose()
    {
        if (_session != null && _session.Connected)
        {
            try
            {
                _logger.LogInformation("通道={ChannelName} 正在断开连接...", ChannelName);
                _session.Close();
                _logger.LogInformation("通道={ChannelName}  断开连接完成!", ChannelName);
            }
            catch (Exception e)
            {
                _logger.LogWarning("通道={ChannelName} 释放异常:{exception}", ChannelName, e.Message);
            }
            finally
            {

            }
        }
    }
}

public record WriteValueErr(NodeId NodeId, StatusCode StatusCode)
{
    public static string ErrsToMsg(IList<WriteValueErr> erritems)
    {
        return string.Join("。", erritems.Select(e => $"{e.NodeId}={e.StatusCode}"));
    }
}