﻿namespace MultiplayerARPG.MMO
{
#nullable enable
    public partial struct CreateBuildingReq
    {
        public string ChannelId { get; set; }
        public string MapName { get; set; }
        public BuildingSaveData BuildingData { get; set; }
    }
}