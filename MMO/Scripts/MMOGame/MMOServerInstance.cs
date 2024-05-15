﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.IO;
using LiteNetLibManager;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Serialization;

namespace MultiplayerARPG.MMO
{
    [RequireComponent(typeof(LogGUI))]
    [DefaultExecutionOrder(DefaultExecutionOrders.MMO_SERVER_INSTANCE)]
    public class MMOServerInstance : MonoBehaviour
    {
        public static MMOServerInstance Singleton { get; protected set; }

        [Header("Server Components")]
        [SerializeField]
        private CentralNetworkManager centralNetworkManager = null;
        [SerializeField]
        private MapSpawnNetworkManager mapSpawnNetworkManager = null;
        [SerializeField]
        private MapNetworkManager mapNetworkManager = null;
        [SerializeField]
        private DatabaseNetworkManager databaseNetworkManager = null;
        [SerializeField]
        [Tooltip("Use custom database client or not, if yes, it won't use `databaseNetworkManager` for network management")]
        private bool useCustomDatabaseClient = false;
        [SerializeField]
        [Tooltip("Which game object has a custom database client attached")]
        private GameObject customDatabaseClientSource = null;

        [Header("Settings")]
        [SerializeField]
        private bool useWebSocket = false;
        [SerializeField]
        private bool webSocketSecure = false;
        [SerializeField]
        private string webSocketCertPath = string.Empty;
        [SerializeField]
        private string webSocketCertPassword = string.Empty;

        public CentralNetworkManager CentralNetworkManager { get { return centralNetworkManager; } }
        public MapSpawnNetworkManager MapSpawnNetworkManager { get { return mapSpawnNetworkManager; } }
        public MapNetworkManager MapNetworkManager { get { return mapNetworkManager; } }
        public IDatabaseClient DatabaseClient
        {
            get
            {
                if (!useCustomDatabaseClient)
                    return databaseNetworkManager;
                else
                    return _customDatabaseClient;
            }
        }
        public bool UseWebSocket { get { return useWebSocket; } }
        public bool WebSocketSecure { get { return webSocketSecure; } }
        public string WebSocketCertificateFilePath { get { return webSocketCertPath; } }
        public string WebSocketCertificatePassword { get { return webSocketCertPassword; } }

        private LogGUI _cacheLogGUI;
        public LogGUI CacheLogGUI
        {
            get
            {
                if (_cacheLogGUI == null)
                    _cacheLogGUI = GetComponent<LogGUI>();
                return _cacheLogGUI;
            }
        }

        public string LogFileName { get; set; }

        [Header("Running In Editor")]
        public bool startCentralOnAwake;
        public bool startMapSpawnOnAwake;
        public bool startDatabaseOnAwake;
        public bool startMapOnAwake;
        public BaseMapInfo startingMap;
        public int databaseOptionIndex;
        [FormerlySerializedAs("databaseDisableCacheReading")]
        public bool disableDatabaseCaching;

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        private List<string> _spawningMaps;
        private List<SpawnAllocateMapByNameData> _spawningAllocateMaps;
        private string _startingMapId;
        private bool _startingCentralServer;
        private bool _startingMapSpawnServer;
        private bool _startingMapServer;
        private bool _startingDatabaseServer;
#endif
        private IDatabaseClient _customDatabaseClient;

