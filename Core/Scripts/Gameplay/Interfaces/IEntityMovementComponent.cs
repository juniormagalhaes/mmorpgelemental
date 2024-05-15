using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public interface IEntityMovementComponent : IEntityMovement, IGameEntityComponent
    {
        /// <summary>
        /// Return `TRUE` if it have something written
        /// </summary>
        /// <param name="writeTimestamp"></param>
        /// <param name="writer"></param>
        /// <param name="shouldSendReliably"></param>
        /// <returns></returns>
        bool WriteClientState(long writeTimestamp, NetDataWriter writer, out bool shouldSendReliably);
        /// <summary>
        /// Return `TRUE` if it have something written
        /// </summary>
        /// <param name="writeTimestamp"></param>
        /// <param name="writer"></param>
        /// <param name="shouldSendReliably"></param>
        /// <returns></returns>
        bool WriteServerState(long writeTimestamp, NetDataWriter writer, out bool shouldSendReliably);
        void ReadClientStateAtServer(long peerTimestamp, NetDataReader reader);
        void ReadServerStateAtClient(long peerTimestamp, NetDataReader reader);
    }
}