        private void Awake()
        {
            if (Singleton != null)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            Singleton = this;

            // Always accept SSL
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback((sender, certificate, chain, policyErrors) => { return true; });

            // Setup custom database client
            if (customDatabaseClientSource == null)
                customDatabaseClientSource = gameObject;
            _customDatabaseClient = customDatabaseClientSource.GetComponent<IDatabaseClient>();

            CacheLogGUI.enabled = false;
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            GameInstance gameInstance = FindObjectOfType<GameInstance>();
            gameInstance.onGameDataLoaded = OnGameDataLoaded;

            if (!Application.isEditor)
            {
                // Json file read
                bool configFileFound = false;
                string configFolder = "./Config";
                string configFilePath = configFolder + "/serverConfig.json";
                Dictionary<string, object> jsonConfig = new Dictionary<string, object>();
                Logging.Log(ToString(), "Reading config file from " + configFilePath);
                if (File.Exists(configFilePath))
                {
                    // Read config file
                    Logging.Log(ToString(), "Found config file");
                    string dataAsJson = File.ReadAllText(configFilePath);
                    jsonConfig = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataAsJson);
                    configFileFound = true;
                }

                // Prepare data
                string[] args = Environment.GetCommandLineArgs();

                // Android fix
                if (args == null)
                    args = new string[0];

                // Database option index
                bool useCustomDatabaseClient = this.useCustomDatabaseClient = false;
                if (_customDatabaseClient != null && _customDatabaseClient as UnityEngine.Object != null)
                {
                    if (ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_USE_CUSTOM_DATABASE_CLIENT, out useCustomDatabaseClient, this.useCustomDatabaseClient))
                    {
                        this.useCustomDatabaseClient = useCustomDatabaseClient;
                    }
                    else if (ConfigReader.IsArgsProvided(args, ProcessArguments.CONFIG_USE_CUSTOM_DATABASE_CLIENT))
                    {
                        this.useCustomDatabaseClient = true;
                    }
                }
                jsonConfig[ProcessArguments.CONFIG_USE_CUSTOM_DATABASE_CLIENT] = useCustomDatabaseClient;

                int dbOptionIndex;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_DATABASE_OPTION_INDEX, out dbOptionIndex, 0) ||
                    ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_DATABASE_OPTION_INDEX, out dbOptionIndex, 0))
                {
                    if (!useCustomDatabaseClient)
                        databaseNetworkManager.SetDatabaseByOptionIndex(dbOptionIndex);
                }
                jsonConfig[ProcessArguments.CONFIG_DATABASE_OPTION_INDEX] = dbOptionIndex;

                // Database disable cache reading or not?
                bool disableDatabaseCaching = this.disableDatabaseCaching = false;
                // Old config key
#pragma warning disable CS0618 // Type or member is obsolete
                if (ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_DATABASE_DISABLE_CACHE_READING, out disableDatabaseCaching, this.disableDatabaseCaching))
                {
                    this.disableDatabaseCaching = disableDatabaseCaching;
                }
                else if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_DATABASE_DISABLE_CACHE_READING))
                {
                    this.disableDatabaseCaching = true;
                }
#pragma warning restore CS0618 // Type or member is obsolete
                // New config key
                if (ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_DISABLE_DATABASE_CACHING, out disableDatabaseCaching, this.disableDatabaseCaching))
                {
                    this.disableDatabaseCaching = disableDatabaseCaching;
                }
                else if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_DISABLE_DATABASE_CACHING))
                {
                    this.disableDatabaseCaching = true;
                }
                jsonConfig[ProcessArguments.CONFIG_DISABLE_DATABASE_CACHING] = disableDatabaseCaching;

                // Use Websocket or not?
                bool useWebSocket = this.useWebSocket = false;
                if (ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_USE_WEB_SOCKET, out useWebSocket, this.useWebSocket))
                {
                    this.useWebSocket = useWebSocket;
                }
                else if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_USE_WEB_SOCKET))
                {
                    this.useWebSocket = true;
                }
                jsonConfig[ProcessArguments.CONFIG_USE_WEB_SOCKET] = useWebSocket;

                // Is websocket running in secure mode or not?
                bool webSocketSecure = this.webSocketSecure = false;
                if (ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_WEB_SOCKET_SECURE, out webSocketSecure, this.webSocketSecure))
                {
                    this.webSocketSecure = webSocketSecure;
                }
                else if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_WEB_SOCKET_SECURE))
                {
                    this.webSocketSecure = true;
                }
                jsonConfig[ProcessArguments.CONFIG_WEB_SOCKET_SECURE] = webSocketSecure;

                // Where is the certification file path?
                string webSocketCertPath;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_WEB_SOCKET_CERT_PATH, out webSocketCertPath, this.webSocketCertPath) ||
                    ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_WEB_SOCKET_CERT_PATH, out webSocketCertPath, this.webSocketCertPath))
                {
                    this.webSocketCertPath = webSocketCertPath;
                }
                jsonConfig[ProcessArguments.CONFIG_WEB_SOCKET_CERT_PATH] = webSocketCertPath;

                // What is the certification password?
                string webSocketCertPassword;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_WEB_SOCKET_CERT_PASSWORD, out webSocketCertPassword, this.webSocketCertPassword) ||
                    ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_WEB_SOCKET_CERT_PASSWORD, out webSocketCertPassword, this.webSocketCertPassword))
                {
                    this.webSocketCertPassword = webSocketCertPassword;
                }
                jsonConfig[ProcessArguments.CONFIG_WEB_SOCKET_CERT_PASSWORD] = webSocketCertPassword;

                // Central network address
                string centralNetworkAddress;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_CENTRAL_ADDRESS, out centralNetworkAddress, mapSpawnNetworkManager.clusterServerAddress) ||
                    ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_CENTRAL_ADDRESS, out centralNetworkAddress, mapSpawnNetworkManager.clusterServerAddress))
                {
                    mapSpawnNetworkManager.clusterServerAddress = centralNetworkAddress;
                    mapNetworkManager.clusterServerAddress = centralNetworkAddress;
                }
                jsonConfig[ProcessArguments.CONFIG_CENTRAL_ADDRESS] = centralNetworkAddress;

                // Central network port
                int centralNetworkPort;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_CENTRAL_PORT, out centralNetworkPort, centralNetworkManager.networkPort) ||
                    ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_CENTRAL_PORT, out centralNetworkPort, centralNetworkManager.networkPort))
                {
                    centralNetworkManager.networkPort = centralNetworkPort;
                }
                jsonConfig[ProcessArguments.CONFIG_CENTRAL_PORT] = centralNetworkPort;

                // Central max connections
                int centralMaxConnections;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_CENTRAL_MAX_CONNECTIONS, out centralMaxConnections, centralNetworkManager.maxConnections) ||
                    ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_CENTRAL_MAX_CONNECTIONS, out centralMaxConnections, centralNetworkManager.maxConnections))
                {
                    centralNetworkManager.maxConnections = centralMaxConnections;
                }
                jsonConfig[ProcessArguments.CONFIG_CENTRAL_MAX_CONNECTIONS] = centralMaxConnections;

                // Central map spawn timeout (milliseconds)
                int mapSpawnMillisecondsTimeout;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_MAP_SPAWN_MILLISECONDS_TIMEOUT, out mapSpawnMillisecondsTimeout, centralNetworkManager.mapSpawnMillisecondsTimeout) ||
                    ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_MAP_SPAWN_MILLISECONDS_TIMEOUT, out mapSpawnMillisecondsTimeout, centralNetworkManager.mapSpawnMillisecondsTimeout))
                {
                    centralNetworkManager.mapSpawnMillisecondsTimeout = mapSpawnMillisecondsTimeout;
                }
                jsonConfig[ProcessArguments.CONFIG_MAP_SPAWN_MILLISECONDS_TIMEOUT] = mapSpawnMillisecondsTimeout;

                // Central - default channels max connections
                int defaultChannelMaxConnections;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_DEFAULT_CHANNEL_MAX_CONNECTIONS, out defaultChannelMaxConnections, centralNetworkManager.defaultChannelMaxConnections) ||
                    ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_DEFAULT_CHANNEL_MAX_CONNECTIONS, out defaultChannelMaxConnections, centralNetworkManager.defaultChannelMaxConnections))
                {
                    centralNetworkManager.defaultChannelMaxConnections = defaultChannelMaxConnections;
                }
                jsonConfig[ProcessArguments.CONFIG_DEFAULT_CHANNEL_MAX_CONNECTIONS] = defaultChannelMaxConnections;

                // Central - channels
                List<ChannelData> channels;
                if (ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_CHANNELS, out channels, centralNetworkManager.channels))
                {
                    centralNetworkManager.channels = channels;
                }
                jsonConfig[ProcessArguments.CONFIG_CHANNELS] = channels;

                // Central->Cluster network port
                int clusterNetworkPort;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_CLUSTER_PORT, out clusterNetworkPort, centralNetworkManager.clusterServerPort) ||
                    ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_CLUSTER_PORT, out clusterNetworkPort, centralNetworkManager.clusterServerPort))
                {
                    centralNetworkManager.clusterServerPort = clusterNetworkPort;
                    mapSpawnNetworkManager.clusterServerPort = clusterNetworkPort;
                    mapNetworkManager.clusterServerPort = clusterNetworkPort;
                }
                jsonConfig[ProcessArguments.CONFIG_CLUSTER_PORT] = clusterNetworkPort;

                // Machine network address, will be set to map spawn / map / chat
                string machineNetworkAddress;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_MACHINE_ADDRESS, out machineNetworkAddress, mapSpawnNetworkManager.machineAddress) ||
                    ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_MACHINE_ADDRESS, out machineNetworkAddress, mapSpawnNetworkManager.machineAddress))
                {
                    mapSpawnNetworkManager.machineAddress = machineNetworkAddress;
                    mapNetworkManager.machineAddress = machineNetworkAddress;
                }
                jsonConfig[ProcessArguments.CONFIG_MACHINE_ADDRESS] = machineNetworkAddress;

                // Map spawn network port
                int mapSpawnNetworkPort;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_MAP_SPAWN_PORT, out mapSpawnNetworkPort, mapSpawnNetworkManager.networkPort) ||
                    ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_MAP_SPAWN_PORT, out mapSpawnNetworkPort, mapSpawnNetworkManager.networkPort))
                {
                    mapSpawnNetworkManager.networkPort = mapSpawnNetworkPort;
                }
                jsonConfig[ProcessArguments.CONFIG_MAP_SPAWN_PORT] = mapSpawnNetworkPort;

                // Map spawn exe path
                string spawnExePath;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_SPAWN_EXE_PATH, out spawnExePath, mapSpawnNetworkManager.exePath) ||
                    ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_SPAWN_EXE_PATH, out spawnExePath, mapSpawnNetworkManager.exePath))
                {
                    mapSpawnNetworkManager.exePath = spawnExePath;
                }
                if (!File.Exists(spawnExePath))
                {
                    spawnExePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                    mapSpawnNetworkManager.exePath = spawnExePath;
                }
                jsonConfig[ProcessArguments.CONFIG_SPAWN_EXE_PATH] = spawnExePath;

                // Map spawn in batch mode
                bool notSpawnInBatchMode = mapSpawnNetworkManager.notSpawnInBatchMode = false;
                if (ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_NOT_SPAWN_IN_BATCH_MODE, out notSpawnInBatchMode, mapSpawnNetworkManager.notSpawnInBatchMode))
                {
                    mapSpawnNetworkManager.notSpawnInBatchMode = notSpawnInBatchMode;
                }
                else if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_NOT_SPAWN_IN_BATCH_MODE))
                {
                    mapSpawnNetworkManager.notSpawnInBatchMode = true;
                }
                jsonConfig[ProcessArguments.CONFIG_NOT_SPAWN_IN_BATCH_MODE] = notSpawnInBatchMode;

                // Map spawn start port
                int spawnStartPort;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_SPAWN_START_PORT, out spawnStartPort, mapSpawnNetworkManager.startPort) ||
                    ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_SPAWN_START_PORT, out spawnStartPort, mapSpawnNetworkManager.startPort))
                {
                    mapSpawnNetworkManager.startPort = spawnStartPort;
                }
                jsonConfig[ProcessArguments.CONFIG_SPAWN_START_PORT] = spawnStartPort;

                // Spawn channels
                List<string> spawnChannels;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_SPAWN_CHANNELS, out spawnChannels, mapSpawnNetworkManager.spawningChannelIds) ||
                    ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_SPAWN_CHANNELS, out spawnChannels, mapSpawnNetworkManager.spawningChannelIds))
                {
                    mapSpawnNetworkManager.spawningChannelIds = spawnChannels;
                }
                jsonConfig[ProcessArguments.CONFIG_SPAWN_CHANNELS] = spawnChannels;

                // Spawn maps
                List<string> defaultSpawnMaps = new List<string>();
                foreach (BaseMapInfo mapInfo in mapSpawnNetworkManager.spawningMaps)
                {
                    if (mapInfo != null)
                        defaultSpawnMaps.Add(mapInfo.Id);
                }
                List<string> spawnMaps;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_SPAWN_MAPS, out spawnMaps, defaultSpawnMaps) ||
                    ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_SPAWN_MAPS, out spawnMaps, defaultSpawnMaps))
                {
                    _spawningMaps = spawnMaps;
                }
                jsonConfig[ProcessArguments.CONFIG_SPAWN_MAPS] = spawnMaps;

                // Spawn allocate maps
                List<SpawnAllocateMapByNameData> defaultSpawnAllocateMaps = new List<SpawnAllocateMapByNameData>();
                foreach (SpawnAllocateMapData spawnAllocateMap in mapSpawnNetworkManager.spawningAllocateMaps)
                {
                    if (spawnAllocateMap.mapInfo != null)
                    {
                        defaultSpawnAllocateMaps.Add(new SpawnAllocateMapByNameData()
                        {
                            mapName = spawnAllocateMap.mapInfo.Id,
                            allocateAmount = spawnAllocateMap.allocateAmount,
                        });
                    }
                }
                List<SpawnAllocateMapByNameData> spawnAllocateMaps;
                if (ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_SPAWN_ALLOCATE_MAPS, out spawnAllocateMaps, defaultSpawnAllocateMaps))
                {
                    _spawningAllocateMaps = spawnAllocateMaps;
                }
                jsonConfig[ProcessArguments.CONFIG_SPAWN_ALLOCATE_MAPS] = spawnAllocateMaps;

                // Map network port
                int mapNetworkPort;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_MAP_PORT, out mapNetworkPort, mapNetworkManager.networkPort) ||
                    ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_MAP_PORT, out mapNetworkPort, mapNetworkManager.networkPort))
                {
                    mapNetworkManager.networkPort = mapNetworkPort;
                }
                jsonConfig[ProcessArguments.CONFIG_MAP_PORT] = mapNetworkPort;

                // Map max connections
                int mapMaxConnections;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_MAP_MAX_CONNECTIONS, out mapMaxConnections, mapNetworkManager.maxConnections) ||
                    ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_MAP_MAX_CONNECTIONS, out mapMaxConnections, mapNetworkManager.maxConnections))
                {
                    mapNetworkManager.maxConnections = mapMaxConnections;
                }
                jsonConfig[ProcessArguments.CONFIG_MAP_MAX_CONNECTIONS] = mapMaxConnections;

                // Map scene name
                string mapName = string.Empty;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_MAP_NAME, out mapName, string.Empty))
                {
                    _startingMapId = mapName;
                }

                // Channel Id
                string channelId = string.Empty;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_CHANNEL_ID, out channelId, string.Empty))
                {
                    mapNetworkManager.ChannelId = channelId;
                }

                // Instance Id
                string instanceId = string.Empty;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_INSTANCE_ID, out instanceId, string.Empty))
                {
                    mapNetworkManager.MapInstanceId = instanceId;
                }

                // Instance Warp Position
                float instancePosX, instancePosY, instancePosZ;
                if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_INSTANCE_POSITION_X, out instancePosX, 0f) &&
                    ConfigReader.ReadArgs(args, ProcessArguments.ARG_INSTANCE_POSITION_Y, out instancePosY, 0f) &&
                    ConfigReader.ReadArgs(args, ProcessArguments.ARG_INSTANCE_POSITION_Z, out instancePosZ, 0f))
                {
                    mapNetworkManager.MapInstanceWarpToPosition = new Vector3(instancePosX, instancePosY, instancePosZ);
                }

                // Instance Warp Override Rotation, Instance Warp Rotation
                mapNetworkManager.MapInstanceWarpOverrideRotation = ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_INSTANCE_OVERRIDE_ROTATION);
                float instanceRotX, instanceRotY, instanceRotZ;
                if (mapNetworkManager.MapInstanceWarpOverrideRotation &&
                    ConfigReader.ReadArgs(args, ProcessArguments.ARG_INSTANCE_ROTATION_X, out instanceRotX, 0f) &&
                    ConfigReader.ReadArgs(args, ProcessArguments.ARG_INSTANCE_ROTATION_Y, out instanceRotY, 0f) &&
                    ConfigReader.ReadArgs(args, ProcessArguments.ARG_INSTANCE_ROTATION_Z, out instanceRotZ, 0f))
                {
                    mapNetworkManager.MapInstanceWarpToRotation = new Vector3(instanceRotX, instanceRotY, instanceRotZ);
                }

                // Allocate map server
                bool isAllocate = false;
                if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_ALLOCATE))
                {
                    mapNetworkManager.IsAllocate = true;
                    isAllocate = true;
                }

                if (!useCustomDatabaseClient)
                {
                    // Database network address
                    string databaseNetworkAddress;
                    if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_DATABASE_ADDRESS, out databaseNetworkAddress, databaseNetworkManager.networkAddress) ||
                        ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_DATABASE_ADDRESS, out databaseNetworkAddress, databaseNetworkManager.networkAddress))
                    {
                        databaseNetworkManager.networkAddress = databaseNetworkAddress;
                    }
                    jsonConfig[ProcessArguments.CONFIG_DATABASE_ADDRESS] = databaseNetworkAddress;

                    // Database network port
                    int databaseNetworkPort;
                    if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_DATABASE_PORT, out databaseNetworkPort, databaseNetworkManager.networkPort) ||
                        ConfigReader.ReadConfigs(jsonConfig, ProcessArguments.CONFIG_DATABASE_PORT, out databaseNetworkPort, databaseNetworkManager.networkPort))
                    {
                        databaseNetworkManager.networkPort = databaseNetworkPort;
                    }
                    jsonConfig[ProcessArguments.CONFIG_DATABASE_PORT] = databaseNetworkPort;
                }

                LogFileName = "Log";
                bool startLog = false;

                if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_START_DATABASE_SERVER))
                {
                    if (!string.IsNullOrEmpty(LogFileName))
                        LogFileName += "_";
                    LogFileName += "Database";
                    startLog = true;
                    _startingDatabaseServer = true;
                }

                if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_START_CENTRAL_SERVER))
                {
                    if (!string.IsNullOrEmpty(LogFileName))
                        LogFileName += "_";
                    LogFileName += "Central";
                    startLog = true;
                    _startingCentralServer = true;
                }

                if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_START_MAP_SPAWN_SERVER))
                {
                    if (!string.IsNullOrEmpty(LogFileName))
                        LogFileName += "_";
                    LogFileName += "MapSpawn";
                    startLog = true;
                    _startingMapSpawnServer = true;
                }

                if (ConfigReader.IsArgsProvided(args, ProcessArguments.ARG_START_MAP_SERVER))
                {
                    if (!string.IsNullOrEmpty(LogFileName))
                        LogFileName += "_";
                    LogFileName += $"Map({mapName})-Channel({channelId})-Allocate({isAllocate})-Instance({instanceId})";
                    startLog = true;
                    _startingMapServer = true;
                }

                if (_startingDatabaseServer || _startingCentralServer || _startingMapSpawnServer || _startingMapServer)
                {
                    if (!configFileFound)
                    {
                        // Write config file
                        Logging.Log(ToString(), "Not found config file, creating a new one");
                        if (!Directory.Exists(configFolder))
                            Directory.CreateDirectory(configFolder);
                        File.WriteAllText(configFilePath, JsonConvert.SerializeObject(jsonConfig, Formatting.Indented));
                    }
                }

                if (startLog)
                    EnableLogger(LogFileName);
            }
            else
            {
                if (!useCustomDatabaseClient)
                {
                    databaseNetworkManager.SetDatabaseByOptionIndex(databaseOptionIndex);
                    databaseNetworkManager.DisableDatabaseCaching = disableDatabaseCaching;
                }

                if (startDatabaseOnAwake)
                    _startingDatabaseServer = true;

                if (startCentralOnAwake)
                    _startingCentralServer = true;

                if (startMapSpawnOnAwake)
                    _startingMapSpawnServer = true;

                if (startMapOnAwake)
                {
                    // If run map-server, don't load home scene (home scene load in `Game Instance`)
                    _startingMapId = startingMap.Id;
                    _startingMapServer = true;
                }
            }
#endif
        }

        public void EnableLogger(string fileName)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            CacheLogGUI.SetupLogger(fileName);
            CacheLogGUI.enabled = true;
#endif
        }

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        private void OnGameDataLoaded()
        {
            databaseNetworkManager.DatabaseCache = new LocalDatabaseCache();
            DatabaseNetworkManager.GuildMemberRoles = GameInstance.Singleton.SocialSystemSetting.GuildMemberRoles;
            DatabaseNetworkManager.GuildExpTree = GameInstance.Singleton.SocialSystemSetting.GuildExpTree;

            if (_startingDatabaseServer)
            {
                // Start database manager server
                StartDatabaseManagerServer();
            }

            if (_startingCentralServer)
            {
                // Start central server
                StartCentralServer();
            }

            if (_startingMapSpawnServer)
            {
                // Start map spawn server
                if (_spawningMaps != null && _spawningMaps.Count > 0)
                {
                    mapSpawnNetworkManager.spawningMaps = new List<BaseMapInfo>();
                    foreach (string spawningMapId in _spawningMaps)
                    {
                        if (!GameInstance.MapInfos.TryGetValue(spawningMapId, out BaseMapInfo tempMapInfo))
                            continue;
                        mapSpawnNetworkManager.spawningMaps.Add(tempMapInfo);
                    }
                }
                if (_spawningAllocateMaps != null && _spawningAllocateMaps.Count > 0)
                {
                    mapSpawnNetworkManager.spawningAllocateMaps = new List<SpawnAllocateMapData>();
                    foreach (SpawnAllocateMapByNameData spawningMap in _spawningAllocateMaps)
                    {
                        if (!GameInstance.MapInfos.TryGetValue(spawningMap.mapName, out BaseMapInfo tempMapInfo))
                            continue;
                        mapSpawnNetworkManager.spawningAllocateMaps.Add(new SpawnAllocateMapData()
                        {
                            mapInfo = tempMapInfo,
                            allocateAmount = spawningMap.allocateAmount,
                        });
                    }
                }
                StartMapSpawnServer();
            }

            if (_startingMapServer)
            {
                // Start map server
                BaseMapInfo tempMapInfo;
                if (!string.IsNullOrEmpty(_startingMapId) && GameInstance.MapInfos.TryGetValue(_startingMapId, out tempMapInfo))
                {
                    mapNetworkManager.Assets.onlineScene.SceneName = tempMapInfo.Scene.SceneName;
                    mapNetworkManager.SetMapInfo(tempMapInfo);
                }
                StartMapServer();
            }

            GameInstance gameInstance = FindObjectOfType<GameInstance>();
            gameInstance.LoadHomeScenePreventions[nameof(MMOServerInstance)] = false;

            if (_startingCentralServer ||
                _startingMapSpawnServer ||
                _startingMapServer)
            {
                // Start database manager client, it will connect to database manager server
                // To request database functions
                gameInstance.LoadHomeScenePreventions[nameof(MMOServerInstance)] = !Application.isEditor || _startingMapServer;
                StartDatabaseManagerClient();
            }
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        #region Server functions
        public void StartCentralServer()
        {
            CentralNetworkManager.useWebSocket = UseWebSocket;
            CentralNetworkManager.webSocketSecure = WebSocketSecure;
            CentralNetworkManager.webSocketCertificateFilePath = WebSocketCertificateFilePath;
            CentralNetworkManager.webSocketCertificatePassword = WebSocketCertificatePassword;
            centralNetworkManager.DatabaseClient = DatabaseClient;
            centralNetworkManager.DataManager = new CentralServerDataManager();
            CentralNetworkManager.StartServer();
        }

        public void StartMapSpawnServer()
        {
            mapSpawnNetworkManager.StartServer();
        }

        public void StartMapServer()
        {
            MapNetworkManager.useWebSocket = UseWebSocket;
            MapNetworkManager.webSocketSecure = WebSocketSecure;
            MapNetworkManager.webSocketCertificateFilePath = WebSocketCertificateFilePath;
            MapNetworkManager.webSocketCertificatePassword = WebSocketCertificatePassword;
            MapNetworkManager.StartServer();
        }

        public void StartDatabaseManagerServer()
        {
            if (!useCustomDatabaseClient)
                databaseNetworkManager.StartServer();
        }

        public void StartDatabaseManagerClient()
        {
            if (!useCustomDatabaseClient)
                databaseNetworkManager.StartClient();
        }
        #endregion
#endif
    }
}
